using PuzzleGame.Items;
using System;
using System.Drawing;
using System.Linq;

namespace PuzzleGame
{
    public partial class GameController
    {
        /// <summary>
        /// Return an array of src rectangles (in the map's image) to draw the wall layer
        /// </summary>
        /// <returns></returns>
        private Grid<Sprite> LoadWalls()
        {
            var cells = LayerToSprites("Walls");

            cells.Each((sprite, p) =>
            {
                // Whether there are walls to the n/s/e/w of us
                bool n = p.Y > 0 && cells[p.X, p.Y - 1] != null;
                bool s = p.Y < cells.GridSize.Height - 1 && cells[p.X, p.Y + 1] != null;
                bool e = p.X < cells.GridSize.Width - 1 && cells[p.X + 1, p.Y] != null;
                bool w = p.X > 0 && cells[p.X - 1, p.Y] != null;

                int column = 10; // column of the wall tile we'll use

                // Pick out the wall tile's column based on the neighbors
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                if (!n && !s && !e && !w) column = 0;
                else if (!n && !s && e && !w) column = 1;
                else if (!n && !s && e && w) column = 2;
                else if (!n && !s && !e && w) column = 3;
                else if (!n && s && !e && !w) column = 4;
                else if (n && s && !e && !w) column = 5;
                else if (n && !s && !e && !w) column = 6;
                else if (!n && s && e && !w) column = 7;
                else if (!n && s && !e && w) column = 8;
                else if (n && !s && e && !w) column = 9;
                else if (n && !s && !e && w) column = 10;
                else if (n && s && e && w) column = 11;
                else if (!n && s && e && w) column = 12;
                else if (n && s && !e && w) column = 13;
                else if (n && s && e && !w) column = 14;
                else if (n && !s && e && w) column = 15;
                // ReSharper restore ConditionIsAlwaysTrueOrFalse

                // Use that column, with the row of the original tile
                // ReSharper disable once PossibleInvalidOperationException
                var rect = new Rectangle(column * TileSize.Width, sprite.Rectangle.Y, TileSize.Width, TileSize.Height);
                cells[p] = new Sprite { Rectangle = rect };

            });
            return cells;
        }

        /// <summary>
        /// Return an array of items representing the floor
        /// </summary>
        /// <returns></returns>
        public Grid<Item> LoadFloors()
        {
            var cells = new Grid<Item>(MapSize);
            var layer = Map.Layers["Floor"];

            foreach (var tile in layer.Tiles)
            {
                if (tile.Gid == 0) continue; // No item here
                var sprite = SpriteLibrary[tile.Gid];

                switch (sprite.Type)
                {
                    case "PlainFloor":
                        cells[tile.X, tile.Y] = new PlainFloor(sprite);
                        break;
                    case "Switch":
                        cells[tile.X, tile.Y] = new SwitchFloor(sprite);
                        break;
                }
            }

            return cells;
        }

        /// <summary>
        /// Load the Items layer from the map into the 2d Items array
        /// </summary>
        /// <returns></returns>
        private Grid<Item> LoadItems()
        {
            var cells = new Grid<Item>(MapSize);
            var layer = Map.Layers["Items"];
            var playerSet = false;

            foreach (var tile in layer.Tiles)
            {
                if (tile.Gid == 0) continue; // No item here
                var sprite = SpriteLibrary[tile.Gid];

                switch (sprite.Type)
                {
                    case "Gold":
                        cells[tile.X, tile.Y] = new Gold();
                        break;
                    case "Key":
                        cells[tile.X, tile.Y] = new Key(sprite);
                        break;
                    case "Start":
                        if (playerSet) throw new ArgumentException("Map contains multiple start locations");
                        PlayerLocation = new Point(tile.X, tile.Y);
                        Player = new Player();
                        playerSet = true;
                        break;
                    case "Door":
                        cells[tile.X, tile.Y] = new Door(sprite);
                        break;
                    case "Gate":
                        cells[tile.X, tile.Y] = new Gate(sprite);
                        break;
                    case "Scroll":
                        if( ! Map.Properties.ContainsKey("ScrollMessage")) throw new ArgumentException("Map contains scrolls but no ScrollMessage");
                        var msg = Map.Properties["ScrollMessage"];
                        msg = msg.Replace('~', '\n');
                        cells[tile.X, tile.Y] = new Scroll(msg, sprite);
                        break;
                    case "Crate":
                        cells[tile.X, tile.Y] = new Crate(sprite);
                        break;
                    case "Boulder":
                        cells[tile.X, tile.Y] = new Boulder(sprite);
                        break;
                    case "Exit":
                        cells[tile.X, tile.Y] = new Exit();
                        break;
                    default:
                        cells[tile.X, tile.Y] = new Item { Type = sprite.Type, Sprite = sprite };
                        break;
                }
            }

            if (!playerSet) throw new ArgumentException("Map contains no start location");

            var exits = cells.Where(item => item is Exit);
            if (!exits.Any()) throw new ArgumentException("Map contains no exit");
            if (exits.Count() > 1) throw new ArgumentException("Map contains multiple exits");

            return cells;
        }

        private Size ReadMapSize()
        {
            return new Size(Map.Width, Map.Height);
        }

        private string ReadNextLevel()
        {
            var props = Map.Properties;
            if (props.ContainsKey("NextLevel"))
            {
                var level = props["NextLevel"];
                return Map.TmxDirectory + @"\" + level + ".tmx";
            }
            return null;
        }

    }
}
