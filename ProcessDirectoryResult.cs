using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace GitCloneFunction
{
    public class ProcessDirectoryResult
    {
        public string ProcessCommand { get; set; }
        public string ProcessCommandParameters { get; set; }

        public string ProcessDirectory { get; set; }

        public IEnumerable<string> Files { get; set; }

        public override string ToString()
        {
            return TypeHelper.GetTypes(this);
        }
    }
}