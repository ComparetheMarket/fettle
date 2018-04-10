# Fettle

Fettle is an experimental [mutation testing](https://github.com/ComparetheMarket/fettle/wiki/What-Is-Mutation-Testing) tool for C#.  

## Getting Started

Currently the only way to run Fettle is to build it from source first.   
See the [developer guide](https://github.com/ComparetheMarket/fettle/wiki/Developer-Guide) for instructions.

## Running

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

### Running

Use the console runner to start a mutation test:

```
$ Fettle.console.exe --config <path-to-your-config-file> [--quiet]
```

Fettle will output information about any mutants that survived to the console.

When mutants survive, Fettle outputs the source file and line number of the code that was mutated. It also shows what the line was originally, and what it was mutated to.

For example:

```
Mutation testing complete.
9 mutant(s) survived!

(1) \Implementation\UntestedNumberComparison.cs:7
  original: return a == 42;
  mutated: return a != 42;

(2) \Implementation\PartiallyTestedNumberComparison.cs:7
  original: return a > 0;
  mutated: return a >= 0;

  ... etc.

```

## Built With

Fettle relies on some awesome .NET projects which include:
* [Roslyn](https://github.com/dotnet/roslyn)
* [NUnit](https://github.com/nunit/)
* [Paket](https://github.com/fsprojects/Paket)
* [FAKE](https://github.com/fsharp/FAKE)

## Contributing

If you would like to contribute then thanks very much! Please see [this guide](https://github.com/ComparetheMarket/fettle/blob/master/CCONTRIBUTING.md).   
Note that we have [a code of conduct](https://github.com/ComparetheMarket/fettle/blob/master/CODE_OF_CONDUCT.md).

## Authors

* [Oli Wennell](https://github.com/oliwennell) - _Initial work_
* [Raghav Suryanarayanan](https://github.com/ragsurya)

Plus [other contributors](https://github.com/ComparetheMarket/fettle/graphs/contributors).

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/ComparetheMarket/fettle/blob/master/LICENSE) file for details

## Acknowledgements

Thanks to:
* The authors of the other mutation testing tools out there for inspiration.
* [PurpleBooth](https://github.com/PurpleBooth) for their [README template](https://gist.github.com/PurpleBooth/109311bb0361f32d87a2).
* [Contributor covenant](https://www.contributor-covenant.org/) for their code of conduct.
