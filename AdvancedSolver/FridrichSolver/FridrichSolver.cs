using RubiksCubeLib;
using RubiksCubeLib.RubiksCube;
using RubiksCubeLib.Solver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using RubiksCubeLib.Solving;

namespace FridrichSolver
{
    public class FridrichSolver : CubeSolver
    {
        public override string Name => "Fridrich";

        public FridrichSolver()
        {
            AddSolutionSteps();
        }

        protected virtual void AddSolutionSteps()
        {
            this.SolutionSteps = new Dictionary<string, Action>();

            this.AddSolutionStep("Cross on bottom layer", SolveFirstCross);
            this.AddSolutionStep("Complete first two layers", CompleteF2L);
            this.AddSolutionStep("Orientation top layer", Oll);
            this.AddSolutionStep("Permutation last layer", Pll);
        }

        private void SolveFirstCross()
        {
            // Step 1: Get the edges with target position on the bottom layer
            IEnumerable<Cube> bottomEdges = Rubik.Cubes.Where(c => c.IsEdge && GetTargetFlags(c).HasFlag(CubeFlag.BottomLayer));

            // Step 2: Rotate a correct orientated edge of the bottom layer to target position
            IEnumerable<Cube> solvedBottomEdges = bottomEdges.Where(bE => bE.Position.Flags == GetTargetFlags(bE) && bE.Faces.First(f => f.Color == Rubik.BottomColor).Position == FacePosition.Bottom);
            if (bottomEdges.Count(bE => bE.Position.HasFlag(CubeFlag.BottomLayer) && bE.Faces.First(f => f.Color == Rubik.BottomColor).Position == FacePosition.Bottom) > 0)
            {
                while (!solvedBottomEdges.Any())
                {
                    SolverMove(CubeFlag.BottomLayer, true);
                    solvedBottomEdges = bottomEdges.Where(bE => bE.Position.Flags == GetTargetFlags(bE) && bE.Faces.First(f => f.Color == Rubik.BottomColor).Position == FacePosition.Bottom);
                }
            }

            // Step 3: Solve incorrect edges of the bottom layer
            while (solvedBottomEdges.Count() < 4)
            {
                IEnumerable<Cube> unsolvedBottomEdges = bottomEdges.Except(solvedBottomEdges);

                Cube e = (unsolvedBottomEdges.FirstOrDefault(c => c.Position.HasFlag(CubeFlag.TopLayer)) != null)
                    ? unsolvedBottomEdges.FirstOrDefault(c => c.Position.HasFlag(CubeFlag.TopLayer))
                    : unsolvedBottomEdges.First();

                Color secondColor =
                    e.Colors.First(co => co != Rubik.BottomColor && co != Color.Black);

                if (e.Position.Flags != GetTargetFlags(e))
                {
                    // Rotate to top layer
                    CubeFlag layer = CubeFlagService.FromFacePosition(e.Faces.First(f => (f.Color == Rubik.BottomColor || f.Color == secondColor)
                      && f.Position != FacePosition.Top && f.Position != FacePosition.Bottom).Position);

                    CubeFlag targetLayer = CubeFlagService.FromFacePosition(StandardCube.Cubes.First(cu => CollectionMethods.ScrambledEquals(cu.Colors, e.Colors))
                      .Faces.First(f => f.Color == secondColor).Position);

                    if (e.Position.HasFlag(CubeFlag.MiddleLayer))
                    {
                        if (layer == targetLayer)
                        {
                            while (!e.Position.HasFlag(CubeFlag.BottomLayer)) SolverMove(layer, true);
                        }
                        else
                        {
                            SolverMove(layer, true);
                            if (e.Position.HasFlag(CubeFlag.TopLayer))
                            {
                                SolverMove(CubeFlag.TopLayer, true);
                                SolverMove(layer, false);
                            }
                            else
                            {
                                for (int i = 0; i < 2; i++) SolverMove(layer, true);
                                SolverMove(CubeFlag.TopLayer, true);
                                SolverMove(layer, true);
                            }
                        }
                    }
                    if (e.Position.HasFlag(CubeFlag.BottomLayer)) for (int i = 0; i < 2; i++) SolverMove(layer, true);

                    // Rotate over target position
                    while (!e.Position.HasFlag(targetLayer))
                        SolverMove(CubeFlag.TopLayer, true);

                    //Rotate to target position
                    for (int i = 0; i < 2; i++)
                        SolverMove(targetLayer, true);
                    //                    GetTargetFlags(e);
                }

                // Flip the incorrect orientated edges with the algorithm: Fi D Ri Di
                if (Solvability.GetOrientation(Rubik, e) != 0)
                {
                    CubeFlag frontSlice = CubeFlagService.FromFacePosition(e.Faces.First(f => f.Color == Rubik.BottomColor).Position);

                    SolverMove(frontSlice, false);
                    SolverMove(CubeFlag.BottomLayer, true);

                    CubeFlag rightSlice = CubeFlagService.FromFacePosition(e.Faces.First(f => f.Color == secondColor).Position);

                    SolverMove(rightSlice, false);
                    SolverMove(CubeFlag.BottomLayer, false);
                }

                solvedBottomEdges =
                    bottomEdges.Where(bE =>
                        bE.Position.Flags == GetTargetFlags(bE)
                        && bE.Faces.First(f => f.Color == Rubik.BottomColor).Position == FacePosition.Bottom
                    );
            }
        }

