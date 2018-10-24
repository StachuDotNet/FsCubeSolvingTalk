namespace TwoPhaseAlgorithmSolver

open System

type CoordCube(cp: byte array, ep: byte array, co: byte array, eo: byte array) = 
    member val CP = cp with get, set
    member val EP = ep with get, set
    member val CO = co with get, set
    member val EO = eo with get, set

    member this.DeepClone() = CoordCube(this.CP, this.EP, this.CO, this.EO)

module CoordCube = 
    [<Literal>] 
    let N_CORNER: int16 = 8s

    [<Literal>] 
    let N_EDGE: int16 = 12s

    let Empty8x: byte array = Array.zeroCreate <| int N_CORNER
    let Empty12x: byte array = Array.zeroCreate <| int N_EDGE

module CoordCubeGetters = 
    let GetTwist(cube: CoordCube): int16 = 
        let mutable res: int16 = 0s

        for i = 0 to ((int)CoordCube.N_CORNER - 1) do
            res <- res + (int16)((float)cube.CO.[i] * Math.Pow(3.0, (float)((int)CoordCube.N_CORNER - (i + 2))))

        res

    let GetFlip (cube: CoordCube) = 
        cube.EO
            |> Seq.mapi(fun i z -> (float)z * Math.Pow(2.0, (float)((int)CoordCube.N_EDGE - (i + 2))))
            |> Seq.sum
            |> int16
            
    let GetFRtoBR(cube: CoordCube) : int16 =
        let mutable a = 0
        let mutable x = 0

        let edge4: byte array = Array.zeroCreate 4

        for j = 11 downto 0 do
            if (int)cube.EP.[j] >= 9 then
                a <- a + Utils.BinomialCoefficient (11 - j) (x + 1)
                edge4.[3 - x] <- cube.EP.[j]
                x <- x + 1

        let mutable b = 0

        // compute the index b < 4! for the
        // permutation in perm
        for j = 3 downto (0 + 1) do
            let mutable k = 0

            while (int)edge4.[j] - 1 <> j + 8 do
                ArrayHelpers.RotateLeft(edge4, j)
                k <- k + 1

            b <- (j + 1) * b + k
        
        (int16)(24 * a + b)

    let GetUBtoDF(cube: CoordCube) : int16 = 
        let mutable a = 0
        let mutable x = 0

        let edge3: byte array = Array.zeroCreate 3

        for i = 0 to (12-1) do
            if ((int)cube.EP.[i] >= 4 && (int)cube.EP.[i] <= 6) then
                a <- a + Utils.BinomialCoefficient i (x + 1)
                edge3.[x] <- cube.EP.[i]
                x <- x + 1

        let mutable b = 0
        for i = 2 downto 1 do
            let mutable k = 0
            
            while ((int)edge3.[i] - 1) <> (i + 3) do
                ArrayHelpers.RotateLeft(edge3, i)
                k <- k + 1

            b <- (i + 1) * b + k
        
        (int16)(6 * a + b)

    let GetURFtoDLF(cube: CoordCube) : int16 = 
        let mutable a = 0
        let mutable x = 0

        let corner6: byte array = Array.zeroCreate 6

        for i = 0 to ((int)CoordCube.N_CORNER - 1) do
            if ((int)cube.CP.[i] <= 6) then
                a <- a + Utils.BinomialCoefficient i (x + 1)
                corner6.[x] <- cube.CP.[i]
                x <- x + 1

        let mutable b = 0
        for j = 5 downto 1 do
            let mutable k = 0
            
            while (((int)corner6.[j] - 1) <> j) do
                ArrayHelpers.RotateLeft(corner6, j)
                k <- k + 1

            b <- (j + 1) * b + k;

        (int16)(720 * a + b)

    let GetURtoUL(cube: CoordCube ): int16 = 
        let mutable a = 0
        let mutable x = 0
        
        let edge3: byte array = Array.zeroCreate 3

        for i = 0 to 11 do
            if ((int)cube.EP.[i] <= 3) then
                a <- a + Utils.BinomialCoefficient i (x + 1)
                edge3.[x] <- cube.EP.[i]
                x <- x + 1

        let mutable b = 0
        for i = 2 downto 1 do
            let mutable k = 0
            
            while ((int)edge3.[i] - 1 <> i) do
                ArrayHelpers.RotateLeft(edge3, i);
                k <- k + 1

            b <- (i + 1) * b + k
        
        (int16)(6 * a + b)

    let GetURtoDF(cube: CoordCube): int = 
        let mutable a = 0
        let mutable x = 0
        
        let edge6: byte array = Array.zeroCreate 6

        for i = 0 to 11 do
            if ((int)cube.EP.[i] <= 6) then
                a <- a + Utils.BinomialCoefficient i (x + 1)
                edge6.[x] <- cube.EP.[i]
                x <- x + 1

        let mutable b = 0
        for i = 5 downto 1 do
            let mutable k = 0
            while ((int)edge6.[i] - 1) <> i do
                ArrayHelpers.RotateLeft(edge6, i);
                k <- k + 1
            b <- (i + 1) * b + k;
        (720 * a + b);


