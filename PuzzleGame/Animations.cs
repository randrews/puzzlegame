
namespace PuzzleGame
{
    public partial class GameController
    {
        internal AnimationFrame[] GoldAnimationFrames { get; private set; }
        internal AnimationFrame[] PlayerAnimationFrames { get; private set; }

        private void SetupAnimationFrames()
        {
            GoldAnimationFrames = new[]
            {
                new AnimationFrame{Rectangle = SpriteLibrary["Gold"].Rectangle, Ticks = 30},
                new AnimationFrame{Rectangle = SpriteLibrary["Gold1"].Rectangle, Ticks = 1},
                new AnimationFrame{Rectangle = SpriteLibrary["Gold2"].Rectangle, Ticks = 1},
                new AnimationFrame{Rectangle = SpriteLibrary["Gold3"].Rectangle, Ticks = 1},
                new AnimationFrame{Rectangle = SpriteLibrary["Gold2"].Rectangle, Ticks = 1},
                new AnimationFrame{Rectangle = SpriteLibrary["Gold1"].Rectangle, Ticks = 1},
            };

            PlayerAnimationFrames = new[]
            {
                new AnimationFrame{Rectangle = SpriteLibrary["Player1"].Rectangle, Ticks = 5},
                new AnimationFrame{Rectangle = SpriteLibrary["Player2"].Rectangle, Ticks = 5},
            };
        }
    }
}
