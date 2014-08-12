using System.Drawing;

namespace PuzzleGame.Items
{
    public class Key : Item
    {
        public Key(Sprite sprite)
        {
            Sprite = sprite;
            Color = sprite.Color;
            Solid = false;
        }

        public override void PlayerEnter(Player player, GameController controller)
        {
            player.Keys[Color]++;
            Dead = true;
        }
    }
}