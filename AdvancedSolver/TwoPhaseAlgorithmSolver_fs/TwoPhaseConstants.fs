module TwoPhaseAlgorithmSolver.TwoPhaseConstants

// phase 1 coordinates
let [<Literal>] N_TWIST: int16 = 2187s // 3^7 possible corner orientations
let [<Literal>] N_FLIP: int16 = 2048s // 2^11 possible edge flips
let [<Literal>] N_SLICE1: int16 = 495s // 12 choose 4 possible positions of FR, FL, BL, BR edges

// phase 2 coordinates
let [<Literal>] N_SLICE2: int16 = 24s // 4! permutations of FR, FL, BL, BR edges
let [<Literal>] N_PARITY: int16 = 2s // 2 possible corner parities
let [<Literal>] N_URFtoDLF: int16 = 20160s // 8! / (8 - 6)! permutation of URF, UFL, ULB, UBR, DFR, DLF corners
let [<Literal>] N_FRtoBR: int16 = 11880s // 12! / (12 - 4)! permutation of FR, FL, BL, BR edges
let [<Literal>] N_URtoUL: int16 = 1320s // 12! / (12 - 3)! permutation of UR, UF, UL edges
let [<Literal>] N_UBtoDF: int16 = 1320s // 12! / (12 - 3)! permutation of UB, DR, DF edges
let [<Literal>] N_URtoDF: int16 = 20160s // 8! / (8 - 6)! permutation of UR, UF, UL, UB, DR, DF edges

let [<Literal>] N_MOVE: int16 = 18s
let [<Literal>] N_CORNER: int16 = 8s
let [<Literal>] N_EDGE: int16 = 12s

let TablePath = @"tables\"
