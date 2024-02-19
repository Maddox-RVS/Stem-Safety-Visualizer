using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace StemSolvers
{
    internal class StateTransitionHandler
    {
        private const float DEGREES_TO_RADIANS = (float) Math.PI / 180.0f;
        private const float RADIANS_TO_DEGREES = 180.0f / (float) Math.PI;

        private float stemLength;
        private float pivotRadians;
        private float wristLength;
        private float wristRadians;
        private Rectangle driveBaseRectangle;
        private RoboState targetState;
        private Robot robot;

        public StateTransitionHandler(Robot robot)
        {
            this.robot = robot;
            this.targetState = new RoboState(robot.getPivotDegrees(), robot.getWristDegrees(), robot.getTelescopePixels());
            updateRobotMechDimensions();
        }

        public void updateRobotMechDimensions()
        {
            this.stemLength = robot.getTelescopePixels();
            this.pivotRadians = robot.getPivotDegrees() * DEGREES_TO_RADIANS;
            this.wristLength = robot.getWristLength() * 0.8f;
            this.wristRadians = robot.getWristDegrees() * DEGREES_TO_RADIANS;
            this.driveBaseRectangle = robot.getDriveBaseRect();
        }

        public void transitionTo(RoboState state)
        {
            if (isValidState(state))
                targetState = state;
        }

        private float getLineIntercept(Vector2 pt1, Vector2 pt2, float y)
        {
            float m = (pt2.Y - pt1.Y) / (pt2.X - pt1.X);
            float b = (pt2.Y - (m * pt2.X)) / m;
            return (y - (m * b)) / m;
        }

        private bool isValidState(RoboState state)
        {
            //Find the wrist end point and wrist axel point using vectors
            Vector2 wristAxelPoint = new Vector2(
                state.getTelescopePixels() * (float)Math.Cos(state.getPivotDegrees() * DEGREES_TO_RADIANS),
                state.getTelescopePixels() * (float)Math.Sin(state.getPivotDegrees() * DEGREES_TO_RADIANS));

            Vector2 wristVector = new Vector2(
                wristLength * (float)Math.Cos((state.getWristDegrees() * DEGREES_TO_RADIANS) - (state.getPivotDegrees() * DEGREES_TO_RADIANS)),
                wristLength * -(float)Math.Sin((state.getWristDegrees() * DEGREES_TO_RADIANS) - (state.getPivotDegrees() * DEGREES_TO_RADIANS)));

             //-----------------------------------------------
            // temporary - because of different axis in game world vs math world
            wristAxelPoint.X += robot.getTelescopeRect().X; 
            wristAxelPoint.Y = -wristAxelPoint.Y;
            wristAxelPoint.Y += robot.getTelescopeRect().Y;
            wristVector.Y *= -1.0f;
            //-----------------------------------------------

            Vector2 wristEndPoint = wristAxelPoint + wristVector;
            
            float wristDriveBaseTopIntercept = getLineIntercept(wristAxelPoint, wristEndPoint, driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
            float wristDriveBaseBottomIntercept = getLineIntercept(wristAxelPoint, wristEndPoint, driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2));

            float telescopeDriveBaseTopIntersect = getLineIntercept(new Vector2(robot.getTelescopeRect().X, robot.getTelescopeRect().Y), wristAxelPoint, driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
            float telescopeDriveBaseBottomIntersect = getLineIntercept(new Vector2(robot.getTelescopeRect().X, robot.getTelescopeRect().Y), wristAxelPoint, driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2));

            bool topWristIntInBase = (wristDriveBaseTopIntercept <= driveBaseRectangle.Right - (driveBaseRectangle.Width / 2) && wristDriveBaseTopIntercept >= driveBaseRectangle.Left - (driveBaseRectangle.Width / 2));
            bool bottomWristIntInBase = (wristDriveBaseBottomIntercept <= driveBaseRectangle.Right - (driveBaseRectangle.Width / 2) && wristDriveBaseBottomIntercept >= driveBaseRectangle.Left - (driveBaseRectangle.Width / 2));
            bool isWristEndPointBelowBase = wristEndPoint.Y >= driveBaseRectangle.Top - (driveBaseRectangle.Height / 2);

            bool topTeleIntInBase = (telescopeDriveBaseTopIntersect <= driveBaseRectangle.Right - (driveBaseRectangle.Width / 2) && telescopeDriveBaseTopIntersect >= driveBaseRectangle.Left - (driveBaseRectangle.Width / 2));
            bool bottomTeleIntInBase = (telescopeDriveBaseBottomIntersect <= driveBaseRectangle.Right - (driveBaseRectangle.Width / 2) && telescopeDriveBaseBottomIntersect >= driveBaseRectangle.Left - (driveBaseRectangle.Width / 2));
            bool isTeleEndPointBelowBase = wristAxelPoint.Y >= driveBaseRectangle.Top - (driveBaseRectangle.Height / 2);

            if ((isWristEndPointBelowBase && (topWristIntInBase || bottomWristIntInBase)) || (isTeleEndPointBelowBase && (topTeleIntInBase || bottomTeleIntInBase)))
                return false;

            return true;
        }

        public void update()
        {
            updateRobotMechDimensions();

            RoboState currentState = new RoboState(robot.getPivotDegrees(), robot.getWristDegrees(), robot.getTelescopePixels());

            bool pivotMovementValid = isValidState(new RoboState(targetState.getPivotDegrees(), robot.getWristDegrees(), robot.getTelescopePixels()));
            bool wristMovementValid = isValidState(new RoboState(robot.getPivotDegrees(), targetState.getWristDegrees(), robot.getTelescopePixels()));
            bool telescopeMovementValid = isValidState(new RoboState(robot.getPivotDegrees(), robot.getWristDegrees(), targetState.getTelescopePixels()));

            float midStatePivotDegrees = pivotMovementValid ? targetState.getPivotDegrees() : currentState.getPivotDegrees();
            float midStateWristDegrees = wristMovementValid ? targetState.getWristDegrees() : currentState.getWristDegrees();
            float midStateTelescopePixels = telescopeMovementValid ? targetState.getTelescopePixels() : currentState.getTelescopePixels();

            if (!pivotMovementValid)
            {
                RoboState invalidState = new RoboState(targetState.getPivotDegrees(), robot.getWristDegrees(), robot.getTelescopePixels());

                //Find the wrist end point and wrist axel point using vectors
                Vector2 wristAxelVector = new Vector2(
                    invalidState.getTelescopePixels() * (float)Math.Cos(invalidState.getPivotDegrees() * DEGREES_TO_RADIANS),
                    invalidState.getTelescopePixels() * (float)Math.Sin(invalidState.getPivotDegrees() * DEGREES_TO_RADIANS));

                Vector2 wristVector = new Vector2(
                    wristLength * (float)Math.Cos((invalidState.getWristDegrees() * DEGREES_TO_RADIANS) - (invalidState.getPivotDegrees() * DEGREES_TO_RADIANS)),
                    wristLength * -(float)Math.Sin((invalidState.getWristDegrees() * DEGREES_TO_RADIANS) - (invalidState.getPivotDegrees() * DEGREES_TO_RADIANS)));

                //-----------------------------------------------
                // temporary - because of different axis in game world vs math world
                wristAxelVector.X += robot.getTelescopeRect().X; 
                wristAxelVector.Y = -wristAxelVector.Y;
                wristAxelVector.Y += robot.getTelescopeRect().Y;
                wristVector.Y *= -1.0f;
                //-----------------------------------------------

                Vector2 wristEndPoint = wristAxelVector + wristVector;

                Debug.WriteLine(wristEndPoint.Y);
                Debug.WriteLine(robot.getDriveBaseRect().Top - (robot.getDriveBaseRect().Height / 2));

                float difference = Math.Abs(wristEndPoint.Y - (robot.getDriveBaseRect().Top - (robot.getDriveBaseRect().Height / 2)));

                Debug.WriteLine(difference);
                Debug.WriteLine(robot.getTelescopePixels());
                Debug.WriteLine(Math.Sin(targetState.getPivotDegrees()));

                float opposite = (robot.getTelescopePixels() * (float) Math.Sin(invalidState.getPivotDegrees() * DEGREES_TO_RADIANS)) + difference;

                Debug.WriteLine(opposite);
                Debug.WriteLine(robot.getTelescopePixels());

                midStatePivotDegrees = (float) (Math.Asin(opposite / robot.getTelescopePixels())) * RADIANS_TO_DEGREES;

                Debug.WriteLine(midStatePivotDegrees + "\n");
            }

            RoboState midState = new RoboState(midStatePivotDegrees, midStateWristDegrees, midStateTelescopePixels);

            robot.moveToState(midState);
        }

        public void debugDraw(SpriteBatch spriteBatch)
        {
            Vector2 wristAxelVector = new Vector2(
                stemLength * (float)Math.Cos(pivotRadians),
                stemLength * (float)Math.Sin(pivotRadians));

            //-----------------------------------------------
            // temporary
            wristAxelVector.X += robot.getTelescopeRect().X; 
            wristAxelVector.Y = -wristAxelVector.Y;
            wristAxelVector.Y += robot.getTelescopeRect().Y;
            //-----------------------------------------------

            Vector2 wristVector = new Vector2(
                wristLength * (float)Math.Cos(wristRadians - pivotRadians),
                wristLength * -(float)Math.Sin(wristRadians - pivotRadians));

            //-----------------------------------------------
            // temporary
            wristVector.Y *= -1.0f;
            //-----------------------------------------------

            Vector2 wristEndPoint = wristAxelVector + wristVector;
            
            float wristDriveBaseTopIntercept = getLineIntercept(wristAxelVector, wristEndPoint, driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
            float wristDriveBaseBottomIntercept = getLineIntercept(wristAxelVector, wristEndPoint, driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2)); 

            float telescopeDriveBaseTopIntersect = getLineIntercept(new Vector2(robot.getTelescopeRect().X, robot.getTelescopeRect().Y), wristAxelVector, driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
            float telescopeDriveBaseBottomIntersect = getLineIntercept(new Vector2(robot.getTelescopeRect().X, robot.getTelescopeRect().Y), wristAxelVector, driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2));

            //------------------------------------------------------------------------------------------------------------------//

            Texture2D texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            texture.SetData(new[] { Color.White });

            Vector2 topWristPos = new Vector2(wristDriveBaseTopIntercept, driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
            Vector2 bottomWristPos = new Vector2(wristDriveBaseBottomIntercept,driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2));

            Vector2 topTelePos = new Vector2(telescopeDriveBaseTopIntersect, driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
            Vector2 bottomTelePos = new Vector2(telescopeDriveBaseBottomIntersect,driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2));

            spriteBatch.Draw(texture, new Rectangle((int) wristAxelVector.X, (int) wristAxelVector.Y, 5, 5), Color.OrangeRed);
            spriteBatch.Draw(texture, new Rectangle((int) wristEndPoint.X, (int) wristEndPoint.Y, 5, 5), Color.OrangeRed);
            spriteBatch.Draw(texture, new Rectangle((int) robot.getTelescopeRect().X, robot.getTelescopeRect().Y, 5, 5), Color.OrangeRed);

            spriteBatch.Draw(texture, new Rectangle((int)topWristPos.X - 2, (int)topWristPos.Y - 2, 5, 5), Color.Black);
            spriteBatch.Draw(texture, new Rectangle((int)bottomWristPos.X - 2, (int)bottomWristPos.Y - 5, 5, 5), Color.Black);

            spriteBatch.Draw(texture, new Rectangle((int) topTelePos.X - 2, (int) topTelePos.Y - 2, 5, 5), Color.Black);
            spriteBatch.Draw(texture, new Rectangle((int) bottomTelePos.X - 2, (int) bottomTelePos.Y - 5, 5, 5), Color.Black);

            spriteBatch.DrawString(Game1.debugFont, "wristAxelVector: " + wristAxelVector.ToString(), new Vector2(10, 90), Color.LimeGreen);
            spriteBatch.DrawString(Game1.debugFont, "wristVector: " + wristVector.ToString(), new Vector2(10, 110), Color.LimeGreen);
            spriteBatch.DrawString(Game1.debugFont, "wristEndPoint: " + wristEndPoint.ToString(), new Vector2(10, 130), Color.LimeGreen);

            spriteBatch.DrawString(Game1.debugFont, "Top: " + (driveBaseRectangle.Top - (driveBaseRectangle.Height / 2)).ToString(), new Vector2(10, 150), Color.LimeGreen);
            spriteBatch.DrawString(Game1.debugFont, "Bottom: " + (driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2)).ToString(), new Vector2(10, 170), Color.LimeGreen);
            spriteBatch.DrawString(Game1.debugFont, "Right: " + (driveBaseRectangle.Right - (driveBaseRectangle.Width / 2)).ToString(), new Vector2(10, 190), Color.LimeGreen);
            spriteBatch.DrawString(Game1.debugFont, "Left: " + (driveBaseRectangle.Left - (driveBaseRectangle.Width / 2)).ToString(), new Vector2(10, 210), Color.LimeGreen);

            spriteBatch.DrawString(Game1.debugFont, "State Valid: " + isValidState(new RoboState(robot.getPivotDegrees(), robot.getWristDegrees(), robot.getTelescopePixels())).ToString(), new Vector2(10, 230), Color.LimeGreen);

            spriteBatch.DrawString(Game1.debugFont, "Top-Int: " + topWristPos.ToString(), new Vector2(10, 250), Color.LimeGreen);
            spriteBatch.DrawString(Game1.debugFont, "Bottom-Int: " + bottomWristPos.ToString(), new Vector2(10, 270), Color.LimeGreen);
        }
    }
}
