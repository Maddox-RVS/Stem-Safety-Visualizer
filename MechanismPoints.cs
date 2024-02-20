using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StemSolvers
{
    internal class MechanismPoints
    {
        private const float DEGREES_TO_RADIANS = (float)Math.PI / 180.0f;
        private const float RADIANS_TO_DEGREES = 180.0f / (float)Math.PI;

        public Vector2 wristAxelPoint;
        public Vector2 umbrellaBottomRightPoint;
        public Vector2 umbrellaTopRightPoint;
        public Vector2 umbrellaBottomLeftPoint;
        public Vector2 umbrellaTopLeftPoint;

        private Robot robot;

        public MechanismPoints(RoboState state, Robot robot)
        {
            //Find the wrist end point and wrist axel point using vectors
            Vector2 wristAxelPoint = new Vector2(
                state.getTelescopePixels() * (float)Math.Cos(state.getPivotDegrees() * DEGREES_TO_RADIANS),
                state.getTelescopePixels() * (float)Math.Sin(state.getPivotDegrees() * DEGREES_TO_RADIANS));

            Vector2 wristVector = new Vector2(
                robot.getUmbrellaLength() * (float)Math.Cos((state.getWristDegrees() * DEGREES_TO_RADIANS) - (state.getPivotDegrees() * DEGREES_TO_RADIANS)),
                robot.getUmbrellaLength() * -(float)Math.Sin((state.getWristDegrees() * DEGREES_TO_RADIANS) - (state.getPivotDegrees() * DEGREES_TO_RADIANS)));

            //-----------------------------------------------
            // temporary - because of different axis in game world vs math world
            wristAxelPoint.X += robot.getTelescopeRect().X;
            wristAxelPoint.Y = -wristAxelPoint.Y;
            wristAxelPoint.Y += robot.getTelescopeRect().Y;
            wristVector.Y *= -1.0f;
            //-----------------------------------------------

            Vector2 umbrellaBottomLeftPoint = new Vector2(
                robot.getWristOffsetLength() * (float)Math.Sin((robot.getWristDegrees() * DEGREES_TO_RADIANS) - (robot.getPivotDegrees() * DEGREES_TO_RADIANS)),
                robot.getWristOffsetLength() * (float)Math.Cos((robot.getWristDegrees() * DEGREES_TO_RADIANS) - (robot.getPivotDegrees() * DEGREES_TO_RADIANS)));
            //-----------------------------------------------
            // temporary
            umbrellaBottomLeftPoint.Y *= -1.0f;
            //-----------------------------------------------
            umbrellaBottomLeftPoint += wristAxelPoint;

            Vector2 umbrellaBottomRightPoint = umbrellaBottomLeftPoint + wristVector;

            Vector2 umbrellaTopRightPoint = new Vector2(
                robot.getUmbrellaHeight() * (float)Math.Sin((robot.getWristDegrees() * DEGREES_TO_RADIANS) - (robot.getPivotDegrees() * DEGREES_TO_RADIANS)),
                robot.getUmbrellaHeight() * (float)Math.Cos((robot.getWristDegrees() * DEGREES_TO_RADIANS) - (robot.getPivotDegrees() * DEGREES_TO_RADIANS)));
            //-----------------------------------------------
            // temporary
            umbrellaTopRightPoint.Y *= -1.0f;
            //-----------------------------------------------
            umbrellaTopRightPoint += umbrellaBottomRightPoint;


            Vector2 umbrellaTopLeftPoint = new Vector2(
                robot.getUmbrellaHeight() * (float)Math.Sin((robot.getWristDegrees() * DEGREES_TO_RADIANS) - (robot.getPivotDegrees() * DEGREES_TO_RADIANS)),
                robot.getUmbrellaHeight() * (float)Math.Cos((robot.getWristDegrees() * DEGREES_TO_RADIANS) - (robot.getPivotDegrees() * DEGREES_TO_RADIANS)));
            //-----------------------------------------------
            // temporary
            umbrellaTopLeftPoint.Y *= -1.0f;
            //-----------------------------------------------
            umbrellaTopLeftPoint += umbrellaBottomLeftPoint;

            this.wristAxelPoint = wristAxelPoint;
            this.umbrellaBottomRightPoint = umbrellaBottomRightPoint;
            this.umbrellaTopRightPoint = umbrellaTopRightPoint;
            this.umbrellaTopLeftPoint = umbrellaTopLeftPoint;
            this.umbrellaBottomLeftPoint = umbrellaBottomLeftPoint;
        }
    }
}
