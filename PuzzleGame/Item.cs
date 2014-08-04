using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGame
{
    public class Item
    {
        public string Type { get; set; }
        public Rectangle? Rectangle { get; set; }

        /// <summary>
        /// Called by the controller when an animation timer event fires
        /// </summary>
        public virtual void Animate() { }
    }

    public class Player : AnimatableItem
    {
        public Player(AnimationFrame[] animationFrames) : base(animationFrames, 0)
        {
            Type = "Player";
        }
    }

    public class Gold : AnimatableItem
    {
        public Gold(AnimationFrame[] animationFrames, int startTick) : base(animationFrames, startTick)
        {
            Type = "Gold";
        }
    }
}
