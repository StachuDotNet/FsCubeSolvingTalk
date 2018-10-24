using RubiksCubeLib;
using RubiksCubeLib.RubiksCube;
using System;
using System.Collections.Generic;
using System.IO;
using RubiksCubeLib.Solving;

namespace TwoPhaseAlgorithmSolver
{
    public class TwoPhaseAlgorithm : CubeSolver
    {
        public override string Name => "Two Phase Algorithm";

        public int MaxDepth { get; set; }
        public long TimeOut { get; set; }
        private CoordCube _coordCube;

        public TwoPhaseAlgorithm()
        {
            SolutionSteps = new Dictionary<string, Action>();
            AddSolutionStep("IDA* search for solution", Solution);

            if (!Directory.Exists(TwoPhaseConstants.TablePath))
                Directory.CreateDirectory(TwoPhaseConstants.TablePath);
        }

        public override void Solve(Rubik cube)
        {
            this.Rubik = cube.DeepClone();
            _coordCube = TwoPhaseHelpers.ToCoordCube(cube);
            this.MaxDepth = 30;
            this.TimeOut = 10000;
            Algorithm = new Algorithm(null);
            InitStandardCube();

            GetSolution();
            Algorithm = Algorithm.RemoveUnnecessaryMoves(Algorithm);
        }

        #region solving logic
        private int[] _axis = new int[31]; // The axis of the move
        private int[] _power = new int[31]; // The power of the move

        private int[] _flip = new int[31]; // phase1 coordinates
        private int[] _twist = new int[31];
        private int[] _slice = new int[31];

        private int[] _parity = new int[31]; // phase2 coordinates
        private int[] _urfdlf = new int[31];
        private int[] _frbr = new int[31];
        private int[] _urul = new int[31];
        private int[] _ubdf = new int[31];
        private int[] _urdf = new int[31];

        private int[] _minDistPhase1 = new int[31]; // IDA* distance do goal estimations
        private int[] _minDistPhase2 = new int[31];

        public void Solution()
        {
            _power[0] = 0;
            _axis[0] = 0;
            _flip[0] = CoordCubeGetters.GetFlip(_coordCube);
            _twist[0] = CoordCubeGetters.GetTwist(_coordCube);
            _parity[0] = CoordCubeExtensionsModule.Parity(ref _coordCube);
            _slice[0] = CoordCubeGetters.GetFRtoBR(_coordCube) / 24;
            _urfdlf[0] = CoordCubeGetters.GetURFtoDLF(_coordCube);
            _frbr[0] = CoordCubeGetters.GetFRtoBR(_coordCube);
            _urul[0] = CoordCubeGetters.GetURtoUL(_coordCube);
            _ubdf[0] = CoordCubeGetters.GetUBtoDF(_coordCube);

            _minDistPhase1[1] = 1;
            int n = 0;
            bool busy = false;
            int depthPhase1 = 0;

            long tStart = DateTime.Now.Millisecond;

            do
            {
                do
                {
                    if ((depthPhase1 - n > _minDistPhase1[n + 1]) && !busy)
                    {
                        n = n + 1;

                        if (_axis[n] == 0 || _axis[n] == 3)
                            _axis[n] = 1;
                        else
                            _axis[n] = 0;

                        _power[n] = 1;
                    }
                    else
                    {
                        _power[n] = _power[n] + 1;

                        if (_power[n] > 3)
                        {
                            var escape15 = false;

                            do
                            {
                                // increment axis
                                _axis[n] = _axis[n] + 1;

                                if (_axis[n] > 5)
                                {
                                    if (n == 0)
                                    {
                                        depthPhase1 = depthPhase1 + 1;
                                        _axis[n] = 0;
                                        _power[n] = 1;
                                        busy = false;
                                        escape15 = true;
                                    }
                                    else
                                    {
                                        n--;
                                        busy = true;
                                        escape15 = true;
                                    }
                                }
                                else
                                {
                                    _power[n] = 1;
                                    busy = false;
                                }
                            } while (!escape15 && (n != 0 && (_axis[n - 1] == _axis[n] || _axis[n - 1] - 3 == _axis[n])));
                        }
                        else busy = false;
                    }
                } while (busy);

                var mv = 3 * _axis[n] + _power[n] - 1;

                _flip[n + 1] = TwoPhaseMoveTables.Flip[_flip[n], mv];
                _twist[n + 1] = TwoPhaseMoveTables.Twist[_twist[n], mv];
                _slice[n + 1] = TwoPhaseMoveTables.FRtoBR[_slice[n] * 24, mv] / 24;

                _minDistPhase1[n + 1] =
                    Math.Max(
                        TwoPhasePruningTableModule.GetPruning(TwoPhasePruningTableModule.SliceFlip, TwoPhaseConstants.N_SLICE1 * _flip[n + 1] + _slice[n + 1]),
                        TwoPhasePruningTableModule.GetPruning(TwoPhasePruningTableModule.SliceTwist, TwoPhaseConstants.N_SLICE1 * _twist[n + 1] + _slice[n + 1])
                    );

                if (_minDistPhase1[n + 1] == 0 && n >= depthPhase1 - 5)
                {
                    _minDistPhase1[n + 1] = 10;// instead of 10 any value >5 is possible

                    var s = 0;

                    if (n == depthPhase1 - 1 && (s = TotalDepth(depthPhase1, this.MaxDepth)) >= 0)
                    {
                        // solution found
                        for (int i = 0; i < s; i++)
                        {
                            if (_power[i] == 0)
                                break;

                            this.SolverMove(TwoPhaseHelpers.IntsToLayerMove(_axis[i], _power[i]));
                        }

                        return;
                    }
                }
            } while (true);
        }

