using System.Drawing;

namespace PuzzleGame.Items
{
    public class SwitchFloor : Item
    {
        public Color Color { get; private set; }

        public SwitchFloor(Color color, Rectangle rectangle)
        {
            Color = color;
            Rectangle = rectangle;
        }
    }
}