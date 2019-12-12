# Handling Failure

F# provides a standard type for dealing with actions that can fail, that makes it
possible to compose several actions together without a lot of boilerplate code.

## Prerequisites

- [dotnet SDK](https://dotnet.microsoft.com/download)

### Tooling

The _most common option_ is Visual Studio Code with the Ionide extension for F# support:

- [Visual Studio Code](https://code.visualstudio.com/)
- [Ionide](http://ionide.io/) (available in the extensions marketplace in VSCode)

But you're free to choose an editor that you prefer (Visual Studio, Rider, Emacs, Vim, etc.)

## Exercise

When receiving data from users there are lots of scenarios where the data received 
might be invalid. In this scenario we will model handling a registration request - 
validating that a username and password meet our requirements and then returning
a validated type or an error.

### 1. Create a directory to work in

```
mkdir fsharp-3
cd fsharp-3
```

### 2. Create a new script file

```
touch UserValidation.fsx
```

F# allows us to create script files as well as creating full applications, this 
can be evaluated in chunks by your editor, or by using `dotnet fsi`.

### 3. Define our request type

```fsharp
type RegistrationRequest = { Username: string; Password: string }
```

This defines both properties as strings, so there aren't many constraints 
enforced on them - we can define types that have constraints enforced by ensuring 
they can only be created if they pass our validation criteria.

### 4. Create a Username type

For a single value like this we can use a single case discriminated union:

```fsharp
type Username = private Username of string
```

By declaring the case as private we enforce that it can only be accessed directly 
from within the same module (or any modules inside that) - then we can create 
functions that allow us to work with the value from outside while ensuring that
our constraints are enforced.

Create a `Username` module and a helper function for extracting the value:

```fsharp
module Username =
    let get (Username x) = x
```

We can pattern match in the parameter list for the function directly to extract 
the value, and then the function returns that value.

This module also needs a function that would allow us to create a Username from
elsewhere in our codebase. We will perform validation in this function to 
ensure that the requirements for a Username are met before one can be created -
which means we will need a mechanism to indicate a validation failure. F# 
provides the `Result` type for this purpose, allowing us to return _either_ 
the valid `Username` _or_ an error. For now, a string describing the failure 
is enough for our error response.

Add this function to the `Username` module:

```fsharp
let tryCreate candidate =
    if String.IsNullOrWhiteSpace candidate then Error "Username cannot be blank"
    elif candidate.Length > 50 then Error "Username must not be longer than 50 chars"
    else Ok (Username candidate)
```

### 5. Create a Password type

The first part of this is very similar to the `Username` type:

```fsharp
type Password = private Password of string
module Password =
    let get (Password x) = x
```

A password is likely to have more validation rules than a username, so instead 
of using one long if statement to check all the constraints we can define a 
function per rule and then use some of the extra functions provided for `Result` 
to compose them together.

First define functions for validating individual password rules:

```fsharp
let private mustNotBeBlank s =
    if String.IsNullOrWhiteSpace s
    then Error "Password cannot be blank"
    else Ok s

let private mustBeAtLeast12Chars (s : string) =
    if s.Length < 12
    then Error "Password must be at least 12 chars long"
    else Ok s
```

Now use the `Result.map` and `Result.bind` functions to compose these together 
and create a `Password` if they all pass.

- `map` takes a `Result` value and a function that works on the success value, 
  and returns another value, and applies the function to the success value of 
  the result if it was successful
- `bind` is similar, but the function that it takes will return a result directly

```fsharp
let tryCreate candidate =
    candidate
    |> mustNotBeBlank
    |> Result.bind mustBeAtLeast12Chars
    |> Result.map Password
```

This function will create a `Password` type if both validation functions pass -
any additional validation functions can be added with another `Result.bind` step
before the last step to create the instance of the type.

### 6. Define a Registration type

```fsharp
type Registration = { Username: Username; Password: Password }
```

This uses our validated types, so it doesn't need any validation of its own - 
but we will need to create a function to go from `RegistrationRequest` to 
`Registration`.

### 7. Result Builder

To build the full validation function we need to take values from 2 results,
and then combine them together if both were successful, otherwise return the 
error. There are various ways of doing that, but in F# we can use computation 
expressions to make the code more readable.

There isn't a predefined computation expression for the result type, but we 
can create a minimal version that will meet our needs here:

```fsharp
type ResultBuilder() =
    member _.Bind(m, f) = Result.bind f m
    member _.Return(v) = Ok v

let result = ResultBuilder()
```

### 8. Validate the whole request

With the result computation expression, we can now create a function to validate 
all our inputs and then create a registration type, and the error handling will be
done by the builder:

```fsharp
let validateRegistrationRequest (request : RegistrationRequest) = result {
    let! username = Username.tryCreate request.Username
    let! password = Password.tryCreate request.Password

    return { Registration.Username = username; Password = password }
}
```

## Conclusion

You've learned how to create constrained types in F# as well as how to use the `Result` 
type to handle failure. Then you saw how computation expressions can be used to 
simplify code around certain concepts.
