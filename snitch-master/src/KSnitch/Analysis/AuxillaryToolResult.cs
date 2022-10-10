using System;
using System.Collections.Generic;
using System.Linq;

namespace KSnitch.Analysis
{
    internal sealed class AuxillaryToolResult
    {
        public string Tool { get; }
        public int ResultCode { get; }

        public AuxillaryToolResult(string tool, int resultCode)
        {
            Tool = tool;
            ResultCode = resultCode;
        }

    }
}
