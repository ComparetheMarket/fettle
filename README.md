# Fettle

[![Build status](https://ci.appveyor.com/api/projects/status/mdy537c3f6rjtlht/branch/master?svg=true)](https://ci.appveyor.com/project/oliwennell/fettle/branch/master)

Fettle is an experimental [mutation testing](https://github.com/ComparetheMarket/fettle/wiki/What-Is-Mutation-Testing) tool for C#.  

## Getting Started

The Fettle console application is [available on NuGet](https://www.nuget.org/packages/Fettle.Console/).

To build from source, see the [developer guide](https://github.com/ComparetheMarket/fettle/wiki/Developer-Guide).

### Prerequisites

Right now Fettle can mutation-test your project if you use:
* .NET Framework 4.x
* NUnit framework 3.x (version 3.10 or higher recommended)

### Configuration

You'll also need a configuration file (in YAML format) to tell Fettle what it should mutate.

Here's an example:

```
# Note: all paths are relative to this config file's location.

# [Required] The solution file that contains your C# code.
solution: .\src\MyProject\MyProject.sln

# [Required] The projects within the solution that contain C# code you want to be mutated.
# In other words: the implementation code and not the tests.
projectFilters:
    - MyProject.App
    - MyProject.Lib

# [Required] A list of the assemblies that contain your tests.
testAssemblies:
    - .\src\MyProject\Tests\bin\Release\Tests.dll
    - .\src\MyProject\IntegrationTests\bin\Release\IntegrationTests.dll

# [Optional] use a filter to define which source files to mutate.
sourceFileFilters:
    - Implementation\*.cs
```

### Catching Mutants

Use the console runner to start a mutation test:

```
$ Fettle.console.exe --config <path-to-your-config-file> [--quiet]
```

Fettle will output information about any mutants that survived to the console.

When mutants survive, Fettle outputs the source file and line number of the code that was mutated. It also shows what the line was originally, and what it was mutated to.

For example:

```
(1) \Implementation\UntestedNumberComparison.cs:7
  original: return a == 42;
  mutated: return a != 42;

(2) \Implementation\PartiallyTestedNumberComparison.cs:7
  original: return a > 0;
  mutated: return a >= 0;
```

## "Killing" Mutants

To fix a gap in your tests that Fettle discovers (aka "killing a mutant") you can:
1. *Improve the tests*  
If at least one test fails when the mutation is made, then the mutant won't survive.  

2. *Remove the mutated code*  
Sometimes mutation testing highlights code that's not required and can be removed.  

3. *Tell Fettle to ignore it*.  
If you aren't able (or don't want to) do 1. or 2., you can mark lines to ignore via special comments.  
For example:
```
// fettle: begin ignore
// <No code in-between these lines will be mutated by Fettle>
// fettle: end ignore
```

## Built With

Fettle relies on some awesome .NET projects which include:
* [Roslyn](https://github.com/dotnet/roslyn)
* [NUnit](https://github.com/nunit/)
* [Paket](https://github.com/fsprojects/Paket)
* [FAKE](https://github.com/fsharp/FAKE)

## Contributing

This is an experimental project run within [comparethemarket](https://techjobs.comparethemarket.com/?) / [BGL Group](https://www.bglgroup.co.uk/). If you are stuck, or keen to help, then please get in touch on the [gitter page](https://gitter.im/fettle-mutation-testing/Lobby#) or open an issue.

Note that we have [a code of conduct](https://github.com/ComparetheMarket/fettle/blob/master/CODE_OF_CONDUCT.md).

## Authors

[Oli Wennell](https://github.com/oliwennell) did the initial work and is the project's maintainer.  
Here's a list of the [lovely people](https://github.com/ComparetheMarket/fettle/graphs/contributors) that have helped the project by contributing.

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/ComparetheMarket/fettle/blob/master/LICENSE) file for details

## Acknowledgements

Thanks to:
* The authors of the other mutation testing tools out there for inspiration.
* [PurpleBooth](https://github.com/PurpleBooth) for their [README template](https://gist.github.com/PurpleBooth/109311bb0361f32d87a2).
* [Contributor covenant](https://www.contributor-covenant.org/) for their code of conduct.
