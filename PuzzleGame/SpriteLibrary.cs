using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;

namespace PuzzleGame
{
    /// <summary>
    /// Represents the things about a tile relevant to drawing and game logic:
    /// - The color, if it has one
    /// - The Type property
    /// - The rectangle for drawing
    /// - the Gid for reading the map
    /// </summary>
    public class Sprite
    {
        public Color Color { get; set; }
        public string Type { get; set; }
        public Rectangle Rectangle { get; set; }
        public int Gid { get; set; }
    }

    public class SpriteLibrary : IEnumerable<Sprite>
    {
        public Size TileSize { get; private set; }
        public Image Image { get; private set; }
        public List<Sprite> Sprites { get; private set; }
        private Dictionary<int, Sprite> SpritesByGid { get; set; }
        private Dictionary<string, Sprite> SpritesByType { get; set; }
        public TmxMap Map { get; private set; }

        public SpriteLibrary(TmxMap map)
        {
            Map = map;
            var tset = Map.Tilesets.First();
            TileSize = new Size(tset.TileWidth, tset.TileHeight);

            var filename = Map.Tilesets.First().Image.Source;
            Image = Image.FromFile(filename);

            Sprites = new List<Sprite>();
            SpritesByGid = new Dictionary<int, Sprite>();
            SpritesByType = new Dictionary<string, Sprite>();

            foreach (var tile in tset.Tiles)
            {
                var spr = new Sprite {Gid = tile.Id + tset.FirstGid};
                spr.Rectangle = GidToRectangle(spr.Gid);

                SpritesByGid[spr.Gid] = spr;
                Sprites.Add(spr);

                if (tile.Properties.ContainsKey("Color"))
                    spr.Color = ReadColor(tile);

                if (tile.Properties.ContainsKey("Type"))
                {
                    spr.Type = tile.Properties["Type"];
                    SpritesByType[spr.Type] = spr;
                }
            }
        }

        private Rectangle GidToRectangle(int gid)
        {
            if (gid == 0) throw new ArgumentException("0 is not a valid Gid");
            gid -= 1;
            int width = Image.Width / TileSize.Width;
            int x = gid % width;
            int y = gid / width;

            return new Rectangle(x * TileSize.Width, y * TileSize.Height, TileSize.Width, TileSize.Height);
        }

        private Color ReadColor(TmxTilesetTile tilesetTile)
        {
            Color color;
            if (!tilesetTile.Properties.ContainsKey("Color")) throw new ArgumentException("Key doesn't have a Color property");
            if (!Enum.TryParse(tilesetTile.Properties["Color"], true, out color))
                throw new ArgumentException("Key has unrecognized color " + tilesetTile.Properties["Color"]);
            return color;
        }

        public IEnumerator<Sprite> GetEnumerator()
        {
            return Sprites.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Sprite this[string type] { get { return SpritesByType[type]; } }
        public Sprite this[int gid] { get { return SpritesByGid[gid]; } }
    }
}
