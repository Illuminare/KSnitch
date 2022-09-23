using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Buildalyzer;
using KSnitch.Analysis.Utilities;
using Spectre.Console;

namespace KSnitch.Analysis
{
    internal sealed class ReferenceRemover
    {
        private readonly IAnsiConsole _console;

        public ReferenceRemover(IAnsiConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public int ClearCan(
            List<ProjectAnalyzerResult> analyzerResults, bool simplifiedOutput = false)
        {
            XNamespace ns = @"http://schemas.microsoft.com/developer/msbuild/2003";
            foreach (var analyseResult in analyzerResults)
            {
                if (analyseResult.CanBeRemoved!=null && analyseResult.CanBeRemoved.Count()>0)
                {
                    _console.WriteLine(
                        $"Removing references that can be removed from {analyseResult.AProject.File}...");
                    var xml = XElement.Load(analyseResult.AProject.Path);
                    foreach (var reftoremove in analyseResult.CanBeRemoved)
                    {
                        var nodes = xml.Descendants("PackageReference");
                        var ver = reftoremove.Package.Version != null
                            ? reftoremove.Package.Version.ToString()
                            : reftoremove.Package.Range.ToString();

                        if (reftoremove.Package.Version != null)
                        {
                            nodes = nodes.Where(r => r.Attribute("Include").Value.Contains(reftoremove.Package.Name));
                            try
                            {
                                if(nodes.Any(n=>n.Attribute("Version") != null))
                                nodes = nodes.Where(r => r.Attribute("Version")!=null && r.Attribute("Version").Value
                                    .Contains(reftoremove.Package.Version.ToString()));
                            }
                            catch
                            {
                                //do nothing. there is no version.
                            }
                        }
                        else
                        {
                            nodes = nodes.Where(r => r.Attribute("Include").Value.Contains(reftoremove.Package.Name));
                        }
                        _console.WriteLine($"Removing {reftoremove.Package.Name} with version {ver}");
                        while (nodes.Any())
                        {
                            var node = nodes.FirstOrDefault();
                            node.Remove();
                        }
                    }

                    xml.Save(analyseResult.AProject.Path);
                }
                else
                {
                    _console.WriteLine(
                        $"Nothing to remove in {analyseResult.AProject.File}...");
                }
            }

            return analyzerResults.Count;
        }
        public int ClearMight(
            List<ProjectAnalyzerResult> analyzerResults, bool simplifiedOutput = false)
        {
            foreach (var analyseResult in analyzerResults)
            {
                if (analyseResult.MightBeRemoved != null && analyseResult.MightBeRemoved.Count() > 0)
                {
                    _console.WriteLine(
                        $"Removing references that might be removed from {analyseResult.AProject.File}...");
                    var xml = XElement.Load(analyseResult.AProject.Path);
                    foreach (var reftoremove in analyseResult.MightBeRemoved)
                    {
                        var nodes = xml.Descendants("PackageReference");
                        var ver = reftoremove.Package.Version != null
                            ? reftoremove.Package.Version.ToString()
                            : reftoremove.Package.Range.ToString();

                        if (reftoremove.Package.Version!=null){
                            nodes = nodes.Where(r => r.Attribute("Include").Value.Contains(reftoremove.Package.Name));
                            try
                            {
                                if (nodes.Any(n => n.Attribute("Version") != null))
                                    nodes = nodes.Where(r => r.Attribute("Version") != null && r.Attribute("Version").Value
                                        .Contains(reftoremove.Package.Version.ToString()));
                            }
                            catch
                            {
                                //do nothing. there is no version.
                            }
                        }
                        else
                        {
                            nodes = nodes.Where(r => r.Attribute("Include").Value.Contains(reftoremove.Package.Name));
                        }
                        _console.WriteLine($"Removing {reftoremove.Package.Name} with version {ver}");
                        while (nodes.Any())
                        {
                            var node = nodes.FirstOrDefault();
                            node.Remove();
                        }
                    }

                    xml.Save(analyseResult.AProject.Path);
                }
                else
                {
                    _console.WriteLine(
                        $"Nothing to remove in {analyseResult.AProject.File}...");
                }
            }

            return analyzerResults.Count;
        }
    }
}
