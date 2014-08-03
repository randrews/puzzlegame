using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using TiledSharp;

namespace PuzzleGame
{
    public class GameController
    {
        public TiledSharp.TmxMap Map { get; set; }
        public MainWindow Window { get; set; }

        /// <summary>
        /// Maps Type name to rectangle in Image, so we can look up tiles that aren't on the map at the start, like effects
        /// </summary>
        public Dictionary<string, Rectangle> Tileset { get; set; }

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
        /// 2D array of all the items in their current locations
        /// </summary>
        public Item[,] Items { get; set; }

        public GameController(MainWindow window, TiledSharp.TmxMap map)
        {
            Window = window;
            Map = map;
            Window.mapView1.Controller = this;
            Tileset = LoadTileset();
            Items = LoadItems();
        }

        /// <summary>
        /// Return an array of src rectangles (in the map's image) to draw the wall layer
        /// </summary>
        /// <returns></returns>
        public Rectangle?[,] GetWalls()
        {
            var cells = LayerToRectangles("Walls");

            for (int y = 0; y < cells.GetLength(1); y++)
            {
                for (int x = 0; x < cells.GetLength(0); x++)
                {
                    if (cells[x, y] == null) continue;

                    // Whether there are walls to the n/s/e/w of us
                    bool n = y > 0 && cells[x, y - 1] != null;
                    bool s = y < cells.GetLength(1)-1 && cells[x, y + 1] != null;
                    bool e = x < cells.GetLength(0)-1 && cells[x + 1, y] != null;
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
                    cells[x,y] = new Rectangle(column*24, ((Rectangle)cells[x,y]).Y, 24, 24);
                }
            }
            return cells;
        }

        /// <summary>
        /// Return an array of rectangles to draw the floor with
        /// </summary>
        /// <returns></returns>
        public Rectangle?[,] GetFloors()
        {
            return LayerToRectangles("Floor");
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
        /// TODO: This assumes only one tileset, and 24x24 tiles.
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        private Rectangle? GidToRectangle(int gid)
        {
            if (gid == 0) return null;
            gid -= 1;
            int width = Image.Width / 24;
            int x = gid%width;
            int y = gid/width;

            return new Rectangle(x*24, y*24, 24, 24);
        }

        /// <summary>
        /// Load the Items layer from the map into the 2d Items array
        /// </summary>
        /// <returns></returns>
        private Item[,] LoadItems()
        {
            var cells = new Item[20, 20];
            var layer = Map.Layers["Items"];
            var tset = Map.Tilesets.First();
            var tiles = tset.Tiles.ToDictionary(t => t.Id + tset.FirstGid);

            var goldFrames = new Rectangle[] {Tileset["Gold"], Tileset["Gold1"], Tileset["Gold2"], Tileset["Gold3"]};
            var rand = new Random();

            foreach (var tile in layer.Tiles)
            {
                if (tile.Gid == 0) continue; // No item here
                var tilesetTile = tiles[tile.Gid];
                if (tilesetTile.Properties.ContainsKey("Type"))
                {
                    var type = tiles[tile.Gid].Properties["Type"];
                    Rectangle? rect = GidToRectangle(tile.Gid);

                    if (type == "Gold")
                    {
                        cells[tile.X, tile.Y] = new Gold(goldFrames, rand.Next(50));
                    }
                    else
                    {
                        cells[tile.X, tile.Y] = new Item { Type = type, Rectangle = rect };                        
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

            foreach (var tile in tset.Tiles)
            {
                if (tile.Properties.ContainsKey("Type"))
                {
                    int gid = tile.Id + tset.FirstGid;
                    var rect = GidToRectangle(gid);
                    if (rect != null) dict[tile.Properties["Type"]] = (Rectangle) rect;
                }
            }

            return dict;
        }

        /// <summary>
        /// Called when the user wants to move the player
        /// </summary>
        /// <param name="direction">The direction we'll be moving</param>
        internal void MoveCommand(Direction direction)
        {

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
        }
    }
}
