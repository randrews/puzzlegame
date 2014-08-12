using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGame.Items
{
    class Gate : Item
    {
        public bool Open
        {
            set
            {
                Sprite = value ? _openSprite : _closedSprite;
                Solid = !value;
            }
        }
        private readonly Sprite _openSprite, _closedSprite;

        public Gate(Sprite sprite)
        {
            Sprite = sprite;
            Color = sprite.Color;
            Solid = true;

            _openSprite = SpriteLibrary.First(s => s.Type == "OpenGate" && s.Color == Color);
            _closedSprite = SpriteLibrary.First(s => s.Type == "Gate" && s.Color == Color);
        }
    }
}
