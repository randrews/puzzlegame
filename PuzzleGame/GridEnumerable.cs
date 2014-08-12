using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGame
{
    /// <summary>
    /// The Grid class is IEnumerable, but the enumeration only goes over cells that have something in them.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class Grid<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            return new GameMapEnumerator(_cells);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class GameMapEnumerator : IEnumerator<T>
        {
            public void Dispose() { }

            public bool MoveNext()
            {
                while (true)
                {
                    _n++;

                    if (_n >= _max)
                        return false;

                    if (_cells[_n%_width, _n/_width] != null)
                    {
                        Current = _cells[_n%_width, _n/_width];
                        return true;
                    }
                }
            }

            public void Reset()
            {
                _n = 0;
            }

            public T Current { get; private set; }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            private int _n;
            private readonly int _max, _width;
            private readonly T[,] _cells;

            internal GameMapEnumerator(T[,] cells)
            {
                _max = cells.GetLength(0) * cells.GetLength(1);
                _width = cells.GetLength(0);
                _cells = cells;
                _n = 0;
            }
        }

    }
}
