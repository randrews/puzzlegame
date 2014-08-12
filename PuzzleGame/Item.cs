using PuzzleGame.Items;
using System;
using System.Drawing;

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

        /// <summary>
        /// Just like we share a sprite library for all items, we'll share a Random. This is just to make the
        /// code in each item a little simpler, and the loading code a little simpler, at the cost of some
        /// extra initialization for the Item class.
        /// </summary>
        public static Random Random { get; set; }

        public Sprite Sprite { get; set; }
        public string Type { get; set; }
        public Color Color { get; set; }
        public Rectangle Rectangle { get { return Sprite.Rectangle; } }

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

        /// <summary>
        /// This is an event called on every logical "turn" of the game: whenever the player moves, and when the turn timer fires
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="location"></param>
        public virtual void Turn(GameController controller, Point location) { }

        /// <summary>
        /// This event fires when the turn timer fires, but NOT when the player moves.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="location"></param>
        public virtual void Tick(GameController controller, Point location) { }

        public Item()
        {
            Dead = false;
            if(Random == null || SpriteLibrary == null)
                throw new Exception("You must initialize the Item class with a Random and a SpliteLibrary before creating items.");
        }
    }
}
