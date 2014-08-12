using System.Collections.Generic;
using System.Linq;

namespace PuzzleGame.Items
{
    public class Player : AnimatableItem
    {
        /// <summary>
        /// The number of keys of each color the player has
        /// </summary>
        public Dictionary<Color, int> Keys { get; private set; }

        public Player()
        {
            SetAnimation(new[]
            {
                new AnimationFrame{Rectangle = SpriteLibrary["Player1"].Rectangle, Ticks = 5},
                new AnimationFrame{Rectangle = SpriteLibrary["Player2"].Rectangle, Ticks = 5},
            },
            0);

            Type = "Player";
            Keys = new Dictionary<Color, int>();
            Keys[Color.Red] = Keys[Color.Blue] = Keys[Color.Green] = Keys[Color.Yellow] = 0;
        }

        public bool HasAnyKeys()
        {
            return Keys.Any(pair => pair.Value > 0);
        }
    }
}