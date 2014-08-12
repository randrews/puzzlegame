using System.Drawing;
using System.Linq;

namespace PuzzleGame.Items
{
    public class SwitchFloor : Item
    {
        public bool Active { get; private set; }
        private Sprite _activeSprite, _inactiveSprite;

        public SwitchFloor(Sprite sprite)
        {
            Sprite = sprite;
            Color = sprite.Color;

            _activeSprite = SpriteLibrary.First(s => s.Type == "ActiveSwitch" && s.Color == Color);
            _inactiveSprite = SpriteLibrary.First(s => s.Type == "Switch" && s.Color == Color);
        }

        public override void Turn(GameController controller, Point location)
        {
            if (controller.PlayerLocation == location) Active = true;
            else
            {
                var item = controller.Items[location];
                Active = (item != null && item.Solid);
            }

            if (Active) Sprite = _activeSprite;
            else Sprite = _inactiveSprite;
        }
    }
}