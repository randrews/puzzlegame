﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using PuzzleGame.Items;
using TiledSharp;

namespace PuzzleGame
{
    public partial class GameController
    {
        public TmxMap Map { get; private set; }
        public MainWindow Window { get; private set; }

        private SpriteLibrary SpriteLibrary { get; set; }
        /// <summary>
        /// The image for the tileset
        /// </summary>
        public Image Image { get { return SpriteLibrary.Image; } }

        /// <summary>
        /// Size of each tile in pixels
        /// </summary>
        public Size TileSize { get { return SpriteLibrary.TileSize; } }
        
        /// <summary>
        /// Size of the map in tiles
        /// </summary>
        public Size MapSize { get; set; }

        /// <summary>
        /// 2D array of all the items in their current locations
        /// </summary>
        public Grid<Item> Items { get; private set; }
        public Grid<Sprite> Walls { get; private set; }
        public Grid<Item> Floors { get; private set; }

        public Point PlayerLocation { get; set; }
        public Player Player { get; private set; }

        /// <summary>
        /// The path to the next level's file.
        /// </summary>
        public string NextLevel { get; private set; }

        public GameController(MainWindow window, TmxMap map)
        {
            Window = window;
            Map = map;
            SpriteLibrary = new SpriteLibrary(map);
            Item.SpriteLibrary = SpriteLibrary;
            Item.Random = new Random();

            MapSize = ReadMapSize();
            NextLevel = ReadNextLevel();
            Items = LoadItems();
            Walls = LoadWalls();
            Floors = LoadFloors();
            if(PlayerLocation == null) throw new ArgumentException("Map doesn't contain a start location");
            Window.UpdateStatusLabel(GetStatusLabel());
        }

        /// <summary>
        /// Reads a layer of the map, and returns the sprites associated with each tile.
        /// This is the sprite for the tile regardless of that tile's Type attribute;
        /// whatever tile Tiled shows, that's the sprite you get.
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        private Grid<Sprite> LayerToSprites(string layerName)
        {
            var cells = new Grid<Sprite>(MapSize);
            var layer = Map.Layers[layerName];

            foreach (var tile in layer.Tiles)
            {
                if(tile.Gid == 0) continue;
                cells[tile.X,tile.Y] = SpriteLibrary[tile.Gid];
            }

            return cells;
        }

        /// <summary>
        /// Called when the user wants to move the player
        /// </summary>
        /// <param name="direction">The direction we'll be moving</param>
        internal void MoveCommand(Direction direction)
        {
            var newLocation = Grid<Item>.TranslateLocation(PlayerLocation, direction);
            TryMove(PlayerLocation, direction, true);

            // We just had a turn, so, fire those events:
            var finished = new HashSet<Item>();
            Items.Each((item, point) =>
            {
                if (finished.Contains(item)) return;
                finished.Add(item);
                item.Turn(this, point);
            });
            Floors.Each((floor, point) => floor.Turn(this, point));

            UpdateLevel();
        }

        /// <summary>
        /// Do all the updates that happen after a turn
        /// </summary>
        private void UpdateLevel()
        {
            // Now, remove any items that died this round
            Items.Each((item, point) => { if (item.Dead) Items[point] = null; });

            // Update the status label
            Window.UpdateStatusLabel(GetStatusLabel());

            // If there's no gold left, open the exit
            if (!Items.OfType<Gold>().Any())
                Items.OfType<Exit>().First().Open();

            // If there are no inactive switches, open some gates:            
            foreach (Color color in Enum.GetValues(typeof (Color)))
            {
                var gates = Items.OfTypeAndColor<Gate>(color);
                var switches = Floors.OfTypeAndColor<SwitchFloor>(color);

                if (switches.Any(s => !((SwitchFloor) s).Active))
                    gates.ForEach(g => ((Gate) g).Open = false);
                else
                    gates.ForEach(g => ((Gate) g).Open = true);
            }
        }