module CoordCubeSettersModule = 
    let SetTwist(cube: byref<CoordCube>, value: int16): unit = 
        let mutable sum = 0
        let mutable value = value

        for i = 0 to ((int)CoordCube.N_CORNER - 2) do
            let divisor = (int)(Math.Pow(3.0, (float)((int)CoordCube.N_CORNER - (i + 2))))

            cube.CO.[i] <- (byte)((int)value / divisor)
            sum <- sum + ((int)value / divisor)
            value <- (int16)((int)value % divisor)

        cube.CO.[(int)CoordCube.N_CORNER - 1] <- (byte)((3 - sum % 3) % 3)

    let SetFlip(cube: byref<CoordCube>, value:int16) : unit = 
        let mutable sum = 0
        let mutable value = value

        for i = 0 to ((int)CoordCube.N_EDGE - 2) do
            let divisor = 
                let rhs: float = (int)CoordCube.N_EDGE - (i + 2) |> float
                Math.Pow(2.0, rhs) |> int

            cube.EO.[i] <- (byte)((int)value / divisor)
            sum <- sum + ((int)value / divisor)
            value <- (int16)((int)value % divisor)

        cube.EO.[(int)CoordCube.N_EDGE - 1] <- (byte)((2 - sum % 2) % 2)
    
    let SetFRtoBR(cube: byref<CoordCube>, value: byref<int16>): unit = 
        let mutable x = 0
        let edge4: byte[] = [|9uy; 10uy; 11uy; 12uy|]
        let otherEdges: byte[] = [|1uy; 2uy; 3uy; 4uy; 5uy; 6uy; 7uy; 8uy|]
 
        let mutable b = (int)value % 24
 
        let mutable a = (int)value / 24
        cube.EP <- Array.replicate ((int)CoordCube.N_EDGE) 8uy

        for i = 1 to 4-1 do
            let mutable k = b % (i + 1)
            b <- b / (i + 1)

            while k > 0 do
                k <- k - 1
                ArrayHelpers.RotateRight(edge4, 0, i)

        x <- 3

        for i = 0 to 11 do
            if (a - (Utils.BinomialCoefficient (11 - i) (x + 1)) >= 0) then
                cube.EP.[i] <- edge4.[3 - x]
                a <- a - (Utils.BinomialCoefficient (11 - i) (x + 1))
                x <- x - 1

        x <- 0

        for j = 0 to 11 do
            if (int)cube.EP.[j] = 8 then
                cube.EP.[j] <- otherEdges.[x]
                x <- x + 1
    
    let SetUBtoDF(cube: byref<CoordCube>, value: int16): unit = 
        let edge3: byte[]= [| 4uy; 5uy; 6uy |]
        let mutable b = (int)value % 6
        let mutable a = (int)value / 6
        cube.EP <- Array.replicate ((int)CoordCube.N_EDGE) 12uy

        for i = 1 to 2 do
            let mutable k = b % (i + 1)
            b <- b / (i + 1)
            
            while k > 0 do
                k <- k - 1
                ArrayHelpers.RotateRight(edge3, 0, i)

        let mutable x = 2;
        for i = 11 downto 0 do
            if (a - Utils.BinomialCoefficient i (x + 1)) >= 0 then
                cube.EP.[i] <- edge3.[x]
                a <- a - Utils.BinomialCoefficient i (x + 1)
                x <- x - 1

    let SetURtoUL(cube: byref<CoordCube>, value: int16):unit = 
        let edge3 = [| 1uy; 2uy; 3uy |]
        let mutable b = (int)value % 6
        let mutable a = (int)value / 6
        cube.EP <- Array.replicate ((int)CoordCube.N_EDGE) 12uy

        for i = 1 to 2 do
            let mutable k = b % (i + 1)
            b <- b / (i + 1)
            while k > 0 do
                k <- k - 1
                ArrayHelpers.RotateRight(edge3, 0, i);

        let mutable x = 2
        for i = 11 downto 0 do
            if (a - Utils.BinomialCoefficient i (x + 1)) >= 0 then
                cube.EP.[i] <- edge3.[x];
                a <- a - Utils.BinomialCoefficient i (x + 1)
                x <- x - 1

                
    let SetURFtoDLF(cube: byref<CoordCube>, value: int16) = 
        let corner6 = [|1uy; 2uy; 3uy; 4uy; 5uy; 6uy|]
        let otherCorner = [|7uy; 8uy|]
        let mutable b = (int)value % 720
        let mutable a = (int)value / 720
        cube.CP <- Array.replicate ((int)CoordCube.N_CORNER) 8uy

        for i = 1 to (6-1) do
            let mutable k = b % (i + 1)
            b <- b / (i + 1)
            while k > 0 do
                k <- k - 1
                ArrayHelpers.RotateRight(corner6, 0, i);
        
        let mutable x = 5
        for i = 7 downto 0 do
            if (a - Utils.BinomialCoefficient i (x + 1))>= 0 then
                cube.CP.[i] <- corner6.[x]
                a <- a - Utils.BinomialCoefficient i (x + 1)
                x <- x - 1
        
        let mutable x = 0
        for j = 0 to (8-1) do
            if ((int)cube.CP.[j] = 8) then
                cube.CP.[j] <- otherCorner.[x]
                x <- x + 1
    
    let SetURtoDF(cube: byref<CoordCube>, value: int): unit = 
        let edge6 = [|1uy; 2uy; 3uy; 4uy; 5uy; 6uy|]
        let otherEdges = [|7uy; 8uy; 9uy; 10uy; 11uy; 12uy|]
        let mutable b = (int)value % 720;
        let mutable a = (int)value / 720;

        cube.EP <- Array.replicate ((int)CoordCube.N_EDGE) 12uy

        for i = 1 to (6-1) do
            let mutable k = b % (i + 1)
            b <- b / (i + 1)
            while k > 0 do
                k <- k - 1
                ArrayHelpers.RotateRight(edge6, 0, i)

        let mutable x = 5
        for i = 11 downto 0 do
            if (a - Utils.BinomialCoefficient i (x + 1)) >= 0 then
                cube.EP.[i] <- edge6.[x]
                a <- a - Utils.BinomialCoefficient i (x + 1)
                x <- x - 1
        
        let mutable x = 0
        for i = 1 to (12-1) do
            if (int)cube.EP.[i] = 12 then
                cube.EP.[i] <- otherEdges.[x]
                x <- x + 1
    
