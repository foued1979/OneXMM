using System;
using NUnit.Framework;
using OneXMM.Math.Extrapolators;
using OneXMM.Math.Interpolators;

namespace OneXMM._UnitTests
{
    [TestFixture]
    internal class MathTests
    {
        [Test]
        public void Flat_Flat_RateInterpolationTest()
        {
            var interpolator = new PiecewiseLinearRateInterpolator<FlatRateExtrapolator, FlatRateExtrapolator>(RateInterpolationTypes.Rates);
            interpolator.Set([0, 1, 2, 3], [1, 2, 3, 4]);

            Assert.That(interpolator.Interpolate(-0.5), Is.EqualTo(1));
            Assert.That(interpolator.Interpolate(4), Is.EqualTo(4));

            Assert.That(interpolator.Interpolate(0.5), Is.EqualTo(1.5));
        }
    }
}
