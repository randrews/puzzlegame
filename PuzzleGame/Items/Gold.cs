namespace PuzzleGame.Items
{
    public class Gold : AnimatableItem
    {
        public Gold()
        {
            SetAnimation(new[]
            {
                new AnimationFrame{Sprite = SpriteLibrary["Gold"], Ticks = 30},
                new AnimationFrame{Sprite = SpriteLibrary["Gold1"], Ticks = 1},
                new AnimationFrame{Sprite = SpriteLibrary["Gold2"], Ticks = 1},
                new AnimationFrame{Sprite = SpriteLibrary["Gold3"], Ticks = 1},
                new AnimationFrame{Sprite = SpriteLibrary["Gold2"], Ticks = 1},
                new AnimationFrame{Sprite = SpriteLibrary["Gold1"], Ticks = 1},
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