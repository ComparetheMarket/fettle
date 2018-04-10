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
    
Target "Build" build
Target "Test" test
"Build" ==> "Test"

Target "Watch" watch

RunTargetOrDefault "Test"