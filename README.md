# <img src="/src/icon.png" height="30px"> K-Snitch

[![NuGet Status](https://img.shields.io/nuget/v/Snitch.svg)](https://www.nuget.org/packages/KSnitch/)

A tool that help you find transitive package references that can be removed. (and remove them)
Also it can clean packages.props from packages not used in any of solution's projects
Rework from the Snitch tool by: spectresystems

## Added functionality:

    -o, --outputsimple         Return results in simple ANSI mode (no colored console!)
    -c, --applycan             Remove the references from 'Can Remove' subset
    -m, --applymay             Remove the references from 'Might Remove' subset
    -n, --noBuild              Analyze via solution file without building of all the projects. (Saves the time)

## Commands list

```
> ksnitch --help
```

## Example

```
> ksnitch --tfm net462
```

Results in:

<!-- snippet: Solution.Default.verified.txt -->
<a id='snippet-Solution.Default.verified.txt'></a>
```txt
Analyzing...
Analyzing Snitch.Tests.Fixtures.sln
Analyzing Foo...
Analyzing Bar...
Analyzing Baz...
Analyzing Qux...
Analyzing Zap...

╭──────────────────────────────────────────────────────────────╮
│  Packages that can be removed from Bar:                      │
│ ┌─────────────────────┬────────────────────────────────────┐ │
│ │ Package             │ Referenced by                      │ │
│ ├─────────────────────┼────────────────────────────────────┤ │
│ │ Autofac             │ Foo                                │ │
│ └─────────────────────┴────────────────────────────────────┘ │
│                                                              │
│  Packages that can be removed from Baz:                      │
│ ┌─────────────────────┬────────────────────────────────────┐ │
│ │ Package             │ Referenced by                      │ │
│ ├─────────────────────┼────────────────────────────────────┤ │
│ │ Autofac             │ Foo                                │ │
│ └─────────────────────┴────────────────────────────────────┘ │
│                                                              │
│  Packages that might be removed from Qux:                    │
│ ┌───────────┬───────────┬──────────────────────────────────┐ │
│ │ Package   │ Version   │ Reason                           │ │
│ ├───────────┼───────────┼──────────────────────────────────┤ │
│ │ Autofac   │ 4.9.3     │ Downgraded from 4.9.4 in Foo     │ │
│ └───────────┴───────────┴──────────────────────────────────┘ │
│                                                              │
│  Packages that might be removed from Zap:                    │
│ ┌─────────────────┬─────────┬──────────────────────────────┐ │
│ │ Package         │ Version │ Reason                       │ │
│ ├─────────────────┼─────────┼──────────────────────────────┤ │
│ │ Newtonsoft.Json │ 12.0.3  │ Updated from 12.0.1 in Foo   │ │
│ │ Autofac         │ 4.9.3   │ Downgraded from 4.9.4 in Foo │ │
│ └─────────────────┴─────────┴──────────────────────────────┘ │
╰──────────────────────────────────────────────────────────────╯
```
<sup><a href='/src/KSnitch.Tests/Expectations/Solution.Default.verified.txt#L1-L38' title='Snippet source file'>snippet source</a> | <a href='#snippet-Solution.Default.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Installation (as global tool)

```
> dotnet tool install -g ksnitch
```

## Usage

_Examine a specific project or solution using the first built 
target framework._

```
> ksnitch MyProject.csproj
```

_Examine a specific project using a specific
target framework moniker._

```
> ksnitch MyProject.csproj --tfm net462
```

_Examine a specific project using a specific target framework moniker
and return exit code 0 only if there was no transitive package collisions.
Useful for continuous integration._

```
> ksnitch MyProject.csproj --tfm net462 --strict
```

_Examine a specific project using a specific target framework moniker
and make sure that the packages Foo and Bar are excluded from the result._

```
> ksnitch MyProject.csproj --tfm net462 --exclude Foo --exclude Bar
```

_Examine a specific project using a specific target framework moniker
and exclude the project OtherProject from analysis._

```
> ksnitch MyProject.csproj --tfm net462 --skip OtherProject
```

## Building Snitch from source

```
> dotnet tool restore
> dotnet cake
```

## Icon

[Hollow](https://thenounproject.com/term/stitch/1571973/) designed by [Ben Davis](https://thenounproject.com/smashicons/) from [The Noun Project](https://thenounproject.com).
