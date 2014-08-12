using System.Drawing;

namespace PuzzleGame.Items
{
    public class Exit : Item
    {
        public Exit()
        {
            Sprite = SpriteLibrary["Exit"];
            Solid = true;
        }

        /// <summary>
        /// Called by the controller when the last gem is picked up
        /// </summary>
        public void Open()
        {
            Solid = false;
            Sprite = SpriteLibrary["ExitOpen"];
        }

        public override void PlayerEnter(Player player, GameController controller)
        {
            base.PlayerEnter(player, controller);
            controller.ExitLevel();
        }
    }
}