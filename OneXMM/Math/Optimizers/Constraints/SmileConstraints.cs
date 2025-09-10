using System;

namespace OneXMM.Math.Optimizers.Constraints
{
    public abstract class SmileConstraints
    {
        public abstract bool CheckSmileParameters(double[] smileParameters);
    }
}
