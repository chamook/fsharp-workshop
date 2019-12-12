module Routing

open System
open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive

let getSingleEntry id =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! entry = Database.readEntry id

            return! negotiate entry next ctx
        }

let webApp: HttpHandler =
    choose [
        GET
        >=> choose [
            route "/health" >=> text "Everything's fine here"
            routef "/entries/%s" getSingleEntry
            ]
        RequestErrors.NOT_FOUND "Damn son"
    ]
