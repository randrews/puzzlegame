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

        public static Item ForType(string type, System.Drawing.Rectangle? rect)
        {
            if (type == "Start")
            {
                return new Start();
            }
            else
            {
                return new Item {Type = type, Rectangle = rect};
            }
        }
    }

    public class Start : Item
    {
        public Start()
        {
            Type = "Start";
        }
    }
}
