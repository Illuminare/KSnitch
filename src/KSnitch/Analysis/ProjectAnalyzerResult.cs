using System;
using System.Collections.Generic;
using System.Linq;

namespace KSnitch.Analysis
{
    internal sealed class ProjectAnalyzerResult
    {
        private readonly List<PackageToRemove> _packages;

        public string Project { get; }
        public Project AProject { get; }
        public IReadOnlyList<PackageToRemove> CanBeRemoved { get; }
        public IReadOnlyList<PackageToRemove> MightBeRemoved { get; }

        public bool NoPackagesToRemove => CanBeRemoved.Count == 0 && MightBeRemoved.Count == 0;

        public ProjectAnalyzerResult(string project, IEnumerable<PackageToRemove> packages, Project aproject = null)
        {
            Project = project;
            if (aproject != null)
            {
                AProject = aproject;
            }
            _packages = new List<PackageToRemove>(packages ?? throw new ArgumentNullException(nameof(packages)));

            CanBeRemoved = new List<PackageToRemove>(packages.Where(p => p.CanBeRemoved));
            MightBeRemoved = new List<PackageToRemove>(packages.Where(p => p.VersionMismatch));
        }

        public ProjectAnalyzerResult Filter(string[]? packages)
        {
            if (packages == null)
            {
                return this;
            }

            var filtered = _packages.Where(p => !packages.Contains(p.Package.Name, StringComparer.OrdinalIgnoreCase));
            return new ProjectAnalyzerResult(Project, filtered);
        }
    }
}
