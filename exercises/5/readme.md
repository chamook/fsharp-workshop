# Building a web application with Giraffe

![giraffe](https://media.giphy.com/media/HdAU3C49OtFKw/giphy.gif)

Moving beyond single file, toy applications - this exercise will go through the 
first steps in setting up a web application using giraffe.

Giraffe is a library for creating web applications that runs on top of Kestrel as
part of the asp.net stack, it replaces MVC with something that is nicer to work
with from F#.

## Prerequisites

- [dotnet SDK](https://dotnet.microsoft.com/download)

### Tooling

The _most common option_ is Visual Studio Code with the Ionide extension for F# support:

- [Visual Studio Code](https://code.visualstudio.com/)
- [Ionide](http://ionide.io/) (available in the extensions marketplace in VSCode)

But you're free to choose an editor that you prefer (Visual Studio, Rider, Emacs, Vim, etc.)

## Exercise

This will create a basic giraffe application with a simple route and a fake database
to give an idea of how to structure such a service.

### 1. Create a directory to work in

```
mkdir giraffe-sample
cd giraffe-sample
```

Optionally, create subdirectories for separating the application and its tests - 
this guide will not cover creating tests but the completed solution does include 
basic test setup.

For the rest of the steps, you should be working in the directory where you want
to keep your application code.

### 2. Create the project

Create a new empty web application from the dotnet CLI:

```
dotnet new web -lang F#
```

and add a reference to giraffe:

```
dotnet add package giraffe
```

remove the `Startup.fs` file, and create a new file called `Routing.fs` note that 
you will need to update the files in the `fsproj` file as the compiler needs an 
ordered lists of files to compile (depending on your editor, it may do this for 
you) - the files listed in the project file should look like this:

```xml
<ItemGroup>
  <Compile Include="Routing.fs" />
  <Compile Include="Program.fs" />
</ItemGroup>
```

### 3. Configure the program startup

To configure the application to use giraffe, use the following code (this does not 
include all the `open` statements needed, but the completed example does include 
them):

```fsharp
let configureApp (app : IApplicationBuilder) =
    app.UseGiraffe Routing.webApp
    
let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore
    
let getWebHostBuilder args =
    WebHost
        .CreateDefaultBuilder(args)
        .UseKestrel(fun c -> c.AddServerHeader <- false)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)

[<EntryPoint>]
let main args =
    getWebHostBuilder(args).Build().Run()

    0
```

### 4. Add a health endpoint

Place this code in `Routing.fs`:

```fsharp
module Routing

open System
open Giraffe

let webApp: HttpHandler =
    choose [
        GET >=> route "/health" >=> text "Everything's fine here"
        RequestErrors.NOT_FOUND "Damn son"
    ]
```

`webApp` is where you will define any other routes for the application - it uses
some giraffe features for defining routes:

- `choose` will select the first matching route from the list, can be nested and 
  used after other filters
- `>=>` the _fish operator_ (it looks a little bit like a fish, if you've never 
  really seen a fish before) is used to combine different handlers - mostly 
  filters - together to create a route
- `GET` is a filter that checks the http verb used as part of the request, GET 
  requests go through
- `route "/health"` filters the path that the request is made to
- `text "Everything's fine here" ` will respond with 200 OK and the specified 
  text
  
### 5. Define types and a "database"

Add a new file called `Types.fs` (remember to add it to the project) and put the
following code in it:

```fsharp
module Types

type Entry = { Title : string; Id : string }
```

Add a new file called `Database.fs` and put this code in it:

```fsharp
module Database

open Types
open FSharp.Control.Tasks.V2.ContextInsensitive

let readEntry id = task {
    return { Title = "Hello World"; Id = "asdlfkj" }
}
```

Note that `Types.fs` _must_ be before `Database.fs` in the project file.

### 6. Add a route to get a value from the database

Modify the code in `Routing.fs` to include a function for the new endpoint, 
and then add it to the `webApp`:

```fsharp
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
```

### 7. Try it out!

You should now be able to run the application with `dotnet run` and make a GET 
request to the available endpoints:

```http
GET http://localhost:5000/health
```

should return:

```
200 OK
Content-Type: text/plain

Everything's fine here
```

```http
GET http://localhost:5000/entries/asdlfkj
```

should return:

```
200 OK
Content-Type: application/json

{
    "title" : "Hello World",
    "id" : "asdlfkj"
}
```

and requests to other paths should return a 404.

## Conclusion

You've learned how to setup a basic giraffe application, combined with your knowledge 
of programming in F# from the other exercises the possibilities now are nearly 
limitless!
