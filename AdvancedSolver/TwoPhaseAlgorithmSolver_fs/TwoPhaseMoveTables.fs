namespace TwoPhaseAlgorithmSolver

open System
open System.IO
open System.Runtime.InteropServices
open TwoPhaseAlgorithmSolver

module TwoPhaseMoveTableHelpers = 
    let LoadMoveTable(filename: string, lengthX: int, lengthY: int): int16[,] = 
        if not <| File.Exists filename then
            failwith "File does not exist!"

        // todo: may need to reverse order
        let newTable: int16[,] = Array2D.zeroCreate lengthY lengthX

        use sr = new StreamReader(filename)

        let mutable rowIndex = 0

        while not sr.EndOfStream do
            let line = sr.ReadLine()
            let entries = line.Split(';')

            if entries.Length <> lengthX then
                failwith "Invalid input file!"

            for columnIndex = 0 to (lengthX - 1) do 
                let mutable ignored = 0s
                if System.Int16.TryParse(entries.[columnIndex], &ignored) then
                    newTable.[rowIndex, columnIndex] <- Int16.Parse(entries.[columnIndex])
                else
                    failwith "Invalid input file!"

            rowIndex <- rowIndex + 1

        if rowIndex <> lengthY then
            failwith "Invalid input file!"

        newTable


    let LoadMoveTableSuccessful(filename: string, lengthX: int, lengthY: int, [<Out>] newTable: byref<int16[,]>): bool = 
        newTable <- Array2D.zeroCreate lengthY lengthX

        try
            newTable <- LoadMoveTable(filename, lengthX, lengthY)
            true
        with
            | :? Exception as ex -> false


    let SaveMoveTable(filename: string, table: int16[,]): unit =
        use sw = new StreamWriter(filename)

        for rowIndex = 0 to (table.GetLength(0) - 1) do
            let mutable line = table.[rowIndex, 0].ToString()

            for columnIndex = 1 to (table.GetLength(1) - 1) do
                line <- line + (sprintf ";%i" table.[rowIndex, columnIndex])

            sw.WriteLine(line)



