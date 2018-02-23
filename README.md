# Fettle

Fettle is an experimental mutation testing tool for C#.

Right now it supports projects that:
* Run on Windows
* Use .NET Framework 4.x
* Are tested using NUnit

...but the plan is to support more platforms and test frameworks in the future.

## What is Mutation Testing?

Mutation testing can highlight gaps in your tests which test coverage tools cannot. At its heart is the premise that if your code's behaviour changes, then at least one test should fail.

Mutation testing involves artificially inserting bugs (or "mutants") into your code and seeing if your tests pick up on them. If a mutant is introduced but no tests fail, then the mutant is said to "survive", indicating a gap in your tests.

You can read more about mutation testing in [this post](https://medium.com/comparethemarket/who-will-test-the-tests-bd8c491e5205).

## Getting Started

### Building

To build locally, clone the repository and `cd` to the cloned location. You can then build/test Fettle like so:

```
$ fake.bat
```

The built console runner will be available in `./src/Console/bin/Release`.

### Configuration

You'll need a configuration YAML file to tell Fettle what it should mutate, and how it should do it:

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

Use the console runner to do some mutation testing like so:

```
$ Fettle.console.exe --config <path-to-your-config-file> [--quiet]
```

Once complete it will output information about any mutants that survived.

Fettle has an example project you can use to get an idea of how it works. It's setup to deliberately have gaps in its testing to produce some surviving mutants when run through Fettle.

The config file is in the root of the Fettle repository, and will produce output like this.
```
$ Fettle.Console.exe --config .\fettle.config.example.yml --quiet
....

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

When mutants survive, Fettle outputs the source file and line number of the code that was mutated. It also shows what the line was originally, and what it was mutated to.
