
using System;
using System.Drawing;

namespace PuzzleGame.Items
{
    class Skull : AnimatableItem
    {
        private static AnimationFrame[] _inactiveAnimation = new[]
        {
            new AnimationFrame {Sprite = SpriteLibrary["Skull"], Ticks = 10}
        };

        private static AnimationFrame[] _activeAnimation = new[]
        {
            new AnimationFrame {Sprite = SpriteLibrary["SkullActive1"], Ticks = 1},
            new AnimationFrame {Sprite = SpriteLibrary["SkullActive2"], Ticks = 1},
        };

        private bool _active;
        private bool Active
        {
            get { return _active; }
            set { _active = value; SetAnimation(_active ? _activeAnimation : _inactiveAnimation); }
        }

        public Skull(Sprite sprite)
        {
            Solid = true;
            Sprite = sprite;
            Active = false;
        }

        public override void Turn(GameController controller, System.Drawing.Point location)
        {
            base.Turn(controller, location);
            var playerLocation = controller.PlayerLocation;

            Active = playerLocation.X == location.X || playerLocation.Y == location.Y;
        }

        public override void Tick(GameController controller, Point location)
        {
            base.Tick(controller, location);
            if (!Active) return;

            var playerLocation = controller.PlayerLocation;
            if (playerLocation.X == location.X)
            {
                if (playerLocation.Y < location.Y) controller.TryMove(location, Direction.Up, false);
                else if (playerLocation.Y > location.Y) controller.TryMove(location, Direction.Down, false);
            }
            else if (playerLocation.Y == location.Y)
            {
                if (playerLocation.X < location.X) controller.TryMove(location, Direction.Left, false);
                else if (playerLocation.X > location.X) controller.TryMove(location, Direction.Right, false);                
            }

        }
    }
}
