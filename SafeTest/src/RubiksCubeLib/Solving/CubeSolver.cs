using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using RubiksCubeLib.RubiksCube;
using RubiksCubeLib.Solver;

namespace RubiksCubeLib.Solving
{
    /// <summary>
    /// Represents the CubeSolver, and forces all implementing classes to have several methods
    /// </summary>
    public abstract class CubeSolver
    {
        /// <summary> The Rubik which will be used to solve the transferred Rubik </summary>
        public Rubik Rubik { get; set; }

        /// <summary> A solved Rubik  </summary>
        protected Rubik StandardCube { get; set; }

        /// <summary> Returns the solution for this solver used for the Rubik </summary>
        public Algorithm Algorithm { get; set; }

        /// <summary> The name of this solver </summary>
        public abstract string Name { get; }

        public Dictionary<string, Action> SolutionSteps { get; protected set; }

        private List<IMove> _movesOfStep = new List<IMove>();
        private Thread solvingThread;

        public delegate void SolutionStepCompletedEventHandler(object sender, SolutionStepCompletedEventArgs e);
        public event SolutionStepCompletedEventHandler OnSolutionStepCompleted;

        public delegate void SolutionErrorEventHandler(object sender, SolutionErrorEventArgs e);
        public event SolutionErrorEventHandler OnSolutionError;

        protected void BroadcastOnSolutionError(string step, string message)
        {
            OnSolutionError?.Invoke(this, new SolutionErrorEventArgs(step, message));

            solvingThread.Abort();
        }

        protected void AddSolutionStep(string key, Action action)
        {
            this.SolutionSteps.Add(key, action);
        }

        /// <summary>
        /// Returns the solution for the transferred Rubik
        /// </summary>
        /// <param name="cube">Defines the Rubik to be solved</param>
        public virtual void Solve(Rubik cube)
        {
            Rubik = cube.DeepClone();
            Algorithm = new Algorithm(null);
            InitStandardCube();

            GetSolution();
            Algorithm = Algorithm.RemoveUnnecessaryMoves(Algorithm);
        }

        protected void GetSolution()
        {
            Stopwatch sw = new Stopwatch();

            foreach (var step in this.SolutionSteps)
            {
                sw.Restart();
                step.Value();
                sw.Stop();

                var alg = new Algorithm(null);
                alg.Moves = _movesOfStep;

                OnSolutionStepCompleted?.Invoke(this,
                    new SolutionStepCompletedEventArgs(
                        step.Key,
                        false,
                        alg,
                        (int)sw.ElapsedMilliseconds
                    )
                );

                _movesOfStep.Clear();
            }
        }

        public void TrySolveAsync(Rubik rubik)
        {
            solvingThread = new Thread(() => SolveAsync(rubik));
            solvingThread.Start();
        }

        private void SolveAsync(Rubik rubik)
        {
            bool solvable = Solvability.FullTest(rubik);

            if (solvable)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Solve(rubik);
                sw.Stop();

                Algorithm = Algorithm.RemoveUnnecessaryMoves(Algorithm);
                var args = new SolutionStepCompletedEventArgs(Name, true, Algorithm, (int) sw.ElapsedMilliseconds);
                OnSolutionStepCompleted?.Invoke(this, args);

                solvingThread.Abort();
            }
            else
            {
                this.BroadcastOnSolutionError(this.Name, "Unsolvable cube");
            }
        }

        /// <summary> Initializes the StandardCube </summary>
        protected void InitStandardCube()
        {
            StandardCube = Rubik.GenStandardCube();
        }

        /// <summary> Returns the position of given cube where it has to be when the Rubik is solved </summary>
        /// <param name="cube">Defines the cube to be analyzed</param>
        public CubeFlag GetTargetFlags(Cube cube)
        {
            return StandardCube.Cubes.First(cu => CollectionMethods.ScrambledEquals(cu.Colors, cube.Colors)).Position.Flags;
        }

        /// <summary> Adds n move to the solution and executes it on the Rubik </summary>
        /// <param name="layer">Defines the layer to be rotated</param>
        /// <param name="direction">Defines the direction of the rotation</param>
        protected void SolverMove(CubeFlag layer, bool direction)
        {
            Rubik.RotateLayerFromCubeFlag(layer, direction);
            Algorithm.Moves.Add(new LayerMove(layer, direction, false));
            _movesOfStep.Add(new LayerMove(layer, direction, false));
        }

        /// <summary> Adds a move to the solution and executes it on the Rubik </summary>
        /// <param name="move">Defines the move to be rotated</param>
        protected void SolverMove(IMove move)
        {
            Rubik.RotateLayer(move);
            Algorithm.Moves.Add(move);
            _movesOfStep.Add(move);
        }

        /// <summary> Executes the given algorithm </summary>
        /// <param name="moves">Defines a notation string, which is filled with placeholders</param>
        /// <param name="placeholders">Defines the objects to be inserted for the placeholders</param>
        protected void SolverAlgorithm(string moves, params object[] placeholders)
        {
            Algorithm algorithm = new Algorithm(moves, placeholders);
            SolverAlgorithm(algorithm);
        }

        /// <summary> Executes the given algorithm on the Rubik </summary>
        /// <param name="algorithm">Defines the algorithm to be executed</param>
        protected void SolverAlgorithm(Algorithm algorithm)
        {
            foreach (IMove m in algorithm.Moves)
                SolverMove(m);
        }
    }
}