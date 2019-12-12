open System

type ResultBuilder() =
    member _.Bind(m, f) = Result.bind f m
    member _.Return(v) = Ok v

let result = ResultBuilder()

type RegistrationRequest = { Username: string; Password: string }

type Username = private Username of string
module Username =
    let get (Username x) = x

    let tryCreate candidate =
        if String.IsNullOrWhiteSpace candidate then Error "Username cannot be blank"
        elif candidate.Length > 50 then Error "Username must not be longer than 50 chars"
        else Ok (Username candidate)

type Password = private Password of string
module Password =
    let get (Password x) = x

    let private mustNotBeBlank s =
        if String.IsNullOrWhiteSpace s
        then Error "Password cannot be blank"
        else Ok s

    let private mustBeAtLeast12Chars (s : string) =
        if s.Length < 12
        then Error "Password must be at least 12 chars long"
        else Ok s

    let tryCreate candidate =
        candidate
        |> mustNotBeBlank
        |> Result.bind mustBeAtLeast12Chars
        |> Result.map Password

type Registration = { Username: Username; Password: Password }

let validateRegistrationRequest (request : RegistrationRequest) = result {
    let! username = Username.tryCreate request.Username
    let! password = Password.tryCreate request.Password

    return { Registration.Username = username; Password = password }
}

validateRegistrationRequest { Username = "Chamook"; Password = "abc123aslækdfjja" }
validateRegistrationRequest { Username = "Test"; Password = "abc" }
