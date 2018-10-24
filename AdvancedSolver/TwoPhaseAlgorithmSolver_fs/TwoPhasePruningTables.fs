module TwoPhasePruningTableModule

open System.IO
open System
open System.Linq
open System.Runtime.InteropServices
open TwoPhaseAlgorithmSolver

let LoadPruningTable(filename: string, length: int): byte[] = 
    let newBytes: byte[] = File.ReadAllBytes(filename)

    if newBytes.Length <> length then
        failwith "Invalid input file"

    newBytes

    
let LoadPruningTableSuccessful(filename: string, length: int, [<Out>] newTable: byref<byte[]>): bool = 
    newTable <- Array.zeroCreate length

    try
        newTable <- LoadPruningTable(filename, length)
        true
    with
        | :? Exception as ex -> false


let SavePruningTable(filename: string, table: byte[]): unit = 
    File.WriteAllBytes(filename, table);


// TODO: make private
let GetPruning(table: byte[], index: int): byte = 
    if (index &&& 1) = 0 then
        (byte)((int)table.[index / 2] &&& (int)0x0F)
    else
        (byte)(((int)table.[index / 2] &&& (int)0xF0) >>> 4)
        
let SetPruning( table: byte[], index: int, value: byte): unit = 
    if (index &&& 1) = 0 then
        table.[index / 2] <- table.[index / 2] &&& (byte)((int)0xF0 ||| (int)value)
    else
        table.[index / 2] <- table.[index / 2] &&& (byte)(0x0F ||| ((int)value <<< 4))


// exposed:
let SliceFlip: byte[] = 
    let mutable table: byte[] = Array.zeroCreate ((int)TwoPhaseConstants.N_SLICE1 * (int)TwoPhaseConstants.N_FLIP / 2)

    let path = Path.Combine(TwoPhaseConstants.TablePath, "slice_flip_prun.file");

    if LoadPruningTableSuccessful(path, (int)TwoPhaseConstants.N_SLICE1 * (int)TwoPhaseConstants.N_FLIP / 2, &table) then
        table
    else
        for i = 0 to ((int)TwoPhaseConstants.N_SLICE1 * (int)TwoPhaseConstants.N_FLIP / 2) - 1 do
            table.[i] <- 0xFFuy // = -1 for signed byte

        let mutable depth = 0
        SetPruning(table, 0, (byte)0)
        let mutable isDone = 1

        while isDone <> (int)TwoPhaseConstants.N_SLICE1 * (int)TwoPhaseConstants.N_FLIP do
            for i = 0 to ((int)TwoPhaseConstants.N_SLICE1 * (int)TwoPhaseConstants.N_FLIP) - 1 do
                let flip: int = i / (int)TwoPhaseConstants.N_SLICE1
                let slice: int = i % (int)TwoPhaseConstants.N_SLICE1

                if (int)(GetPruning(table, i)) = depth then
                    for j = 0 to (int)TwoPhaseConstants.N_MOVE - 1 do
                        let newSlice: int = (int)TwoPhaseMoveTables.FRtoBR.[slice * 24, j] / 24
                        let newFlip: int = (int)TwoPhaseMoveTables.Flip.[flip, j]

                        if (GetPruning(table, (int)TwoPhaseConstants.N_SLICE1 * newFlip + newSlice) = 0x0Fuy) then
                            SetPruning(table, (int)TwoPhaseConstants.N_SLICE1 * newFlip + newSlice, (byte)(depth + 1));
                            isDone <- isDone + 1

            depth <- depth + 1

        SavePruningTable(path, table)

        table



let SliceTwist: byte[] =
    let mutable table = Array.zeroCreate ((int)TwoPhaseConstants.N_SLICE1 * (int)TwoPhaseConstants.N_FLIP / 2)

    let path = Path.Combine(TwoPhaseConstants.TablePath, "slice_twist_prun.file");

    if (LoadPruningTableSuccessful(path, (int)TwoPhaseConstants.N_SLICE1 * (int)TwoPhaseConstants.N_TWIST / 2 + 1, &table)) then
        table
    else 
        for i = 0 to (int)TwoPhaseConstants.N_SLICE1 * (int)TwoPhaseConstants.N_TWIST / 2 + 1 - 1 do
            table.[i] <- 0xFFuy // = -1 for signed byte

        let mutable depth = 0;
        SetPruning(table, 0, 0uy);
        let mutable isDone = 1;

        while isDone <> (int)TwoPhaseConstants.N_SLICE1 * (int)TwoPhaseConstants.N_TWIST do
            for i = 0 to (int)TwoPhaseConstants.N_SLICE1 * (int)TwoPhaseConstants.N_TWIST - 1 do
                let twist: int = i / (int)TwoPhaseConstants.N_SLICE1
                let slice: int = i % (int)TwoPhaseConstants.N_SLICE1

                if (int)(GetPruning(table, i)) = depth then
                    for j = 0 to (int)TwoPhaseConstants.N_MOVE - 1 do
                        let newSlice: int = (int)TwoPhaseMoveTables.FRtoBR.[slice * 24, j] / 24
                        let newTwist: int = (int)TwoPhaseMoveTables.Twist.[twist, j]

                        if (GetPruning(table, (int)TwoPhaseConstants.N_SLICE1 * newTwist + newSlice) = 0x0Fuy) then
                            SetPruning(table, (int)TwoPhaseConstants.N_SLICE1 * newTwist + newSlice, (byte)(depth + 1))
                            isDone <- isDone + 1
        
            depth <- depth + 1

        SavePruningTable(path, table)

        table


