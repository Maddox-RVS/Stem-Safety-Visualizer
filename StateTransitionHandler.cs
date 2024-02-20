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
            this.wristLength = robot.getUmbrellaLength();
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
            MechanismPoints mechPts = new MechanismPoints(state, robot);

            // should be less than, but is opposite because of game world, also should be 0 for floor not whatever is here
            if (mechPts.umbrellaBottomRightPoint.Y >= robot.floorBoundY || mechPts.umbrellaTopRightPoint.Y >= robot.floorBoundY)
                return false;

            float wristDriveBaseTopIntercept = getLineIntercept(mechPts.wristAxelPoint, mechPts.umbrellaBottomRightPoint, driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
            float wristDriveBaseBottomIntercept = getLineIntercept(mechPts.wristAxelPoint, mechPts.umbrellaBottomRightPoint, driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2));

            float telescopeDriveBaseTopIntercept = getLineIntercept(new Vector2(robot.getTelescopeRect().X, robot.getTelescopeRect().Y), mechPts.wristAxelPoint, driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
            float telescopeDriveBaseBottomIntercept = getLineIntercept(new Vector2(robot.getTelescopeRect().X, robot.getTelescopeRect().Y), mechPts.wristAxelPoint, driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2));

            // Vertical/horizontal lines don't have slope, so make them slightly not vertical/horizontal anymore :)
            if (float.IsNaN(wristDriveBaseTopIntercept) ||
                float.IsNaN(wristDriveBaseBottomIntercept))
            {
                wristDriveBaseTopIntercept = getLineIntercept(mechPts.wristAxelPoint, new Vector2(mechPts.umbrellaBottomRightPoint.X + 0.0001f, mechPts.umbrellaBottomRightPoint.Y + 0.0001f), driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
                wristDriveBaseBottomIntercept = getLineIntercept(mechPts.wristAxelPoint, new Vector2(mechPts.umbrellaBottomRightPoint.X + 0.0001f, mechPts.umbrellaBottomRightPoint.Y + 0.0001f), driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2));
            }
            if (float.IsNaN(telescopeDriveBaseTopIntercept) ||
                float.IsNaN(telescopeDriveBaseBottomIntercept))
            {
                telescopeDriveBaseTopIntercept = getLineIntercept(new Vector2(robot.getTelescopeRect().X + 0.0001f, robot.getTelescopeRect().Y + 0.0001f), mechPts.wristAxelPoint, driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
                telescopeDriveBaseBottomIntercept = getLineIntercept(new Vector2(robot.getTelescopeRect().X + 0.0001f, robot.getTelescopeRect().Y + 0.0001f), mechPts.wristAxelPoint, driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2));
            }

            bool topWristIntInBase = (wristDriveBaseTopIntercept <= driveBaseRectangle.Right - (driveBaseRectangle.Width / 2) && wristDriveBaseTopIntercept >= driveBaseRectangle.Left - (driveBaseRectangle.Width / 2));
            bool bottomWristIntInBase = (wristDriveBaseBottomIntercept <= driveBaseRectangle.Right - (driveBaseRectangle.Width / 2) && wristDriveBaseBottomIntercept >= driveBaseRectangle.Left - (driveBaseRectangle.Width / 2));
            bool isWristRightPointsBelowBase = (mechPts.umbrellaBottomRightPoint.Y >= driveBaseRectangle.Top - (driveBaseRectangle.Height / 2)) || mechPts.umbrellaTopRightPoint.Y >= driveBaseRectangle.Top - (driveBaseRectangle.Height / 2);

            bool topTeleIntInBase = (telescopeDriveBaseTopIntercept <= driveBaseRectangle.Right - (driveBaseRectangle.Width / 2) && telescopeDriveBaseTopIntercept >= driveBaseRectangle.Left - (driveBaseRectangle.Width / 2));
            bool bottomTeleIntInBase = (telescopeDriveBaseBottomIntercept <= driveBaseRectangle.Right - (driveBaseRectangle.Width / 2) && telescopeDriveBaseBottomIntercept >= driveBaseRectangle.Left - (driveBaseRectangle.Width / 2));
            bool isTeleEndPointBelowBase = mechPts.wristAxelPoint.Y >= driveBaseRectangle.Top - (driveBaseRectangle.Height / 2);

            if ((isWristRightPointsBelowBase && (topWristIntInBase || bottomWristIntInBase)) || 
                (isTeleEndPointBelowBase && (topTeleIntInBase || bottomTeleIntInBase)) ||
                mechPts.wristAxelPoint.X >= robot.frontWallBoundX ||
                mechPts.umbrellaBottomLeftPoint.X >= robot.frontWallBoundX ||
                mechPts.umbrellaBottomRightPoint.X >= robot.frontWallBoundX ||
                mechPts.umbrellaTopLeftPoint.X >= robot.frontWallBoundX ||
                mechPts.umbrellaTopRightPoint.X >= robot.frontWallBoundX ||
                mechPts.wristAxelPoint.X <= robot.backWallBoundX ||
                mechPts.umbrellaBottomLeftPoint.X <= robot.backWallBoundX ||
                mechPts.umbrellaBottomRightPoint.X <= robot.backWallBoundX ||
                mechPts.umbrellaTopLeftPoint.X <= robot.backWallBoundX ||
                mechPts.umbrellaTopRightPoint.X <= robot.backWallBoundX ||
                mechPts.wristAxelPoint.Y <= robot.roofBoundY ||
                mechPts.umbrellaBottomLeftPoint.Y <= robot.roofBoundY ||
                mechPts.umbrellaBottomRightPoint.Y <= robot.roofBoundY ||
                mechPts.umbrellaTopLeftPoint.Y <= robot.roofBoundY ||
                mechPts.umbrellaTopRightPoint.Y <= robot.roofBoundY) // all the y values should be checked with >= but are opposite because of game world axis
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

                float difference = Math.Abs(wristEndPoint.Y - (robot.getDriveBaseRect().Top - (robot.getDriveBaseRect().Height / 2)));
                float opposite = (robot.getTelescopePixels() * (float) Math.Sin(invalidState.getPivotDegrees() * DEGREES_TO_RADIANS)) + difference;
                midStatePivotDegrees = (float) (Math.Asin(opposite / robot.getTelescopePixels())) * RADIANS_TO_DEGREES;
            }

            RoboState midState = new RoboState(midStatePivotDegrees, midStateWristDegrees, midStateTelescopePixels);

            robot.moveToState(midState);
        }

        public void debugDraw(SpriteBatch spriteBatch)
        {
            MechanismPoints mechPts = new MechanismPoints(new RoboState(robot.getPivotDegrees(), robot.getWristDegrees(), robot.getTelescopePixels()), robot);

            float wristDriveBaseTopIntercept = getLineIntercept(mechPts.wristAxelPoint, mechPts.umbrellaBottomRightPoint, driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
            float wristDriveBaseBottomIntercept = getLineIntercept(mechPts.wristAxelPoint, mechPts.umbrellaBottomRightPoint, driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2)); 

            float telescopeDriveBaseTopIntersect = getLineIntercept(new Vector2(robot.getTelescopeRect().X, robot.getTelescopeRect().Y), mechPts.wristAxelPoint, driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
            float telescopeDriveBaseBottomIntersect = getLineIntercept(new Vector2(robot.getTelescopeRect().X, robot.getTelescopeRect().Y), mechPts.wristAxelPoint, driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2));

            //------------------------------------------------------------------------------------------------------------------//

            Texture2D texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            texture.SetData(new[] { Color.White });

            Vector2 topWristPos = new Vector2(wristDriveBaseTopIntercept, driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
            Vector2 bottomWristPos = new Vector2(wristDriveBaseBottomIntercept,driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2));

            Vector2 topTelePos = new Vector2(telescopeDriveBaseTopIntersect, driveBaseRectangle.Top - (driveBaseRectangle.Height / 2));
            Vector2 bottomTelePos = new Vector2(telescopeDriveBaseBottomIntersect,driveBaseRectangle.Bottom - (driveBaseRectangle.Height / 2));

            spriteBatch.Draw(texture, new Rectangle((int)mechPts.wristAxelPoint.X, (int)mechPts.wristAxelPoint.Y, 5, 5), Color.OrangeRed);
            spriteBatch.Draw(texture, new Rectangle((int)mechPts.umbrellaBottomRightPoint.X, (int)mechPts.umbrellaBottomRightPoint.Y, 5, 5), Color.OrangeRed);
            spriteBatch.Draw(texture, new Rectangle((int)mechPts.umbrellaTopRightPoint.X, (int)mechPts.umbrellaTopRightPoint.Y, 5, 5), Color.OrangeRed);
            spriteBatch.Draw(texture, new Rectangle((int)mechPts.umbrellaTopLeftPoint.X, (int)mechPts.umbrellaTopLeftPoint.Y, 5, 5), Color.OrangeRed);
            spriteBatch.Draw(texture, new Rectangle((int)mechPts.umbrellaBottomLeftPoint.X, (int)mechPts.umbrellaBottomLeftPoint.Y, 5, 5), Color.OrangeRed);

            spriteBatch.Draw(texture, new Rectangle((int)topWristPos.X - 2, (int)topWristPos.Y - 2, 5, 5), Color.Black);
            spriteBatch.Draw(texture, new Rectangle((int)bottomWristPos.X - 2, (int)bottomWristPos.Y - 5, 5, 5), Color.Black);

            spriteBatch.Draw(texture, new Rectangle((int) topTelePos.X - 2, (int) topTelePos.Y - 2, 5, 5), Color.Black);
            spriteBatch.Draw(texture, new Rectangle((int) bottomTelePos.X - 2, (int) bottomTelePos.Y - 5, 5, 5), Color.Black);

            spriteBatch.DrawString(Game1.debugFont, "State Valid: " + isValidState(new RoboState(robot.getPivotDegrees(), robot.getWristDegrees(), robot.getTelescopePixels())).ToString(), new Vector2(10, 230), Color.LimeGreen);

            spriteBatch.DrawString(Game1.debugFont, "Top-Int: " + topWristPos.ToString(), new Vector2(10, 250), Color.LimeGreen);
            spriteBatch.DrawString(Game1.debugFont, "Bottom-Int: " + bottomWristPos.ToString(), new Vector2(10, 270), Color.LimeGreen);
        }
    }
}