module CoordCubeExtensionsModule = 
    let FromInversions (perm: byte[]): byte[] = 
        let cubies: byte[] = Array.zeroCreate perm.Length
        let mutable upperBound = (byte)perm.Length
        let mutable lowerBound = 1uy

        let mutable cancelled = false
        let mutable not_ = false

        for index = (perm.Length - 1) downto 0 do
            if (not not_) then
                if index = (int)perm.[index] then
                    cubies.[index] <- lowerBound
                    lowerBound <- lowerBound + 1uy
                    not_ <- true

            if not cancelled then
                if (int)perm.[index] = 0 then
                    cubies.[index] <- upperBound
                    upperBound <- upperBound - 1uy
                    cancelled <- true

        while upperBound >= lowerBound do
            let mutable cancelled2 = false

            for i = (perm.Length - 1) downto 0 do
                if (int)cubies.[i] = 0 && (not cancelled2) then
                    let mutable count = 0
                    for j = 0 to (i-1) do
                        if cubies.[j] > upperBound then
                            count <- count + 1

                    if count = (int)perm.[i] then
                        cubies.[i] <- upperBound
                        upperBound <- upperBound - 1uy
                        cancelled2 <- true

        cubies

    let DefaultCube() = 
        CoordCube(
            FromInversions(CoordCube.Empty8x),
            FromInversions(CoordCube.Empty12x),
            CoordCube.Empty8x,
            CoordCube.Empty12x
        );
    
    let Parity(cube: byref<CoordCube>): int16 =
        let mutable s = 0

        for i = 7 downto 1 do
            for j = (i-1) downto 0 do
                if cube.CP.[j] > cube.CP.[i] then
                    s <- s + 1

        (int16)(s % 2)


    let GetURtoDF(idx1: int16, idx2: int16): int = 
        let mutable a = DefaultCube()
        let mutable b = DefaultCube()

        CoordCubeSettersModule.SetURtoUL(&a, idx1)
        CoordCubeSettersModule.SetUBtoDF(&b, idx2)

        let mutable earlyReturn = false

        for i = 0 to (8-1) do
            if (not earlyReturn) && (int)a.EP.[i] <> 12 then
                if ((int)b.EP.[i] <> 12) then
                    earlyReturn <- true
                else
                    b.EP.[i] <- a.EP.[i]

        if earlyReturn then 
            -1
        else
            CoordCubeGetters.GetURtoDF(b)


module CoordCubeMultiply =
    let private multiply(permA: byte[],  orieA: byte[],  permB: byte[],  orieB: byte[]) = 
        
        let AxB : byte[] = Array.zeroCreate permA.Length
        let AxBo : byte[] = Array.zeroCreate permA.Length

        let _mod = if permA.Length = 8 then 3 else 2

        for i = 0 to (permA.Length - 1) do 
            // (A*B)(x).c=A(B(x).c).c
            AxB.[i] <- permA.[(int)permB.[i] - 1]

            // (A*B)(x).o=A(B(x).c).o+B(x).o
            AxBo.[i] <- (byte)(((int)orieA.[(int)permB.[i] - 1] + (int)orieB.[i]) % _mod);
        
        AxB, AxBo

    let Multiply(a: CoordCube, b: CoordCube): CoordCube = 
        let ep, eo = multiply(a.EP, a.EO, b.EP, b.EO)
        let cp, co = multiply(a.CP, a.CO, b.CP, b.CO)

        CoordCube(cp, ep, co, eo)
