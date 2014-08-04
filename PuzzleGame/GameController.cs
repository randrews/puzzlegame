using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TiledSharp;

namespace PuzzleGame
{
    public partial class GameController
    {
        public TmxMap Map { get; set; }
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
        public Item[,] Items { get; private set; }
        public Rectangle?[,] Walls { get; private set; }
        public Rectangle?[,] Floors { get; private set; }

        public Point? PlayerLocation { get; set; }
        public Player Player { get; private set; }

        public GameController(MainWindow window, TmxMap map)
        {
            Window = window;
            Map = map;
            Window.mapView1.Controller = this;
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

            if(Player != null) Player.Animate();
        }
    }
}