        private void CompleteF2L()
        {
            List<Tuple<Cube, Cube>> unsolvedPairs = GetPairs(this.Rubik).ToList();

            while (unsolvedPairs.Count > 0) // 4 pairs
            {
                Tuple<Cube, Cube> currentPair = unsolvedPairs.First();

                Cube edge = currentPair.Item1;
                Cube corner = currentPair.Item2;

                CubePosition target = new CubePosition(Rubik.GetTargetFlags(corner));

                if (!corner.Position.HasFlag(CubeFlag.TopLayer) && Rubik.GetTargetFlags(corner) != corner.Position.Flags)
                {
                    CubeFlag rotationLayer = CubeFlagService.FirstNotInvalidFlag(corner.Position.Flags, CubeFlag.BottomLayer);
                    bool direction = new TestScenario(Rubik, new LayerMove(rotationLayer, true, false)).TestCubePosition(corner, CubeFlag.TopLayer);
                    SolverMove(rotationLayer, direction);
                    SolverMove(CubeFlag.TopLayer, true);
                    SolverMove(rotationLayer, !direction);
                }
                // move edge to top position if necessary
                if (!edge.Position.HasFlag(CubeFlag.TopLayer) && Rubik.GetTargetFlags(edge) != edge.Position.Flags)
                {
                    CubeFlag rotationLayer = CubeFlagService.FirstNotInvalidFlag(edge.Position.Flags, CubeFlag.MiddleLayer);
                    bool direction = new TestScenario(Rubik, new LayerMove(rotationLayer, true, false)).TestCubePosition(edge, CubeFlag.TopLayer);
                    SolverMove(rotationLayer, direction);
                    while ((corner.Position.HasFlag(rotationLayer) && !corner.Position.HasFlag(CubeFlag.BottomLayer)) || edge.Position.HasFlag(rotationLayer)) SolverMove(CubeFlag.TopLayer, true);
                    SolverMove(rotationLayer, !direction);
                }

                // detect right and front slice
                CubeFlag rightSlice = CubeFlagService.ToInt(target.X) == CubeFlagService.ToInt(target.Z) ? target.Z : target.X;
                CubeFlag frontSlice = CubeFlagService.FirstNotInvalidFlag(target.Flags, CubeFlagModule.YFlags | rightSlice);

                while (!corner.Position.HasFlag(target.Flags & ~CubeFlag.BottomLayer)) SolverMove(CubeFlag.TopLayer, true);

                PatternFilter filter = new PatternFilter(delegate (Pattern p1, Pattern p2)
                {
                    PatternItem item = new PatternItem(corner.Position, Solvability.GetOrientation(this.Rubik, corner), target.Flags);
                    return p2.Items.Any(i => i.Equals(item));
                }, true);

                for (int i = 0; i < 4; i++)
                {
                    F2LPattern pattern = new F2LPattern(Rubik.GetTargetFlags(edge), Rubik.GetTargetFlags(corner), rightSlice, frontSlice);
                    var algo = pattern.FindBestMatch(PatternHelpers.FromRubik(Rubik), CubeFlag.None, filter);

                    if (algo != null)
                    {
                        SolverAlgorithm(algo);
                        break;
                    }

                    SolverMove(CubeFlag.TopLayer, true);
                }

                int count = unsolvedPairs.Count;

                unsolvedPairs = GetPairs(this.Rubik).ToList();

                if (unsolvedPairs.Count == count)
                {
                    this.BroadcastOnSolutionError("Complete first two layers", "Wrong algorithm");
                    return;
                }
            }
        }

        private static IEnumerable<Tuple<Cube, Cube>> GetPairs(Rubik rubik)
        {
            foreach (Cube edge in rubik.Cubes.Where(c => c.IsEdge && rubik.GetTargetFlags(c).HasFlag(CubeFlag.MiddleLayer)))
            {
                Cube corner =
                    rubik.Cubes
                        .First(c =>
                        {
                            return c.IsCorner
                                   && (rubik.GetTargetFlags(c) & ~CubeFlag.BottomLayer) ==
                                   (rubik.GetTargetFlags(edge) & ~CubeFlag.MiddleLayer);
                        });

                if (!rubik.IsCorrect(corner) || !rubik.IsCorrect(edge))
                    yield return new Tuple<Cube, Cube>(edge, corner);
            }
        }

        private void Oll()
        {
            OllPattern p = new OllPattern();
            Algorithm oll = p.FindBestMatch(PatternHelpers.FromRubik(this.Rubik), CubeFlag.TopLayer, CommonPatternFilters.SameFlipCount);

            if (oll != null)
            {
                SolverAlgorithm(oll); // else no oll algorithm required
            }
        }

        private void Pll()
        {
            var p = new PllPattern();

            for (int i = 0; i < 4; i++)
            {
                Algorithm pll = p.FindBestMatch(PatternHelpers.FromRubik(Rubik), CubeFlag.TopLayer, CommonPatternFilters.SameInversionCount);

                if (pll != null)
                {
                    SolverAlgorithm(pll);
                    break;
                }

                SolverMove(CubeFlag.TopLayer, true);
            }
        }
    }
}
