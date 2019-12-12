open System

type Kitty = { Id: Guid; Name: string; Colour: string; FavouriteToy: string }

module Database =
    let private data =
        [|
           { Id = Guid.Parse "22c5078b-4997-49c8-a160-445fb6c5b5ab"
             Name = "Mittens"
             Colour = "Black"
             FavouriteToy = "Mouse" }
           { Id = Guid.Parse "ef8d587a-2e40-4368-90ff-4350254ccf45"
             Name = "Rocky"
             Colour = "Brown"
             FavouriteToy = "Rabbit" }
           { Id = Guid.Parse "89718c4f-cd29-430f-99e2-b5c8be3b1742"
             Name = "Susie"
             Colour = "White"
             FavouriteToy = "Teddy" }
        |]

    let get id = async {
        let kitty = Array.tryFind (fun x -> x.Id = id) data

        match kitty with
        | Some k -> return Ok k
        | None -> return Error "Could not find kitty"
    }

module Guid =
    let tryCreate (candidate : string) =
        match Guid.TryParse candidate with
        | true, guid -> Ok guid
        | false, _ -> Error "Invalid input"

let getFavouriteToy kitty = kitty.FavouriteToy

[<RequireQualifiedAccess>]
module AsyncResult =
    let fromResult (x : Result<_,_>) = async.Return x

    let map f x = async {
        let! x = x

        return Result.map f x
    }

    let bind (f : _ -> Async<Result<_,_>) x = async {
        match! x with
        | Ok x -> return! f x
        | Error e -> return! Error e
    }

let getFavouriteToyForKitty id =
    id
    |> Guid.tryCreate
    |> AsyncResult.fromResult
    |> AsyncResult.bind Database.get
    |> AsyncResult.map getFavouriteToy
