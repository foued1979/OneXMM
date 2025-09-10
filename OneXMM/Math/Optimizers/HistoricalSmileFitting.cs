using Accord.Genetic;
using System;
using System.Runtime.CompilerServices;

namespace OneXMM.Math.Optimizers
{
    using Accord.Math.Optimization;
    using OneXMM.Interfaces;
    using OneXMM.Math.Distributions;
    using OneXMM.Math.Models;

    public static class HistoricalSmileFitting
    {
        public static bool GAFit(double[] historicalReturns, double spot, ISmile marketCorrelatedSmile, ICurve fwdCurve, double fitExpiry, double dt, int maxGenerations = 500)
        {
            double[] targetMoments = GetTargetMoments(historicalReturns, marketCorrelatedSmile);

            var population = new Population(
                50,
                new SmileChromosome(),
                new SmileFitnessFunction(targetMoments, spot, marketCorrelatedSmile, fwdCurve, fitExpiry, dt),
                selectionMethod: new EliteSelection()
            );

            var success = false;

            for (int i = 0; i < maxGenerations; i++)
            {
                population.RunEpoch();

                if (((SmileChromosome)population.BestChromosome).Fitness < 1E-14) 
                {
                    marketCorrelatedSmile.UpdateParameters(((SmileChromosome)population.BestChromosome).Parameters);
                    success = true;
                    break; }
            }

            return success;
        }

        public static bool NMFit(double[] historicalReturns, double spot, ISmile marketCorrelatedSmile, ICurve fwdCurve, double fitExpiry, double dt)
        {
            double[] targetMoments = GetTargetMoments(historicalReturns, marketCorrelatedSmile);
            var nm = new NelderMead(4, GetErrorFunction(spot, marketCorrelatedSmile, fwdCurve, fitExpiry, dt, [2.5, 2.5, 1.0, 1.0, 1.0], targetMoments));

            nm.LowerBounds[0] = 0.01;
            nm.LowerBounds[1] = 0.01;
            nm.LowerBounds[2] = 0.001;
            nm.LowerBounds[3] = -0.99;

            nm.UpperBounds[0] = 5.0;
            nm.UpperBounds[1] = 5.0;
            nm.UpperBounds[2] = 1.0;
            nm.UpperBounds[3] = 0.99;

            var success = nm.Minimize(marketCorrelatedSmile.GetInitialGuess());
            if (success) marketCorrelatedSmile.UpdateParameters(nm.Solution);
            return success;
        }

        private sealed class SmileChromosome : IChromosome
        {
            private static readonly double[] minValues = { 0.01, 0.01, 0.001, -0.99 }; // [alpha, vega, beta, rho]
            private static readonly double[] maxValues = { 5.0, 5.0, 1.0, 0.99 };

            private static readonly Random rng = new Random();

            public double[] Parameters { get; private set; }
            public double Fitness { get; private set; }

            public SmileChromosome()
            {
                Parameters = new double[minValues.Length];
                Generate();
            }

            /// <summary>
            /// Clones the <see cref="SmileChromosome"/>.
            /// </summary>
            public IChromosome Clone()
            {
                return new SmileChromosome() { Parameters = (double[])Parameters.Clone() };
            }

            public int CompareTo(object? obj)
            {
                if (obj is not null && obj is SmileChromosome smileChromosome)
                {
                    return (Parameters[0] * Parameters[1]).CompareTo(smileChromosome.Parameters[0] * smileChromosome.Parameters[1]);
                }

                return -1;
            }

            /// <summary>
            /// Creates a new <see cref="SmileChromosome"/>.
            /// </summary>
            /// <returns></returns>
            public IChromosome CreateNew()
            {
                return new SmileChromosome();
            }

            public void Crossover(IChromosome pair)
            {
                var other = (SmileChromosome)pair;

                int point = rng.Next(Parameters.Length);

                for (int i = point; i < Parameters.Length; i++)
                {
                    double temp = Parameters[i];
                    Parameters[i] = other.Parameters[i];
                    other.Parameters[i] = temp;
                }
            }

            public void Evaluate(IFitnessFunction function)
            {
                Fitness = function.Evaluate(this);
            }

            public void Generate()
            {
                for (int i = 0; i < Parameters.Length; i++)
                {
                    Parameters[i] = minValues[i] + rng.NextDouble() * (maxValues[i] - minValues[i]);
                }
            }

            public void Mutate()
            {
                int index = rng.Next(Parameters.Length);

                double range = (maxValues[index] - minValues[index]) * 0.2; // 20% mutation step
                double delta = (rng.NextDouble() * 2 - 1) * range;

                Parameters[index] = System.Math.Max(minValues[index], System.Math.Min(maxValues[index], Parameters[index] + delta));
            }
        }

        private sealed class SmileFitnessFunction : IFitnessFunction
        {
            private readonly ISmile _smile;
            private readonly ICurve _fwdCurve;

            private readonly double _targetExpiry;
            private readonly double _dt;
            private readonly double[] _targetMoments;
            private readonly double[] _weights;
            private readonly double _spot;

            public SmileFitnessFunction(double[] targetMoments, double spot, ISmile smile, ICurve fwdCurve, double targetExpiry, double dt)
            {
                _targetMoments = targetMoments;
                _smile = smile;
                _fwdCurve = fwdCurve;

                _targetExpiry = targetExpiry;
                _dt = dt;
                _weights = [2.5, 2.5, 1.0, 1.0, 1.0];
                _spot = spot;
            }

            public double Evaluate(IChromosome chromosome)
            {
                var genes = ((SmileChromosome)chromosome).Parameters;
                var f = GetErrorFunction(_spot, _smile, _fwdCurve, _targetExpiry, _dt, _weights, _targetMoments);

                try
                {
                    return -f(genes);
                }
                catch
                {
                    return double.NegativeInfinity;
                }
            }
        }

        private static double[] GetTargetMoments(double[] historicalReturns, ISmile marketCorrelatedSmile)
        {
            double[] targetMoments = [];
            CustomDistribution? marketDist = null;

            var initTasks = new Task[2];

            initTasks[0] = Task.Run(() => targetMoments = Distributions.Tools.ComputeMoments(historicalReturns));
            initTasks[1] = Task.Run(() => { marketDist = new CustomDistribution(marketCorrelatedSmile); marketDist.ComputeMoments(); });

            Task.WaitAll(initTasks);

            targetMoments[0] = marketDist!.StandardizedMoments[0];
            targetMoments[1] = marketDist!.StandardizedMoments[1];

            return targetMoments;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Func<double[], double> GetErrorFunction(double spot, ISmile smile, ICurve fwdCurve, double targetExpiry, double dt,
            double[] weights, double[] targetMoments)
        {
            return (x) =>
            {
                // 1. Build SABR smile for given parameters
                smile.UpdateParameters(x);
                var fp = new FokkerPlanck(spot, smile.Copy(), fwdCurve, dt);

                // 2. Compute Fokker-Planck until target expiry
                for (double t = 0.0; t < targetExpiry - 0.5 * dt; t += dt) fp.SolveOneStep();
                var fwdSmile = fp.Smile.GetForwardSmile(smile);

                // 3. Compute moments (skewness, kurtosis, etc.)
                var cs = new CustomDistribution(fwdSmile);
                cs.ComputeMoments();

                double error = 0.0;
                for (int i = 0; i < targetMoments.Length; i++)
                {
                    double diff = cs.StandardizedMoments[i] - targetMoments[i];
                    error += weights[i] * diff * diff;
                }

                return error;
            };
        }
    }
}
