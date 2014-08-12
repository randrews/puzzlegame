using System;
using System.Drawing;

namespace PuzzleGame
{
    public struct AnimationFrame
    {
        public Rectangle Rectangle { get; set; }
        public int Ticks { get; set; }
    }

    public class AnimatableItem : Item
    {
        /// <summary>
        /// The tick we're at in the current frame
        /// </summary>
        private int _currentTick;

        /// <summary>
        /// The current animation frame
        /// </summary>
        private int _frame;

        private AnimationFrame[] _animationFrames;

        /// <summary>
        /// Setup animation-related fields.
        /// </summary>
        /// <param name="animationFrames">The list of animation frames</param>
        /// <param name="startTick">The tick to start on (must be between 0 and the total number of ticks in animationFrames)</param>
        protected void SetAnimation(AnimationFrame[] animationFrames, int startTick)
        {
            if (animationFrames == null) throw new ArgumentNullException("animationFrames");
            if (animationFrames.Length < 1) throw new ArgumentException("No animation frames given");

            _animationFrames = animationFrames;
            _frame = 0;
            while (startTick >= _animationFrames[_frame].Ticks)
            {
                startTick -= _animationFrames[_frame].Ticks;
                _frame++;
                if(_frame >= _animationFrames.Length) throw new ArgumentException("startTick is higher than the total ticks in the animation");
            }

            _currentTick = startTick;
            Rectangle = _animationFrames[_frame].Rectangle;
        }

        private Rectangle[] CreateFrameOrder(Rectangle[] animationFrames)
        {
            var frames = new Rectangle[animationFrames.Length * 2 - 2];
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

        /// <summary>
        /// Callback for the animation timer. If _currentTick is still lower
        /// than the ticks for this frame, then just wait, otherwise advance to the next frame.
        /// </summary>
        public override void Animate()
        {
            base.Animate();
            if (_currentTick < _animationFrames[_frame].Ticks) // Still waiting
            {
                _currentTick++;
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
                }

                _currentTick = 0;
                Rectangle = _animationFrames[_frame].Rectangle;
            }
        }
    }
}