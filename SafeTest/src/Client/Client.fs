module Client

open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Shared
open Fulma
open Fable.PowerPack.Fetch

type Model = {
    Scramble: string
    Solution: string option
    SolvingMethod: SolvingMethod
}

type Msg =
    | UpdateScramble of string
    | UpdateMethod of SolvingMethod
    | GenerateRandomScramble
    | Solve
    | CubeSolved of Result<string, exn>

let init () : Model * Cmd<Msg> =
    let initialModel = { 
        Scramble = ""
        Solution = None
        SolvingMethod = SolvingMethod.BeginnerMethod
    }

    initialModel, Cmd.none

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match msg with
    | UpdateScramble s -> 
        { currentModel with 
            Scramble = s 
            Solution = None}, Cmd.none
    | UpdateMethod m -> 
        printfn "Updating method to %A" m
        { currentModel with
            Solution = None
            SolvingMethod = m }, Cmd.none
    | GenerateRandomScramble ->
        { currentModel with 
            Scramble = Shared.Scramblers.normalScramble 25
            Solution = None }, Cmd.none

    | Solve ->
        let data = {
            Scramble = currentModel.Scramble
            SolvingMethod = currentModel.SolvingMethod
        }

        let defaultProps = [ 
            RequestProperties.Method HttpMethod.POST
            requestHeaders [ContentType "application/json"]
            RequestProperties.Body <| unbox(Thoth.Json.Encode.Auto.toString (4, data))
        ]

        let myCmd =
            Cmd.ofPromise 
                (fetchAs<string> "/api/getSolution" Thoth.Json.Decode.string)
                defaultProps
                (Ok >> CubeSolved)
                (Error >> CubeSolved)

        { currentModel with Solution = None }, myCmd

    | CubeSolved (Ok solution) -> 
        { currentModel with Solution = Some solution }, Cmd.none

    | _ -> 
        // as if software would ever fail...
        currentModel, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    div [] [ 
        Container.container [] [
            str "Scramble:"
            input [ OnChange (fun s -> UpdateScramble s.Value |> dispatch); Value model.Scramble ]

            button
                [ OnClick (fun _ -> GenerateRandomScramble |> dispatch) ]
                [ str "Random!"]

            br []

            select 
                [
                    OnChange (fun ev ->  
                        printfn "%A" (int ev.Value)
                        let inted = int ev.Value
                        let casted = enum<SolvingMethod>(inted)
                        UpdateMethod casted
                            |> dispatch 
                    )
                ] 
                [
                    Fable.Helpers.React.option 
                        [ Value (SolvingMethod.BeginnerMethod.ToString())
                          OnClick (fun _ -> UpdateMethod SolvingMethod.BeginnerMethod |> dispatch )]
                        [ str "Beginner's Method" ]

                    Fable.Helpers.React.option 
                        [ Value (SolvingMethod.TwoPhase.ToString())
                          ]
                        [ str "2-Phase Algorithm"]
                ]


            hr []

            button 
                [ OnClick (fun _ -> Solve |> dispatch)]
                [ str "Solve!"]

            hr []

            str "Solution:"
            br []

            match model.Solution with
                | None -> "No solution"
                | Some solution -> solution
            |> str
        ]
    ]


#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
    #if DEBUG
    |> Program.withConsoleTrace
    |> Program.withHMR
    #endif
    |> Program.withReact "elmish-app"
    #if DEBUG
    |> Program.withDebugger
    #endif
    |> Program.run
