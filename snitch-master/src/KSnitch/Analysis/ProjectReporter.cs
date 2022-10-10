using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Spectre.Console;

namespace KSnitch.Analysis
{
    internal class ProjectReporter
    {
        private readonly IAnsiConsole _console;

        public ProjectReporter(IAnsiConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public void WriteToConsole([NotNull] List<ProjectAnalyzerResult> results, bool Simpleoutput,
            bool noBuild = false)
        {
            var resultsWithPackageToRemove = results.Where(r => r.CanBeRemoved.Count > 0).ToList();
            var resultsWithPackageMayBeRemove = results.Where(r => r.MightBeRemoved.Count > 0).ToList();

            if (results.All(x => x.NoPackagesToRemove))
            {
                // Output the result.
                _console.WriteLine();
                if (!Simpleoutput)
                {
                    _console.MarkupLine("[green]Everything looks good![/]");
                }
                else
                {
                    _console.WriteLine("Everything looks good!");
                }

                _console.WriteLine();
                return;
            }

            if (!Simpleoutput)
            {
                var report = new Grid();
                report.AddColumn();

                if (resultsWithPackageToRemove.Count > 0)
                {
                    foreach (var (_, _, last, result) in resultsWithPackageToRemove.Enumerate())
                    {
                        var table = new Table().BorderColor(Color.Grey).Expand();
                        table.AddColumns("[grey]Package[/]", "[grey]Referenced by[/]");
                        foreach (var item in result.CanBeRemoved)
                        {
                            table.AddRow(
                                $"[green]{item.Package.Name}[/]",
                                $"[aqua]{item.Original.Project.Name}[/]");
                        }

                        report.AddRow($" [yellow]Packages that can be removed from[/] [aqua]{result.Project}[/]:");
                        report.AddRow(table);

                        if (!last || (last && resultsWithPackageMayBeRemove.Count > 0))
                        {
                            report.AddEmptyRow();
                        }
                    }
                }

                if (resultsWithPackageMayBeRemove.Count > 0)
                {
                    foreach (var (_, _, last, result) in resultsWithPackageMayBeRemove.Enumerate())
                    {
                        var table = new Table().BorderColor(Color.Grey).Expand();
                        table.AddColumns("[grey]Package[/]", "[grey]Version[/]", "[grey]Reason[/]");

                        foreach (var item in result.MightBeRemoved)
                        {
                            if (item.Package.IsGreaterThan(item.Original.Package, out var indeterminate))
                            {
                                var name = item.Original.Project.Name;
                                var version = item.Original.Package.GetVersionString();
                                var verb = indeterminate ? "Might be updated from" : "Updated from";
                                var reason = $"[grey]{verb}[/] [silver]{version}[/] [grey]in[/] [aqua]{name}[/]";

                                table.AddRow(
                                    $"[green]{item.Package.Name}[/]",
                                    item.Package.GetVersionString(),
                                    reason);
                            }
                            else
                            {
                                var name = item.Original.Project.Name;
                                var version = item.Original.Package.GetVersionString();
                                var verb = "";
                                if (noBuild)
                                {
                                    verb = indeterminate ? "Does not match" : "";
                                }
                                else
                                {
                                    verb = indeterminate ? "Does not match" : "Downgraded from";
                                }
                                var reason = $"[grey]{verb}[/] [silver]{version}[/] [grey]in[/] [aqua]{name}[/]";

                                table.AddRow(
                                    $"[green]{item.Package.Name}[/]",
                                    item.Package.GetVersionString(),
                                    reason);
                            }
                        }

                        report.AddRow(
                            $" [yellow]Packages that [u]might[/] be removed from[/] [aqua]{result.Project}[/]:");
                        report.AddRow(table);

                        if (!last)
                        {
                            report.AddEmptyRow();
                        }
                    }
                }

                _console.WriteLine();
                _console.Render(
                    new Panel(report)
                        .RoundedBorder()
                        .BorderColor(Color.Grey));
            }
            else
            {
                if (resultsWithPackageToRemove.Count > 0)
                {
                    foreach (var (_, _, last, result) in resultsWithPackageToRemove.Enumerate())
                    {
                        var table = new Table().BorderColor(Color.Grey).Expand();
                        _console.WriteLine($" Packages that can be removed from {result.Project}:");
                        _console.WriteLine("Package \t Referenced by");
                        foreach (var item in result.CanBeRemoved)
                        {
                            _console.WriteLine($"{item.Package.Name}\t{item.Original.Project.Name}");
                        }

                       if (!last || (last && resultsWithPackageMayBeRemove.Count > 0))
                        {
                            _console.WriteLine();
                        }
                    }
                }

                if (resultsWithPackageMayBeRemove.Count > 0)
                {
                    foreach (var (_, _, last, result) in resultsWithPackageMayBeRemove.Enumerate())
                    {
                        var table = new Table().BorderColor(Color.Grey).Expand();
                        _console.WriteLine($"Packages that might be removed from {result.Project}:");
                        _console.WriteLine("Package \t Version \t Reason");

                        foreach (var item in result.MightBeRemoved)
                        {
                            var name = "";
                            var version = "";
                            var verb = "";
                            var reason = "";
                            if (item.Package.IsGreaterThan(item.Original.Package, out var indeterminate))
                            {
                                name = item.Original.Project.Name;
                                version = item.Original.Package.GetVersionString();
                                verb = indeterminate ? "Might be updated from" : "Updated from";
                                reason = $"{verb} \t {version} \t in \t {name}";
                                _console.WriteLine(
                                    $"{item.Package.Name} \t {item.Package.GetVersionString()} \t {reason}");
                            }
                            else
                            {
                                 name = item.Original.Project.Name;
                                 version = item.Original.Package.GetVersionString();
                                 if (noBuild)
                                 {
                                     verb = indeterminate ? "Does not match" : "";
                                 }
                                 else
                                 {
                                     verb = indeterminate ? "Does not match" : "Downgraded from";
                                 }

                                 reason = $"{verb} \t {version} \t in \t {name}";

                                 _console.WriteLine(
                                     $"{item.Package.Name} \t {item.Package.GetVersionString()} \t {reason}");
                            }
                        }

                        if (!last)
                        {
                            _console.WriteLine();
                        }
                    }
                }
            }
        }

        public void WriteToConsoleTools(List<AuxillaryToolResult> toolsResults, bool settingsSimpleOutput)
        {
            foreach (var toolRes in toolsResults)
            {
                if (!settingsSimpleOutput)
                {
                    _console.MarkupLine($"[green]{toolRes.Tool}[/]: {toolRes.ResultCode}");
                }
                else
                {
                    _console.WriteLine($"{toolRes.Tool}: {toolRes.ResultCode}");
                }

            }
            
        }
    }
}
