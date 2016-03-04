using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TileMap.Dean
{
    class CoinPile
    {
        private int locationX;
        private int locationY;
        private float lifeTime = 0.0f;
        private int price;
        private float iTimer = 0.0f;
        private Point position;

        private float appearTime = 0.0f;
        private DateTime disappearTime;

        public CoinPile(int x, int y)
        {
            locationX = x;
            locationY = y;
            position = new Point(locationX, locationY);

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
        public  float LifeTime
        {
            get { return lifeTime; }
            set { lifeTime = (value);
            DateTime gametime = DateTime.Now;
            disappearTime = gametime.AddMilliseconds(lifeTime);
            }
        }
        public int Price
        {
            get { return price; }
            set { price = value; }
        }
        public float AppearTime
        {
            get { return appearTime; }
        }

        public DateTime DisappearTime
        {
            get { return disappearTime; }
        }
    }
}
