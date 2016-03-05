#region File Description
//-----------------------------------------------------------------------------
// PathFinder.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Pathfinding
{
    #region Search Status Enum
    public enum SearchStatus
    {
        Stopped,
        Searching,
        NoPath,
        PathFound,
    }
    #endregion

    class PathFinder
    {
        #region Search Node Struct

        /// Reresents one node in the search space

        private struct SearchNode
        {

            public Point Position;      /// Location on the map

            public int DistanceToGoal;      /// Distance to goal estimate

            public int DistanceTraveled;        /// Distance traveled from the start

            public SearchNode(Point mapPosition, int distanceToGoal, int distanceTraveled)
            {
                Position = mapPosition;
                DistanceToGoal = distanceToGoal;
                DistanceTraveled = distanceTraveled;
            }
        }
        #endregion

        #region Constants        

        /// Scales the draw size of the search nodes
        const float searchNodeDrawScale = .75f;

        #endregion

        #region Fields

        //Draw data
        private Texture2D nodeTexture;
        private Vector2 nodeTextureCenter;
        private Color openColor = Color.Green;
        private Color closedColor = Color.Red;


        private float timeSinceLastSearchStep = 0f;     // How much time has passed since the last search step
        private List<SearchNode> openList;              // Holds search nodes that are avaliable to search
        private List<SearchNode> closedList;            // Holds the nodes that have already been searched
        private Dictionary<Point, Point> paths;         // Holds all the paths we've creted so far

        private Map map;                                // The map we're searching

        public float timeStep = .1f;                     // Seconds per search step 

        #endregion
        
        #region Properties

        // Tells us if the search is stopped, started, finished or failed
        public SearchStatus SearchStatus
        {
            get { return searchStatus; }
        }
        private SearchStatus searchStatus;

        public float Scale
        {
            get { return scale; }
            set { scale = value * searchNodeDrawScale; }
        }
        private float scale;
        
        // Seconds per search step
        public float TimeStep
        {
            get { return timeStep; }
            set { timeStep = value; }
        }

        /// Toggles searching on and off
        public bool IsSearching
        {
            get { return searchStatus == SearchStatus.Searching; }
            set 
            {
                if (searchStatus == SearchStatus.Searching)
                {
                    searchStatus = SearchStatus.Stopped;
                }
                else if (searchStatus == SearchStatus.Stopped)
                {
                    searchStatus = SearchStatus.Searching;
                }
            }
        }

        /// How many search steps have elapsed on this map
        public int TotalSearchSteps
        {
            get { return totalSearchSteps; }
        }
        private int totalSearchSteps = 0;

        #endregion

        #region Initialization

        /// Setup search
        /// <param name="mazeMap">Map to search</param>
        public void Initialize(Map mazeMap)
        {
            searchStatus = SearchStatus.Stopped;
            openList = new List<SearchNode>();
            closedList = new List<SearchNode>();
            paths = new Dictionary<Point, Point>();
            map = mazeMap;
        }

        /// Load the Draw texture
        public void LoadContent(ContentManager content)
        {
            nodeTexture = content.Load<Texture2D>("dot");
            nodeTextureCenter = new Vector2(nodeTexture.Width / 2, nodeTexture.Height / 2);
        }

        #endregion

        #region Update and Draw

        /// Search Update
        public void Update(GameTime gameTime)
        {
            if (searchStatus == SearchStatus.Searching)
            {
                timeSinceLastSearchStep += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timeSinceLastSearchStep >= timeStep)
                {
                    DoSearchStep();
                    timeSinceLastSearchStep = 0f;
                }
            }
        }
        
        /// Draw the search space
        public void Draw(SpriteBatch spriteBatch)
        {
            if (searchStatus != SearchStatus.PathFound)
            {
                spriteBatch.Begin();
                foreach (SearchNode node in openList)
                {
                    spriteBatch.Draw(nodeTexture, 
                        map.MapToWorld(node.Position, true), null, openColor, 0f,
                        nodeTextureCenter, scale, SpriteEffects.None, 0f);
                }
                foreach (SearchNode node in closedList)
                {
                    spriteBatch.Draw(nodeTexture, 
                        map.MapToWorld(node.Position, true), null, closedColor, 0f,
                        nodeTextureCenter, scale, SpriteEffects.None, 0f);
                }
                spriteBatch.End();
            }
        }

        #endregion

        #region Methods

        /// Reset the search
        public void Reset()
        {
            searchStatus = SearchStatus.Stopped;
            totalSearchSteps = 0;
            Scale = map.Scale;
            openList.Clear();
            closedList.Clear();
            paths.Clear();
            openList.Add(new SearchNode(map.StartTile,
                Map.StepDistance(map.StartTile, map.EndTile)
                , 0));
        }


        /// This method find the next path node to visit, puts that node on the 
        /// closed list and adds any nodes adjacent to the visited node to the open list.
        private void DoSearchStep()
        {
            SearchNode newOpenListNode;

            bool foundNewNode = SelectNodeToVisit(out newOpenListNode);
            if (foundNewNode)
            {
                Point currentPos = newOpenListNode.Position;
                foreach (Point point in map.OpenMapTiles(currentPos))
                {
                    SearchNode mapTile = new SearchNode(point, 
                        map.StepDistanceToEnd(point), 
                        newOpenListNode.DistanceTraveled + 1);
                    if (!InList(openList,point) &&
                        !InList(closedList,point))
                    {
                        openList.Add(mapTile);
                        paths[point] = newOpenListNode.Position;
                    }
                }
                if (currentPos == map.EndTile)
                {
                    searchStatus = SearchStatus.PathFound;
                }
                openList.Remove(newOpenListNode);
                closedList.Add(newOpenListNode);
            }
            else
            {
                searchStatus = SearchStatus.NoPath;
            }
        }

        /// Determines if the given Point is inside the SearchNode list given
        private static bool InList(List<SearchNode> list, Point point)
        {
            bool inList = false;
            foreach (SearchNode node in list)
            {
                if (node.Position == point)
                {
                    inList = true;
                }
            }
            return inList;
        }

        /// This Method looks at everything in the open list and chooses the next path to visit based on which search type is currently selected. 
        /// <param name="result">The node to be visited</param>
        /// <returns>Whether or not SelectNodeToVisit found a node to examine
        /// </returns>
        private bool SelectNodeToVisit(out SearchNode result)
        {
            result = new SearchNode();
            bool success = false;
            float smallestDistance = float.PositiveInfinity;
            float currentDistance = 0f;
            if (openList.Count > 0)
            {
                totalSearchSteps++;
                foreach (SearchNode node in openList)
                {
                    currentDistance = Heuristic(node);
                    // The heuristic value gives us our optimistic estimate 
                    // for the path length, while any path with the same 
                    // heuristic value is equally ‘good’ in this case we’re 
                    // favoring paths that have the same heuristic value 
                    // but are longer.
                    if (currentDistance <= smallestDistance)
                    {
                        if (currentDistance < smallestDistance)
                        {
                            success = true;
                            result = node;
                            smallestDistance = currentDistance;
                        }
                        else if (currentDistance == smallestDistance &&
                            node.DistanceTraveled > result.DistanceTraveled)
                        {
                            success = true;
                            result = node;
                            smallestDistance = currentDistance;
                        }
                    }
                }
            }
            return success;
        }

        /// Generates an optimistic estimate of the total path length to the goal from the given position.
        /// <param name="location">Location to examine</param>
        /// <returns>Path length estimate</returns>
        private static float Heuristic(SearchNode location)
        {
            return location.DistanceTraveled + location.DistanceToGoal;
        }

        /// Generates the path from start to end.
        /// <returns>The path from start to end</returns>
        public LinkedList<Point> FinalPath()
        {
            LinkedList<Point> path = new LinkedList<Point>();
            if (searchStatus == SearchStatus.PathFound)
            {
                Point curPrev = map.EndTile;
                path.AddFirst(curPrev);
                while (paths.ContainsKey(curPrev))
                {
                    curPrev = paths[curPrev];
                    path.AddFirst(curPrev);
                }
            }
            return path;
        }

        #endregion
    }
}
