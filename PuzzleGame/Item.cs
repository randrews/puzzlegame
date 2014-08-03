using System;
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

    public class Gold : Item
    {
        /// <summary>
        /// Number of animation ticks before we start animating
        /// </summary>
        private int _delay;

        /// <summary>
        /// The current animation frame
        /// </summary>
        private int _frame;

        /// <summary>
        /// The array of animation frames for the gold sparkling. Frame 0 is the "idle" frame,
        /// when delay hits 0 we go through the others, then reset delay and back to 0
        /// </summary>
        private Rectangle[] _animationFrames;

        public Gold(Rectangle[] animationFrames, int startDelay)
        {
            if(animationFrames.Length < 1) throw new ArgumentException("No animation frames given");
            Type = "Gold";
            Rectangle = animationFrames[0]; // Start on the idle frame
            _animationFrames = CreateFrameOrder(animationFrames);
            _frame = 0;
            _delay = startDelay;
        }

        private Rectangle[] CreateFrameOrder(Rectangle[] animationFrames)
        {
            var frames = new Rectangle[animationFrames.Length*2 - 2];
            for (int i = 0; i < animationFrames.Length; i++)
            {
                frames[i] = animationFrames[i];
            }
            for (int i = 1; i < animationFrames.Length; i++)
            {
                frames[frames.Length - i] = animationFrames[i];
            }
            return frames;
        }

        public override void Animate()
        {
            base.Animate();
            if (_delay > 0) // Still waiting
            {
                _delay--;
            }
            else
            {
                if (_frame < _animationFrames.Length - 1)
                {
                    _frame++;
                }
                else
                {
                    _frame = 0;
                    _delay = 50;
                }
                Rectangle = _animationFrames[_frame];
            }
        }
    }
}
