using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        public Size TileSize { get; private set; }

        /// <summary>
        /// 2D array of all the items in their current locations
        /// </summary>
        public Item[,] Items { get; private set; }
        public Rectangle?[,] Walls { get; private set; }
        public Rectangle?[,] Floors { get; private set; }

        public Point? PlayerLocation { get; set; }
        public Player Player { get; private set; }

        public GameController(MainWindow window, TmxMap map)
        {
            Window = window;
            Map = map;
            Tileset = LoadTileset();
            SetupAnimationFrames();
            Items = LoadItems();
            Walls = LoadWalls();
            Floors = LoadFloors();
            if(PlayerLocation == null) throw new ArgumentException("Map doesn't contain a start location");
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
        /// TODO: Assumes 20x20 map
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        private Rectangle?[,] LayerToRectangles(string layerName)
        {
            var cells = new Rectangle?[20,20];
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
            if (PlayerLocation == null) return;
            var newLocation = new Point(((Point)PlayerLocation).X, ((Point)PlayerLocation).Y);

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

            if (newLocation.X >= 0 && newLocation.X < Walls.GetLength(0) && // X in bounds
                newLocation.Y >= 0 && newLocation.Y < Walls.GetLength(1) && // Y in bounds
                Walls[newLocation.X, newLocation.Y] == null) // No wall there
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
                        item.PlayerEnter(Player);
                    }
                }
            }

            // Now, remove any items that died this round
            for (int y = 0; y < Items.GetLength(1); y++)
                for (int x = 0; x < Items.GetLength(0); x++)
                    if (Items[x, y] != null && Items[x, y].Dead)
                        Items[x, y] = null;
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
    }
}
