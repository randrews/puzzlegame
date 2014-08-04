
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
                new AnimationFrame{Rectangle = Tileset["Gold"], Ticks = 30},
                new AnimationFrame{Rectangle = Tileset["Gold1"], Ticks = 1},
                new AnimationFrame{Rectangle = Tileset["Gold2"], Ticks = 1},
                new AnimationFrame{Rectangle = Tileset["Gold3"], Ticks = 1},
                new AnimationFrame{Rectangle = Tileset["Gold2"], Ticks = 1},
                new AnimationFrame{Rectangle = Tileset["Gold1"], Ticks = 1},
            };

            PlayerAnimationFrames = new[]
            {
                new AnimationFrame{Rectangle = Tileset["Player1"], Ticks = 5},
                new AnimationFrame{Rectangle = Tileset["Player2"], Ticks = 5},
            };
        }
    }
}
