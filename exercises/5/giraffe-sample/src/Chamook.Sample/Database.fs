module Database

open Types
open FSharp.Control.Tasks.V2.ContextInsensitive

let readEntry id = task {
    return { Title = "Hello World"; Id = "asdlfkj" }
}
