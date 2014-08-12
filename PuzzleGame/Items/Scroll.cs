using System.Drawing;

namespace PuzzleGame.Items
{
    public class Scroll : Item
    {
        private string _message;

        public Scroll(string message, Rectangle rectangle)
        {
            Rectangle = rectangle;
            _message = message;
            Solid = false;
        }

        public override void PlayerEnter(Player player, GameController controller)
        {
            controller.ShowMessage(_message);
        }
    }
}