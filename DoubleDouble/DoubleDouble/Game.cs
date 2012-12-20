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

namespace DoubleDouble
{
    public enum BlockType { RED = 0, GREEN, BLUE, YELLOW }
    public enum GameState { Menu = 0, Running, Paused, GameOver }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        public static GraphicsDeviceManager graphics;
        public static SpriteBatch sb;

        public static Dictionary<String, Texture2D> tex;
        public static Dictionary<String, SpriteFont> font;
        public static Dictionary<String, SoundEffect> sfx;

        public static List<Animation> anims;
        public static List<Label> labels;

        public static KeyboardState oldKS, newKS;
        public static MouseState oldMS, newMS;
        public static GamePadState oldGPS, newGPS;
        public static Point mPoint;
        public static Vector2 mVec;

        Random rand;

        bool DEBUG = false;

        //sound settings
        public static bool MUSIC = false;
        public static bool SOUNDFX = true;
        public static float sfxVolume = 0.15f;
        public static float bgmVolume = 0.5f;

        GameState gState = GameState.Menu;

        BlockGrid bGrid;

        BlockType nextClear;
        TimeSpan clearTimer;
        TimeSpan clearTime;
        TimeSpan speed;

        public static Label lPause, lTitle, lMenu1, lMenu2, lStart, lScore, lNext, lGameOver;

        public static int score;

        public static Color[] blockColor = { Color.DarkOrange, Color.Lime, Color.CornflowerBlue, Color.Yellow };
        public static string[] blockTex = { "block1", "block2", "block3", "block4" };
        public static string[] blockName = { "Eye of Newt", "Adder's Fork", "Tooth of Wolf", "Lizard's Leg" };

        public Game()
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
            oldKS = Keyboard.GetState();
            oldMS = Mouse.GetState();
            oldGPS = GamePad.GetState(0);

            rand = new Random();

            InitGraphics();

            Window.Title = "Double, Double!";