        /// <summary>
        /// Try to move something at location, in direction. Returns true (and moves the object) if the move is possible.
        /// </summary>
        /// <param name="location">Location the object starts in</param>
        /// <param name="direction">Direction it wants to move</param>
        /// <param name="byPlayer">True iff it's the player moving</param>
        /// <returns></returns>
        public bool TryMove(Point location, Direction direction, bool byPlayer)
        {
            var newLocation = Grid<Item>.TranslateLocation(location, direction);
            var canMove = false;

            // A couple obvious cases: out of the map, and into a wall, are impossible.
            if (!Items.InBounds(newLocation)) return false;
            if (Walls[newLocation] != null) return false;

            // Don't let non-player things move on to the player
            if (!byPlayer && newLocation == PlayerLocation) return false;

            var itemAtDest = Items[newLocation];

            if (itemAtDest == null) canMove = true; // Nothing in the way, go ahead and do the move:
            else if (!itemAtDest.Solid) canMove = byPlayer; // The player may move into non-solid objects
            else if (itemAtDest.Pushable) // Maybe we can push it?
                canMove = TryPush(newLocation, direction, byPlayer);
            else if (byPlayer) itemAtDest.Bump(Player); // The player can bump things he can't move into

            if (canMove)
            {
                // Actually do the move, either by changing player location or by moving an Item
                if (byPlayer) PlayerLocation = newLocation;
                else
                {
                    Items[newLocation] = Items[location];
                    Items[location] = null;
                }

                // Do PlayerEnter stuff:
                if (byPlayer && itemAtDest != null) itemAtDest.PlayerEnter(Player, this);
                if (byPlayer && Floors[newLocation] != null) Floors[newLocation].PlayerEnter(Player, this);
            }

            return canMove;
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
            var newLocation = Grid<Item>.TranslateLocation(location, direction);
            if (! Items.InBounds(newLocation)) return false; // First, are we trying to push it off the map?
            if (Walls[newLocation] != null) return false; // Trying to push into a wall, so no.
            if (newLocation == PlayerLocation) return false; // We also can't push something on to the player

            var pushed = Items[location]; // The thing we're pushing
            var pushedInto = Items[newLocation]; // Contents of the space we're pushing it into

            // Wait, there's no item being pushed? That's the caller's problem
            if(pushed == null) throw new ArgumentException("Trying to push an empty space");

            // This thing can't be pushed.
            if (!pushed.Solid || !pushed.Pushable) return false;

            // Trying to push into an empty space, that's totally cool
            if (pushedInto == null)
            {
                if (pushed.Heavy && !byPlayer) return false; // Heavy things can only be pushed by the player directly, not other things

                Items[newLocation] = pushed;
                Items[location] = null;
                return true;
            }

            // Otherwise, there's something there.

            if (pushed.Heavy) return false; // Heavy things can only be pushed into empty spaces

            if (TryPush(newLocation, direction, false)) // Try to push it. If this returns true, then newLocation must be empty...
            {
                Items[newLocation] = pushed;
                Items[location] = null;
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

        /// <summary>
        /// Called once a second when the turn timer ticks. This lets things move around
        /// without the player taking action.
        /// </summary>
        public void TurnTick()
        {
            var finished = new HashSet<Item>();

            Items.Each((item, point) =>
            {
                if (finished.Contains(item)) return;
                finished.Add(item);
                item.Tick(this, point);
                item.Turn(this, point);
            });

            Floors.Each((item, point) =>
            {
                item.Tick(this, point);
                item.Turn(this, point);                
            });

            UpdateLevel();
        }

        public string GetStatusLabel()
        {
            if (!Player.HasAnyKeys()) return "";
            var keys = new List<string>();

            foreach (Color color in Enum.GetValues(typeof(Color)))
            {
                if (Player.Keys[color] == 1)
                    keys.Add(color.ToString());
                else if (Player.Keys[color] > 1)
                    keys.Add(string.Format("{0} ({1})", color.ToString(), Player.Keys[color]));                
            }

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
                Window.OpenMap(NextLevel);
            }
            else
            {
                ShowMessage("Congratulations!\n\nYou have finished the\nlast level");
            }
        }
    }

    static class Extensions
    {
        public static IEnumerable<Item> OfTypeAndColor<T>(this Grid<Item> grid, Color color) where T : Item
        {
            return grid.OfType<T>().Where(i => i.Color == color);
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action )
        {
            enumerable.ToList().ForEach(action);
        }
    }
}
