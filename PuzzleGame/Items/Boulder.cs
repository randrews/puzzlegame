using System.Drawing;

namespace PuzzleGame.Items
{
    public class Boulder : Item
    {
        public Boulder(Sprite sprite)
        {
            Sprite = sprite;
            Pushable = true;
            Solid = true;
            Heavy = true;
        }
    }
}