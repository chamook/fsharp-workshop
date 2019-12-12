// Learn more about F# at http://fsharp.org

open System

let greet name =
    printfn "Hello %s, I'm an F# program" name

[<EntryPoint>]
let main argv =
    argv |> Array.iter greet
    0 // return an integer exit code
