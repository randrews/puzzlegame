using System.Drawing;

namespace PuzzleGame.Items
{
    public class SwitchFloor : Item
    {
        public Color Color { get; private set; }

        public SwitchFloor(Sprite sprite)
        {
            Color = Sprite.Color;
            Sprite = sprite;
        }
    }
}