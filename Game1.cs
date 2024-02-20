using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.ComponentModel;

namespace StemSolvers
{
    public class Game1 : Game
    {
        private const float DEGREES_TO_RADIANS = (float) Math.PI / 180.0f;
        private const float RADIANS_TO_DEGREES = 180.0f / (float) Math.PI;

        private GraphicsDeviceManager graphics;
        public static Rectangle screenBounds;
        public static SpriteFont debugFont;
        private SpriteBatch spriteBatch;
        private Robot robot;
        private StateTransitionHandler stateTransitionHandler;
        private Random rnd = new Random();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreparingDeviceSettings += (object s, PreparingDeviceSettingsEventArgs args) =>
            {
                args.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            };
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 500;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            screenBounds = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            debugFont = Content.Load<SpriteFont>("debugFont");

            Texture2D texture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            texture.SetData(new[] { Color.White });

            robot = new Robot(
                texture,
                new Vector2((screenBounds.Width / 2) - 100, screenBounds.Height - 25),
                20.0f,
                30.0f,
                50.0f,
                GraphicsDevice);

            stateTransitionHandler = new StateTransitionHandler(robot);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            robot.update();
            stateTransitionHandler.update();

            if (robot.hasReachedTarget() && Keyboard.GetState().IsKeyDown(Keys.Down))
                stateTransitionHandler.transitionTo(new RoboState(rnd.Next(1, 361), rnd.Next(0, 181), rnd.Next(50, 401)));

            //if (Keyboard.GetState().IsKeyDown(Keys.E)) robot.moveToState(new RoboState(124, 149, 59));
            if (Keyboard.GetState().IsKeyDown(Keys.E)) stateTransitionHandler.transitionTo(new RoboState(124, 149, 59));
            else if (Keyboard.GetState().IsKeyDown(Keys.D)) stateTransitionHandler.transitionTo(new RoboState(20, 0, 420));

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();

            robot.draw(spriteBatch);

            drawDebug(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public void drawDebug(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(debugFont, "Pivot Degrees: " + robot.getPivotDegrees() % 360, new Vector2(10, 10), Color.LimeGreen);
            spriteBatch.DrawString(debugFont, "Wrist Degrees: " + robot.getWristDegrees() % 360, new Vector2(10, 30), Color.LimeGreen);
            spriteBatch.DrawString(debugFont, "Telescope Pixel Length: " + robot.getTelescopePixels(), new Vector2(10, 50), Color.LimeGreen);
            spriteBatch.DrawString(debugFont, "Mouse Pos: " + Mouse.GetState().Position.ToString(), new Vector2(10, 70), Color.LimeGreen);
            stateTransitionHandler.debugDraw(spriteBatch);
        }
    }
}