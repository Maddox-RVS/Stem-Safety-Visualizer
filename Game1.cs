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
        public static RenderTarget2D viewPort;

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
            viewPort = new RenderTarget2D(
                GraphicsDevice,
                Game1.screenBounds.Width,
                Game1.screenBounds.Height,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                GraphicsDevice.PresentationParameters.DepthStencilFormat);

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
                new Vector2((screenBounds.Width / 2) - 100, 25),
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

            if (robot.hasReachedTarget() /*&& Keyboard.GetState().IsKeyDown(Keys.Down)*/)
                stateTransitionHandler.transitionTo(new RoboState(rnd.Next(5, 95), rnd.Next(0, 116), rnd.Next(50, 401)));

            //if (Keyboard.GetState().IsKeyDown(Keys.E)) robot.moveToState(new RoboState(127, 154, 123));
            if (Keyboard.GetState().IsKeyDown(Keys.A)) robot.moveToState(new RoboState(22, 0, 440));
            //if (Keyboard.GetState().IsKeyDown(Keys.D)) robot.moveToState(new RoboState(130, 130, 150));
            //if (Keyboard.GetState().IsKeyDown(Keys.S)) robot.moveToState(new RoboState(30, 50, 200));
            //if (Keyboard.GetState().IsKeyDown(Keys.W)) robot.moveToState(new RoboState(80, 50, 200));

            if (Keyboard.GetState().IsKeyDown(Keys.E)) stateTransitionHandler.transitionTo(new RoboState(127, 154, 123));
            //if (Keyboard.GetState().IsKeyDown(Keys.A)) stateTransitionHandler.transitionTo(new RoboState(22, 0, 440));
            //if (Keyboard.GetState().IsKeyDown(Keys.D)) stateTransitionHandler.transitionTo(new RoboState(130, 130, 150));
            //if (Keyboard.GetState().IsKeyDown(Keys.S)) stateTransitionHandler.transitionTo(new RoboState(30, 50, 200));
            //if (Keyboard.GetState().IsKeyDown(Keys.W)) stateTransitionHandler.transitionTo(new RoboState(80, 50, 200));

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.GraphicsDevice.SetRenderTarget(viewPort);
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();

            robot.draw(spriteBatch);
            stateTransitionHandler.debugDraw(spriteBatch);

            spriteBatch.End();
            spriteBatch.GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();

            spriteBatch.Draw(
                ((Texture2D) viewPort),
                Vector2.Zero,
                null,
                Color.White,
                0.0f,
                Vector2.Zero,
                1f,
                SpriteEffects.FlipVertically,
                0.0f);
            drawDebug(spriteBatch);
            stateTransitionHandler.debugInfo(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public void drawDebug(SpriteBatch spriteBatch)
        {
            Vector2 mousePos = new Vector2(Mouse.GetState().Position.X, screenBounds.Height - Mouse.GetState().Position.Y);

            spriteBatch.DrawString(debugFont, "Pivot Degrees: " + robot.getPivotDegrees() % 360, new Vector2(10, 10), Color.LimeGreen);
            spriteBatch.DrawString(debugFont, "Wrist Degrees: " + robot.getWristDegrees() % 360, new Vector2(10, 30), Color.LimeGreen);
            spriteBatch.DrawString(debugFont, "Telescope Pixel Length: " + robot.getTelescopePixels(), new Vector2(10, 50), Color.LimeGreen);
            if (robot.getAllowedBounds().Contains(mousePos))
                spriteBatch.DrawString(debugFont, "Mouse Pos: " + mousePos.ToString(), new Vector2(10, 70), Color.Red);
            else
                spriteBatch.DrawString(debugFont, "Mouse Pos: " + mousePos.ToString(), new Vector2(10, 70), Color.LimeGreen);
        }
    }
}