using System.Drawing;

namespace PuzzleGame.Items
{
    public class Door : Item
    {
        public Color Color { get; private set; }

        public Door(Sprite sprite)
        {
            Color = sprite.Color;
            Sprite = sprite;
            Solid = true;
        }

        public override void Bump(Player player)
        {
            base.Bump(player);
            if (player.Keys[Color] == 0) return;
            player.Keys[Color]--;
            Dead = true;
        }
    }
}