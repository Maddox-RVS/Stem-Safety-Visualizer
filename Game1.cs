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
                2.0f,
                3.0f,
                5.0f,
                GraphicsDevice);

            stateTransitionHandler = new StateTransitionHandler(robot);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            robot.update();
            stateTransitionHandler.update();

            if (Keyboard.GetState().IsKeyDown(Keys.E)) stateTransitionHandler.transitionTo(new RoboState(-30, -30, 400));
            else if (Keyboard.GetState().IsKeyDown(Keys.A)) stateTransitionHandler.transitionTo(new RoboState(100, 30, 200));
            else if (Keyboard.GetState().IsKeyDown(Keys.D)) stateTransitionHandler.transitionTo(new RoboState(20, 50, 400));
            else if (Keyboard.GetState().IsKeyDown(Keys.W)) stateTransitionHandler.transitionTo(new RoboState(0, 50, 400));
            else if (Keyboard.GetState().IsKeyDown(Keys.S)) stateTransitionHandler.transitionTo(new RoboState(30, 90, 150));

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
            //stateTransitionHandler.debugDraw(spriteBatch);
        }
    }
}