# Handling Failure...asynchronously

Sometimes we do things over a network, or by interacting with a file system - so 
they're asynchronous, but also they could fail.

We can combine the Async and Result types in F# to handle these in a familiar way.

## Prerequisites

- [dotnet SDK](https://dotnet.microsoft.com/download)

### Tooling

The _most common option_ is Visual Studio Code with the Ionide extension for F# support:

- [Visual Studio Code](https://code.visualstudio.com/)
- [Ionide](http://ionide.io/) (available in the extensions marketplace in VSCode)

But you're free to choose an editor that you prefer (Visual Studio, Rider, Emacs, Vim, etc.)

## Exercise

When looking for values in a database we can use async to handle the delay, and result 
to handle the potential for failure. These can be combined so that we work with them as 
a composite and don't have to handle the properties of each individually.

### 1. Create a directory to work in

```
mkdir fsharp-4
cd fsharp-4
```

### 2. Create a new script file

```
touch DatabaseLookup.fsx
```

F# allows us to create script files as well as creating full applications, this 
can be evaluated in chunks by your editor, or by using `dotnet fsi`.

### 3. Create a fake database

Or spin up a docker image, populate it with some data, and find the appropriate 
dotnet library for connecting to it I guess...

It's probably easier to copy and paste this code for now:

```fsharp
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
        | None -> return Error "Oh no! the database is down"
    }
```

The `get` function has the type `Guid -> Async<Result<Kitty, string>>`.

### 4. Define some input validation

If we expose this lookup to users, we can enforce that we have a valid id before
we need to query the database. So we'll define a function with the type 
`string -> Result<Guid, string>` to validate input that we receive.

```fsharp
module Guid =
    let tryCreate (candidate : string) =
        match Guid.TryParse candidate with
        | true, guid -> Ok guid
        | false, _ -> Error "Invalid input"
```

F# has special sugar around C# TryParse methods that look like 
`T TryParse(string input, out T output)` - it returns the result as a tuple 
instead of requiring a mutable value to be passed in and modified; that allows 
us to pattern match on the result as shown in this function.


### 5. Add a function to do something with the database result

We can get the favourite toy value for our kitty:

```fsharp
let getFavouriteToy kitty = kitty.FavouriteToy
```

### 6. Make some glue

At this point we have several functions that we'd like to join together, but 
the types don't quite line up. 

- our validation function has the type `string -> Result<Guid, string>`
- our database lookup has the type `Guid -> Async<Result<Kitty, string>>`
- our data processing function has the type `Kitty -> string`

We can't combine these directly, but there are common patterns for making helper
functions that will make the types line up. In this case, we want to preserve all
the effects (rather than converting them to something else) so we'll need to 
work with an `AsyncResult`. For this we'll need a `map` and a `bind` function 
like for `Result` as well as a way to lift a `Result` into an `Async` context.

```fsharp
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
```

### 7. Put it all together

We have functions for each step, and we have glue to bind them together. Now we 
can compose a function to validate input, lookup a kitty in the database, and 
then extract its favourite toy.

```fsharp
let getFavouriteToyForKitty id =
    id
    |> Guid.tryCreate
    |> AsyncResult.fromResult
    |> AsyncResult.bind Database.get
    |> AsyncResult.map getFavouriteToy
```

## Conclusion

You've learned how to work with Asyncs in F#, and how to combine that with 
Results to handle errors at the same time. That's pretty cool, you should feel
good about that.
