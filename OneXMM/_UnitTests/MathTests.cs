using NUnit.Framework;
using NUnit.Framework.Internal;
using OneXMM.Interfaces;
using OneXMM.Math.Distributions;
using OneXMM.Math.Extrapolators;
using OneXMM.Math.Interpolators;
using OneXMM.Math.Models;
using OneXMM.Math.Models.Smiles;
using OneXMM.Pricing.Curves;
using System;

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

        [Test]
        public void CustomDistributionTest()
        {
            var smile = new SABR(DateTime.Today, DateTime.Today.AddDays(7), 100.35);
            smile.UpdateParameters(1.2, 0.32, 0.82, 0.15);

            var custom = new CustomDistribution(smile);
            var integral = new Accord.Math.Integration.NonAdaptiveGaussKronrod()
            {
                Function = new Func<double, double>((x) => custom.Pdf(x)),
                Range = new Accord.DoubleRange(-8, 8),
                ToleranceAbsolute = 1E-12,
                ToleranceRelative = 1E-8
            };

            integral.Compute();
        }

        [Test]
        public void FokkerPlanckTest()
        {
            var curve = new Curve<PiecewiseLinearRateInterpolator<FlatRateExtrapolator, LinearRateExtrapolator>>(DateTime.Today,
                [0.0, 1.0 / 365, 7.0 / 365.0, 14.0 / 365],
                [0.0, 0.06, 0.08, 0.09],
                RateInterpolationTypes.Linear | RateInterpolationTypes.Forwards);

            var smile = new SABR(DateTime.Today, DateTime.Today.AddDays(7), curve.GetForward(100, DateTime.Today.AddDays(7)));
            smile.UpdateParameters(1.2, 0.32, 0.82, 0.15);

            var fokkerPlanck = new FokkerPlanck(100, smile, curve, 1.0 / (365 * 24));

            for (int i = 0; i < 24; ++i)
            {
                fokkerPlanck.SolveOneStep();
            }

            fokkerPlanck.ComputeMoments();
        }

        [Test]
        public void GATest()
        {
            var histoReturn = 
        }
    }
}