module TwoPhaseMoveTables = 
    let private Moves: CoordCube[] = 
        let moves: CoordCube[] = Array.zeroCreate ((int)TwoPhaseConstants.N_MOVE)

        // U?
        let u = 
            let cp = CoordCubeExtensionsModule.FromInversions([| 0uy; 1uy; 1uy; 1uy; 0uy; 0uy; 0uy; 0uy |])
            let ep = CoordCubeExtensionsModule.FromInversions([| 0uy; 1uy; 1uy; 1uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |])
            let co = Array.replicate ((int)TwoPhaseConstants.N_CORNER) 0uy
            let eo = Array.replicate ((int)TwoPhaseConstants.N_EDGE) 0uy
            CoordCube(cp, ep, co, eo)
        moves.[0] <- u

        let mysteryMove3 = 
            let cp = CoordCubeExtensionsModule.FromInversions([| 0uy; 1uy; 1uy; 3uy; 0uy; 1uy; 1uy; 4uy |])
            let ep = CoordCubeExtensionsModule.FromInversions([| 0uy; 1uy; 1uy; 1uy; 0uy; 2uy; 2uy; 2uy; 5uy; 1uy; 1uy; 11uy |])
            let co = [| 2uy; 0uy; 0uy; 1uy; 1uy; 0uy; 0uy; 2uy |]
            let eo = [| 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
            CoordCube(cp, ep, co, eo)
        moves.[3] <- mysteryMove3

        let mysteryMove6 = 
            let cp = CoordCubeExtensionsModule.FromInversions([| 0uy; 0uy; 1uy; 1uy; 4uy; 1uy; 0uy; 0uy |])
            let ep = CoordCubeExtensionsModule.FromInversions([| 0uy; 0uy; 1uy; 1uy; 1uy; 1uy; 2uy; 2uy; 7uy; 4uy; 0uy; 0uy |])
            let co = [| 1uy; 2uy; 0uy; 0uy; 2uy; 1uy; 0uy; 0uy |]
            let eo = [| 0uy; 1uy; 0uy; 0uy; 0uy; 1uy; 0uy; 0uy; 1uy; 1uy; 0uy; 0uy |]
            CoordCube(cp, ep, co, eo)
        moves.[6] <- mysteryMove6

        let mysteryMove9 = 
            let cp = CoordCubeExtensionsModule.FromInversions([| 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 3uy |])
            let ep = CoordCubeExtensionsModule.FromInversions([| 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 3uy; 0uy; 0uy; 0uy; 0uy |])
            let co = [| 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
            let eo = [| 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
            CoordCube(cp, ep, co, eo)
        moves.[9] <- mysteryMove9

        let mysteryMove12 = 
            let cp = CoordCubeExtensionsModule.FromInversions([| 0uy; 0uy; 0uy; 1uy; 1uy; 4uy; 1uy; 0uy |])
            let ep = CoordCubeExtensionsModule.FromInversions([| 0uy; 0uy; 0uy; 1uy; 1uy; 1uy; 1uy; 2uy; 2uy; 7uy; 4uy; 0uy |])
            let co = [| 0uy; 1uy; 2uy; 0uy; 0uy; 2uy; 1uy; 0uy |]
            let eo = [| 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
            CoordCube(cp, ep, co, eo)
        moves.[12] <- mysteryMove12

        // B?
        let mysteryMove15 = 
            let cp = CoordCubeExtensionsModule.FromInversions([| 0uy; 0uy; 0uy; 0uy; 1uy; 1uy; 4uy; 1uy |])
            let ep = CoordCubeExtensionsModule.FromInversions([| 0uy; 0uy; 0uy; 0uy; 1uy; 1uy; 1uy; 1uy; 2uy; 2uy; 7uy; 4uy |])
            let co = [| 0uy; 0uy; 1uy; 2uy; 0uy; 0uy; 2uy; 1uy |]
            let eo = [| 0uy; 0uy; 0uy; 1uy; 0uy; 0uy; 0uy; 1uy; 0uy; 0uy; 1uy; 1uy |]
            CoordCube(cp, ep, co, eo)
        moves.[15] <- mysteryMove15

        for i  in 0 .. 3 .. ((int)TwoPhaseConstants.N_MOVE - 1) do
            let mutable move = moves.[i].DeepClone();

            for j = 1 to 2 do
                move <- CoordCubeMultiply.Multiply(move, moves.[i])
                moves.[i + j] <- move.DeepClone()

        moves

    let Twist: int16[,] = 
        let mutable table: int16[,] = Array2D.zeroCreate ((int)TwoPhaseConstants.N_TWIST) ((int)TwoPhaseConstants.N_MOVE)

        let path = Path.Combine(TwoPhaseConstants.TablePath, "twist_move.file")

        if (TwoPhaseMoveTableHelpers.LoadMoveTableSuccessful(path, ((int)TwoPhaseConstants.N_MOVE), ((int)TwoPhaseConstants.N_TWIST), &table)) then
            table
        else
            let mutable a = CoordCubeExtensionsModule.DefaultCube()

            for i = 0 to ((int)TwoPhaseConstants.N_TWIST - 1) do
                for j = 0 to ((int)TwoPhaseConstants.N_MOVE - 1) do
                    CoordCubeSettersModule.SetTwist(&a, (int16)i)
                    a <- CoordCubeMultiply.Multiply(a, Moves.[j])
                    table.[i, j] <- CoordCubeGetters.GetTwist(a)

            TwoPhaseMoveTableHelpers.SaveMoveTable(path, table)

            table

    let Flip: int16[,] = 
        let mutable table: int16[,] = Array2D.zeroCreate ((int)TwoPhaseConstants.N_FLIP) ((int)TwoPhaseConstants.N_MOVE)

        let path = Path.Combine(TwoPhaseConstants.TablePath, "flip_move.file")

        if (TwoPhaseMoveTableHelpers.LoadMoveTableSuccessful(path, ((int)TwoPhaseConstants.N_MOVE), ((int)TwoPhaseConstants.N_FLIP), &table)) then
            table
        else 
            let mutable a = CoordCubeExtensionsModule.DefaultCube()

            for i = 0 to ((int)TwoPhaseConstants.N_FLIP - 1) do
                for j = 0 to ((int)TwoPhaseConstants.N_MOVE - 1) do
                    CoordCubeSettersModule.SetFlip(&a, (int16)i)
                    a <- CoordCubeMultiply.Multiply(a, Moves.[j])
                    table.[i, j] <- CoordCubeGetters.GetFlip(a)

            TwoPhaseMoveTableHelpers.SaveMoveTable(path, table)

            table
    

    let FRtoBR: int16[,] = 
        let mutable table: int16[,] = Array2D.zeroCreate ((int)TwoPhaseConstants.N_MOVE) ((int)TwoPhaseConstants.N_FRtoBR)

        let path = Path.Combine(TwoPhaseConstants.TablePath, "fr_to_br_move.file")

        if (TwoPhaseMoveTableHelpers.LoadMoveTableSuccessful(path, ((int)TwoPhaseConstants.N_MOVE), ((int)TwoPhaseConstants.N_FRtoBR), &table)) then
            table
        else
            let mutable a = CoordCubeExtensionsModule.DefaultCube()
            
            for i = 0 to ((int)TwoPhaseConstants.N_FRtoBR - 1) do
                for j = 0 to ((int)TwoPhaseConstants.N_MOVE - 1) do
                    let mutable castedI = int16 i
                    CoordCubeSettersModule.SetFRtoBR(&a, &castedI)
                    a <- CoordCubeMultiply.Multiply(a, Moves.[j]);
                    table.[i, j] <- CoordCubeGetters.GetFRtoBR(a);

            TwoPhaseMoveTableHelpers.SaveMoveTable(path, table)

            table

    let URFtoDLF: int16[,] = 
        let mutable table: int16[,] = Array2D.zeroCreate ((int)TwoPhaseConstants.N_MOVE) ((int)TwoPhaseConstants.N_URFtoDLF)

        let path = Path.Combine(TwoPhaseConstants.TablePath, "urf_to_dlf_move.file")

        if (TwoPhaseMoveTableHelpers.LoadMoveTableSuccessful(path, ((int)TwoPhaseConstants.N_MOVE), ((int)TwoPhaseConstants.N_URFtoDLF), &table)) then
            table
        else
            let mutable a = CoordCubeExtensionsModule.DefaultCube()

            for i = 0 to ((int)TwoPhaseConstants.N_URFtoDLF - 1) do
                for j = 0 to ((int)TwoPhaseConstants.N_MOVE - 1) do
                    CoordCubeSettersModule.SetURFtoDLF(&a, int16 i)
                    a <- CoordCubeMultiply.Multiply(a, Moves.[j])
                    table.[i, j] <- CoordCubeGetters.GetURFtoDLF(a)

            TwoPhaseMoveTableHelpers.SaveMoveTable(path, table)

            table

    let URtoUL: int16[,] = 
        let mutable table: int16[,] = Array2D.zeroCreate ((int)TwoPhaseConstants.N_MOVE) ((int)TwoPhaseConstants.N_URtoUL)

        let path = Path.Combine(TwoPhaseConstants.TablePath, "ur_to_ul_move.file")

        if (TwoPhaseMoveTableHelpers.LoadMoveTableSuccessful(path, ((int)TwoPhaseConstants.N_MOVE), ((int)TwoPhaseConstants.N_URtoUL), &table)) then
            table
        else
            let mutable a = CoordCubeExtensionsModule.DefaultCube()

            for i = 0 to ((int)TwoPhaseConstants.N_URtoUL - 1) do
                for j = 0 to ((int)TwoPhaseConstants.N_MOVE - 1) do
                    CoordCubeSettersModule.SetURtoUL(&a, int16 i);
                    a <- CoordCubeMultiply.Multiply(a, Moves.[j]);
                    table.[i, j] <- CoordCubeGetters.GetURtoUL(a);

            TwoPhaseMoveTableHelpers.SaveMoveTable(path, table);

            table

    
    let UBtoDF: int16[,] = 
        let x = ((int)TwoPhaseConstants.N_MOVE)
        let y = ((int)TwoPhaseConstants.N_UBtoDF)
        let mutable table: int16[,] = Array2D.zeroCreate x y

        let path = Path.Combine(TwoPhaseConstants.TablePath, "ub_to_df_move.file")

        if (TwoPhaseMoveTableHelpers.LoadMoveTableSuccessful(path, x, y, &table)) then
            table
        else
            let mutable a = CoordCubeExtensionsModule.DefaultCube()

            for i = 0 to y - 1 do
                for j = 0 to x - 1 do
                    CoordCubeSettersModule.SetUBtoDF(&a, int16 i)
                    a <- CoordCubeMultiply.Multiply(a, Moves.[j])
                    table.[i, j] <- CoordCubeGetters.GetUBtoDF(a)

            TwoPhaseMoveTableHelpers.SaveMoveTable(path, table)

            table

    let URtoDF: int16[,] = 
            
        let x = ((int)TwoPhaseConstants.N_MOVE)
        let y = ((int)TwoPhaseConstants.N_URtoDF)
        
        let mutable table: int16[,] = Array2D.zeroCreate x y

        let path = Path.Combine(TwoPhaseConstants.TablePath, "ur_to_df_move.file")

        if (TwoPhaseMoveTableHelpers.LoadMoveTableSuccessful(path, x, y, &table)) then
            table
        else 
            let mutable a = CoordCubeExtensionsModule.DefaultCube()

            for i = 0 to y - 1 do
                for j = 0 to x - 1 do 
                    CoordCubeSettersModule.SetURtoDF(&a, i)
                    a <- CoordCubeMultiply.Multiply(a, Moves.[j])
                    table.[i, j] <- CoordCubeGetters.GetURtoDF a |> int16

            TwoPhaseMoveTableHelpers.SaveMoveTable(path, table);

            table

     
    let URtoULandUBtoDF: int16[,] = 
        let x = 336
        let y = 336

        let mutable table: int16[,] = Array2D.zeroCreate x y

        let path = Path.Combine(TwoPhaseConstants.TablePath, "merge_move.file")

        if (TwoPhaseMoveTableHelpers.LoadMoveTableSuccessful(path, x, y, &table)) then
            table
        else 
            for uRtoUL = 0 to y - 1 do
                for uBtoDF = 0 to x - 1 do
                    table.[uRtoUL, uBtoDF] <- (int16)(CoordCubeExtensionsModule.GetURtoDF(int16 uRtoUL, int16 uBtoDF))

            TwoPhaseMoveTableHelpers.SaveMoveTable(path, table)

            table

    let ParityMove: int16[,] =
        let mutable table: int16[,] = Array2D.zeroCreate 2 18

        table.[0,*] <- [| 1s; 0s; 1s; 1s; 0s; 1s; 1s; 0s; 1s; 1s; 0s; 1s; 1s; 0s; 1s; 1s; 0s; 1s |]
        table.[1,*] <- [| 0s; 1s; 0s; 0s; 1s; 0s; 0s; 1s; 0s; 0s; 1s; 0s; 0s; 1s; 0s; 0s; 1s; 0s |]

        table

