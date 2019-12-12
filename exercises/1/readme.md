# In the beginning...

This exercise will ensure that you have dotnet & F# tooling properly configured - then
you'll create your first F# application.

## Prerequisites

- [dotnet SDK](https://dotnet.microsoft.com/download)

### Tooling

The _most common option_ is Visual Studio Code with the Ionide extension for F# support:

- [Visual Studio Code](https://code.visualstudio.com/)
- [Ionide](http://ionide.io/) (available in the extensions marketplace in VSCode)

But you're free to choose an editor that you prefer (Visual Studio, Rider, Emacs, Vim, etc.)

## Exercise

This will start with a _hello world_ application, and then expand it to provide an 
example of defining a function and applying it to values.

### 1. Create a directory to work in

```
mkdir fsharp-1
cd fsharp-1
```

### 2. Use the dotnet CLI to create a new F# console application

```
dotnet new console -lang F#
```

### 3. Build and run the application

```
dotnet build
dotnet run
```

The output from the last command should be:

```
Hello World from F#!
```

### 4. Declare a new function

Open the file `Program.fs` in your editor of choice, it should look like this:

```fsharp
// Learn more about F# at http://fsharp.org

open System

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    0 // return an integer exit code
```

Now add the following function at line 4 (in between the open declaration 
and the entrypoint attribute) - remember that whitespace is significant in F#,
so be careful with indentation.

```fsharp
let greet name =
    printfn "Hello %s, I'm an F# program" name
```

If your editor shows you function signatures, this should have the type:

```
string -> unit
```

### 5. Apply the function to an input value

Modify the main function to the following:

```fsharp
[<EntryPoint>]
let main argv =
    argv |> Array.iter greet
    0 // return an integer exit code
```

`Array.iter` will iterate over all the values in the input arguments array (`argv`)
and apply the function `greet` to them.

### 6. Build and run

Save the file, and then build it from the CLI:

```
dotnet build
```

Running the program without any input won't produce any output, but you can now pass 
it some names to get a greeting:

```
dotnet run Adam Anna
```

should output the following:

```
Hello Adam, I'm an F# program
Hello Anna, I'm an F# program
```

## Conclusion

You're officially an F# programmer, congratulations!

You learned how to create a new F# application, and how to define a function and
apply it to a collection of values.
