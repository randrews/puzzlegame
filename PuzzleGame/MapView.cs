using System;
using System.Drawing;
using System.Windows.Forms;

namespace PuzzleGame
{
    public partial class MapView : UserControl
    {
        private GameController _controller;
        private Image _image;
        public bool ShowingMessage { get; private set; }
        private string _message;

        public string Message
        {
            set
            {
                _message = value;
                ShowingMessage = (_message != null);
            }
            private get { return _message; }
        }

        public GameController Controller
        {
            get
            {
                return _controller;
            }
            set
            {
                _controller = value;
                _image = value == null ? null : value.Image;
            }
        }
        
        public MapView()
        {
            InitializeComponent();
            ShowingMessage = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Controller == null) return;

            // The total size of the map if we showed all of it
            var mapSize = new Size(Controller.TileSize.Width * Controller.MapSize.Width, Controller.TileSize.Height * Controller.MapSize.Height);
            
            // The pixel coordinate (in the map pixel rectangle) of the center of the player tile
            var playerCenter = new Point((int)((Controller.PlayerLocation.X + 0.5) * Controller.TileSize.Width),
                (int)((Controller.PlayerLocation.Y + 0.5) * Controller.TileSize.Height));

            // First, figure out the translation
            // If the control is larger than the map, center the map
            if (Width > mapSize.Width)
                e.Graphics.TranslateTransform(Width / 2 - mapSize.Width / 2, 0);

            if (Height > mapSize.Height)
                e.Graphics.TranslateTransform(0, Height / 2 - mapSize.Height / 2);

            // If the control is smaller than the map, then center it on the player
            if (Width < mapSize.Width)
                e.Graphics.TranslateTransform(Width / 2 - playerCenter.X, 0);

            if (Height < mapSize.Height)
                e.Graphics.TranslateTransform(0, Height / 2 - playerCenter.Y);

            DrawRectangles(Controller.FloorRectangles, e.Graphics);
            DrawRectangles(Controller.Walls, e.Graphics);
            DrawRectangles(Controller.GetItemRectangles(), e.Graphics);
            DrawPlayer(e.Graphics);

            if (ShowingMessage) DisplayMessageBox(e.Graphics);
        }

        private void DisplayMessageBox(Graphics g)
        {
            g.ResetTransform(); // This is all in normal, not map, coordinates

            var brush = new SolidBrush(System.Drawing.Color.Linen);
            var pen = new Pen(System.Drawing.Color.DimGray);
            var textBrush = new SolidBrush(System.Drawing.Color.DimGray);
            var rect = MessageBoxRectangle();
            g.FillRectangle(brush, rect);
            g.DrawRectangle(pen, rect);

            var textSize = TextRenderer.MeasureText(Message, Font);
            g.DrawString(Message, Font, textBrush, Width / 2 - textSize.Width / 2, Height / 2 - textSize.Height / 2);
        }

        private Rectangle MessageBoxRectangle()
        {
            return new Rectangle(Width/2 - 100, Height/2 - 75, 200, 150);
        }

        private void DrawPlayer(Graphics g)
        {
            if (Controller.Player.Rectangle != null)
            {
                Point p = (Point)Controller.PlayerLocation;
                int w = Controller.TileSize.Width;
                int h = Controller.TileSize.Height;
                var dest = new Rectangle(p.X * w, p.Y * h, w, h);
                var src = (Rectangle)Controller.Player.Rectangle;
                g.DrawImage(_image, dest, src, GraphicsUnit.Pixel);
            }
        }

        private void DrawRectangles(Rectangle?[,] rectangles, Graphics g)
        {
            int w = Controller.TileSize.Width;
            int h = Controller.TileSize.Height;

            for (int y = 0; y < rectangles.GetLength(1); y++)
            {
                for (int x = 0; x < rectangles.GetLength(0); x++)
                {
                    var dest = new Rectangle(x * w, y * h, w, h);
                    var src = rectangles[x, y];

                    if (src != null) g.DrawImage(_image, dest, (Rectangle)src, GraphicsUnit.Pixel);
                }
            }            
        }

        private void DrawRectangles(Grid<Sprite> sprites, Graphics g)
        {
            int w = Controller.TileSize.Width;
            int h = Controller.TileSize.Height;

            sprites.Each((sprite, p) =>
            {
                var dest = new Rectangle(p.X * w, p.Y * h, w, h);
                g.DrawImage(_image, dest, sprite.Rectangle, GraphicsUnit.Pixel);                
            });
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            if (ShowingMessage) return;
            if (Controller != null) Controller.AnimationTick();
            Refresh();
        }
    }
}
