using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TileMap
{
    class Map
    {
        private List<CollisionTiles> collisionTiles = new List<CollisionTiles>();
        public List<CollisionTiles> CollisionTiles
        {
            get { return collisionTiles; }

        }
        private int width, height;
        public int Width
        {
            get { return width; }
        }
        public int Height
        {
            get { return height; }
        }
        public Map() { }

        public void Generate(int[,] map , int size){
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y <10; y++)
                {
                    int num = map[y, x];
                    if (num > 0)
                        collisionTiles.Add(new CollisionTiles(num, new Rectangle(x * size, y * size, size, size)));
                    width = (x + 1) * size;
                    height = (y + 1) * size;
                }
            }
        }
        public void draw(SpriteBatch spritebatch)
        {
            foreach (CollisionTiles tile in collisionTiles)
            {
                tile.draw(spritebatch);
          
            }
               
        }


    }
}
