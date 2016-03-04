using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace PathFindingSample
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D tileImage;
        A_Star a_star;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 500;
            graphics.PreferredBackBufferHeight = 500;
        }

        TileInfo[,] tileInfo = new TileInfo[10, 10];

        Point startLoc = new Point(3, 4);
        Point endLoc = new Point(9, 9);

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            

            // Create the tile objects in the array
            for (int x = 0; x < tileInfo.GetLength(0); x++)
            {
                for (int y = 0; y < tileInfo.GetLength(0); y++)
                {
                    tileInfo[x, y] = new TileInfo();
                }
            }

            // Change some tiles to walls
            tileInfo[4, 0].tileType = TileInfo.TileType.Wall;
            tileInfo[4, 1].tileType = TileInfo.TileType.Wall;
            tileInfo[4, 2].tileType = TileInfo.TileType.Wall;
            tileInfo[4, 3].tileType = TileInfo.TileType.Wall;
            tileInfo[4, 4].tileType = TileInfo.TileType.Wall;
            tileInfo[4, 5].tileType = TileInfo.TileType.Wall;
            tileInfo[3, 5].tileType = TileInfo.TileType.Wall;
            tileInfo[2, 5].tileType = TileInfo.TileType.Wall;
            tileInfo[1, 5].tileType = TileInfo.TileType.Wall;
            tileInfo[1, 4].tileType = TileInfo.TileType.Wall;
            tileInfo[1, 3].tileType = TileInfo.TileType.Wall;
            tileInfo[1, 2].tileType = TileInfo.TileType.Wall;

            tileInfo[7, 6].tileType = TileInfo.TileType.Wall;
            tileInfo[7, 7].tileType = TileInfo.TileType.Wall;
            tileInfo[7, 8].tileType = TileInfo.TileType.Wall;
            tileInfo[7, 9].tileType = TileInfo.TileType.Wall;


            // Pass the tile information and a weight for the H
            // the lower the H weight value shorter the path
            // the higher it is the less number of checks it take to determine
            // a path
            a_star = new A_Star(tileInfo, 100);

            a_star.start(startLoc.X, startLoc.Y, endLoc.X, endLoc.Y);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            tileImage = Content.Load<Texture2D>("tile");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            for (int x = 0; x < tileInfo.GetLength(0); x++)
            {
                for (int y = 0; y < tileInfo.GetLength(0); y++)
                {
                    if (tileInfo[x, y].tileType == TileInfo.TileType.Floor)
                        spriteBatch.Draw(tileImage, new Vector2(x * 50, y * 50), Color.White);
                    else if (tileInfo[x, y].tileType == TileInfo.TileType.Wall)
                        spriteBatch.Draw(tileImage, new Vector2(x * 50, y * 50), Color.DarkGray);
                }
            }

            for (int i = 0; i < a_star.Path.Count; i++)
            {
                 spriteBatch.Draw(tileImage, new Vector2(a_star.Path[i].X * 50, a_star.Path[i].Y * 50), Color.Yellow);
            }

            spriteBatch.Draw(tileImage, new Vector2(startLoc.X * 50, startLoc.Y * 50), Color.Green);
            spriteBatch.Draw(tileImage, new Vector2(endLoc.X * 50, endLoc.Y * 50), Color.Red);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
