using System;
using NUnit.Framework;
using OneXMM.Math.Extrapolators;
using OneXMM.Math.Interpolators;
using OneXMM.Pricing.Curves;

namespace OneXMM._UnitTests.Curves
{
    [TestFixture]
    internal class CurveTests
    {
        [Test]
        public void RateInterpolationTest()
        {
            double[] tenors = [0.0, 1.0 / 365, 7.0 / 365, 14.0 / 365, 30.0 / 365];
            double[] rates = [0.0, 0.08, 0.085, 0.09, 0.088];

            var curve = new Curve<PiecewiseLinearRateInterpolator<FlatRateExtrapolator, LinearRateExtrapolator>>(DateTime.Now, tenors, rates, RateInterpolationTypes.Rates);

            Assert.That(curve.GetForwardYield(7.0 / 365), Is.EqualTo(0.085));
            Assert.That(curve.GetForwardYield(0.5 * (7.0 + 14.0) / 365), Is.EqualTo(0.0875));
        }
    }
}