            base.Initialize();
        }

        void InitGraphics()
        {
            //set window size
            Point windowSize = new Point(1000, 800);
            graphics.PreferredBackBufferWidth = windowSize.X;
            graphics.PreferredBackBufferHeight = windowSize.Y;
            graphics.ApplyChanges();

            //show mouse cursor
            base.IsMouseVisible = true;

            tex = new Dictionary<string, Texture2D>();
            font = new Dictionary<string, SpriteFont>();
            sfx = new Dictionary<string, SoundEffect>();

            anims = new List<Animation>();
            labels = new List<Label>();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            sb = new SpriteBatch(GraphicsDevice);

            tex["background"] = Content.Load<Texture2D>("background2");
            tex["block"] = Content.Load<Texture2D>("block");
            tex["cursor"] = Content.Load<Texture2D>("cursor2");
            tex["pop"] = Content.Load<Texture2D>("ani_pop");
            tex["blank"] = Content.Load<Texture2D>("blank");
            tex["block1"] = Content.Load<Texture2D>("block1");
            tex["block2"] = Content.Load<Texture2D>("block2");
            tex["block3"] = Content.Load<Texture2D>("block3");
            tex["block4"] = Content.Load<Texture2D>("block4");

            font["debug"] = Content.Load<SpriteFont>("font_uh20");
            font["label"] = Content.Load<SpriteFont>("font_labels");
            font["number"] = Content.Load<SpriteFont>("font_numbers");
            font["title"] = Content.Load<SpriteFont>("font_title");

            sfx["bump"] = Content.Load<SoundEffect>("bump");
            sfx["stir"] = Content.Load<SoundEffect>("stir");
            sfx["clear"] = Content.Load<SoundEffect>("clear");
            sfx["lose"] = Content.Load<SoundEffect>("lose");
            sfx["start"] = Content.Load<SoundEffect>("start");

            lPause = new Label(GraphicsDevice.Viewport.Bounds.Center.toVector2(), "PAUSED");

            lTitle = new Label(new Vector2(GraphicsDevice.Viewport.Bounds.Width / 2, 300), "Double,\nDouble!", -1, "title");
            lMenu1 = new Label(new Vector2(GraphicsDevice.Viewport.Bounds.Width / 2, 450), "Move with arrows, stir blocks with A and D");
            lMenu2 = new Label(new Vector2(GraphicsDevice.Viewport.Bounds.Width / 2, 550), "Match 4, 8, 16 or 32 blocks in rectangles\n     at the proper time to clear them!");
            lStart = new Label(new Vector2(GraphicsDevice.Viewport.Bounds.Width / 2, 700), "Press SPACE to Start!");

            lScore = new Label(new Vector2(GraphicsDevice.Viewport.Bounds.Width / 2 + 300, 40), "Score: 0", -1);
            lNext = new Label(new Vector2(GraphicsDevice.Viewport.Bounds.Width / 2 - 200, 40), "Next: COLOR in 10s");

            lGameOver = new Label(GraphicsDevice.Viewport.Bounds.Center.toVector2(), "Game Over!");
            //NewGame();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        void NewGame()
        {
            bGrid = new BlockGrid(new Point(8, 12), new Point(300, 125));

            speed = TimeSpan.FromSeconds(10);
            clearTime = TimeSpan.FromSeconds(5) + speed;
            clearTimer = TimeSpan.FromSeconds(0);
            nextClear = (BlockType) rand.Next(4);

            score = 0;
            lScore.SetText("Score: " + score);

            if (SOUNDFX) sfx["start"].Play(sfxVolume, 0, 0);

            gState = GameState.Running;
        }

        void GameOver()
        {
            gState = GameState.GameOver;
        }

        public static void AddScore(int points)
        {
            score += points;
            lScore.SetText("Score: " + score);
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

            UpdateInput(gameTime);

            if (gState == GameState.Running || gState == GameState.GameOver)
            {
                bGrid.Update(gameTime);

                //update and cull animations
                foreach (Animation anim in anims)
                {
                    anim.Update(gameTime);
                }
                for (int i = anims.Count - 1; i >= 0; i--)
                {
                    if (anims[i].killMe) anims.RemoveAt(i);
                }

                foreach (Label l in labels)
                {
                    l.Update(gameTime);
                }
                for (int i = labels.Count - 1; i >= 0; i--)
                {
                    if (labels[i].killMe) labels.RemoveAt(i);
                }
            }

            if (gState == GameState.Running)
            {
                //update color clearing
                clearTimer += gameTime.ElapsedGameTime;
                if (clearTimer > clearTime)
                {
                    clearTimer -= clearTime;

                    AddScore(bGrid.ClearType(nextClear));

                    if (bGrid.AddRow() == false)
                    {
                        //Console.WriteLine("Game Over!!!");

                        if (SOUNDFX) sfx["lose"].Play(sfxVolume, 0, 0);

                        GameOver();
                        //NewGame();
                    }
                    else
                    {
                        //if (SOUNDFX) sfx["clear"].Play(sfxVolume, 0, 0);
                    }

                    nextClear = (BlockType)(((int)nextClear + 1) % 4);

                    //change speed
                    if (speed > TimeSpan.FromSeconds(0))
                    {
                        speed -= TimeSpan.FromSeconds(0.3);
                        clearTime = TimeSpan.FromSeconds(5) + speed;
                    }
                    else
                    {
                        clearTime = TimeSpan.FromSeconds(5);
                    }
                }

                lNext.SetText(String.Format("Clear: {0} in {1}s", blockName[(int)nextClear], (clearTime - clearTimer).Seconds + 1));
            }

            base.Update(gameTime);
        }

        void UpdateInput(GameTime gameTime)
        {
            newKS = Keyboard.GetState();
            newMS = Mouse.GetState();

            //set mouse location in useful formats
            mPoint = new Point(newMS.X, newMS.Y);
            mVec = new Vector2(newMS.X, newMS.Y);

            //check if mouse is within the game window
            if (GraphicsDevice.Viewport.Bounds.Contains(mPoint))
            {
                if (DEBUG && oldMS.LeftButton == ButtonState.Released && newMS.LeftButton == ButtonState.Pressed)
                {
                    Console.WriteLine("Click at {0}, {1}", mPoint.X, mPoint.Y);
                    labels.Add(new Label(BlockGrid.Pos2Vector(3.5f, 2.5f), "Test!", 30));
                    anims.Add(new Animation(BlockGrid.Pos2Vector(3, 3), "pop"));
                }
            }

            if (gState == GameState.Running)
            {
                if (isPressed(Keys.Up)) bGrid.MoveCursor(0, 1);
                else if (isPressed(Keys.Down)) bGrid.MoveCursor(0, -1);

                if (isPressed(Keys.Left)) bGrid.MoveCursor(-1, 0);
                else if (isPressed(Keys.Right)) bGrid.MoveCursor(1, 0);

                if (isPressed(Keys.A)) bGrid.StirBlocks(false);
                else if (isPressed(Keys.D)) bGrid.StirBlocks();
            }

            if (isPressed(Keys.Space))
            {
                if (gState == GameState.Menu)
                {
                    NewGame();
                }
                else if (gState == GameState.Running) gState = GameState.Paused;
                else if (gState == GameState.Paused) gState = GameState.Running;
                else if (gState == GameState.GameOver) gState = GameState.Menu;
            }

            oldKS = newKS;
            oldMS = newMS;
        }

        bool isPressed(Keys key)
        {
            return (oldKS.IsKeyUp(key) && newKS.IsKeyDown(key));
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);

            DrawBackground();

            if (gState == GameState.Menu)
            {
                lTitle.Draw();
                lMenu1.Draw();
                lMenu2.Draw();
                lStart.Draw();
                //TODO: display title
            }
            else if (gState == GameState.Running || gState == GameState.Paused || gState == GameState.GameOver)
            {
                bGrid.DrawBackground();

                bGrid.DrawBlocks();

                bGrid.DrawCursor();

                foreach (Animation anim in anims)
                {
                    anim.Draw();
                }

                foreach (Label l in labels)
                {
                    l.Draw();
                }

                lScore.Draw();
                lNext.Draw(blockColor[(int)nextClear]);

                if (DEBUG)
                {
                    string output = string.Format("Cursor: {0}, {1}  Clear: {2} in {3}s \nBlocks: {4}", BlockGrid.cLoc.X, BlockGrid.cLoc.Y, nextClear, (clearTime - clearTimer).Seconds + 1, BlockGrid.bList.Count);
                    Vector2 FontPos = new Vector2(5, 5);
                    sb.DrawString(font["debug"], output, FontPos, Color.White);
                }
            }

            if (gState == GameState.Paused)
            {
                DrawOverlay();

                lPause.Draw();
            }
            
            if (gState == GameState.GameOver)
            {
                DrawOverlay();

                lGameOver.Draw(new Color(255, 150, 150));
                //TODO: display game over message
            }

            sb.End();

            base.Draw(gameTime);
        }

        void DrawBackground()
        {
            sb.Draw(tex["background"], GraphicsDevice.Viewport.Bounds, Color.White);
        }

        void DrawOverlay()
        {
            sb.Draw(tex["blank"], GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
        }
    }
}
