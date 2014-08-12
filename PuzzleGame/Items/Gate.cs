using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGame.Items
{
    class Gate : Item
    {
        public Color Color { get; private set; }

        public Gate(Sprite sprite)
        {
            Sprite = sprite;
            Color = sprite.Color;
            Solid = true;
        }
    }
}
