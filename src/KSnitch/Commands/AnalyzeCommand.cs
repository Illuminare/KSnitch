using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using KSnitch.Analysis;
using KSnitch.Analysis.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;

namespace KSnitch.Commands
{
    [Description("Shows transitive package dependencies that can be removed")]
    public sealed class AnalyzeCommand : Command<AnalyzeCommand.Settings>
    {
        private readonly IAnsiConsole _console;
        private readonly ProjectBuilder _builder;
        private readonly ProjectAnalyzer _analyzer;
        private readonly ProjectReporter _reporter;
        private readonly ReferenceRemover _refremover;

        public sealed class Settings : CommandSettings
        {
            [CommandArgument(0, "[PROJECT|SOLUTION]")]
            [Description("The project or solution you want to analyze.")]
            public string ProjectOrSolutionPath { get; set; } = string.Empty;

            [CommandOption("-t|--tfm <MONIKER>")]
            [Description("The target framework moniker to analyze.")]
            public string? TargetFramework { get; set; }

            [CommandOption("-e|--exclude <PACKAGE>")]
            [Description("One or more packages to exclude.")]
            public string[]? Exclude { get; set; }

            [CommandOption("--skip <PROJECT>")]
            [Description("One or more project references to exclude.")]
            public string[]? Skip { get; set; }

            [CommandOption("-s|--strict")]
            [Description("Returns exit code 0 only if no conflicts were found.")]
            public bool Strict { get; set; }

            [CommandOption("-o|--outputsimple")]
            [Description("Return results in simple ANSI mode.")]
            public bool simpleOutput { get; set; }

            [CommandOption("-c|--applycan")]
            [Description("Remove the references from 'Can Remove' subset.")]
            public bool applyCan { get; set; }

            [CommandOption("-m|--applymay")]
            [Description("Remove the references from 'Might Remove' subset.")]
            public bool applyMight { get; set; }

            [CommandOption("-n|--noBuild")]
            [Description("Analyze via solution file without building of all the projects. (Saves the time)")]
            public bool noBuild { get; set; }
        }

