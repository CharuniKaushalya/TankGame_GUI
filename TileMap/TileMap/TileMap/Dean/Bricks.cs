using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TileMap.Dean
{
    class Bricks
    {
        Point pos;
        public Bricks(Point position)
        {
            pos = position;
        }
        int damageLevel = 0;

        public int DamageLevel
        {
            get { return damageLevel; }
            set { damageLevel = value; }
        }
    }
}
