using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TiledSharp;

namespace PuzzleGame
{
    public partial class Grid<T>
    {
        private T[,] _cells;
        public Size GridSize { get; private set; }

        public Grid(int width, int height) : this(new Size(width, height)){}

        public Grid(Size size)
        {
            _cells = new T[size.Width, size.Height];
            GridSize = size;
        }

        public bool InBounds(Point p)
        {
            return p.X >= 0 && p.X < GridSize.Width &&
                   p.Y >= 0 && p.Y < GridSize.Height;
        }

        public static Point TranslateLocation(Point p, Direction direction)
        {
            var newLocation = new Point(p.X, p.Y);

            switch (direction)
            {
                case Direction.Up:
                    newLocation.Y--;
                    break;
                case Direction.Down:
                    newLocation.Y++;
                    break;
                case Direction.Left:
                    newLocation.X--;
                    break;
                case Direction.Right:
                    newLocation.X++;
                    break;
            }

            return newLocation;
        }

        public T this[int x, int y] { get { return _cells[x, y]; } set { _cells[x, y] = value; } }
        public T this[Point p] { get { return this[p.X, p.Y]; } set { this[p.X, p.Y] = value; } }

        public void Each(Action<T, Point> act)
        {
            for (int y = 0; y < GridSize.Height; y++)
            {
                for (int x = 0; x < GridSize.Width; x++)
                {
                    var cell = this[x, y];
                    if(cell != null) act.Invoke(cell, new Point(x, y));
                }
            }
        }
    }

}
