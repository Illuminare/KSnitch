using System.Collections.Generic;
using System.Linq;

namespace KSnitch.Analysis
{
    internal sealed class RefResult
    {
        public Project Project { get; }
        public IReadOnlyList<string> References { get; }

        public RefResult(Project project, IEnumerable<Project> dependencies)
        {
            Project = project;
            References = new List<string>();
        }
    }
}
