module RubiksCubeLib.CollectionMethods

open System.Collections.Generic
open System.Linq

/// <summary> Returns true if both lists are equal even if they are scrambled (i.e. in a different order) </summary>
/// <typeparam name="T">Defines the type of both lists</typeparam>
/// <param name="list1">Defines the first list to be analyzed</param>
/// <param name="list2">Defines the second list to be analyzed</param>
let ScrambledEquals<'T when 'T : equality> (list1: IEnumerable<'T>) (list2: IEnumerable<'T>) = 
    let cnt = new Dictionary<'T, int>()

    list1 |> Seq.iter (fun s ->
        if cnt.ContainsKey(s) then
            cnt.[s] <- cnt.[s] + 1
        else
            cnt.Add(s, 1)
    )

    let mutable index = 0
    let iteratedList = list2.ToList()
    let mutable returnEarly = false;

    while not returnEarly && index < iteratedList.Count do
        let key = iteratedList.[index]

        if cnt.ContainsKey(key) then
            cnt.[key] <- cnt.[key] - 1
        else
            returnEarly <- true

        index <- index + 1

    if returnEarly then
        false
    else
        cnt.Values.All(fun c -> c = 0)