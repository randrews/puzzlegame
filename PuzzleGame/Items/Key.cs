using System.Drawing;

namespace PuzzleGame.Items
{
    public class Key : Item
    {
        public Color Color { get; private set; }

        public Key(Color color, Rectangle rectangle)
        {
            Color = color;
            Rectangle = rectangle;
            Solid = false;
        }

        public override void PlayerEnter(Player player, GameController controller)
        {
            player.Keys[Color]++;
            Dead = true;
        }
    }
}