let SliceURFtoDLF: byte[] = 
    let mutable table: byte[] = Array.zeroCreate ((int)TwoPhaseConstants.N_SLICE2 * (int)TwoPhaseConstants.N_URFtoDLF * (int)TwoPhaseConstants.N_PARITY / 2)

    let path = Path.Combine(TwoPhaseConstants.TablePath, "slice_urf_to_dlf_prun.file")

    if (LoadPruningTableSuccessful(path, (int)TwoPhaseConstants.N_SLICE2 * (int)TwoPhaseConstants.N_URFtoDLF * (int)TwoPhaseConstants.N_PARITY / 2, &table)) then
        table
    else
        for i = 0 to (int)TwoPhaseConstants.N_SLICE2 * (int)TwoPhaseConstants.N_URFtoDLF * (int)TwoPhaseConstants.N_PARITY / 2 - 1 do
            table.[i] <- 0xFFuy; // -1

        let mutable depth = 0
        SetPruning(table, 0, 0uy)
        let mutable isDone = 1
        let forbidden: int[] = [| 3; 5; 6; 8; 12; 14; 15; 17 |]

        while isDone < ((int)TwoPhaseConstants.N_SLICE2 * (int)TwoPhaseConstants.N_URFtoDLF * (int)TwoPhaseConstants.N_PARITY) do
            for i = 0 to (int)TwoPhaseConstants.N_SLICE2 * (int)TwoPhaseConstants.N_URFtoDLF * (int)TwoPhaseConstants.N_PARITY - 1 do
                let parity: int = i % 2
                let URFtoDLF: int = (i / 2) / (int)TwoPhaseConstants.N_SLICE2
                let slice: int = (i / 2) % (int)TwoPhaseConstants.N_SLICE2
                let prun: byte = GetPruning(table, i)
     
                if (int)prun = depth then
                    for j = 0 to 18 - 1 do 
                        if not <| forbidden.Contains(j) then
                            let newSlice: int = (int)TwoPhaseMoveTables.FRtoBR.[slice, j] % 24
                            let newURFtoDLF: int = (int)TwoPhaseMoveTables.URFtoDLF.[URFtoDLF, j]
                            let newParity: int = (int)TwoPhaseMoveTables.ParityMove.[parity, j]

                            if (GetPruning(table, ((int)TwoPhaseConstants.N_SLICE2 * newURFtoDLF + newSlice) * 2 + newParity) = 0x0Fuy) then
                                SetPruning(table, ((int)TwoPhaseConstants.N_SLICE2 * newURFtoDLF + newSlice) * 2 + newParity, (byte)(depth + 1))
                                isDone <- isDone + 1

            depth <- depth + 1

        SavePruningTable(path, table);

        table


let SliceURtoDF: byte[] = 
    let mutable table: byte[] = Array.zeroCreate ((int)TwoPhaseConstants.N_SLICE2 * (int)TwoPhaseConstants.N_URtoDF * (int)TwoPhaseConstants.N_PARITY / 2)

    let path = Path.Combine(TwoPhaseConstants.TablePath, "slice_ur_to_df_prun.file")

    if (LoadPruningTableSuccessful(path, (int)TwoPhaseConstants.N_SLICE2 * (int)TwoPhaseConstants.N_URtoDF * (int)TwoPhaseConstants.N_PARITY / 2, &table)) then
        table
    else
        for i = 0 to (int)TwoPhaseConstants.N_SLICE2 * (int)TwoPhaseConstants.N_URtoDF * (int)TwoPhaseConstants.N_PARITY / 2 - 1 do
            table.[i] <- 0xFFuy; // = -1 for signed byte

        let mutable depth = 0
        SetPruning(table, 0, 0uy)
        let mutable isDone = 1

        while isDone <> ((int)TwoPhaseConstants.N_SLICE2 * (int)TwoPhaseConstants.N_URtoDF * (int)TwoPhaseConstants.N_PARITY) do
            let forbidden: int[] = [| 3; 5; 6; 8; 12; 14; 15; 17 |]

            for i = 0 to (int)TwoPhaseConstants.N_SLICE2 * (int)TwoPhaseConstants.N_URtoDF * (int)TwoPhaseConstants.N_PARITY - 1 do
                let parity: int = i % 2
                let URtoDF: int = (i / 2) / (int)TwoPhaseConstants.N_SLICE2
                let slice: int = (i / 2) % (int)TwoPhaseConstants.N_SLICE2

                if (int)(GetPruning(table, i)) = depth then
                    for j = 0 to 18 - 1 do
                        if not <| forbidden.Contains(j) then
                            let newSlice: int = (int)TwoPhaseMoveTables.FRtoBR.[slice, j] % 24
                            let newURtoDF: int = (int)TwoPhaseMoveTables.URtoDF.[URtoDF, j];
                            let newParity: int = (int)TwoPhaseMoveTables.ParityMove.[parity, j]

                            if (GetPruning(table, ((int)TwoPhaseConstants.N_SLICE2 * newURtoDF + newSlice) * 2 + newParity) = 0x0Fuy) then
                                SetPruning(table, ((int)TwoPhaseConstants.N_SLICE2 * newURtoDF + newSlice) * 2 + newParity, (byte)(depth + 1))
                                isDone <- isDone + 1

            depth <- depth + 1

        SavePruningTable(path, table)

        table
