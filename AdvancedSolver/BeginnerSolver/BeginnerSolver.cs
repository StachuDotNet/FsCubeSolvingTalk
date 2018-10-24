using RubiksCubeLib;
using RubiksCubeLib.RubiksCube;
using RubiksCubeLib.Solver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using RubiksCubeLib.Solving;

namespace BeginnerSolver
{
    public class BeginnerSolver : CubeSolver
    {
        public override string Name => "Beginner";

        public BeginnerSolver()
        {
            this.SolutionSteps = new Dictionary<string, Action>
            {
                { "Cross on bottom layer", SolveFirstCross },
                { "Complete bottom layer", CompleteFirstLayer },
                { "Complete middle layer", CompleteMiddleLayer },
                //{ "Cross on top layer", SolveCrossTopLayer },
                //{ "Complete top layer", CompleteLastLayer }
            };
        }

        #region Converted
        private void SolveFirstCross()
        {
            // Step 1: Get the edges with target position on the bottom layer
            IEnumerable<Cube> bottomEdges =
                Rubik.Cubes.Where(c =>
                    c.IsEdge

                    // target position on bottom layer
                    && GetTargetFlags(c).HasFlag(CubeFlag.BottomLayer)
                );

            // Step 2: Rotate a correct orientated edge of the bottom layer to target position
            IEnumerable<Cube> bottomEdgesInBottomLayerCorrectlyOriented =
                bottomEdges.Where(bottomEdge =>
                    // is in bottom layer
                    bottomEdge.Position.Flags == GetTargetFlags(bottomEdge)

                    // is oriented correctly
                    && bottomEdge.Faces.First(f => f.Color == Rubik.BottomColor).Position == FacePosition.Bottom
                );

            var anyEdgesAreSolvableWithDMoves =
                bottomEdges.Count(bE =>
                    bE.Position.HasFlag(CubeFlag.BottomLayer)
                    && bE.Faces.First(f => f.Color == Rubik.BottomColor).Position == FacePosition.Bottom
                ) > 0;

            if (anyEdgesAreSolvableWithDMoves)
            {
                while (!bottomEdgesInBottomLayerCorrectlyOriented.Any())
                {
                    // turn the bottom layer until at least one is 
                    SolverMove(CubeFlag.BottomLayer, true);

                    bottomEdgesInBottomLayerCorrectlyOriented =
                        bottomEdges.Where(bE =>
                            bE.Position.Flags == GetTargetFlags(bE)
                            && bE.Faces.First(f => f.Color == Rubik.BottomColor).Position == FacePosition.Bottom
                        );
                }
            }

            // Step 3: Solve incorrect edges of the bottom layer
            while (bottomEdgesInBottomLayerCorrectlyOriented.Count() < 4)
            {
                IEnumerable<Cube> unsolvedBottomEdges = bottomEdges.Except(bottomEdgesInBottomLayerCorrectlyOriented);

                Cube e =
                    unsolvedBottomEdges.FirstOrDefault(c => c.Position.HasFlag(CubeFlag.TopLayer))
                    ?? unsolvedBottomEdges.First();

                Color secondColor =
                    e.Colors.First(co =>
                        co != Rubik.BottomColor
                        && co != Color.Black
                    );

                if (e.Position.Flags != GetTargetFlags(e))
                {
                    // Rotate to top layer
                    CubeFlag layer =
                        CubeFlagService.FromFacePosition(
                            e.Faces.First(f =>
                                (f.Color == Rubik.BottomColor || f.Color == secondColor)
                                && f.Position != FacePosition.Top && f.Position != FacePosition.Bottom
                            ).Position
                        );

                    CubeFlag targetLayer =
                        CubeFlagService.FromFacePosition(
                            StandardCube.Cubes.First(cu =>
                                CollectionMethods.ScrambledEquals(cu.Colors, e.Colors)
                            )
                            .Faces
                            .First(f => f.Color == secondColor)
                            .Position
                        );

                    if (e.Position.HasFlag(CubeFlag.MiddleLayer))
                    {
                        if (layer == targetLayer)
                        {
                            while (!e.Position.HasFlag(CubeFlag.BottomLayer))
                                SolverMove(layer, true);
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
                                for (int i = 0; i < 2; i++)
                                    SolverMove(layer, true);

                                SolverMove(CubeFlag.TopLayer, true);
                                SolverMove(layer, true);
                            }
                        }
                    }

                    if (e.Position.HasFlag(CubeFlag.BottomLayer))
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            SolverMove(layer, true);
                        }
                    }

