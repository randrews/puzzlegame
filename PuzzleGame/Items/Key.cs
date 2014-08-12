using System.Drawing;

namespace PuzzleGame.Items
{
    public class Key : Item
    {
        public Color Color { get; private set; }

        public Key(Sprite sprite)
        {
            Color = sprite.Color;
            Sprite = sprite;
            Solid = false;
        }

        public override void PlayerEnter(Player player, GameController controller)
        {
            player.Keys[Color]++;
            Dead = true;
        }
    }
}