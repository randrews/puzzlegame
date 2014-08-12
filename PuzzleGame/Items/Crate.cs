using System.Drawing;

namespace PuzzleGame.Items
{
    public class Crate : Item
    {
        public Crate(Rectangle rectangle)
        {
            Rectangle = rectangle;
            Pushable = true;
            Solid = true;
            Heavy = false;
        }
    }
}