                    // Rotate over target position
                    while (!e.Position.HasFlag(targetLayer))
                    {
                        SolverMove(CubeFlag.TopLayer, true);
                    }

                    //Rotate to target position
                    for (int i = 0; i < 2; i++)
                    {
                        SolverMove(targetLayer, true);
                    }
                }

                // Flip the incorrect orientated edges with the algorithm: F' D R' D'
                if (Solvability.GetOrientation(Rubik, e) != 0)
                {
                    CubeFlag frontSlice = CubeFlagService.FromFacePosition(e.Faces.First(f => f.Color == Rubik.BottomColor).Position);

                    SolverMove(frontSlice, false);
                    SolverMove(CubeFlag.BottomLayer, true);

                    CubeFlag rightSlice = CubeFlagService.FromFacePosition(e.Faces.First(f => f.Color == secondColor).Position);

                    SolverMove(rightSlice, false);
                    SolverMove(CubeFlag.BottomLayer, false);
                }

                bottomEdgesInBottomLayerCorrectlyOriented =
                    bottomEdges.Where(bE =>
                        bE.Position.Flags == GetTargetFlags(bE)
                        && bE.Faces.First(f => f.Color == Rubik.BottomColor).Position == FacePosition.Bottom
                    );
            }
        }

        private void CompleteFirstLayer()
        {
            // Step 1: Get the corners with target position on bottom layer
            IEnumerable<Cube> bottomCorners =
                Rubik.Cubes.Where(c =>
                    c.IsCorner
                    && GetTargetFlags(c).HasFlag(CubeFlag.BottomLayer)
                );

            IEnumerable<Cube> solvedBottomCorners =
                bottomCorners.Where(bC =>
                    bC.Position.Flags == GetTargetFlags(bC)
                    && bC.Faces.First(f => f.Color == Rubik.BottomColor).Position == FacePosition.Bottom
                );

            // Step 2: Solve incorrect edges
            while (solvedBottomCorners.Count() < 4)
            {
                IEnumerable<Cube> unsolvedBottomCorners = bottomCorners.Except(solvedBottomCorners);

                // here.
                Cube c =
                    (unsolvedBottomCorners.FirstOrDefault(bC => bC.Position.HasFlag(CubeFlag.TopLayer)) != null)
                          ? unsolvedBottomCorners.First(bC => bC.Position.HasFlag(CubeFlag.TopLayer))
                          : unsolvedBottomCorners.First();

                if (c.Position.Flags != GetTargetFlags(c))
                {
                    // Rotate to top layer
                    if (c.Position.HasFlag(CubeFlag.BottomLayer))
                    {
                        Face leftFace = c.Faces.First(f => f.Position != FacePosition.Bottom && f.Color != Color.Black);
                        CubeFlag leftSlice = CubeFlagService.FromFacePosition(leftFace.Position);

                        SolverMove(leftSlice, false);

                        if (c.Position.HasFlag(CubeFlag.BottomLayer))
                        {
                            SolverMove(leftSlice, true);

                            leftFace =
                                c.Faces.First(f =>
                                    f.Position != FacePosition.Bottom
                                    && f.Color != leftFace.Color
                                    && f.Color != Color.Black
                                );

                            leftSlice = CubeFlagService.FromFacePosition(leftFace.Position);

                            SolverMove(leftSlice, false);
                        }

                        SolverAlgorithm("U' {0} U", CubeFlagService.ToNotationString(leftSlice));
                    }

                    // Rotate over target position
                    CubeFlag targetPos = CubeFlagService.ExceptFlag(GetTargetFlags(c), CubeFlag.BottomLayer);

                    while (!c.Position.HasFlag(targetPos))
                        SolverMove(CubeFlag.TopLayer, true);
                }

                // Rotate to target position with the algorithm: Li Ui L U
                Face leftFac =
                    c.Faces.First(f =>
                        f.Position != FacePosition.Top
                        && f.Position != FacePosition.Bottom
                        && f.Color != Color.Black
                    );

                CubeFlag leftSlic = CubeFlagService.FromFacePosition(leftFac.Position);

                SolverMove(leftSlic, false);
                if (!c.Position.HasFlag(CubeFlag.TopLayer))
                {
                    SolverMove(leftSlic, true);

                    leftFac =
                        c.Faces.First(f =>
                            f.Position != FacePosition.Top
                            && f.Position != FacePosition.Bottom
                            && f.Color != leftFac.Color
                            && f.Color != Color.Black
                        );

                    leftSlic = CubeFlagService.FromFacePosition(leftFac.Position);
                }
                else SolverMove(leftSlic, true);

                while (c.Faces.First(f => f.Color == Rubik.BottomColor).Position != FacePosition.Bottom)
                {
                    if (c.Faces.First(f => f.Color == Rubik.BottomColor).Position == FacePosition.Top)
                    {
                        SolverAlgorithm("{0}' U U {0} U", CubeFlagService.ToNotationString(leftSlic));
                    }
                    else
                    {
                        Face frontFac =
                            c.Faces.First(f =>
                                f.Position != FacePosition.Top
                                && f.Position != FacePosition.Bottom
                                && f.Color != Color.Black
                                && f.Position != CubeFlagService.ToFacePosition(leftSlic)
                            );

                        var thingA = c.Faces.First(f => f.Color == Rubik.BottomColor).Position == frontFac.Position;
                        var thingB = !c.Position.HasFlag(CubeFlag.BottomLayer);

                        if (thingA && thingB)
                            SolverAlgorithm("U' {0}' U {0}", CubeFlagService.ToNotationString(leftSlic));
                        else
                            SolverAlgorithm("{0}' U' {0} U", CubeFlagService.ToNotationString(leftSlic));
                    }
                }

                solvedBottomCorners =
                    bottomCorners.Where(bC =>
                        bC.Position.Flags == GetTargetFlags(bC)
                        && bC.Faces.First(f => f.Color == Rubik.BottomColor).Position == FacePosition.Bottom
                    );
            }
        }
        #endregion Converted

        private void CompleteMiddleLayer()
        {
            // Step 1: Get the egdes of the middle layer
            List<Cube> middleEdges =
                Rubik.Cubes
                    .Where(c => c.IsEdge)
                    .Where(c => GetTargetFlags(c).HasFlag(CubeFlag.MiddleLayer))
                    .ToList();

            List<Face> coloredFaces = new List<Face>();

            Rubik.Cubes.Where(cu => cu.IsCenter)
                .ToList()
                .ForEach(cu =>
                    coloredFaces.Add(cu.Faces.First(f => f.Color != Color.Black))
                );

            IEnumerable<Cube> solvedMiddleEdges =
                middleEdges
                    .Where(mE =>
                        mE.Position.Flags == GetTargetFlags(mE)
                        && mE.Faces.Count(f => coloredFaces.Count(cf => cf.Color == f.Color && cf.Position == f.Position) == 1) == 2
                    );

            // Step 2: solve incorrect middle edges 
            while (solvedMiddleEdges.Count() < 4)
            {
                IEnumerable<Cube> unsolvedMiddleEdges = middleEdges.Except(solvedMiddleEdges);
                Cube c =
                    (unsolvedMiddleEdges.FirstOrDefault(cu => !cu.Position.HasFlag(CubeFlag.MiddleLayer)) != null)
                        ? unsolvedMiddleEdges.First(cu => !cu.Position.HasFlag(CubeFlag.MiddleLayer))
                        : unsolvedMiddleEdges.First();

                // Rotate to top layer
                if (!c.Position.HasFlag(CubeFlag.TopLayer))
                {
                    Face frontFace = c.Faces.First(f => f.Color != Color.Black);
                    CubeFlag frontSlice = CubeFlagService.FromFacePosition(frontFace.Position);
                    Face face = c.Faces.First(f => f.Color != Color.Black && f.Color != frontFace.Color);
                    CubeFlag slice = CubeFlagService.FromFacePosition(face.Position);

                    if (new TestScenario(Rubik, new LayerMove(slice, true, false)).TestCubePosition(c, CubeFlag.TopLayer))
                    {
                        // Algorithm to the right: U R Ui Ri Ui Fi U F
                        SolverAlgorithm(
                            "U {0} U' {0}' U' {1}' U {1}",
                            CubeFlagService.ToNotationString(slice),
                            CubeFlagService.ToNotationString(frontSlice)
                        );
                    }
                    else
                    {
                        // Algorithm to the left: Ui Li U L U F Ui Fi
                        SolverAlgorithm(
                            "U' {0}' U {0} U {1} U' {1}'",
                            CubeFlagService.ToNotationString(slice),
                            CubeFlagService.ToNotationString(frontSlice)
                        );
                    }
                }

                // Rotate to start position for the algorithm
                List<Cube> centers =
                    Rubik.Cubes
                        .Where(m => m.IsCenter)
                        .Where(m =>
                        {
                            var thing2 =
                                m.Colors.First(co => co != Color.Black) == 
                                    c.Faces.First(f => f.Color != Color.Black && f.Position != FacePosition.Top).Color;

                            var thing3 =
                                (m.Position.Flags & ~CubeFlag.MiddleLayer) == (c.Position.Flags & ~CubeFlag.TopLayer);

                            return thing2 && thing3;
                        })
                        .ToList();

                while (!centers.Any())
                {
                    SolverMove(CubeFlag.TopLayer, true);

                    centers =
                        Rubik.Cubes
                            .Where(cu => cu.IsCenter)
                            .Where(m =>
                                m.Colors.First(co => co != Color.Black) == c.Faces.First(f => f.Color != Color.Black && f.Position != FacePosition.Top).Color
                                && (m.Position.Flags & ~CubeFlag.MiddleLayer) == (c.Position.Flags & ~CubeFlag.TopLayer)
                            )
                      .ToList();
                }

                // Rotate to target position
                Face frontFac = c.Faces.First(f => f.Color != Color.Black && f.Position != FacePosition.Top);
                CubeFlag frontSlic = CubeFlagService.FromFacePosition(frontFac.Position);

                CubeFlag slic = CubeFlagService.FirstNotInvalidFlag(GetTargetFlags(c), CubeFlag.MiddleLayer | frontSlic);

                if (!new TestScenario(Rubik, new LayerMove(CubeFlag.TopLayer, true, false)).TestCubePosition(c, slic))
                {
                    // Algorithm to the right: U R Ui Ri Ui Fi U F
                    SolverAlgorithm(
                        "U {0} U' {0}' U' {1}' U {1}",
                        CubeFlagService.ToNotationString(slic),
                        CubeFlagService.ToNotationString(frontSlic)
                    );
                }
                else
                {
                    // Algorithm to the left: Ui Li U L U F Ui Fi
                    SolverAlgorithm(
                        "U' {0}' U {0} U {1} U' {1}'",
                        CubeFlagService.ToNotationString(slic),
                        CubeFlagService.ToNotationString(frontSlic)
                    );
                }

                solvedMiddleEdges =
                    middleEdges.Where(mE =>
                        mE.Faces.Count(f =>
                            coloredFaces.Count(cf => cf.Color == f.Color && cf.Position == f.Position) == 1
                        ) == 2
                    );
            }
        }

        #region Later

        private void SolveCrossTopLayer()
        {
            // Step 1: Get edges with the color of the top face
            IEnumerable<Cube> topEdges = Rubik.Cubes.Where(c => c.IsEdge).Where(c => c.Position.HasFlag(CubeFlag.TopLayer));

            // Step 2: Check if the cube is insoluble
            if (topEdges.Count(tE => tE.Faces.First(f => f.Position == FacePosition.Top).Color == Rubik.TopColor) % 2 != 0)
                return;

            var correctEdges = topEdges.Where(c => c.Faces.First(f => f.Position == FacePosition.Top).Color == Rubik.TopColor);
            var solveTopCrossAlgorithmI = new Algorithm("F R U R' U' F'");
            var solveTopCrossAlgorithmII = new Algorithm("F S R U R' U' F' S'");

            // Step 3: Solve the cross on the top layer
            if (Rubik.CountEdgesWithCorrectOrientation() == 0)
            {
                SolverAlgorithm(solveTopCrossAlgorithmI);
                correctEdges = topEdges.Where(c => c.Faces.First(f => f.Position == FacePosition.Top).Color == Rubik.TopColor);
            }

            if (Rubik.CountEdgesWithCorrectOrientation() == 2)
            {
                var firstCorrect = correctEdges.First();
                var secondCorrect = correctEdges.First(f => f != firstCorrect);

                bool opposite = false;

                foreach (CubeFlag flag in firstCorrect.Position.GetFlags())
                {
                    CubeFlag pos = CubeFlagService.GetOppositeFlag(flag);

                    if (secondCorrect.Position.HasFlag(pos) && pos != CubeFlag.None)
                    {
                        opposite = true;
                        break;
                    }
                }

                if (opposite)
                {
                    while (correctEdges.Count(c => c.Position.HasFlag(CubeFlag.RightSlice)) != 1)
                        SolverMove(CubeFlag.TopLayer, true);

                    SolverAlgorithm(solveTopCrossAlgorithmI);
                }
                else
                {
                    while (correctEdges.Count(c => c.Position.HasFlag(CubeFlag.RightSlice) || c.Position.HasFlag(CubeFlag.FrontSlice)) != 2)
                        SolverMove(CubeFlag.TopLayer, true);

                    SolverAlgorithm(solveTopCrossAlgorithmII);
                }
            }

            // Step 4: Move the edges of the cross to their target positions
            while (topEdges.Count(c => c.Position.Flags == GetTargetFlags(c)) < 4)
            {
                IEnumerable<Cube> correctEdges2 = topEdges.Where(c => c.Position.Flags == GetTargetFlags(c));
                while (correctEdges2.Count() < 2) SolverMove(CubeFlag.TopLayer, true);

                CubeFlag rightSlice = CubeFlagService.FromFacePosition(correctEdges2.First().Faces
                  .First(f => f.Position != FacePosition.Top && f.Color != Color.Black).Position);
                SolverMove(CubeFlag.TopLayer, false);

                if (correctEdges2.Count(c => c.Position.HasFlag(rightSlice)) == 0)
                {
                    SolverMove(CubeFlag.TopLayer, true);
                }
                else
                {
                    SolverMove(CubeFlag.TopLayer, true);

                    rightSlice =
                        CubeFlagService.FromFacePosition(
                            correctEdges2
                                .First(cE => !cE.Position.HasFlag(rightSlice))
                                .Faces
                                .First(f =>
                                    f.Position != FacePosition.Top
                                    && f.Color != Color.Black
                                )
                                .Position
                        );
                }

                // Algorithm: R U R' U R U U R'
                SolverAlgorithm(
                    "{0} U {0}' U {0} U U {0}'",
                    CubeFlagService.ToNotationString(rightSlice)
                );

                while (correctEdges2.Count() < 2)
                    SolverMove(CubeFlag.TopLayer, true);
            }
        }

        private void CompleteLastLayer()
        {
            // Step 1: Get edges with the color of the top face
            IEnumerable<Cube> topCorners =
                Rubik.Cubes
                    .Where(c => c.IsCorner)
                    .Where(c => c.Position.HasFlag(CubeFlag.TopLayer));

            // Step 2: Bring corners to their target position
            while (topCorners.Count(c => c.Position.Flags == GetTargetFlags(c)) < 4)
            {
                var correctCorners = topCorners.Where(c => c.Position.Flags == GetTargetFlags(c));

                CubeFlag rightSlice;
                if (correctCorners.Count() != 0)
                {
                    Cube firstCube = correctCorners.First();
                    Face rightFace = firstCube.Faces.First(f => f.Color != Color.Black && f.Position != FacePosition.Top);

                    rightSlice = CubeFlagService.FromFacePosition(rightFace.Position);

                    if (!new TestScenario(Rubik, new LayerMove(rightSlice, true, false)).TestCubePosition(firstCube, CubeFlag.TopLayer))
                    {
                        rightSlice =
                            CubeFlagService.FromFacePosition(
                                firstCube.Faces
                                    .First(f =>
                                        f.Color != rightFace.Color
                                        && f.Color != Color.Black
                                        && f.Position != FacePosition.Top
                                    ).Position
                            );
                    }
                }
                else
                {
                    rightSlice = CubeFlag.RightSlice;
                }

                SolverAlgorithm(
                    "U {0} U' {1}' U {0}' U' {1}",
                    CubeFlagService.ToNotationString(rightSlice),
                    CubeFlagService.ToNotationString(CubeFlagService.GetOppositeFlag(rightSlice))
                );
            }

            // Step 3: Orientation of the corners on the top layer
            Face rightFac = topCorners.First().Faces.First(f => f.Color != Color.Black && f.Position != FacePosition.Top);

            var scenario = new TestScenario(this.Rubik, new LayerMove(CubeFlagService.FromFacePosition(rightFac.Position), true, false));
            var scenarioPassed = scenario.TestCubePosition(topCorners.First(), CubeFlag.TopLayer);

            var positionForRightSlic =
                !scenarioPassed
                    ? rightFac.Position
                    : topCorners.First().Faces.First(f => f.Color != rightFac.Color && f.Color != Color.Black && f.Position != FacePosition.Top).Position;

            CubeFlag rightSlic = CubeFlagService.FromFacePosition(positionForRightSlic);



            foreach (Cube c in topCorners)
            {
                while (!c.Position.HasFlag(rightSlic))
                    SolverMove(CubeFlag.TopLayer, true);

                if (!new TestScenario(Rubik, new LayerMove(rightSlic, true, false)).TestCubePosition(c, CubeFlag.TopLayer))
                    SolverMove(CubeFlag.TopLayer, true);

                // Algorithm: R' D' R D
                while (c.Faces.First(f => f.Position == FacePosition.Top).Color != Rubik.TopColor)
                    SolverAlgorithm("{0}' D' {0} D", CubeFlagService.ToNotationString(rightSlic));
            }

            while (topCorners.Count(tC => tC.Position.Flags == GetTargetFlags(tC)) != 4)
                SolverMove(CubeFlag.TopLayer, true);
        }
        #endregion Later
    }
}