using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace KSnitch.Analysis
{
    [DebuggerDisplay("{GetProjectName(),nq}")]
    internal sealed class Project
    {
        public string Path { get; }
        public string File { get; }
        public string Name { get; }
        public string TargetFramework { get; set; }
        public string? LockFilePath { get; set; }
        public List<Project> ProjectReferences { get; }
        public List<Package> Packages { get; }

        public Project(string path, bool readfile = false)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            File = System.IO.Path.GetFileName(Path);
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
            TargetFramework = string.Empty;
            ProjectReferences = new List<Project>();
            Packages = new List<Package>();
            if (readfile)
            {
                var xml = XElement.Load(Path);
                var packageNodes = xml.Descendants("PackageReference");
                foreach (var node in packageNodes)
                {
                    string pname = node.Attribute("Include").Value.ToString();
                    string pversion = node.Attribute("Version") != null
                        ? node.Attribute("Version").Value.ToString()
                        : "*";
                    var pkg = new Package(pname,pversion);
                    Packages.Add(pkg);
                }
                var projectNodes = xml.Descendants("ProjectReference");
                foreach (var node in projectNodes)
                {
                    var projpath = node.Attribute("Include").Value.ToString();
                    if (projpath.StartsWith(".."))
                    {//the path is relative
                        var baseDirectory = new System.IO.DirectoryInfo(path).Parent;
                        while (projpath.StartsWith("..\\"))
                        {
                            baseDirectory = baseDirectory.Parent;
                            projpath = projpath.Substring(3);
                        }
                        projpath = System.IO.Path.Combine(baseDirectory.FullName+"\\", projpath);
                    }
                    var proj = new Project(projpath, true);
                    ProjectReferences.Add(proj);
                }
            }
        }


        public void RemovePackages(IEnumerable<string> packages)
        {
            if (packages != null)
            {
                foreach (var package in packages)
                {
                    RemovePackage(package);
                }
            }
        }

        private void RemovePackage(string package)
        {
            Packages.RemoveAll(p => p.Name.Equals(package, StringComparison.OrdinalIgnoreCase));
            foreach (var parentProject in ProjectReferences)
            {
                parentProject.RemovePackage(package);
            }
        }

        private string GetProjectName()
        {
            return System.IO.Path.GetFileName(Path);
        }
    }
}
