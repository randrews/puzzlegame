using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TiledSharp;

namespace PuzzleGame
{
    public partial class GameController
    {
        public TmxMap Map { get; private set; }
        public MainWindow Window { get; private set; }

        /// <summary>
        /// Maps Type name to rectangle in Image, so we can look up tiles that aren't on the map at the start, like effects
        /// </summary>
        public Dictionary<string, Rectangle> Tileset { get; private set; }

        /// <summary>
        /// The image for the tileset
        /// </summary>
        private Image _image;
        public Image Image
        {
            get
            {
                if (_image == null)
                {
                    var filename = Map.Tilesets.First().Image.Source;
                    _image = Image.FromFile(filename);
                }
                return _image;
            } 
        }

        /// <summary>
        /// Size of each tile in pixels
        /// </summary>
        public Size TileSize { get; private set; }
        
        /// <summary>
        /// Size of the map in tiles
        /// </summary>
        public Size MapSize { get; set; }

        /// <summary>
        /// 2D array of all the items in their current locations
        /// </summary>
        public Item[,] Items { get; private set; }
        public Rectangle?[,] Walls { get; private set; }
        public Rectangle?[,] Floors { get; private set; }

        public Point PlayerLocation { get; set; }
        public Player Player { get; private set; }
        public Exit Exit { get; private set; }

        /// <summary>
        /// The path to the next level's file.
        /// </summary>
        public string NextLevel { get; private set; }
        public GameController(MainWindow window, TmxMap map)
        {
            Window = window;
            Map = map;
            Tileset = LoadTileset();
            SetupAnimationFrames();
            MapSize = ReadMapSize();
            NextLevel = ReadNextLevel();
            Items = LoadItems();
            Walls = LoadWalls();
            Floors = LoadFloors();
            if(PlayerLocation == null) throw new ArgumentException("Map doesn't contain a start location");
            Window.UpdateStatusLabel(GetStatusLabel());
        }

        /// <summary>
        /// Return an array of rectangles for the item layer. This is the item layer as it currently
        /// stands, not what it was when the map loaded.
        /// </summary>
        /// <returns></returns>
        public Rectangle?[,] GetItemRectangles()
        {
            var rectangles = new Rectangle?[Items.GetLength(0), Items.GetLength(1)];

            for (int y = 0; y < Items.GetLength(1); y++)
            {
                for (int x = 0; x < Items.GetLength(0); x++)
                {
                    if(Items[x,y] != null) rectangles[x, y] = Items[x, y].Rectangle;
                }
            }

            return rectangles;
        }

        /// <summary>
        /// Reads a layer of the map, and returns the rectangles associated with each tile.
        /// This is the rectangles for the tile regardless of that tile's Type attribute;
        /// whatever tile Tiled shows, that's the rect you get.
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        private Rectangle?[,] LayerToRectangles(string layerName)
        {
            var cells = new Rectangle?[MapSize.Width, MapSize.Height];
            var layer = Map.Layers[layerName];

            foreach (var tile in layer.Tiles)
            {
                cells[tile.X,tile.Y] = GidToRectangle(tile.Gid);
            }

            return cells;
        }

        /// <summary>
        /// Take a tile Gid and return its rectangle.
        /// TODO: This assumes only one tileset
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        private Rectangle? GidToRectangle(int gid)
        {
            if (gid == 0) return null;
            gid -= 1;
            int width = Image.Width / TileSize.Width;
            int x = gid%width;
            int y = gid/width;

            return new Rectangle(x*TileSize.Width, y*TileSize.Height, TileSize.Width, TileSize.Height);
        }

        /// <summary>
        /// Called when the user wants to move the player
        /// </summary>
        /// <param name="direction">The direction we'll be moving</param>
        internal void MoveCommand(Direction direction)
        {
            var newLocation = TranslateLocation(PlayerLocation, direction);

            if (InBounds(newLocation) && Walls[newLocation.X, newLocation.Y] == null) // No wall there
            {
                // First, is there an item there? If not, we're good
                if (Items[newLocation.X, newLocation.Y] == null)
                    PlayerLocation = newLocation;
                else
                {
                    var item = Items[newLocation.X, newLocation.Y];
                    if (!item.Solid) // There's an item but it's not solid. Move on top of it, and tell it so
                    {
                        PlayerLocation = newLocation;
                        item.PlayerEnter(Player, this);
                    }
                    else if (item.Pushable) // Try pushing the object
                    {
                        if(TryPush(newLocation, direction, true)) // This returns true if it can be pushed, with the side effect of actually pushing it
                        {
                            PlayerLocation = newLocation;
                        }
                    }
                    else // It's solid, so bump into it and see what it does
                    {
                        item.Bump(Player);
                    }
                }
            }

            // Now, remove any items that died this round
            for (int y = 0; y < Items.GetLength(1); y++)
                for (int x = 0; x < Items.GetLength(0); x++)
                    if (Items[x, y] != null && Items[x, y].Dead)
                        Items[x, y] = null;

            // Update the status label
            Window.UpdateStatusLabel(GetStatusLabel());

            // If there's no gold left, open the exit
            if (!Items.OfType<Gold>().Any())
            {
                Exit.Open();
            }
        }

