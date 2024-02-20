using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            Vector2 umbrellaLengthVector = new Vector2(
                robot.getUmbrellaLength() * (float)Math.Cos((180 - (180 - state.getWristDegrees()) - state.getPivotDegrees()) * DEGREES_TO_RADIANS),
                robot.getUmbrellaLength() * -(float)Math.Sin((180 - (180 - state.getWristDegrees()) - state.getPivotDegrees()) * DEGREES_TO_RADIANS));

            wristAxelPoint.X += robot.getTelescopeRect().X;
            wristAxelPoint.Y += robot.getTelescopeRect().Y;

            Vector2 umbrellaBottomLeftPoint = new Vector2(
                robot.getWristOffsetLength() * (float)Math.Cos((state.getPivotDegrees() * DEGREES_TO_RADIANS) + (90 * DEGREES_TO_RADIANS) - (state.getWristDegrees() * DEGREES_TO_RADIANS)),
                robot.getWristOffsetLength() * (float)Math.Sin((state.getPivotDegrees() * DEGREES_TO_RADIANS) + (90 * DEGREES_TO_RADIANS) - (state.getWristDegrees() * DEGREES_TO_RADIANS)));
            umbrellaBottomLeftPoint += wristAxelPoint;

            Vector2 umbrellaBottomRightPoint = umbrellaBottomLeftPoint + umbrellaLengthVector;

            Vector2 umbrellaTopRightPoint = new Vector2(
                robot.getUmbrellaHeight() * (float)Math.Cos((state.getPivotDegrees() * DEGREES_TO_RADIANS) + (90 * DEGREES_TO_RADIANS) - (state.getWristDegrees() * DEGREES_TO_RADIANS)),
                robot.getUmbrellaHeight() * (float)Math.Sin((state.getPivotDegrees() * DEGREES_TO_RADIANS) + (90 * DEGREES_TO_RADIANS) - (state.getWristDegrees() * DEGREES_TO_RADIANS)));
            umbrellaTopRightPoint += umbrellaBottomRightPoint;

            Vector2 umbrellaTopLeftPoint = new Vector2(
                robot.getUmbrellaHeight() * (float)Math.Cos((state.getPivotDegrees() * DEGREES_TO_RADIANS) + (90 * DEGREES_TO_RADIANS) - (state.getWristDegrees() * DEGREES_TO_RADIANS)),
                robot.getUmbrellaHeight() * (float)Math.Sin((state.getPivotDegrees() * DEGREES_TO_RADIANS) + (90 * DEGREES_TO_RADIANS) - (state.getWristDegrees() * DEGREES_TO_RADIANS)));
            umbrellaTopLeftPoint += umbrellaBottomLeftPoint;

            this.wristAxelPoint = wristAxelPoint;
            this.umbrellaBottomRightPoint = umbrellaBottomRightPoint;
            this.umbrellaTopRightPoint = umbrellaTopRightPoint;
            this.umbrellaTopLeftPoint = umbrellaTopLeftPoint;
            this.umbrellaBottomLeftPoint = umbrellaBottomLeftPoint;
        }

        private float getSmallest(params float[] values)
        {
            float smallest = values[0];
            foreach (float value in values)
                if (value < smallest) smallest = value;
            return smallest;
        }

        private float getLargest(params float[] values)
        {
            float largest = values[0];
            foreach (float value in values)
                if (value > largest) largest = value;
            return largest;
        }

        public Rectangle getCastedRect() 
        {
            float umbrellaRectLeft = getSmallest(umbrellaBottomLeftPoint.X, umbrellaBottomRightPoint.X, umbrellaTopLeftPoint.X, umbrellaTopRightPoint.X);
            float umbrellaRectRight = getLargest(umbrellaBottomLeftPoint.X, umbrellaBottomRightPoint.X, umbrellaTopLeftPoint.X, umbrellaTopRightPoint.X);
            float umbrellaRectTop = getSmallest(umbrellaBottomLeftPoint.Y, umbrellaBottomRightPoint.Y, umbrellaTopLeftPoint.Y, umbrellaTopRightPoint.Y); //use get largest for real world one
            float umbrellaRectBottom = getLargest(umbrellaBottomLeftPoint.Y, umbrellaBottomRightPoint.Y, umbrellaTopLeftPoint.Y, umbrellaTopRightPoint.Y); //use get smallest for real world one
            return new Rectangle((int) umbrellaRectLeft, (int) umbrellaRectTop, (int) Math.Abs(umbrellaRectLeft - umbrellaRectRight), (int) Math.Abs(umbrellaRectTop - umbrellaRectBottom));
        }
    }
}
