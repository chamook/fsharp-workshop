# A Nice Game of Tennis

This exercise covers modelling a domain using the F# type system to help us by 
restricting the states that can be represented.

## Prerequisites

- [dotnet SDK](https://dotnet.microsoft.com/download)
- An understanding of the rules of tennis

### Tooling

The _most common option_ is Visual Studio Code with the Ionide extension for F# support:

- [Visual Studio Code](https://code.visualstudio.com/)
- [Ionide](http://ionide.io/) (available in the extensions marketplace in VSCode)

But you're free to choose an editor that you prefer (Visual Studio, Rider, Emacs, Vim, etc.)

## Exercise

Tennis has a deceptively complicated scoring system that can be difficult to model properly.
Players start with 0 points, but this is called _love_ - then go up to 15, and then 30 points -
from there the increment changes and they can go to 40 points. If both players get to 40 
points, then that is called _deuce_ and they must play for an _advantage_ before they can
win and take the game. 

This exercise will show you a way of modelling these rules using the F# type system to 
rule out any illegal values (such as a score of 45 to 30, or a player winning directly 
from deuce). The instructions are broken into several chunks - and if you're feeling brave 
you can try to fill in some parts without reading the details below - some example sequences 
for scoring a tennis game are provided at the bottom for you to verify your work.

### 1. Create a directory to work in

```
mkdir fsharp-1
cd fsharp-1
```

### 2. Use the dotnet CLI to create a new F# console application

```
dotnet new console -lang F#
```

> Implementation of the game starts below here - take a moment to think of how you 
> might model the game before proceeding, and see if the code here matches your
> expectations.

### 3. Define types to represent game states

First, tennis is a game for 2 players (singles tennis at least) - so we can start by
defining a player type as _either_ player 1 _or_ player 2.

```fsharp
type Player = PlayerOne | PlayerTwo
```

Next we need to model the score of a game, starting with points. In a game of tennis 
points don't really behave like numbers, both players start at zero and then the names 
of the points are the same as the names of some numbers - but that's where the similarities 
end - we never need any mathematical operations, because they have their own progression,
and notably the progression changes at various points. It's better then to model the few 
options for points as a discriminated union:

```fsharp
type Points = Love | Fifteen | Thirty | Forty
```

This looks good, but there's still more complexity being hidden - while the game progresses 
in a standard way for _Love_, _Fifteen_, and _Thirty_ when a player reaches _Forty_ the 
factors that influence game progression change - Forty to Forty is referred to as _Deuce_ 
and then the rules change _again_. We're going to need to represent each of these steps somehow.

We can define a _Score_ type to hold that information:

```fsharp
type Score =
    | Points of PointsData
    | Deuce
    | Advantage of Player
    | Game of Player
```

and use the _and_ keyword to define the extra types we've introduced immediately below it:

```fsharp
and PointsData =  { PlayerOnePoint: Point; PlayerTwoPoint: Point }
and Point = Love | Fifteen | Thirty | Forty
```

But that still leaves us with some complexity around a player having _Forty_ points - so 
we'll also extract that to its own state so we're clear about handling it. That gives us
this completed _Score_ type:

```fsharp
type Score =
    | Points of PointsData
    | Forty of FortyData
    | Deuce
    | Advantage of Player
    | Game of Player
and PointsData =  { PlayerOnePoint: Point; PlayerTwoPoint: Point }
and Point = Love | Fifteen | Thirty
and FortyData = { Player: Player; OtherPlayerPoint: Point }
```

Which is enough to represent all the possible scores for a single game of tennis.

> The next step will implement functions for moving between different states in a game.
> If you want to try to model this without reading the details below, you will need to 
> define a function that takes both the _current score_ and the _winner of a point_ as 
> parameters, and then returns the _new score_. To make things easier, the version below 
> will implement functions for each of the possible states of a _Score_.

### 4. Implement functions to move between different game states

Our overall _score_ function will take the _current score_ and the _winner of a point_ 
and use that to return a _new score_. For each different state we'll extract any data 
and pass that data plus the winner to a function specifically for handling that state.

The last states of the game are the simplest to model, so we'll start there.

If a game has been won, we don't change state - so a function to model that would just return
the same state.

```fsharp
let scoreWhenGame player = Game player
```

When a function just passes a value through, like with _player_ in this function - we can
omit it, so this can also be written as:

```fsharp
let scoreWhenGame = Game
```

This is a matter of preference and doesn't impact the functionality, so choose whichever
you prefer.

Next, if one player has advantage the two possible outcomes are either they win the game
or the game goes back to deuce. The advantage state has the information on which player has
advantage, so we'll pass that plus the winner of the current point to the function, and it 
should return a new score.

```fsharp
let scoreWhenAdvantage winner current =
    if winner = current then Game winner else Deuce
```

If the score is Deuce, the winner gains the advantage - our function for handling Deuce 
should just return a new score showing that the winner of the point has the advantage:

```fsharp
let scoreWhenDeuce winner = Advantage winner
```

Again, as this just passes the argument through the name can be omitted:

```fsharp
let scoreWhenDeuce = Advantage
```

The remaining steps are more complex, as we'll need to increment point values and keep 
track of more pieces of information.

When _one_ player has forty points, if they win the point they also win the game - so 
we can start by handling that:

```fsharp
let scoreWhenForty winner current =
    if current.Player = winner 
    then Game winner
    else failwith "Other player won"
```

`failwith` is a built-in function that just raises a basic `System.Exception` with
the provided message.

If the _other_ player wins the point, the state is determined by their new score -
if they would now be on 15 or 30 points, then the game is still in the Forty state 
and we need to update their points, but if they are now also on 40 points we should
return Deuce.

Incrementing a player's points is straightforward (but a little bit tedious) and 
definitely something that will show up again when we implement `scoreWithPoints` 
so we can go ahead and define a helper function for that now:

```fsharp
let incrementPoint = function
    | Love -> Some Fifteen
    | Fifteen -> Some Thirty
    | Thirty -> None
```

This function takes a `Point` value and returns a `Point option` because if a player
already has 30 points, we can't represent their next score with the `Point` type 
(and the game state would transition to `Forty` or `Deuce`) so in that case we 
return `None` and understand that it means we've moved passed the points phase of the 
game.

The `function` syntax in F# is a shorthand way for matching on the last parameter to
the function, that is especially useful if we don't want to work with the unmatched 
value directly - `incrementPoint` could also be written like this:

```fsharp
let incrementPoint point = 
    match point with
    | Love -> Some Fifteen
    | Fifteen -> Some Thirty
    | Thirty -> None
```

In the `scoreWithForty` function we can now handle the case where the losing player 
wins the point. 

```fsharp
let scoreWhenForty winner current = 
    if current.Player = winner
    then Game winner
    else
        match incrementPoint current.OtherPlayerPoint with
        | Some points -> Forty { current with OtherPlayerPoint = points }
        | _ -> Deuce
```

This uses the copy and update syntax for record types to take the current state 
and just update the `OtherPlayerPoint` value in the case that the other player 
won the point but hasn't reached 40 points yet.

Now we only need to define a function to handle scoring while the game state is
`Points` before we can compose a general `score` function. While the game is in 
this state we should increment the points value for the winning player until one
of them reaches 40 points at which point the game should transition to the `Forty` 
state.

While we're dealing more with both players in this stage, it is useful to define
a few more helper functions:

```fsharp
let other = function | PlayerOne -> PlayerTwo | PlayerTwo -> PlayerOne

let pointTo player point current =
    match player with
    | PlayerOne -> { current with PlayerOnePoint = point }
    | PlayerTwo -> { current with PlayerTwoPoint = point }

let pointFor player current =
    match player with
    | PlayerOne -> current.PlayerOnePoint
    | PlayerTwo -> current.PlayerTwoPoint
```

- `other` helps us to get the other player
- `pointTo` updates the score of a player when they win a point
- `pointFor` gets the current score for the provided player

These all save some boilerplate code, and help to make the `scoreWhenPoints` 
function more readable.

With those in place, our function should increment the points value for the 
winning player and then either return an updated `Points` state, or if they 
reach 40 points should transition to the `Forty` state:

```fsharp
let scoreWhenPoints winner current =
    match pointFor winner current |> incrementPoint with
    | Some points -> pointTo winner points current |> Points
    | None -> Forty { Player = winner; OtherPlayerPoint = pointFor (other winner) current }
```

Now that we have a function to handle each of the individual score states, we 
can build a general `score` function.

```fsharp
let score current winner =
    match current with
    | Points p -> scoreWhenPoints winner p
    | Forty f -> scoreWhenForty winner f
    | Deuce -> scoreWhenDeuce winner
    | Advantage p -> scoreWhenAdvantage winner p
    | Game w -> scoreWhenGame w
```

If we now add a `newGame` state to start from, we have enough to model a game of 
tennis in our code:

```fsharp
let newGame = Points { PlayerOnePoint = Love; PlayerTwoPoint = Love }
```

### 5. Add a "UI" layer

The model is useful when working directly with the code, but we need to add a 
way of interacting with it from the outside. For this exercise we'll work from the
console - but it would be possible to represent this in different way using the 
same model code, for example it could be done as a HTTP API.

The following code will complete the console application:

```fsharp
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
```

Of special interest is the `play` function that is recursive (noted by the `rec`
keyword) - that takes the current state of the game, and then prompts the user
for input to create a new state and pass that to itself.

## Conclusion

Using the F# type system we can model a series of interactions as a state machine
where each possible state is represented in the code, the compiler will then help 
us to ensure that we handle all these possible states to ensure that our code is 
valid for all cases.
