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

            drawRectangles(Controller.GetFloors(), e.Graphics);
            drawRectangles(Controller.GetWalls(), e.Graphics);
            drawRectangles(Controller.GetItemRectangles(), e.Graphics);
        }

        private void drawRectangles(Rectangle?[,] rectangles, Graphics g)
        {
            for (int y = 0; y < rectangles.GetLength(1); y++)
            {
                for (int x = 0; x < rectangles.GetLength(0); x++)
                {
                    var dest = new Rectangle(x * 24, y * 24, 24, 24);
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
