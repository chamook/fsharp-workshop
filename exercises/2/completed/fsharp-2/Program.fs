open System

// Tennis
type Player = PlayerOne | PlayerTwo

type Score =
    | Points of PointsData
    | Forty of FortyData
    | Deuce
    | Advantage of Player
    | Game of Player
and PointsData =  { PlayerOnePoint: Point; PlayerTwoPoint: Point }
and Point = Love | Fifteen | Thirty
and FortyData = { Player: Player; OtherPlayerPoint: Point }

let other = function | PlayerOne -> PlayerTwo | PlayerTwo -> PlayerOne

let incrementPoint = function
    | Love -> Some Fifteen
    | Fifteen -> Some Thirty
    | Thirty -> None

let pointTo player point current =
    match player with
    | PlayerOne -> { current with PlayerOnePoint = point }
    | PlayerTwo -> { current with PlayerTwoPoint = point }
let pointFor player current =
    match player with
    | PlayerOne -> current.PlayerOnePoint
    | PlayerTwo -> current.PlayerTwoPoint

let scoreWhenGame = Game
let scoreWhenAdvantage winner current =
    if winner = current then Game winner else Deuce
let scoreWhenDeuce = Advantage
let scoreWhenForty winner current =
    if current.Player = winner
    then Game winner
    else
        match incrementPoint current.OtherPlayerPoint with
        | Some points -> Forty { current with OtherPlayerPoint = points }
        | _ -> Deuce
let scoreWhenPoints winner current =
    match pointFor winner current |> incrementPoint with
    | Some points -> pointTo winner points current |> Points
    | None -> Forty { Player = winner; OtherPlayerPoint = pointFor (other winner) current }

let score current winner =
    match current with
    | Points p -> scoreWhenPoints winner p
    | Forty f -> scoreWhenForty winner f
    | Deuce -> scoreWhenDeuce winner
    | Advantage p -> scoreWhenAdvantage winner p
    | Game w -> scoreWhenGame w

let newGame = Points { PlayerOnePoint = Love; PlayerTwoPoint = Love }

// Formatting
let formatPoints = function
    | Love -> "Love"
    | Fifteen -> "15"
    | Thirty -> "30"
let formatForty { Player = player; OtherPlayerPoint = point } =
    match player with
    | PlayerOne -> sprintf "Player One 40 : %s Player Two" (point |> formatPoints)
    | PlayerTwo -> sprintf "Player One %s : 40 Player Two" (point |> formatPoints)
let formatAdvantage player =
    match player with
    | PlayerOne -> "Player One 40A : 40 Player Two"
    | PlayerTwo -> "Player One 40 : 40A Player Two"
let formatGame winner =
    match winner with
    | PlayerOne -> "Player One Wins!"
    | PlayerTwo -> "Player Two Wins!"

let formatScore current =
    match current with
    | Points p ->
        sprintf
            "Player One %s : %s Player Two"
            (formatPoints p.PlayerOnePoint)
            (formatPoints p.PlayerTwoPoint)
    | Forty f -> formatForty f
    | Deuce -> "Player One DEUCE Player Two"
    | Advantage p -> formatAdvantage p
    | Game w -> formatGame w

// Interaction
let parseWinner s =
    if s = "1" then Some PlayerOne
    elif s = "2" then Some PlayerTwo
    else None

let rec play current =
    match current with
    | Game x -> printfn "%s" (formatGame x)
    | _ ->
        printfn "%s" (formatScore current)
        printfn "%s" "Who won?"
        match Console.ReadLine() |> parseWinner with
        | Some w -> play (score current w)
        | None -> play current

[<EntryPoint>]
let main argv =
    printfn "%s" "Let's play tennis!"
    play newGame
    0