        private int TotalDepth(int depthPhase1, int maxDepth)
        {
            int mv = 0, d1 = 0, d2 = 0;
            int maxDepthPhase2 = Math.Min(10, maxDepth - depthPhase1);

            for (int i = 0; i < depthPhase1; i++)
            {
                mv = 3 * _axis[i] + _power[i] - 1;
                _urfdlf[i + 1] = TwoPhaseMoveTables.URFtoDLF[_urfdlf[i], mv];
                _frbr[i + 1] = TwoPhaseMoveTables.FRtoBR[_frbr[i], mv];
                _parity[i + 1] = TwoPhaseMoveTables.ParityMove[_parity[i], mv];
            }


            d1 = 
                TwoPhasePruningTableModule.GetPruning(
                    TwoPhasePruningTableModule.SliceURFtoDLF,
                    (TwoPhaseConstants.N_SLICE2 * _urfdlf[depthPhase1] + _frbr[depthPhase1]) * 2 + _parity[depthPhase1]
                );

            if (d1 > maxDepthPhase2)
            {
                return -1;
            }

            for (int i = 0; i < depthPhase1; i++)
            {
                mv = 3 * _axis[i] + _power[i] - 1;
                _urul[i + 1] = TwoPhaseMoveTables.URtoUL[_urul[i], mv];
                _ubdf[i + 1] = TwoPhaseMoveTables.UBtoDF[_ubdf[i], mv];
            }

            _urdf[depthPhase1] = TwoPhaseMoveTables.URtoULandUBtoDF[_urul[depthPhase1], _ubdf[depthPhase1]];

            d2 = 
                TwoPhasePruningTableModule.GetPruning(
                    TwoPhasePruningTableModule.SliceURtoDF,
                    (TwoPhaseConstants.N_SLICE2 * _urdf[depthPhase1] + _frbr[depthPhase1]) * 2 + _parity[depthPhase1]
                );

            if (d2 > maxDepthPhase2)
                return -1;

            if ((_minDistPhase2[depthPhase1] = Math.Max(d1, d2)) == 0)// already solved
                return depthPhase1;

            // now set up search

            int depthPhase2 = 1;
            int n = depthPhase1;
            bool busy = false;
            _power[depthPhase1] = 0;
            _axis[depthPhase1] = 0;
            _minDistPhase2[n + 1] = 1;
            // else failure for depthPhase2=1, n=0
            // +++++++++++++++++++ end initialization +++++++++++++++++++++++++++++++++
            do
            {
                do
                {
                    if ((depthPhase1 + depthPhase2 - n > _minDistPhase2[n + 1]) && !busy)
                    {

                        if (_axis[n] == 0 || _axis[n] == 3)// Initialize next move
                        {
                            _axis[++n] = 1;
                            _power[n] = 2;
                        }
                        else
                        {
                            _axis[++n] = 0;
                            _power[n] = 1;
                        }
                    }
                    else if ((_axis[n] == 0 || _axis[n] == 3) ? (++_power[n] > 3) : ((_power[n] = _power[n] + 2) > 3))
                    {
                        do
                        {// increment axis
                            if (++_axis[n] > 5)
                            {
                                if (n == depthPhase1)
                                {
                                    if (depthPhase2 >= maxDepthPhase2)
                                        return -1;
                                    else
                                    {
                                        depthPhase2++;
                                        _axis[n] = 0;
                                        _power[n] = 1;
                                        busy = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    n--;
                                    busy = true;
                                    break;
                                }

                            }
                            else
                            {
                                if (_axis[n] == 0 || _axis[n] == 3)
                                    _power[n] = 1;
                                else
                                    _power[n] = 2;
                                busy = false;
                            }
                        } while (n != depthPhase1 && (_axis[n - 1] == _axis[n] || _axis[n - 1] - 3 == _axis[n]));
                    }
                    else
                        busy = false;
                } while (busy);

                // +++++++++++++ compute new coordinates and new minDist ++++++++++
                mv = 3 * _axis[n] + _power[n] - 1;

                _urfdlf[n + 1] = TwoPhaseMoveTables.URFtoDLF[_urfdlf[n], mv];
                _frbr[n + 1] = TwoPhaseMoveTables.FRtoBR[_frbr[n], mv];
                _parity[n + 1] = TwoPhaseMoveTables.ParityMove[_parity[n], mv];
                _urdf[n + 1] = TwoPhaseMoveTables.URtoDF[_urdf[n], mv];

                _minDistPhase2[n + 1] =
                    Math.Max(
                        TwoPhasePruningTableModule.GetPruning(
                            TwoPhasePruningTableModule.SliceURtoDF,
                            (TwoPhaseConstants.N_SLICE2 * _urdf[n + 1] + _frbr[n + 1]) * 2 + _parity[n + 1]
                        ),
                        TwoPhasePruningTableModule.GetPruning(
                            TwoPhasePruningTableModule.SliceURFtoDLF,
                            (TwoPhaseConstants.N_SLICE2 * _urfdlf[n + 1] + _frbr[n + 1]) * 2 + _parity[n + 1]
                        )
                    );
                // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            } while (_minDistPhase2[n + 1] != 0);
            return depthPhase1 + depthPhase2;
        }
        #endregion
    }
}