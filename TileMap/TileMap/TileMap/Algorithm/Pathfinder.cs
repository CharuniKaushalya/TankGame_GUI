using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TileMap.Algorithm
{
    class Pathfinder
    {
        public List<Point> Pathfind(Point start, Point end, int[,] Weight)
        {
            #region Variables
            var closedSet = new List<Point>();          // cells that already analyzed and have a path from the start to them

            var openSet = new List<Point> { start };    // cells that identified as a neighbor of an analyzed node, but have yet to be fully analyzed
            var cameFrom = new Dictionary<Point, Point>();          // to back-track from the end to find the optimal path
            var currentDistance = new Dictionary<Point, int>();     // to store distance from the start  
            var predictedDistance = new Dictionary<Point, float>(); //to store distance to reach the end

            // initialize the start node as having a distance of 0, and an estmated distance 
            // of y-distance + x-distance, which is the optimal path in a square grid that 
            // doesn't allow for diagonal movement
            currentDistance.Add(start, 0);
            #endregion

            predictedDistance.Add(
                start,
                0 + +Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y)
            );

            // if there are any unanalyzed nodes, process them
            while (openSet.Count > 0)
            {
                // get the node with the lowest estimated cost to finish
                var current = (
                    from p in openSet orderby predictedDistance[p] ascending select p
                ).First();

                // if it is the finish, return the path
                if (current.X == end.X && current.Y == end.Y)
                {
                    // generate the found path
                    return ReconstructPath(cameFrom, end);
                }

                // move current node from open to closed
                openSet.Remove(current);
                closedSet.Add(current);

                // process each valid node around the current node
                IEnumerable<Point> neghbors = GetNeighborNodes(current, Weight);
                if (neghbors.Count() > 0)
                {
                    foreach (var neighbor in neghbors)
                    {
                        var tempCurrentDistance = currentDistance[current] + 1;

                        // if we already know a faster way to this neighbor, use that route and 
                        // ignore this one
                        if (closedSet.Contains(neighbor)
                            && tempCurrentDistance >= currentDistance[neighbor])
                        {
                            continue;
                        }

                        // if we don't know a route to this neighbor, or if this is faster, 
                        // store this route
                        if (!closedSet.Contains(neighbor)
                            || tempCurrentDistance < currentDistance[neighbor])
                        {
                            if (cameFrom.Keys.Contains(neighbor))
                            {
                                cameFrom[neighbor] = current;
                            }
                            else
                            {
                                cameFrom.Add(neighbor, current);
                            }

                            currentDistance[neighbor] = tempCurrentDistance;
                            predictedDistance[neighbor] =
                                currentDistance[neighbor]
                                + Math.Abs(neighbor.X - end.X)
                                + Math.Abs(neighbor.Y - end.Y);

                            // if this is a new node, add it to processing
                            if (!openSet.Contains(neighbor))
                            {
                                openSet.Add(neighbor);
                            }
                        }
                    }
                }
                else
                {
                    return new List<Point>();
                }
            }
            return new List<Point>(); //if there is no path betwen end point return empty list
        }

        // Return a list of accessible nodes neighboring a specified node
        private IEnumerable<Point> GetNeighborNodes(Point node, int[,] Weight)
        {
            var nodes = new List<Point>();

            // up
            if (node.Y - 1 >= 0)
            {
                if (Weight[node.X, node.Y - 1] == 1 || Weight[node.X, node.Y - 1] == 5 || Weight[node.X, node.Y - 1] == 6)
                {
                    nodes.Add(new Point(node.X, node.Y - 1));
                }
            }

            // right
            if (node.X + 1 <10)
            {
                if (Weight[node.X + 1, node.Y] == 1 || Weight[node.X + 1, node.Y] == 5 || Weight[node.X + 1, node.Y] == 6)
            {
                nodes.Add(new Point(node.X + 1, node.Y));
            }}

            // down
            if (node.Y + 1 < 10)
            {
                if (Weight[node.X, node.Y + 1] == 1 || Weight[node.X, node.Y + 1] == 5 || Weight[node.X, node.Y + 1] == 6)
                {
                    nodes.Add(new Point(node.X, node.Y + 1));
                }
            }

            // left
            if (node.X - 1 >= 0)
            {
                if (Weight[node.X - 1, node.Y] == 1 || Weight[node.X - 1, node.Y] == 5 || Weight[node.X - 1, node.Y] == 6)
                {
                    nodes.Add(new Point(node.X - 1, node.Y));
                }
            }

            return nodes;
        }

        // Process a list of valid paths generated by the Pathfind function and return 
        // a coherent path to current.
        private List<Point> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current)
        {
            if (!cameFrom.Keys.Contains(current))
            {
                return new List<Point> { current };
            }

            var path = ReconstructPath(cameFrom, cameFrom[current]);
            path.Add(current);
            return path;
        }

    }
}
