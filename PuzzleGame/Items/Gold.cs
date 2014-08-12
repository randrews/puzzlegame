namespace PuzzleGame.Items
{
    public class Gold : AnimatableItem
    {
        public Gold()
        {
            SetAnimation(new[]
            {
                new AnimationFrame{Rectangle = SpriteLibrary["Gold"].Rectangle, Ticks = 30},
                new AnimationFrame{Rectangle = SpriteLibrary["Gold1"].Rectangle, Ticks = 1},
                new AnimationFrame{Rectangle = SpriteLibrary["Gold2"].Rectangle, Ticks = 1},
                new AnimationFrame{Rectangle = SpriteLibrary["Gold3"].Rectangle, Ticks = 1},
                new AnimationFrame{Rectangle = SpriteLibrary["Gold2"].Rectangle, Ticks = 1},
                new AnimationFrame{Rectangle = SpriteLibrary["Gold1"].Rectangle, Ticks = 1},
            }, Random.Next(30));
            Type = "Gold";
            Solid = false;
        }

        public override void PlayerEnter(Player player, GameController controller)
        {
            Dead = true;
        }
    }
}