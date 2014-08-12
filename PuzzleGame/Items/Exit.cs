using System.Drawing;

namespace PuzzleGame.Items
{
    public class Exit : Item
    {
        private Rectangle _openRectangle;

        public Exit(Rectangle rectangle, Rectangle openRectangle)
        {
            Rectangle = rectangle;
            _openRectangle = openRectangle;
            Solid = true;
        }

        /// <summary>
        /// Called by the controller when the last gem is picked up
        /// </summary>
        public void Open()
        {
            Solid = false;
            Rectangle = _openRectangle;
        }

        public override void PlayerEnter(Player player, GameController controller)
        {
            base.PlayerEnter(player, controller);
            controller.ExitLevel();
        }
    }
}