        public AnalyzeCommand(IAnsiConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _builder = new ProjectBuilder(console);
            _analyzer = new ProjectAnalyzer();
            _reporter = new ProjectReporter(console);
            _refremover = new ReferenceRemover(console);
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {

           
                //works not good. just scans all the subfolders
                var projectsToAnalyze = PathUtility.GetProjectPaths(settings.ProjectOrSolutionPath, out var entry);
                projectsToAnalyze.RemoveAll(p =>
                {
                    var projectName = Path.GetFileNameWithoutExtension(p);
                    return settings.Skip?.Contains(projectName, StringComparer.OrdinalIgnoreCase) ?? false;
                });

                var targetFramework = settings.TargetFramework;
                var analyzerResults = new List<ProjectAnalyzerResult>();
                var projectCache = new HashSet<Project>(new ProjectComparer());

                _console.WriteLine();

                return _console.Status().Start($"Analyzing...", ctx =>
                {
                    ctx.Refresh();

                    if (!settings.simpleOutput)
                    {
                        _console.MarkupLine($"Analyzing [yellow]{Path.GetFileName(entry)}[/]");
                    }
                    else
                    {
                        _console.WriteLine($"Analyzing nuget references in {Path.GetFileName(entry)}");
                    }

                    foreach (var projectToAnalyze in projectsToAnalyze)
                    {
                        ProjectBuildResult buildResult=new ProjectBuildResult(new Project(projectToAnalyze),new List<Project>());

                        if (settings.noBuild)
                        {
                            var proj = new Project(projectToAnalyze,true);
                            var dependencies = proj.ProjectReferences;
                            buildResult = new ProjectBuildResult(proj, dependencies);
                        }
                        else
                        {
                        // Perform a design time build of the project.
                            buildResult = _builder.Build(
                            projectToAnalyze,
                            targetFramework,
                            settings.Skip,
                            projectCache,
                            settings.simpleOutput);
                        }
                        // Update the cache of built projects.
                        projectCache.Add(buildResult.Project);
                        foreach (var item in buildResult.Dependencies)
                        {
                            projectCache.Add(item);
                        }

                        // Analyze the project.
                        var analyzeResult = _analyzer.Analyze(buildResult.Project);
                        if (settings.Exclude?.Length > 0)
                        {
                            // Filter packages that should be excluded.
                            analyzeResult = analyzeResult.Filter(settings.Exclude);
                        }

                        analyzerResults.Add(analyzeResult);
                    }

                    // Write the report to the console
                    _reporter.WriteToConsole(analyzerResults, settings.simpleOutput, settings.noBuild);

                    if (settings.applyCan)
                    {
                        if (!settings.simpleOutput)
                        {
                            _console.MarkupLine($"Removing references that [yellow] can [/] be removed...");
                        }
                        else
                        {
                            _console.WriteLine($"Removing references that can be removed...");
                        }

                        var appCanResult = _refremover.ClearCan(analyzerResults, settings.simpleOutput);

                    }

                    if (settings.applyMight)
                    {
                        if (!settings.simpleOutput)
                        {
                            _console.MarkupLine($"Removing references that [yellow] might [/] be removed...");
                        }
                        else
                        {
                            _console.WriteLine($"Removing references that might be removed...");
                        }

                        var appMightResult = _refremover.ClearMight(analyzerResults, settings.simpleOutput);

                    }

                    // Return the correct exit code.
                    return GetExitCode(settings, analyzerResults);
                });
            
            /*else
            {
                if (!settings.ProjectOrSolutionPath.EndsWith(".sln"))
                {
                    _console.WriteLine();
                    _console.WriteLine("-nobuild (-n) option works only with one .sln file");
                    return -1;
                }
                var projectsToAnalyze = PathUtility.GetProjectsFromSolution(settings.ProjectOrSolutionPath);
                var entry = projectsToAnalyze[0];
                projectsToAnalyze.RemoveAll(p =>
                {
                    var projectName = Path.GetFileNameWithoutExtension(p);
                    return settings.Skip?.Contains(projectName, StringComparer.OrdinalIgnoreCase) ?? false;
                });

                var targetFramework = settings.TargetFramework;
                var analyzerResults = new List<ProjectAnalyzerResult>();
                var projectCache = new HashSet<Project>(new ProjectComparer());

                return _console.Status().Start($"Analyzing...", ctx =>
                {
                    ctx.Refresh();

                    if (!settings.simpleOutput)
                    {
                        _console.MarkupLine($"Analyzing [yellow]{Path.GetFileName(entry)}[/]");
                    }
                    else
                    {
                        _console.WriteLine($"Analyzing nuget references in {Path.GetFileName(entry)}");
                    }



                    foreach (var projectToAnalyze in projectsToAnalyze)
                    {
                        Microsoft.Build.Construction.ProjectInSolution ProjFile = 
                        // Perform a design time build of the project.
                        var buildResult = _builder.Build(
                            projectToAnalyze,
                            targetFramework,
                            settings.Skip,
                            projectCache,
                            settings.simpleOutput);

                        // Update the cache of built projects.
                        projectCache.Add(buildResult.Project);
                        foreach (var item in buildResult.Dependencies)
                        {
                            projectCache.Add(item);
                        }

                        // Analyze the project.
                        var analyzeResult = _analyzer.Analyze(buildResult.Project);
                        if (settings.Exclude?.Length > 0)
                        {
                            // Filter packages that should be excluded.
                            analyzeResult = analyzeResult.Filter(settings.Exclude);
                        }

                        analyzerResults.Add(analyzeResult);
                    }

                    // Write the report to the console
                    _reporter.WriteToConsole(analyzerResults, settings.simpleOutput);

                    if (settings.applyCan)
                    {
                        if (!settings.simpleOutput)
                        {
                            _console.MarkupLine($"Removing references that [yellow] can [/] be removed...");
                        }
                        else
                        {
                            _console.WriteLine($"Removing references that can be removed...");
                        }

                        var appCanResult = _refremover.ClearCan(analyzerResults, settings.simpleOutput);

                    }

                    if (settings.applyMight)
                    {
                        if (!settings.simpleOutput)
                        {
                            _console.MarkupLine($"Removing references that [yellow] might [/] be removed...");
                        }
                        else
                        {
                            _console.WriteLine($"Removing references that might be removed...");
                        }

                        var appMightResult = _refremover.ClearMight(analyzerResults, settings.simpleOutput);

                    }

                    // Return the correct exit code.
                    return GetExitCode(settings, analyzerResults);
                });

                return GetExitCode(settings, analyzerResults);
            }*/
        }

        private static int GetExitCode(Settings settings, List<ProjectAnalyzerResult> result)
        {
            if (settings.Strict && result.Any(r => !r.NoPackagesToRemove))
            {
                return -1;
            }

            return 0;
        }
    }
}
