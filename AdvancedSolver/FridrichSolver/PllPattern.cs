using RubiksCubeLib;
using RubiksCubeLib.Solver;
using System.Collections.Generic;

namespace FridrichSolver
{
    public class PllPattern : PatternTable
    {
        public override Dictionary<Pattern, Algorithm> Patterns { get; } = new Dictionary<Pattern, Algorithm>
        {
            // Permutations of edges or corners only
            {Pattern.FromPatternStringArray(new[] {"MFU,MBU", "MBU,RSU", "RSU,MFU"}, 1.0 / 18.0), new Algorithm("R2 U F B' R2 F' B U R2")}, // PLL Ub
            {Pattern.FromPatternStringArray(new[] {"MFU,RSU", "MBU,MFU", "RSU,MBU"}, 1.0 / 18.0), new Algorithm("R2 U' F B' R2 F' B U' R2")}, // PLL Ua
            {Pattern.FromPatternStringArray(new[] {"MFU,LSU", "MBU,RSU", "RSU,MBU", "LSU,MFU"}, 1.0 / 36.0), new Algorithm("x' F R U' R' U D R' D U' R' U R D2 x")}, // PLL Z
            {Pattern.FromPatternStringArray(new[] {"MFU,MBU", "MBU,MFU", "RSU,LSU", "LSU,RSU"}, 1.0 / 72.0), new Algorithm("M2 U' M2 U2 M2 U' M2")}, // PLL H
            {Pattern.FromPatternStringArray(new[] {"LBU,RBU", "RBU,RFU", "RFU,LBU"}, 1.0 / 18.0), new Algorithm("x R' U R' D2 R U' R' D2 R2 x'")}, // PLL Aa
            {Pattern.FromPatternStringArray(new[] {"LBU,RFU", "RBU,LBU", "RFU,RBU"}, 1.0 / 18.0), new Algorithm("x R2 D2 R U R' D2 R U' R x'")}, // PLL Ab
            {Pattern.FromPatternStringArray(new[] {"LBU,LFU", "LFU,LBU", "RFU,RBU", "RBU,RFU"}, 1.0 / 36.0), new Algorithm("x' R U' R' D R U R' D' R U R' D R U' R' D' x")}, // PLL E

            // Swap one set of adjacent corners
            {Pattern.FromPatternStringArray(new[] {"MBU,RSU", "RSU,MBU", "RFU,LFU", "LFU,RFU"}, 1.0 / 18.0), new Algorithm("R U2 R' U2 R B' R' U' R U R B R2 U")}, // PLL Ra
            {Pattern.FromPatternStringArray(new[] {"MFU,RSU", "RSU,MFU", "RBU,LBU", "LBU,RBU"}, 1.0 / 18.0), new Algorithm("R' U2 R U2 R' F R U R' U' R' F' R2 U'")}, // PLL Rb
            {Pattern.FromPatternStringArray(new[] {"MFU,LSU", "LSU,MFU", "LBU,LFU", "LFU,LBU"}, 1.0 / 18.0), new Algorithm("R U' L' U R' U2 L U' L' U2 L")}, // PLL Ja
            {Pattern.FromPatternStringArray(new[] {"MFU,RSU", "RSU,MFU", "RBU,RFU", "RFU,RBU"}, 1.0 / 18.0), new Algorithm("L' U R U' L U2 R' U R U2 R'")}, // PLL Jb
            {Pattern.FromPatternStringArray(new[] {"LSU,RSU", "RSU,LSU", "RBU,RFU", "RFU,RBU"}, 1.0 / 18.0), new Algorithm("R U R' U' R' F R2 U' R' U' R U R' F'")}, // PLL T
            {Pattern.FromPatternStringArray(new[] {"LSU,RSU", "RSU,LSU", "RBU,LBU", "LBU,RBU"}, 1.0 / 18.0), new Algorithm("R' U R U' R2 y' R' U' R U y x R U R' U' R2 B' x'")}, // PLL F

            // Swap one set of corners diagonally
            {Pattern.FromPatternStringArray(new[] {"MBU,RSU", "RSU,MBU", "LBU,RFU", "RFU,LBU"}, 1.0 / 18.0), new Algorithm("R' U R' U' y R' F' R2 U' R' U R' F R F")}, // PLL V
            {Pattern.FromPatternStringArray(new[] {"MBU,LSU", "LSU,MBU", "LBU,RFU", "RFU,LBU"}, 1.0 / 18.0), new Algorithm("F R U' R' U' R U R' F' R U R' U' R' F R F'")}, // PLL Y
            {Pattern.FromPatternStringArray(new[] {"LSU,RSU", "RSU,LSU", "LFU,RBU", "RBU,LFU"}, 1.0 / 72.0), new Algorithm("L U' R U2 L' U R' L U' R U2 L' U R' U'")}, // PLL Na
            {Pattern.FromPatternStringArray(new[] {"LSU,RSU", "RSU,LSU", "RFU,LBU", "LBU,RFU"}, 1.0 / 72.0), new Algorithm("R' U L' U2 R U' L R' U L' U2 R U' L U")}, // PLL Nb

            // Double spins
            {Pattern.FromPatternStringArray(new[] {"LSU,RSU", "RSU,MBU", "MBU,LSU", "LFU,LBU", "LBU,RBU", "RBU,LFU"}, 1.0 / 18.0), new Algorithm("R2 u R' U R' U' R u' R2 y' R' U R")}, // PLL Ga
            {Pattern.FromPatternStringArray(new[] {"LSU,MBU", "RSU,LSU", "MBU,RSU", "RFU,RBU", "LBU,RFU", "RBU,LBU"}, 1.0 / 18.0), new Algorithm("L2 u' L U' L U L' u L2 y' R U' R'")}, // PLL Gc
            {Pattern.FromPatternStringArray(new[] {"LSU,MFU", "MFU,MBU", "MBU,LSU", "LFU,LBU", "LBU,RBU", "RBU,LFU"}, 1.0 / 18.0), new Algorithm("R U R' y' R2 u' R U' R' U R' u R2")}, // PLL Gd
            {Pattern.FromPatternStringArray(new[] {"MBU,RSU", "RSU,MFU", "MFU,MBU", "RFU,RBU", "LBU,RFU", "RBU,LBU"}, 1.0 / 18.0), new Algorithm("L' U' L y L2 u L' U L U' L u' L2")}, // PLL Gb
        };
    }
}
