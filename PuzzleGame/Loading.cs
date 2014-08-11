using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TiledSharp;

namespace PuzzleGame
{
    public partial class GameController
    {
        /// <summary>
        /// Return an array of src rectangles (in the map's image) to draw the wall layer
        /// </summary>
        /// <returns></returns>
        private Rectangle?[,] LoadWalls()
        {
            var cells = LayerToRectangles("Walls");

            for (int y = 0; y < cells.GetLength(1); y++)
            {
                for (int x = 0; x < cells.GetLength(0); x++)
                {
                    if (cells[x, y] == null) continue;

                    // Whether there are walls to the n/s/e/w of us
                    bool n = y > 0 && cells[x, y - 1] != null;
                    bool s = y < cells.GetLength(1) - 1 && cells[x, y + 1] != null;
                    bool e = x < cells.GetLength(0) - 1 && cells[x + 1, y] != null;
                    bool w = x > 0 && cells[x - 1, y] != null;

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
                    cells[x, y] = new Rectangle(column * TileSize.Width, ((Rectangle)cells[x, y]).Y, TileSize.Width, TileSize.Height);
                }
            }
            return cells;
        }

        /// <summary>
        /// Return an array of items representing the floor
        /// </summary>
        /// <returns></returns>
        public Item[,] LoadFloors()
        {
            var cells = new Item[MapSize.Width, MapSize.Height];
            var layer = Map.Layers["Floor"];
            var tset = Map.Tilesets.First();
            var tiles = tset.Tiles.ToDictionary(t => t.Id + tset.FirstGid);

            foreach (var tile in layer.Tiles)
            {
                if (tile.Gid == 0) continue; // No item here
                var tilesetTile = tiles[tile.Gid];
                if (tilesetTile.Properties.ContainsKey("Type"))
                {
                    var type = tiles[tile.Gid].Properties["Type"];
                    Rectangle? rect = GidToRectangle(tile.Gid);
                    if (rect == null) throw new ArgumentException("Invalid gid " + tile.Gid);

                    switch (type)
                    {
                        case "PlainFloor":
                            cells[tile.X, tile.Y] = new PlainFloor((Rectangle) rect);
                            break;
                        case "Switch":
                            cells[tile.X, tile.Y] = new SwitchFloor(ReadColor(tilesetTile), (Rectangle) rect);
                            break;
                    }
                }
            }

            return cells;
        }

        /// <summary>
        /// Load the Items layer from the map into the 2d Items array
        /// </summary>
        /// <returns></returns>
        private Item[,] LoadItems()
        {
            var cells = new Item[MapSize.Width, MapSize.Height];
            var layer = Map.Layers["Items"];
            var tset = Map.Tilesets.First();
            var tiles = tset.Tiles.ToDictionary(t => t.Id + tset.FirstGid);
            var playerSet = false;
            var exitSet = false;

            var rand = new Random();

            foreach (var tile in layer.Tiles)
            {
                if (tile.Gid == 0) continue; // No item here
                var tilesetTile = tiles[tile.Gid];
                if (tilesetTile.Properties.ContainsKey("Type"))
                {
                    var type = tiles[tile.Gid].Properties["Type"];
                    Rectangle? rect = GidToRectangle(tile.Gid);
                    if(rect == null) throw new ArgumentException("Invalid gid " + tile.Gid);

                    switch (type)
                    {
                        case "Gold":
                            cells[tile.X, tile.Y] = new Gold(GoldAnimationFrames, rand.Next(30));
                            break;
                        case "Key":
                            cells[tile.X, tile.Y] = new Key(ReadColor(tilesetTile), (Rectangle) rect);
                            break;
                        case "Start":
                            if (playerSet) throw new ArgumentException("Map contains multiple start locations");
                            PlayerLocation = new Point(tile.X, tile.Y);
                            Player = new Player(PlayerAnimationFrames);
                            playerSet = true;
                            break;
                        case "Door":
                            cells[tile.X, tile.Y] = new Door(ReadColor(tilesetTile), (Rectangle) rect);
                            break;
                        case "Scroll":
                            if( ! Map.Properties.ContainsKey("ScrollMessage")) throw new ArgumentException("Map contains scrolls but no ScrollMessage");
                            var msg = Map.Properties["ScrollMessage"];
                            msg = msg.Replace('~', '\n');
                            cells[tile.X, tile.Y] = new Scroll(msg, (Rectangle) rect);
                            break;
                        case "Crate":
                            cells[tile.X, tile.Y] = new Crate((Rectangle)rect);
                            break;
                        case "Boulder":
                            cells[tile.X, tile.Y] = new Boulder((Rectangle)rect);
                            break;
                        case "Exit":
                            if(exitSet) throw new ArgumentException("Map contains multiple exits");
                            Exit = new Exit((Rectangle) rect, SpriteLibrary["ExitOpen"].Rectangle);
                            cells[tile.X, tile.Y] = Exit;
                            exitSet = true;
                            break;
                        default:
                            cells[tile.X, tile.Y] = new Item { Type = type, Rectangle = rect };
                            break;
                    }
                }
            }

            if (!playerSet) throw new ArgumentException("Map contains no start location");
            if (!exitSet) throw new ArgumentException("Map contains no exit");

            return cells;
        }

        private Color ReadColor(TmxTilesetTile tilesetTile)
        {
            Color color;
            if (! tilesetTile.Properties.ContainsKey("Color")) throw new ArgumentException("Key doesn't have a Color property");
            if (! Enum.TryParse(tilesetTile.Properties["Color"], true, out color))
                throw new ArgumentException("Key has unrecognized color " + tilesetTile.Properties["Color"]);
            return color;
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
