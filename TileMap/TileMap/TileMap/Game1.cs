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
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TileMap
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class ThreadInfo
    {
        public string com { get; set; }
    }
    
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region variables
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont Font1;
        Map map;
        int[,] mappArray;
        Texture2D t2dTanks;
        Texture2D bullet;
        Texture2D BackMark;
        Player.MobileSprite bul;
        String text;
        String points;
        String coins;
        TcpListener listener;
        int Player_No;
        int Players_added;

        //key states
        Connection.InputHandler inputh;
        KeyboardState currentState;
        KeyboardState perviousState;

        //listen thread
        Thread bThread;
        ThreadInfo lthreadInfo;
        ThreadInfo rthreadInfo;
        ThreadInfo uthreadInfo;
        ThreadInfo dthreadInfo;
        ThreadInfo sthreadInfo;

        //boolean connection
        private bool isConnectedToServer;
        Connection.Communicator com;
        Connection.Decoder dec;

        //lists
        List<Dean.Bricks> BricksList = new List<Dean.Bricks>();
        List<Point> StonesList = new List<Point>();
        List<Point> waterList = new List<Point>();
        List<Dean.MapItem> maplist = new List<Dean.MapItem>();
        List<Dean.LifePack> lifepacklist = new List<Dean.LifePack>();
        List<Dean.CoinPile> coinpilelist = new List<Dean.CoinPile>();
        List<Player.MobileSprite> playerlist = new List<Player.MobileSprite>();
        List<Player.MobileSprite> bulletlist = new List<Player.MobileSprite>();

        //path finder
        int[,] pathArray;
        Algorithm.Pathfinder pathFinder;
        List<Point> pathList = new List<Point>();
        #endregion

        #region button states
        enum BState
        {
            HOVER,
            UP,
            JUST_RELEASED,
            DOWN
        }
        const int
            BUTTON_HEIGHT = 40,
            BUTTON_WIDTH = 100;
        Color background_color;
        Color button_color = new Color();
        Rectangle button_rectangle = new Rectangle();
        BState button_state = new BState();
        Texture2D button_texture;
        double button_timer = new double();
        //mouse pressed and mouse just pressed
        bool mpressed, prev_mpressed = false;
        //mouse location in window
        int mx, my;
        double frame_time;
        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            #region threads
            lthreadInfo = new ThreadInfo();
            lthreadInfo.com = "LEFT#";
            rthreadInfo = new ThreadInfo();
            rthreadInfo.com = "RIGHT#";
            uthreadInfo = new ThreadInfo();
            uthreadInfo.com = "UP#";
            dthreadInfo = new ThreadInfo();
            dthreadInfo.com = "DOWN#";
            sthreadInfo = new ThreadInfo();
            sthreadInfo.com = "SHOOT#";

            #endregion

            #region button
            button_state = BState.UP;
            button_color = Color.White;
            button_timer = 0.0;
            button_rectangle = new Rectangle(10, 505, BUTTON_WIDTH, BUTTON_HEIGHT);
            IsMouseVisible = true;
            background_color = Color.CornflowerBlue;
            #endregion

            #region texts
            //draw lebels
            text = "";
            points = "0";
            coins = "0";
            #endregion

            //initialization logic here
            graphics.PreferredBackBufferWidth = 500;
            graphics.PreferredBackBufferHeight = 550;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            #region Map Initialization
            Players_added = 0;
            map = new Map();
            inputh = new Connection.InputHandler();
            com = new Connection.Communicator();
            dec = new Connection.Decoder();
            mappArray = new int[,]{
                {1,1,1,1,1,1,1,1,1,1,},
                {1,1,1,1,1,1,1,1,1,1,},
                {1,1,1,1,1,1,1,1,1,1,},
                {1,1,1,1,1,1,1,1,1,1,},
                {1,1,1,1,1,1,1,1,1,1,},
                {1,1,1,1,1,1,1,1,1,1,},
                {1,1,1,1,1,1,1,1,1,1,},
                {1,1,1,1,1,1,1,1,1,1,},
                {1,1,1,1,1,1,1,1,1,1,},
                {1,1,1,1,1,1,1,1,1,1,}
            };
            pathFinder = new Algorithm.Pathfinder();
            pathArray = new int[,]{
                {1,0,0,1,1,1,0,1,0,1,},
                {1,1,0,1,1,1,0,0,0,0,},
                {0,1,1,1,1,1,0,1,1,1,},
                {0,0,0,1,0,0,0,1,1,1,},
                {0,0,0,1,1,1,1,1,1,1,},
                {1,1,1,1,1,1,1,1,1,1,},
                {1,1,1,1,1,1,1,1,1,1,},
                {1,1,0,1,0,1,1,0,1,1,},
                {1,1,1,1,1,1,1,1,1,1,},
                {1,1,1,1,1,1,1,1,1,1,}
            };
           List<Point> pathList2 =  pathFinder.Pathfind(new Point(0, 0), new Point(7, 0), pathArray);
            #endregion

            base.Initialize(); 
        }

        #region Connection Methods
        //Connect to client
        private void ConnectAsClient()
        {
            try
            {
                //send inizialization message to server
                com.ConnectAsClient();
                text = "Connected!";
            }
            catch
            {
                text = "Commiunication fail!..." ;
            }

        }
        private void Listen()
        {
            const int PORT_NO = 7000;
            const string SERVER_IP = "127.0.0.1";


            //---listen at the specified IP and port no.---
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            listener = new TcpListener(localAdd, PORT_NO);

            listener.Start();


            while (true)
            {
                connectListener();

            }
        }

        //to send response to server to move player
        private void Command(String Command)
        {
            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Parse("127.0.0.1"), 6000);
            Console.WriteLine("\ncommand : " +Command);

            //get the incoming data through a network stream
            NetworkStream stream = client.GetStream();
            byte[] message = Encoding.ASCII.GetBytes(Command);
            stream.Write(message, 0, message.Length);
            stream.Flush();
            stream.Close();
            client.Close();
        
        }

        //to send response to server to move player
        private void handleCommand(object state)
        {
            ThreadInfo threadInfo = state as ThreadInfo;
            String Command = threadInfo.com;
            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Parse("127.0.0.1"), 6000);
            Console.WriteLine("\ncommand : " + Command);
            text = Command.Remove(Command.Length - 1);
            //get the incoming data through a network stream
            NetworkStream stream = client.GetStream();
            byte[] message = Encoding.ASCII.GetBytes(Command);
            stream.Write(message, 0, message.Length);
            stream.Flush();
            stream.Close();
            client.Close();
        }

        //get the incoming data through a network
        private void connectListener()
        {
            //---incoming client connected---
            TcpClient client = listener.AcceptTcpClient();

            //---get the incoming data through a network stream---
            NetworkStream nwStream = client.GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];

            //---read incoming stream---
            try
            {
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                //---convert the data received into a string---
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                Thread myNewThread = new Thread(() => displayGame(dataReceived));
                myNewThread.Start();

                Console.WriteLine("Received : " + dataReceived);
            }
            catch (IOException er)
            {
                Console.WriteLine(er);
              
            }
            

            
        }

        #endregion

        #region Server reply Parser Methods
        private void displayGame(string s)
        {
            

            if (s.EndsWith("#"))
            {
                s = s.Remove(s.Length - 1);
            }
            if (serverReply(s) == 1)
            {
                text = s;
            }
            else if(s.Length >0)
            {
                if (s[0] == 'I')
                {
                    //...split responce by colon....
                    string[] tokens = dec.splitByColon(s);
                    Player_No = int.Parse(tokens[1].Substring(1));
                    text = "No : " + Player_No;
                    string place = tokens[1].ToString();

                    //...get bricks cordinates...
                    Console.WriteLine("Bricks Points:");
                    updateMap(tokens, 7, 2);

                    //...get stones cordinates...
                    Console.WriteLine("Stoness Points:");
                    updateMap(tokens, 3, 3);

                    //...get water cordinates...
                    Console.WriteLine("Water Points:");
                    updateMap(tokens, 4, 4);
                } if (s[0] == 'S')
                {
                    //text += "\n"+s;
                    string[] tokens = dec.splitByColon(s);
                    string[] token1 = dec.splitBySemiColon(tokens, 1);
                    string player = "player" + token1[1];
                    string[] cordinates = dec.splitByComma(token1, 1);

                }
                if (s[0] == 'G')
                {
                    string[] tokens = dec.splitByColon(s);
                    if (tokens.Length - 2 == playerlist.Count)
                    {
                        //move players
                        for (int i = 0; i < tokens.Length - 2; i++)
                        {
                            string[] token1 = dec.splitBySemiColon(tokens, i + 1);
                            //Console.WriteLine("c-x is : " + playerlist[i].Position.X + " c-y is : " + playerlist[i].Position.Y);
                            string[] cordinates = dec.splitByComma(token1, 1);
                            int x = int.Parse(cordinates[0]) * 50 - (int)playerlist[i].Position.X;
                            int y = int.Parse(cordinates[1]) * 50 - (int)playerlist[i].Position.Y;
                            //Console.WriteLine("x is : " + int.Parse(cordinates[0]) + ", " + int.Parse(cordinates[1]) + ", " + x + ", " + " y is : " + y);
                            //playerlist[i].Sprite.MoveBy(x*50, y*50);
                            //playermovement(x, y);
                            playerlist[i].Position = new Vector2(int.Parse(cordinates[0]) * 50, int.Parse(cordinates[1]) * 50);
                            playerlist[i].Sprite.getRotation(int.Parse(token1[2]));
                            playerlist[i].Direction = int.Parse(token1[2]);
                            playerlist[i].WhetheShot = int.Parse(token1[3]);
                            
                           /*f (playerlist[i].WhetheShot == 1)
                            {
                                addBullet(int.Parse(cordinates[0]) * 50, int.Parse(cordinates[1]) * 50, playerlist[i].Direction);
                            }*/
                                //p.Sprite.MoveBy(1, 0);
                            if(i != Player_No)
                                PlayerUpdate(i);
                            else if (i == Player_No && coinpilelist.Count > 0)
                            {
                                coins = token1[5];
                                points = token1[6];
                                Console.WriteLine("point y : " + coinpilelist[0].LocationY + "po y : " + (int)playerlist[Player_No].Position.Y / 50 + "point x : " + coinpilelist[0].LocationX + "po x : " + (int)playerlist[Player_No].Position.X / 50);
                                // pathList = pathFinder.Pathfind(new Point((int)playerlist[Player_No].Position.Y / 50, (int)playerlist[Player_No].Position.X / 50), new Point(coinpilelist[0].LocationX, coinpilelist[0].LocationY), mappArray);  
                                pathList = findPathlist();
                                if (pathList.Count > 0 && playerlist.Count > 0 )
                                {
                                    if (playerlist[Player_No].Position.X / 50 == pathList[0].Y && playerlist[Player_No].Position.Y / 50 == pathList[0].X)
                                    {
                                        pathList.RemoveAt(0);

                                    } if (pathList.Count > 0)
                                        pathFInder();
                                }
                            }
                            UpdateMapWithPlayer(playerlist[i]);
                        }
                        
                    }
                  /*  else if (tokens.Length == 3)
                    {
                        string[] token1 = dec.splitBySemiColon(tokens, 1);
                        string[] cordinates = dec.splitByComma(token1, 1);
                        addPlayer(token1[0].Substring(1), int.Parse(cordinates[0]) * 100, int.Parse(cordinates[1]) * 100, int.Parse(token1[2]));
                    }*/
                    else
                    {
                        int length = tokens.Length - 2- Players_added;
                        for (int i = 0; i < length; i++)
                        {
                            string[] token1 = dec.splitBySemiColon(tokens, 1 + i);
                            string[] cordinates = dec.splitByComma(token1, 1);
                            addPlayer(token1[0].Substring(1), int.Parse(cordinates[0]) * 50, int.Parse(cordinates[1]) * 50, int.Parse(token1[2]));
                            Players_added += 1;
                        }
                    }
                    string[] bricksCordinates = dec.splitBySemiColon(tokens, tokens.Length - 1);
                    for (int i = 0; i < bricksCordinates.Length; i++)
                    {
                        string[] cordinates = dec.splitByComma(bricksCordinates, i);
                        Dean.Bricks brick =  BricksList.Find(item => item.Position == new Point(int.Parse(cordinates[1]), int.Parse(cordinates[0])));
                        brick.DamageLevel = int.Parse(cordinates[2]);
                    }


                }//handle coins
                if (s[0] == 'C')
                {
                    Console.WriteLine("Appearing pile of coins");
                    string[] tokens = dec.splitByColon(s);
                    string[] cordinates = dec.splitByComma(tokens, 1);
                    Console.WriteLine("X: " + cordinates[0] + " Y: " + cordinates[1]);
                    mappArray[int.Parse(cordinates[1].Substring(0, 1)), int.Parse(cordinates[0])] = 5;
                    Dean.CoinPile tempCoinPile = new Dean.CoinPile(int.Parse(cordinates[1].Substring(0, 1)), int.Parse(cordinates[0]));
                    tempCoinPile.LocationX = int.Parse(cordinates[1].Substring(0, 1));
                    tempCoinPile.LocationY = int.Parse(cordinates[0]);
                    tempCoinPile.LifeTime = int.Parse(tokens[2]);
                    tempCoinPile.Price = int.Parse(tokens[3]);
                    coinpilelist.Add(tempCoinPile);
                    maplist.Add(tempCoinPile);
                } //handle lifepacks
                if (s[0] == 'L')
                {
                    Console.WriteLine("Appearing event of LifePack");
                    string[] tokens = dec.splitByColon(s);
                    string[] cordinates = dec.splitByComma(tokens, 1);
                    Console.WriteLine("X: " + cordinates[0] + " Y: " + cordinates[1]);
                    mappArray[int.Parse(cordinates[1].Substring(0, 1)), int.Parse(cordinates[0])] = 6;
                    Dean.LifePack tempLifePack = new Dean.LifePack(int.Parse(cordinates[1].Substring(0, 1)), int.Parse(cordinates[0]));
                    tempLifePack.LocationX = int.Parse(cordinates[1].Substring(0, 1));
                    tempLifePack.LocationY = int.Parse(cordinates[0]);
                    tempLifePack.LifeTime = int.Parse(tokens[2]);
                    lifepacklist.Add(tempLifePack);
                    maplist.Add(tempLifePack);

                }
            }
        }

        private void UpdateMapWithPlayer(Player.MobileSprite player)
        {
            int Lx = (int)player.Position.Y / 50;
            int Ly = (int)player.Position.X / 50;
            if (mappArray[Lx, Ly] == 5)
            {
                Dean.CoinPile pile = coinpilelist.Find(item => item.LocationX == Lx && item.LocationY == Ly);
                coinpilelist.Remove(pile);
                maplist.Remove(pile);
                mappArray[Lx, Ly] = 1;
            }
            if (mappArray[Lx, Ly] == 6)
            {
                Dean.LifePack pile = lifepacklist.Find(item => item.LocationX == Lx && item.LocationY == Ly);
                lifepacklist.Remove(pile);
                maplist.Remove(pile);
                mappArray[Lx, Ly] = 1;
            }
        }
        public int serverReply(String reply)
        {
            switch (reply)
            {
                case "PLAYERS_FULL": Console.WriteLine("Players full"); return 1;
                case "ALREADY_ADDED": Console.WriteLine("Already added"); return 1;
                case "GAME_ALREADY_STARTED": Console.WriteLine("Game already started"); return 1;

                case "INVALID_CELL": Console.WriteLine("Invalid cell"); return 1;
                case "NOT_A_VALID_CONTESTANT": Console.WriteLine("Invalid contestant"); return 1;
                case "TOO_QUICK": Console.WriteLine("Too quick"); return 1;
                case "CELL_OCCUPIED": Console.WriteLine("Cell occupied"); return 1;
                case "OBSTACLE": Console.WriteLine("Obstacle"); return 1;
                case "PITFALL": Console.WriteLine("Pitfall"); return 1;

                case "DEAD": Console.WriteLine("Game finished"); return 1;
                case "GAME_HAS_FINISHED": Console.WriteLine("dead"); return 1;



                default: return 0;
            }
        }

        private void updateMap(string[] tokens, int val, int pos)
        {
            //...split responce by semicolon....
            string[] tokens2 = dec.splitBySemiColon(tokens, pos);

            for (int j = 0; j < tokens2.Length; j++)
            {
                //...split responce by comma....
                string[] cordinates = dec.splitByComma(tokens2, j);
                Console.WriteLine("\nX: " + cordinates[0] + "  Y: " + cordinates[1]);
                mappArray[int.Parse(cordinates[1].Substring(0, 1)),int.Parse(cordinates[0])] = val;
                if(pos == 2){
                    Dean.Bricks brick = new Dean.Bricks(new Point(int.Parse(cordinates[1].Substring(0, 1)), int.Parse(cordinates[0])));
                    brick.DamageLevel = 0;
                    BricksList.Add(brick);
                }
                if (pos == 3)
                {
                    StonesList.Add(new Point(int.Parse(cordinates[1].Substring(0, 1)), int.Parse(cordinates[0])));
                } if (pos == 4)
                {
                    waterList.Add(new Point(int.Parse(cordinates[1].Substring(0, 1)), int.Parse(cordinates[0])));
                }
            }
        
        }

        #endregion

        private void addPlayer(string id, int x, int y, int z)
        {
            Player.MobileSprite p = new Player.MobileSprite(t2dTanks);
            p.Sprite.AddAnimation("leftstop", 0, (int.Parse(id) + 1) * 32, 32, 32, 8, 0.1f);
            p.Sprite.AddAnimation("left", 0, (int.Parse(id) + 1) * 32, 32, 32, 8, 0.1f);
            p.Sprite.AddAnimation("rightstop", 0, (int.Parse(id) + 1) * 32, 32, 32, 8, 0.1f);
            p.Sprite.AddAnimation("right", 0, (int.Parse(id) + 1) * 32, 32, 32, 8, 0.1f);
            p.Sprite.AddAnimation("upstop", 0, (int.Parse(id) + 1) * 32, 32, 32, 8, 0.1f);
            p.Sprite.AddAnimation("up", 0, (int.Parse(id) + 1) * 32, 32, 32, 8, 0.1f);
            p.Sprite.AddAnimation("downstop", 0, (int.Parse(id) + 1) * 32, 32, 32, 8, 0.1f);
            p.Sprite.AddAnimation("down", 0, (int.Parse(id) + 1) * 32, 32, 32, 8, 0.1f);
            p.Sprite.CurrentAnimation = "upstop";
            p.Position = new Vector2(x, y);
            p.Direction = z;
            p.Sprite.AutoRotate = true;
            p.IsPathing = true;
            p.IsMoving = false;
            playerlist.Add(p);
            Console.WriteLine("player added to list");
        }

        private void addBullet(int x, int y, int z)
        {
            Player.MobileSprite bul = new Player.MobileSprite(bullet);
            bul.Sprite.AutoRotate = true;
            bul.Sprite.AddAnimation("leftstop", 0, 0, 6, 6, 1, 0.1f);
            bul.Sprite.CurrentAnimation = "leftstop";
            bul.IsPathing = true;
            bul.Speed = 2;
            bul.Sprite.AutoRotate = true;
            x += 12;
            y += 12;
            bul.Position = new Vector2(x, y);

            if (z == 0)
            {
                bul.AddPathNode(x, 0);
                bul.Target = new Vector2(x, 0);
            }
            if (z == 2)
            {
                bul.AddPathNode(x, 500);
                bul.Target = new Vector2(x, 500);
            }
            if (z == 1)
            {
                bul.AddPathNode(500, y);
                bul.Target = new Vector2(500, y);
            }
            if (z == 3)
            {
                bul.AddPathNode(0, y);
                bul.Target = new Vector2(0, y);
            }

            bulletlist.Add(bul);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Font1 = Content.Load<SpriteFont>("SpriteFont1");                //add fonts 
            Tiles.Content = Content;
            map.Generate(mappArray, 50);
            t2dTanks = Content.Load<Texture2D>("MulticolorTanks");         //add tank texture
            bullet= Content.Load<Texture2D>("PlayerBullet");               //add bullet texture
            BackMark = Content.Load<Texture2D>("back");                    //add background texture
            button_texture =  Content.Load<Texture2D>("button");            //add button texture
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
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

            //Move each player
            foreach (Player.MobileSprite p in playerlist) { p.Update(gameTime); }

            // update mouse variables
            MouseState mouse_state = Mouse.GetState();
            mx = mouse_state.X;
            my = mouse_state.Y;
            prev_mpressed = mpressed;
            mpressed = mouse_state.LeftButton == ButtonState.Pressed;
            update_buttons();

            //keyboard intput
            inputh.Update();
            keyBoradUpdate(gameTime);

            //bullet update
            foreach (Player.MobileSprite  bul in bulletlist.ToList())
            {
              bul.Sprite.CurrentAnimation= "leftstop";
              if (bul.Position == bul.Target) { bul.IsVisible = false;}
              bul.Update(gameTime);
            }

            //Coinpile Update
            if(coinpilelist.Count > 0  && playerlist.Count > 0 ){
            List<Dean.CoinPile> current = coinpilelist;
            foreach (Dean.CoinPile pile in current.ToList())
            {
                if (DateTime.Now >= pile.DisappearTime)
                {
                    Console.WriteLine("here we com " + pile.LocationX + " and " + pile.LocationY);
                    mappArray[pile.LocationX, pile.LocationY] = 1;
                    coinpilelist.Remove(pile);
                    maplist.Remove(pile);
                }
            }
            }

            //Lifepack Update
            if (lifepacklist.Count > 0 && playerlist.Count > 0)
            {
                
                List<Dean.LifePack> current = lifepacklist;
                foreach (Dean.LifePack pile in current.ToList())
                {
                    if (DateTime.Now >= pile.DisappearTime)
                    {
                        Console.WriteLine("here we com " + pile.LocationX + " and " + pile.LocationY);
                        mappArray[pile.LocationX, pile.LocationY] = 1;
                        lifepacklist.Remove(pile);
                        maplist.Remove(pile);
                    }
                }
            }

            //Map update
            map.Generate(mappArray, 50);
            base.Update(gameTime);
        }

        #region ButtonMethods
        // wrapper for hit_image_alpha taking Rectangle and Texture
        Boolean hit_image_alpha(Rectangle rect, Texture2D tex, int x, int y)
        {
            return hit_image_alpha(0, 0, tex, tex.Width * (x - rect.X) /
                rect.Width, tex.Height * (y - rect.Y) / rect.Height);
        }

        // wraps hit_image then determines if hit a transparent part of image 
        Boolean hit_image_alpha(float tx, float ty, Texture2D tex, int x, int y)
        {
            if (hit_image(tx, ty, tex, x, y))
            {
                uint[] data = new uint[tex.Width * tex.Height];
                tex.GetData<uint>(data);
                if ((x - (int)tx) + (y - (int)ty) *
                    tex.Width < tex.Width * tex.Height)
                {
                    return ((data[
                        (x - (int)tx) + (y - (int)ty) * tex.Width
                        ] &
                                0xFF000000) >> 24) > 20;
                }
            }
            return false;
        }

        // determine if x,y is within rectangle formed by texture located at tx,ty
        Boolean hit_image(float tx, float ty, Texture2D tex, int x, int y)
        {
            return (x >= tx &&
                x <= tx + tex.Width &&
                y >= ty &&
                y <= ty + tex.Height);
        }

        // determine state and color of button
        void update_buttons()
        {

                if (hit_image_alpha(
                    button_rectangle, button_texture, mx, my))
                {
                    button_timer = 0.0;
                    if (mpressed)
                    {
                        // mouse is currently down
                        button_state = BState.DOWN;
                        button_color = Color.Blue;
                    }
                    else if (!mpressed && prev_mpressed)
                    {
                        // mouse was just released
                        if (button_state == BState.DOWN)
                        {
                            // button i was just down
                            button_state = BState.JUST_RELEASED;
                        }
                    }
                    else
                    {
                        button_state = BState.HOVER;
                        button_color = Color.Gold;
                    }
                }
                else
                {
                    button_state = BState.UP;
                    if (button_timer > 0)
                    {
                        button_timer = button_timer - frame_time;
                    }
                    else
                    {
                        button_color = Color.White;
                    }
                }

                if (button_state == BState.JUST_RELEASED)
                {
                    take_action_on_button();
                }
        }


        // Logic for each button click goes here
        void take_action_on_button()
        {
            //take action corresponding to which button was clicked
            if (isConnectedToServer == false)
            {
                //send join# request to server to connect with server
                Thread aThread = new Thread(new ThreadStart(ConnectAsClient));
                aThread.Start();

                //get serverresponses
                bThread = new Thread(new ThreadStart(Listen));
                bThread.Start();

                isConnectedToServer = true;
            }
            else
            {
                text = "You are already connected to the server";
            }
            text = "work button";

        }
        #endregion

        #region Player Update Methods
        private void pathFInder()
        {  
            playermovement(pathList[0]);
            Console.WriteLine(pathList[0].Y + " to move from  y " + pathList[0].X);
        }

        //find list of points to nearest mapitem from player current location
        private List<Point> findPathlist()
        {
            List<Point> distanceList = new List<Point>();
            int Lx = (int)playerlist[Player_No].Position.Y / 50;            //current player position in  x direnction
            int Ly = (int)playerlist[Player_No].Position.X / 50;            //current player position in y direction
            Dean.MapItem minimumCoin = FindNearestMapItem(Lx, Ly);
            //find shortest path from player locsation to nearest mapitem
            distanceList = pathFinder.Pathfind(new Point(Lx, Ly), new Point(minimumCoin.LocationX, minimumCoin.LocationY), mappArray);
            if(distanceList.Count == 0)
                distanceList = pathFinder.Pathfind(new Point(Lx, Ly), new Point(maplist[0].LocationX, maplist[0].LocationY), mappArray);
            return distanceList;
        }

        //find nearest mapitem using minimum distance
        private Dean.MapItem FindNearestMapItem(int Lx, int Ly)
        {
            List<Dean.MapItem> current = maplist;
            Dean.MapItem minimumCoin = maplist[0];

            int distance = Math.Abs(Lx - maplist[0].LocationX) + Math.Abs(Ly - maplist[0].LocationY);
            //calculate distance to each mapitem that is currently available
            for (int i = 1; i < current.ToList().Count;i++ )
            {
                int pDistance = Math.Abs(Lx - maplist[i].LocationX) + Math.Abs(Ly - maplist[i].LocationY);
                if (pDistance < distance)
                    minimumCoin = maplist[i];


            }
            return minimumCoin;
        }
        
        //send messages to server regarding player movement
        private void playermovement(Point point)
        {
            int x = point.Y - (int)playerlist[Player_No].Position.X / 50;         //distance along x direction
            int y = point.X - (int)playerlist[Player_No].Position.Y / 50;         //distance along y direction
            if (y > 0)
            {
                if (playerlist[Player_No].Direction != 2)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), dthreadInfo);
                ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), dthreadInfo);
            }
            else if (y < 0)
            {
                if (playerlist[Player_No].Direction != 0)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), uthreadInfo);
                ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), uthreadInfo);
            }
            else if (x > 0)
            {
                if (playerlist[Player_No].Direction != 1)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), rthreadInfo);
                ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), rthreadInfo);
            }
            else if (x < 0)
            {
                if (playerlist[Player_No].Direction != 3)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), lthreadInfo);
                ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), lthreadInfo);
            }
        }

        //send shoot messages to server 
        private void PlayerUpdate(int i)
        {
            if (playerlist.Count > 0)
            {
                Vector2 playerPosition = playerlist[Player_No].Position;
                
                Console.WriteLine("no: " + i + " x : " + playerlist[i].Position.X + " y : " + playerlist[i].Position.Y);
                    if (playerlist[i].Position.X == playerPosition.X && playerPosition.Y < playerlist[i].Position.Y)
                    {
                        if (playerlist[Player_No].Direction != 2)
                            ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), dthreadInfo);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), sthreadInfo);
                        ShootWithBullet((int)playerlist[i].Position.X, (int)playerlist[i].Position.Y, playerlist[Player_No].Direction);
                    }
                    if (playerlist[i].Position.X == playerPosition.X && playerPosition.Y > playerlist[i].Position.Y )
                    {
                        if (playerlist[Player_No].Direction != 0)
                            ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), uthreadInfo);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), sthreadInfo);
                        ShootWithBullet((int)playerlist[i].Position.X, (int)playerlist[i].Position.Y, playerlist[Player_No].Direction);
                    }
                    if (playerlist[i].Position.Y == playerPosition.Y && playerPosition.X > playerlist[i].Position.X )
                    {
                        if (playerlist[Player_No].Direction != 3)
                            ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), lthreadInfo);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), sthreadInfo);
                        ShootWithBullet((int)playerlist[i].Position.X, (int)playerlist[i].Position.Y, playerlist[Player_No].Direction);
                    }
                    if (playerlist[i].Position.Y == playerPosition.Y && playerPosition.X < playerlist[i].Position.X)
                    {
                        if (playerlist[Player_No].Direction != 1)
                            ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), rthreadInfo);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(handleCommand), sthreadInfo);
                        ShootWithBullet((int)playerlist[i].Position.X, (int)playerlist[i].Position.Y, playerlist[Player_No].Direction);

                    }
                    
                }
            
        }

        //add bullets
        private void ShootWithBullet(int x,int y, int z)
        {
            Player.MobileSprite bul = new Player.MobileSprite(bullet);
            bul.Sprite.AutoRotate = true;
            bul.Sprite.AddAnimation("leftstop", 0, 0, 6, 6, 1, 0.1f);
            bul.Sprite.CurrentAnimation = "leftstop";
            bul.IsPathing = true;
            bul.Speed = 2;
            bul.Sprite.AutoRotate = true;

            bul.Position = new Vector2(playerlist[Player_No].Position.X + 12, playerlist[Player_No].Position.Y + 12);
            x += 12;
            y += 12;
            bul.AddPathNode(x, y);
            bul.Target = new Vector2(x, y);
            bulletlist.Add(bul);
        }

        #endregion


        #region Keboard Update Methods
        public void keyBoradUpdate(GameTime gameTime)
        {
            if (playerlist.Count > 0)
            {
                Player.MobileSprite gameTank = playerlist[Player_No];
                if (inputh.IsNewPress(Keys.Left))
                {
                    Thread myNewThread = new Thread(() => Command("LEFT#"));
                    myNewThread.Start();
                    if (gameTank.Sprite.CurrentAnimation != "left")
                    {
                        gameTank.Sprite.CurrentAnimation = "left";
                    }
                    gameTank.Sprite.MoveBy(-8, 0);
                }

                if (inputh.IsNewPress(Keys.Right))
                {
                    Thread myNewThread = new Thread(() => Command("RIGHT#"));
                    myNewThread.Start();
                    if (gameTank.Sprite.CurrentAnimation != "right")
                    {
                        gameTank.Sprite.CurrentAnimation = "right";
                    }
                    gameTank.Sprite.MoveBy(8, 0);
                }
                if (inputh.IsNewPress(Keys.Up))
                {
                    Thread myNewThread = new Thread(() => Command("UP#"));
                    myNewThread.Start();
                    if (gameTank.Sprite.CurrentAnimation != "up")
                    {
                        gameTank.Sprite.CurrentAnimation = "up";
                    }
                    gameTank.Sprite.MoveBy(0, -8);
                }
                if (inputh.IsNewPress(Keys.Down))
                {
                    Thread myNewThread = new Thread(() => Command("DOWN#"));
                    myNewThread.Start();
                    if (gameTank.Sprite.CurrentAnimation != "down")
                    {
                        gameTank.Sprite.CurrentAnimation = "down";
                    }
                    gameTank.Sprite.MoveBy(0, 8);
                }

                gameTank.Update(gameTime);
            }
        }
        #endregion

        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            map.draw(spriteBatch);                                  //draw map
            foreach (Player.MobileSprite p in playerlist)
            {
                p.Draw(spriteBatch);                                //draw each player
            }
            foreach (Player.MobileSprite bul in bulletlist)
                bul.Draw(spriteBatch);                             //draw bullets
            drawMarks();
            spriteBatch.End();
            base.Draw(gameTime);
        }

        //draw texts
        private void drawMarks()
        {
            spriteBatch.Draw(BackMark,new Rectangle(0,500,500,50),Color.AliceBlue);
            spriteBatch.Draw(button_texture, button_rectangle, button_color);
            spriteBatch.DrawString(Font1, "Connect", new Vector2(20, 510), Color.DarkGreen);
            spriteBatch.DrawString(Font1, text, new Vector2(200, 510), Color.Gold);
            spriteBatch.DrawString(Font1, "Status: ", new Vector2(125, 510), Color.Gold);
            spriteBatch.DrawString(Font1, "Points: ", new Vector2(325, 503), Color.Gold);
            spriteBatch.DrawString(Font1, "Coins : ", new Vector2(325, 518), Color.Gold);
            spriteBatch.DrawString(Font1, coins, new Vector2(425, 503), Color.Gold);
            spriteBatch.DrawString(Font1, points, new Vector2(425, 518), Color.Gold);
        }
        #endregion

    }
}