        private bool InBounds(Point p)
        {
            return p.X >= 0 && p.X < MapSize.Width && // X in bounds
                   p.Y >= 0 && p.Y < MapSize.Height;
        }

        private Point TranslateLocation(Point p, Direction direction)
        {
            var newLocation = new Point(p.X, p.Y);

            switch (direction)
            {
                case Direction.Up:
                    newLocation.Y--;
                    break;
                case Direction.Down:
                    newLocation.Y++;
                    break;
                case Direction.Left:
                    newLocation.X--;
                    break;
                case Direction.Right:
                    newLocation.X++;
                    break;
            }

            return newLocation;
        }

        /// <summary>
        /// Try to push an item in a direction
        /// </summary>
        /// <param name="location">Location of the item being pushed</param>
        /// <param name="direction">Direction it's being pushed</param>
        /// <param name="byPlayer">Whether the player is doing the pushing, or something else</param>
        /// <returns></returns>
        private bool TryPush(Point location, Direction direction, bool byPlayer)
        {
            var newLocation = TranslateLocation(location, direction);
            if (! InBounds(newLocation)) return false; // First, are we trying to push it off the map?
            if (Walls[newLocation.X, newLocation.Y] != null) return false; // Trying to push into a wall, so no.

            var pushed = Items[location.X, location.Y]; // The thing we're pushing
            var pushedInto = Items[newLocation.X, newLocation.Y]; // Contents of the space we're pushing it into

            // Wait, there's no item being pushed? That's the caller's problem
            if(pushed == null) throw new ArgumentException("Trying to push an empty space");

            // This thing can't be pushed.
            if (!pushed.Solid || !pushed.Pushable) return false;

            // Trying to push into an empty space, that's totally cool
            if (pushedInto == null)
            {
                if (pushed.Heavy && !byPlayer) return false; // Heavy things can only be pushed by the player directly, not other things

                Items[newLocation.X, newLocation.Y] = pushed;
                Items[location.X, location.Y] = null;
                return true;
            }

            // Otherwise, there's something there.

            if (pushed.Heavy) return false; // Heavy things can only be pushed into empty spaces

            if (TryPush(newLocation, direction, false)) // Try to push it. If this returns true, then newLocation must be empty...
            {
                Items[newLocation.X, newLocation.Y] = pushed;
                Items[location.X, location.Y] = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called every half-second when the view's animation timer fires
        /// </summary>
        internal void AnimationTick()
        {
            foreach (var item in Items)
            {
                if (item == null) continue;
                item.Animate();
            }

            if(Player != null) Player.Animate();
        }

        public string GetStatusLabel()
        {
            if (!Player.HasAnyKeys()) return "";
            var keys = new List<string>();

            if (Player.Keys[Color.Blue] == 1)
                keys.Add("blue");
            else if (Player.Keys[Color.Blue] > 1)
                keys.Add(string.Format("blue ({0})", Player.Keys[Color.Blue]));

            if (Player.Keys[Color.Green] == 1)
                keys.Add("green");
            else if (Player.Keys[Color.Green] > 1)
                keys.Add(string.Format("green ({0})", Player.Keys[Color.Green]));

            if (Player.Keys[Color.Red] == 1)
                keys.Add("red");
            else if (Player.Keys[Color.Red] > 1)
                keys.Add(string.Format("red ({0})", Player.Keys[Color.Red]));

            if (Player.Keys[Color.Yellow] == 1)
                keys.Add("yellow");
            else if (Player.Keys[Color.Yellow] > 1)
                keys.Add(string.Format("yellow ({0})", Player.Keys[Color.Yellow]));

            return string.Format("Keys: {0}", string.Join(", ", keys));
        }

        public void ShowMessage(string message)
        {
            Window.ShowMessage(message);
        }

        public void ExitLevel()
        {
            if (NextLevel != null)
            {
                ShowMessage("Congratulations!\n\nPress any key to start the\nnext level");
                Window.OpenMap("maps\\level2.tmx");
            }
            else
            {
                ShowMessage("Congratulations!\n\nYou have finished the\nlast level");
            }
        }

    }
}
