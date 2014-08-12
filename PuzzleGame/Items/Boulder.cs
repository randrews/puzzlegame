using System.Drawing;

namespace PuzzleGame.Items
{
    public class Boulder : Item
    {
        public Boulder(Rectangle rectangle)
        {
            Rectangle = rectangle;
            Pushable = true;
            Solid = true;
            Heavy = true;
        }
    }
}