using System;

namespace OneXMM.Math.Optimizers.Constraints
{
    public sealed class SABRConstraints : SmileConstraints
    {
        public override bool CheckSmileParameters(double[] smileParameters)
        {
            return smileParameters[3] >= -1 && smileParameters[3] <= 1 && smileParameters[2] > 0 && smileParameters[2] <= 1;
        }
    }
}
