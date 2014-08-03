using System.Drawing;
using System.Linq;
using System.Net.Sockets;

namespace PuzzleGame
{
    public class GameController
    {
        public TiledSharp.TmxMap Map { get; set; }
        public MainWindow Window { get; set; }

        public GameController(MainWindow window, TiledSharp.TmxMap map)
        {
            Window = window;
            Map = map;
            Window.mapView1.Controller = this;
            Items = LoadItems();
        }

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

        public Item[,] Items { get; set; }

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

        public Rectangle?[,] GetFloors()
        {
            return LayerToRectangles("Floor");
        }

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

        private Rectangle? GidToRectangle(int gid)
        {
            if (gid == 0) return null;
            gid -= 1;
            int width = Image.Width / 24;
            int x = gid%width;
            int y = gid/width;

            return new Rectangle(x*24, y*24, 24, 24);
        }

        private Item[,] LoadItems()
        {
            var cells = new Item[20, 20];
            var layer = Map.Layers["Items"];
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
                    cells[tile.X, tile.Y] = Item.ForType(type, rect);
                }
            }

            return cells;
        }
    }
}
