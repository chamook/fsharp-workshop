module Tests

open System
open System.Net
open Xunit
open Microsoft.AspNetCore.TestHost
open Swensen.Unquote

let server = new TestServer(Program.getWebHostBuilder [||])
let client = server.CreateClient()

[<Fact>]
let ``health endpoint should return 200`` () =
    client.GetAsync("/health").Result.StatusCode =! HttpStatusCode.OK
