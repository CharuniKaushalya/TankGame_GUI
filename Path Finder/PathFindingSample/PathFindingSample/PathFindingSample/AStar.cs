using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PathFindingSample
{
    public class Unit
    {
        public bool isPlayer;
    }

    public class TileInfo
    {
        public enum TileType { Floor, Wall, Door, OpenDoor };
        public TileType tileType = TileType.Floor;
        public Unit unitOnTile;
    }

    public class A_Star
    {
        TileInfo[,] colMap;
        Dictionary<Point, listitem> list = new Dictionary<Point, listitem>();
        List<listitem> openList = new List<listitem>();

        int hWeight;

        int goalX;
        int goalY;
        int startX;
        int startY;


        int Width, Height;

     
        public List<Vector2> Path;

        public bool pathAvailable = false;

        public struct listitem
        {
            public listitem(int X, int Y)
            {
                x = X;
                y = Y;
                opened = false;
                closed = false;
                F = 0;
                G = 0;
                H = 0;
                parentX = 0;
                parentY = 0;
            }
            public int x;
            public int y;
            public bool opened;
            public bool closed;
            public int F;
            public int G;
            public int H;
            public int parentX;
            public int parentY;
        }

        public A_Star(TileInfo[,] ColMap, int _hWeight)
        {
            hWeight = _hWeight;
            colMap = ColMap;
            Path = new List<Vector2>();
            Width = colMap.GetLength(0);
            Height = colMap.GetLength(1);
        }

        public bool start(int Sx, int Sy, int Gx, int Gy)
        {
            pathAvailable = false;

            goalX = Gx;
            goalY = Gy;

            startX = Sx;
            startY = Sy;

            Path.Clear();

            openList.Clear();
           
            list.Clear();

            bool noPath = false;

            if ((Gx - 1 < 0 || colMap[Gx - 1, Gy].tileType == TileInfo.TileType.Wall) &&
               (Gx + 1 > Width || colMap[Gx + 1, Gy].tileType == TileInfo.TileType.Wall) &&
                (Gy - 1 < 0 || colMap[Gx, Gy - 1].tileType == TileInfo.TileType.Wall) &&
                (Gy + 1 > Width || colMap[Gx, Gy + 1].tileType == TileInfo.TileType.Wall))
                noPath = true;


            if (!(Sx == Gx && Sy == Gy) && colMap[Gx, Gy].tileType != TileInfo.TileType.Wall && !noPath)
            {
                list.Add(new Point(Sx, Sy), new listitem(Sx, Sy));
                list.Add(new Point(Gx, Gy), new listitem(Gx, Gy));
                openList.Add(new listitem(Sx, Sy));

                setOpenOnLowestF(Sx, Sy, Gx, Gy);

                if (pathAvailable)
                {
                    makePath(Gx, Gy, Sx, Sy);
                }
                else if (colMap[Gx, Gy].unitOnTile != null && colMap[Gx, Gy].unitOnTile.isPlayer
                    && Math.Abs(goalX - startX) < 2 && Math.Abs(goalY - startY) <  2)
                {
                    // If the player is on the goal tile then return true so we can attack him
                    Path.Add(new Vector2(Gx, Gy));
                    return true;
                }


            }

            //Console.WriteLine(Path.ToString);

            return pathAvailable;
        }


        void setOpenOnLowestF(int Sx, int Sy, int Gx, int Gy)
        {
            int test = 0;
            int loops = 0;
            int removeIndex = 0;

            do
            {
                Point startLoc = new Point(Sx, Sy);
                listitem ListItem = list[startLoc];
                loops++;


                ListItem.opened = false;
                openList.RemoveAt(removeIndex);
                ListItem.closed = true;
                list[startLoc] = ListItem;

                int YFin = Sy + 1;
                int XFin = Sx + 1;
                for (int y = Sy - 1; y <= YFin; ++y)
                {
                    if (y < 0 || y > Height - 1)
                        continue;

                    for (int x = Sx - 1; x <= XFin; ++x)
                    {
                        Point Location = new Point(x, y);

                        if (!list.ContainsKey(Location))
                            list.Add(new Point(x, y), new listitem(x, y));

                        if ((x == Sx && y == Sy) || (x < 0 || x > Width - 1)
                                || (!isWalkable(x, y)) || (list[Location].closed)) // || npcthere(x, y))
                            continue;

                        TileInfo ColNode = colMap[x, y];

                        listitem tempListItem = list[Location];

                        if (tempListItem.opened)
                        {
                            int tempG = 0;
                            if (x == Sx || y == Sy)
                            {
                                if (ColNode.unitOnTile == null)
                                    tempG = 10 + list[Location].G;
                                else
                                {

                                    tempListItem.G = 45 + tempListItem.G;
                                }

                            }
                            else
                            {
                                if (ColNode.unitOnTile == null)
                                    tempG = 14 + tempListItem.G;
                                else
                                {

                                    tempListItem.G = 62 + tempListItem.G;

                                }

                            }

                            if (tempG < list[Location].G)
                            {
                                tempListItem.G = tempG;
                                tempListItem.parentX = Sx;
                                tempListItem.parentY = Sy;
                                tempListItem.F = tempListItem.G + tempListItem.H;
                            }

                            list[Location] = tempListItem;
                            continue;

                        }

                        if (x == Sx || y == Sy)
                        {


                            if (ColNode.unitOnTile == null)
                                tempListItem.G = 10 + list[new Point(Sx, Sy)].G;
                            else
                            {
                                        tempListItem.G = 45 + list[new Point(Sx, Sy)].G;

                            }
                            tempListItem.opened = true;
                            tempListItem.x = x;
                            tempListItem.y = y;
                            tempListItem.parentX = Sx;
                            tempListItem.parentY = Sy;
                            tempListItem.H = calculateH(goalX, x) + calculateH(goalY, y);
                            tempListItem.F = tempListItem.G + tempListItem.H;
                            openList.Add(tempListItem);
                            list[Location] = tempListItem;
                        }
                        else
                        {
                            if ((x + 1 < Width && !isWalkable(x + 1, y)) || 
                                (x - 1 >= 0 &&!isWalkable(x - 1, y)) || 
                                (y - 1 >= 0 && !isWalkable(x, y - 1)) || 
                                (y + 1 < Height && !isWalkable(x, y + 1)))
                            {
                                tempListItem.opened = false;
                                list[Location] = tempListItem;
                                continue;
                            }

                            if (ColNode.unitOnTile == null)
                                tempListItem.G = 14 + list[new Point(Sx, Sy)].G;
                            else
                            {
                                tempListItem.G = 62 + list[new Point(Sx, Sy)].G;
                            }

                            tempListItem.opened = true;
                            tempListItem.x = x;
                            tempListItem.y = y;
                            tempListItem.parentX = Sx;
                            tempListItem.parentY = Sy;
                            tempListItem.H = calculateH(goalX, x) + calculateH(goalY, y);
                            tempListItem.F = tempListItem.G + tempListItem.H;
                            openList.Add(tempListItem);
                            list[Location] = tempListItem;

                        }
                    }
                }

                if (!(list[new Point(Gx, Gy)].opened))
                {
                    int lowestF = 10000;
                    test = 0;
                    int ITEMS = openList.Count;
                    for (int index = 0; index < ITEMS; ++index)
                    {
                        test++;
                        if (openList[index].F < lowestF)
                        {
                            lowestF = openList[index].F;
                            Sx = openList[index].x;
                            Sy = openList[index].y;
                            removeIndex = index;
                        }

                    }

                }
                diplayCurrent(new Point(Sx, Sy));                
            } while (test != 0 && !list[new Point(Gx, Gy)].opened && loops < 300);

            diplayCurrent(new Point(Sx, Sy));

            if (test == 0 || loops > 249)
                pathAvailable = false;
            else
                pathAvailable = true;

        }



        bool isWalkable(int x, int y)
        {
            listitem tempItem = new listitem();
            Point Loc = new Point(x, y);

            if (colMap[x, y].tileType == TileInfo.TileType.Wall)
            {
                tempItem.closed = true;
                if (!list.ContainsKey(Loc))
                    list.Add(Loc, tempItem);
                list[new Point(x, y)] = tempItem;
                return false;
            }
            else if (colMap[x, y].unitOnTile != null && colMap[x, y].unitOnTile.isPlayer && x != goalX && y != goalY)
            {
                tempItem.closed = true;
                if (!list.ContainsKey(Loc))
                    list.Add(Loc, tempItem);
                list[new Point(x, y)] = tempItem;
                return false;
            }

            return true;
        }

        int calculateH(int G, int S)
        {
            if (S > G)
                return (S - G) * hWeight;
            else if (G > S)
                return (G - S) * hWeight;
            else
                return 0;
        }


        void makePath(int Gx, int Gy, int Sx, int Sy)
        {
            Vector2 temp = new Vector2(Gx, Gy);


            while (!(temp.X == Sx && temp.Y == Sy))
            {
                Path.Insert(0, temp);

                int x = (int)temp.X;
                int y = (int)temp.Y;
                Point loc = new Point(x, y);
                temp = new Vector2(list[loc].parentX, list[loc].parentY);
            }
        }

        Vector2 getParentof(int x, int y)
        {
            Point loc = new Point(x, y);
            return new Vector2(list[loc].parentX, list[loc].parentY);
        }



        public bool getOpened(int x, int y)
        {
            Point loc = new Point(x, y);
            return list[loc].opened;
        }

        public bool getClosed(int x, int y)
        {
            Point loc = new Point(x, y);
            return list[loc].closed;
        }

        public int getF(int x, int y)
        {
            Point loc = new Point(x, y);
            return list[loc].F;
        }

        public int getG(int x, int y)
        {
            Point loc = new Point(x, y);
            return list[loc].G;
        }

        public int getH(int x, int y)
        {
            Point loc = new Point(x, y);
            return list[loc].H;
        }

#if WINDOWS
        public void diplayCurrent(Point p)
        {
            Console.Clear();

            Console.WriteLine("Open or Closed");

            for (int y = -1; y < colMap.GetLength(1); ++y)
            {
                for (int x = -1; x < colMap.GetLength(0); ++x)
                {
                    if (x == -1 || y == -1)
                    {
                        if (y == -1 && x == -1)
                            Console.Write("\t");
                        if (y == -1 && x != -1)
                        {
                            if (x < 10)
                                Console.Write("  " + x);
                            else
                                Console.Write(" " + x);
                        }
                        if (y != -1 && x == -1)
                            Console.Write(y + "\t");
                    }
                    else
                    {
                        Point loc = new Point(x, y);
                        if (startX == x && startY == y)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write(" S ");
                        }
                        else if (p.X == x && p.Y == y)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.Write(" X ");
                        }
                        else if (goalX == x && goalY == y)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write(" G ");
                        }
                        else if (list.ContainsKey(loc) && getOpened(x, y))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(" O ");
                        }
                        else if (list.ContainsKey(loc) && getClosed(x, y))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(" C ");
                        }
                        else if (colMap[x, y].tileType == TileInfo.TileType.Wall)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(" # ");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(" x ");
                        }

                    }


                }
                Console.WriteLine();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                
            }
            Console.ReadKey();
            //Console.WriteLine("Values");

            //for (int y = -1; y < colMap.GetLength(1); ++y)
            //{
            //    for (int x = -1; x < colMap.GetLength(0); ++x)
            //    {
            //        if (x == -1 || y == -1)
            //        {
            //            if (y == -1 && x == -1)
            //                Console.Write("\t");
            //            if (y == -1 && x != -1)
            //            {
            //                if (x < 10)
            //                    Console.Write("   " + x);
            //                else
            //                    Console.Write("  " + x);
            //            }
            //            if (y != -1 && x == -1)
            //                Console.Write(y + "\t");
            //        }
            //        else
            //        {
            //            Point loc = new Point(x, y);
            //            if (list.ContainsKey(loc))
            //            {
            //                Console.ForegroundColor = ConsoleColor.Yellow;
            //                if (list[loc].F < 10)
            //                    Console.Write(" 00" + list[loc].H + " ");
            //                else if (list[loc].F < 100)
            //                    Console.Write(" 0" + list[loc].H + " ");
            //                else
            //                    Console.Write(" " + list[loc].H + " ");
            //            }
            //            else
            //                Console.Write(" XXX");
            //        }


            //    }
            //    Console.WriteLine();
            //    Console.WriteLine();
            //    Console.ForegroundColor = ConsoleColor.White;
            //}


            //Console.WriteLine();
            //Console.WriteLine("Values of G");

            //for (int y = 0; y < colMap.GetLength(1); ++y)
            //{
            //    for (int x = 0; x < colMap.GetLength(0); ++x)
            //    {
            //        Point loc = new Point(x, y);
            //        if (list.ContainsKey(loc))
            //        {
            //            if (list[loc].G > 9)
            //                Console.Write("" + list[loc].G + " ");
            //            else
            //                Console.Write(" 0" + list[loc].G + " ");
            //        }
            //    }
            //    Console.WriteLine();
            //    Console.WriteLine();
            //}

            //Console.WriteLine();
            //Console.WriteLine("H Values");

            //     for (int y = 0; y < colMap.GetLength(1); ++y)
            //{
            //    for (int x = 0; x < colMap.GetLength(0); ++x)
            //    {
            //        Point loc = new Point(x, y);
            //        if (list.ContainsKey(loc))
            //        {
            //            if (list[loc].H > 9)
            //            Console.Write(" " + list[loc].H + " ");
            //                else
            //            Console.Write(" 0" + list[loc].H + " ");
            //        }
            //    }
            //    Console.WriteLine();
            //    Console.WriteLine();
            //}


            //Console.ReadKey();



        } // End of Function
#endif



    }
}
