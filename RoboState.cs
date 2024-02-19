using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StemSolvers
{
    internal class RoboState
    {
        private float pivotDegrees, wristDegrees, telescopePixels;

        public RoboState(float pivotDegrees, float wristDegrees, float telescopePixels)
        {
            this.pivotDegrees = pivotDegrees;
            this.wristDegrees = wristDegrees;
            this.telescopePixels = telescopePixels;
        }

        public float getPivotDegrees()
        {
            return pivotDegrees;
        }
        public float getWristDegrees()
        {
            return wristDegrees;
        }
        public float getTelescopePixels()
        {
            return telescopePixels;
        }
    }
}
