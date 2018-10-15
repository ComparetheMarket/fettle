# Fettle

[![Build status](https://ci.appveyor.com/api/projects/status/mdy537c3f6rjtlht/branch/master?svg=true)](https://ci.appveyor.com/project/oliwennell/fettle/branch/master) [![nuget](https://buildstats.info/nuget/Fettle.Console)](https://nuget.org/packages/Fettle.Console/)

Fettle is an experimental [mutation testing](https://github.com/ComparetheMarket/fettle/wiki/What-Is-Mutation-Testing) tool for C#.  

## Prerequisites

Right now Fettle can mutation-test your project if you use:
* .NET Framework 4.x
* NUnit framework 3.x (version 3.10 or higher recommended)

## Getting Started

To start using Fettle, see the [getting started guide](https://github.com/ComparetheMarket/fettle/wiki/Getting-Started).  
To build from source, see the [developer guide](https://github.com/ComparetheMarket/fettle/wiki/Developer-Guide).

## Features

Fettle comes with some optimisations to make mutation testing faster:
* It will [perform test impact analysis](https://github.com/ComparetheMarket/fettle/wiki/Coverage-Analysis) on your project to try and find the subset of tests to run for a given piece of source-code.
* It can be configured to [only mutate your local changes](https://github.com/ComparetheMarket/fettle/wiki/Mutating-Modifications-Only) rather than your entire code-base.

## More Info

If you're having problems running Fettle see the [troubleshooting guide](https://github.com/ComparetheMarket/fettle/wiki/Troubleshooting).  
If you're still having issues feel free to get in touch on the [gitter page](https://gitter.im/fettle-mutation-testing/Lobby#) or create an issue.

There's also a [list of supported mutations](https://github.com/ComparetheMarket/fettle/wiki/Supported-Mutations).

## Built With

Fettle relies on some awesome .NET projects which include:
* [Roslyn](https://github.com/dotnet/roslyn)
* [NUnit](https://github.com/nunit/)
* [Paket](https://github.com/fsprojects/Paket)
* [FAKE](https://github.com/fsharp/FAKE)

## Contributing

This is an experimental project run within [comparethemarket](https://tech.comparethemarket.com/?) / [BGL Group](https://www.bglgroup.co.uk/). If you are stuck, or keen to help, then please get in touch on the [gitter page](https://gitter.im/fettle-mutation-testing/Lobby#) or open an issue.

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
