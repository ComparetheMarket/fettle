#r "./tools/FAKE.4.61.2/tools/FakeLib.dll" 

open Fake
open Fake.Testing.NUnit3

let solutionFilePath = "./src/Fettle.sln"
let mode = getBuildParamOrDefault "mode" "Release"

let build() =
    !! (sprintf "./src/**/bin/%s/" mode) |> CleanDirs
    
    build (fun x -> 
        { x with Verbosity = Some MSBuildVerbosity.Quiet 
                 Properties = [ "Configuration", mode ] }) solutionFilePath 

let test() =
    let tests = !! (sprintf "./src/**/bin/%s/*Tests.dll" mode)
    let nUnitParams _ = { NUnit3Defaults with Workers = Some(1) }
    tests |> NUnit3 nUnitParams

let watch() =

    use watcher = !! "src/**/*.fs" |> WatchChanges (fun changes -> 
        printfn ">>>>> Files changed: %s" (changes |> Seq.map (fun c -> c.FullPath) |> String.concat ", ")
        
        build()
        test()

        printfn ">>>>> Watching the file-system for changes..."
    )

    printfn ">>>>> Watching the file-system for changes..."
    System.Console.ReadLine() |> ignore
    watcher.Dispose() 

let coverExample() =
    let nunitArgs = [
                        sprintf "src/Examples/HasSurvivingMutants/Tests/bin/%s/HasSurvivingMutants.Tests.dll" mode;
                        "--trace=Off";
                        "--output=./nunit-output.log"
                    ] |> String.concat " "
    let allArgs = [ 
                    "-register:path64"; 
                    "-output:\"opencover.xml\"";
                    "-target:\"./Tools/Nunit/nunit3-console.exe\"";
                    "-returntargetcode:1";
                    "-filter:+[HasSurvivingMutants.Implementation]*";
                    "-hideskipped:All"
                    sprintf "-targetargs:\"%s\"" nunitArgs
                  ]|> String.concat " "
    let result = 
        ExecProcess (fun info ->
            info.FileName <- "./tools/OpenCover.4.6.519/tools/OpenCover.Console.exe"
            info.Arguments <- allArgs
        )(System.TimeSpan.FromMinutes 7.0)

    if result <> 0 then failwith "Test coverage via OpenCover failed or timed-out"

Target "Build" build
Target "Test" test
"Build" ==> "Test"

Target "Watch" watch
Target "CoverExample" coverExample    
"Build" ==> "CoverExample"

RunTargetOrDefault "Test"