namespace Shared

open System

type SolvingMethod = 
    | BeginnerMethod = 0
    | TwoPhase = 1

[<CLIMutable>]
type GetSolutionParams = {
    Scramble: string
    SolvingMethod: SolvingMethod
}

type ICounterApi =
    { getSolution: GetSolutionParams -> Async<string> }


module Scramblers = 
    let rnd = Random()

    let scramble333 (turns: string list list) (suffixes: string list) (len: int) =
        let mutable doneMoves = Array.replicate 2 false
        let mutable lastAxis = -1
        let mutable outputScramble = ""

        for j = 0 to len - 1 do
            let mutable isDone = false
            
            while not isDone do
                let axis = 
                    System.Math.Floor(rnd.Next turns.Length |> float) 
                    |> int

                let axisTurns = turns.[axis]

                // if axis changes, clear some things
                if axis <> lastAxis then
                    doneMoves <- Array.replicate 2 false

                lastAxis <- axis

                let turn = System.Math.Floor(rnd.Next axisTurns.Length |> float) |> int

                if doneMoves.[turn] = false then
                    doneMoves.[turn] <- true
                    
                    outputScramble <- 
                        let randomSuffix = suffixes.[rnd.Next suffixes.Length]
                        outputScramble + axisTurns.[turn] + randomSuffix + " "
                        
                    isDone <- true

        outputScramble
        
    let normalScramble = 
        let turns = [ ["U"; "D"]; ["R"; "L"]; ["F"; "B"] ]
        let suffixes = [""; "2"; "'"]
        scramble333 turns suffixes
        
    let twoGenScramble =
        let turns = [ ["U"]; ["R"]]
        let suffixes = [""; "2"; "'"]
        scramble333 turns suffixes
        