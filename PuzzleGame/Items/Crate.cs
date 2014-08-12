using System.Drawing;

namespace PuzzleGame.Items
{
    public class Crate : Item
    {
        public Crate(Sprite sprite)
        {
            Sprite = sprite;
            Pushable = true;
            Solid = true;
            Heavy = false;
        }
    }
}