using System;
using System.Drawing;
using System.Windows.Forms;

namespace PuzzleGame
{
    public partial class MapView : UserControl
    {
        private GameController _controller;
        private Image _image;
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
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Controller == null) return;

            DrawRectangles(Controller.Floors, e.Graphics);
            DrawRectangles(Controller.Walls, e.Graphics);
            DrawRectangles(Controller.GetItemRectangles(), e.Graphics);
            DrawPlayer(e.Graphics);
        }

        private void DrawPlayer(Graphics g)
        {
            if (Controller.Player.Rectangle != null && Controller.PlayerLocation != null)
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

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            if (Controller != null) Controller.AnimationTick();
            Refresh();
        }
    }
}
