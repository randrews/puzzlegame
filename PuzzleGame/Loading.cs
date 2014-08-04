using System;
using System.Collections.Generic;
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
        /// Return an array of rectangles to draw the floor with
        /// </summary>
        /// <returns></returns>
        public Rectangle?[,] LoadFloors()
        {
            return LayerToRectangles("Floor");
        }
        /// <summary>
        /// Load the Items layer from the map into the 2d Items array
        /// TODO: This assumes 20x20 map
        /// </summary>
        /// <returns></returns>
        private Item[,] LoadItems()
        {
            var cells = new Item[20, 20];
            var layer = Map.Layers["Items"];
            var tset = Map.Tilesets.First();
            var tiles = tset.Tiles.ToDictionary(t => t.Id + tset.FirstGid);

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
                        {
                            if( ! tilesetTile.Properties.ContainsKey("Color")) throw new ArgumentException("Key doesn't have a Color property");
                            Color color;
                            if( ! Enum.TryParse(tilesetTile.Properties["Color"], true, out color)) throw new ArgumentException("Key has unrecognized color " + tilesetTile.Properties["Color"]);
                            cells[tile.X, tile.Y] = new Key(color, (Rectangle) rect);
                        }
                            break;
                        case "Start":
                            if (PlayerLocation != null) throw new ArgumentException("Map contains multiple start locations");
                            PlayerLocation = new Point(tile.X, tile.Y);
                            Player = new Player(PlayerAnimationFrames);
                            break;
                        default:
                            cells[tile.X, tile.Y] = new Item { Type = type, Rectangle = rect };
                            break;
                    }
                }
            }

            return cells;
        }

        /// <summary>
        /// Reads through the map's tileset to find which rectangles go to which tile types.
        /// We assume that all the tiles we care about have Type properties; these are what tell the
        /// game semantically what the tiles mean.
        /// 
        /// Some tiles will have non-unique types, like "door" and "key". That's okay because when we
        /// read the map Items layer, anything on there that has a Type will use whatever rect it has
        /// in the editor. This is more for looking up rects that aren't in the map at the start, like
        /// the player sprite and effect frames.
        /// </summary>
        /// <returns>A dictionary from Type string to rectangle</returns>
        private Dictionary<string, Rectangle> LoadTileset()
        {
            var tset = Map.Tilesets.First();
            var dict = new Dictionary<string, Rectangle>();

            TileSize = new Size(tset.TileWidth, tset.TileHeight);

            foreach (var tile in tset.Tiles)
            {
                if (tile.Properties.ContainsKey("Type"))
                {
                    int gid = tile.Id + tset.FirstGid;
                    var rect = GidToRectangle(gid);
                    if (rect != null) dict[tile.Properties["Type"]] = (Rectangle)rect;
                }
            }

            return dict;
        }
    }
}
