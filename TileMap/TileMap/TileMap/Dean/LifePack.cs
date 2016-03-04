using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TileMap.Dean
{
    class LifePack
    {
        private int locationX;
        private int locationY;
        private float lifeTime = 0.0f;
        private float iTimer = 0.0f;
        private DateTime disappearTime;

        public LifePack(int x, int y)
        {
            locationX = x;
            locationY = y;
        }
        public int LocationX
        {
            get;
            set;
        }
        public int LocationY
        {
            get;
            set;
        }
        public float LifeTime
        {
            get { return lifeTime; }
            set { lifeTime = (value);
            DateTime gametime = DateTime.Now;
            disappearTime = gametime.AddMilliseconds(lifeTime);
            }
        }
        public DateTime DisappearTime
        {
            get { return disappearTime; }
        }
    }
}
