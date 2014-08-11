using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGame
{
    public class Item
    {
        /// <summary>
        /// We can only have one level loaded (and thus one tileset) at a time, so, to keep from having to
        /// deal with passing things around a lot, we'll give the Item classes the ability to access the
        /// sprites themselves. This makes the map loading code a LOT simpler.
        /// </summary>
        public static SpriteLibrary SpriteLibrary { get; set; }

        public string Type { get; set; }
        public Rectangle? Rectangle { get; set; }

        /// <summary>
        /// Solid objects, the player can't walk through.
        /// </summary>
        public bool Solid { get; protected set; }

        /// <summary>
        /// Pushable objects (which must be solid) can be pushed around by the player, or other objects
        /// </summary>
        public bool Pushable { get; protected set; }

        /// <summary>
        /// Heavy objects can only be pushed one at a time, and only by the player.
        /// Heavy implies pushable implies solid.
        /// </summary>
        public bool Heavy { get; protected set; }

        /// <summary>
        /// Dead items will be removed from the map by the controller at the end of each turn.
        /// So, an item can mark itself as dead in order to remove itself.
        /// </summary>
        public bool Dead { get; protected set; }
        /// <summary>
        /// Called by the controller when an animation timer event fires
        /// </summary>
        public virtual void Animate() { }

        /// <summary>
        /// Called when the player stands on top of a non-solid object
        /// </summary>
        /// <param name="player"></param>
        /// <param name="controller"></param>
        public virtual void PlayerEnter(Player player, GameController controller) { }

        /// <summary>
        /// Called for a solid, non-pushable object when the player tries to move into it
        /// </summary>
        public virtual void Bump(Player player) { }

        public Item()
        {
            Dead = false;
        }
    }

    public class Player : AnimatableItem
    {
        /// <summary>
        /// The number of keys of each color the player has
        /// </summary>
        public Dictionary<Color, int> Keys { get; private set; }

        public Player(AnimationFrame[] animationFrames) : base(animationFrames, 0)
        {
            Type = "Player";
            Keys = new Dictionary<Color, int>();
            Keys[Color.Red] = Keys[Color.Blue] = Keys[Color.Green] = Keys[Color.Yellow] = 0;
        }

        public bool HasAnyKeys()
        {
            return Keys.Any(pair => pair.Value > 0);
        }
    }

    public class Gold : AnimatableItem
    {
        public Gold(AnimationFrame[] animationFrames, int startTick) : base(animationFrames, startTick)
        {
            Type = "Gold";
            Solid = false;
        }

        public override void PlayerEnter(Player player, GameController controller)
        {
            Dead = true;
        }
    }

    public class Key : Item
    {
        public Color Color { get; private set; }

        public Key(Color color, Rectangle rectangle)
        {
            Color = color;
            Rectangle = rectangle;
            Solid = false;
        }

        public override void PlayerEnter(Player player, GameController controller)
        {
            player.Keys[Color]++;
            Dead = true;
        }
    }

    public class Door : Item
    {
        public Color Color { get; private set; }

        public Door(Color color, Rectangle rectangle)
        {
            Color = color;
            Rectangle = rectangle;
            Solid = true;
        }

        public override void Bump(Player player)
        {
            base.Bump(player);
            if (player.Keys[Color] == 0) return;
            player.Keys[Color]--;
            Dead = true;
        }
    }

    public class Scroll : Item
    {
        private string _message;

        public Scroll(string message, Rectangle rectangle)
        {
            Rectangle = rectangle;
            _message = message;
            Solid = false;
        }

        public override void PlayerEnter(Player player, GameController controller)
        {
            controller.ShowMessage(_message);
        }
    }

    public class Exit : Item
    {
        private Rectangle _openRectangle;

        public Exit(Rectangle rectangle, Rectangle openRectangle)
        {
            Rectangle = rectangle;
            _openRectangle = openRectangle;
            Solid = true;
        }

        /// <summary>
        /// Called by the controller when the last gem is picked up
        /// </summary>
        public void Open()
        {
            Solid = false;
            Rectangle = _openRectangle;
        }

        public override void PlayerEnter(Player player, GameController controller)
        {
            base.PlayerEnter(player, controller);
            controller.ExitLevel();
        }
    }

    public class Crate : Item
    {
        public Crate(Rectangle rectangle)
        {
            Rectangle = rectangle;
            Pushable = true;
            Solid = true;
            Heavy = false;
        }
    }

    public class Boulder : Item
    {
        public Boulder(Rectangle rectangle)
        {
            Rectangle = rectangle;
            Pushable = true;
            Solid = true;
            Heavy = true;
        }
    }

    public class PlainFloor : Item
    {
        public PlainFloor(Rectangle rectangle)
        {
            Rectangle = rectangle;
        }
    }

    public class SwitchFloor : Item
    {
        public Color Color { get; private set; }

        public SwitchFloor(Color color, Rectangle rectangle)
        {
            Color = color;
            Rectangle = rectangle;
        }
    }
}
