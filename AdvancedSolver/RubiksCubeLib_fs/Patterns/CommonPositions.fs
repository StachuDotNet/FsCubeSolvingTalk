namespace RubiksCubeLib.Solver

open System.Linq
open RubiksCubeLib

type CommonPositions () = 
    /// Gets all possible edge positions
    static member EdgePositions
        with get() = 
            ([
                new CubePosition(CubeFlag.MiddleSliceSides,CubeFlag.TopLayer, CubeFlag.BackSlice)
                new CubePosition(CubeFlag.RightSlice,CubeFlag.TopLayer, CubeFlag.MiddleSlice)
                new CubePosition(CubeFlag.MiddleSliceSides,CubeFlag.TopLayer, CubeFlag.FrontSlice)
                new CubePosition(CubeFlag.LeftSlice,CubeFlag.TopLayer, CubeFlag.MiddleSlice)

                new CubePosition(CubeFlag.LeftSlice, CubeFlag.MiddleLayer, CubeFlag.BackSlice)
                new CubePosition(CubeFlag.RightSlice, CubeFlag.MiddleLayer, CubeFlag.BackSlice)
                new CubePosition(CubeFlag.RightSlice, CubeFlag.MiddleLayer, CubeFlag.FrontSlice)
                new CubePosition(CubeFlag.LeftSlice, CubeFlag.MiddleLayer, CubeFlag.FrontSlice)

                new CubePosition(CubeFlag.MiddleSliceSides,CubeFlag.BottomLayer, CubeFlag.BackSlice)
                new CubePosition(CubeFlag.RightSlice, CubeFlag.BottomLayer, CubeFlag.MiddleSlice)
                new CubePosition(CubeFlag.MiddleSliceSides, CubeFlag.BottomLayer, CubeFlag.FrontSlice)
                new CubePosition(CubeFlag.LeftSlice, CubeFlag.BottomLayer, CubeFlag.MiddleSlice)
            ]).ToList()


    /// Gets all possible corner positions
    static member CornerPositions 
        with get() = 
            ([
                new CubePosition(CubeFlag.LeftSlice,CubeFlag.TopLayer, CubeFlag.BackSlice)
                new CubePosition(CubeFlag.RightSlice,CubeFlag.TopLayer, CubeFlag.BackSlice)
                new CubePosition(CubeFlag.RightSlice,CubeFlag.TopLayer, CubeFlag.FrontSlice)
                new CubePosition(CubeFlag.LeftSlice,CubeFlag.TopLayer, CubeFlag.FrontSlice)

                new CubePosition(CubeFlag.LeftSlice,CubeFlag.BottomLayer, CubeFlag.BackSlice)
                new CubePosition(CubeFlag.RightSlice,CubeFlag.BottomLayer, CubeFlag.BackSlice)
                new CubePosition(CubeFlag.RightSlice,CubeFlag.BottomLayer, CubeFlag.FrontSlice)
                new CubePosition(CubeFlag.LeftSlice,CubeFlag.BottomLayer, CubeFlag.FrontSlice)
            ]).ToList()

    /// Gets all possible cube positions
    static member AllPositions with get() = Enumerable.Union(CommonPositions.EdgePositions, CommonPositions.CornerPositions)
