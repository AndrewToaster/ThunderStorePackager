using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace ThunderStorePackager
{
    public class Options
    {
        [Option("files", HelpText = "Sets additional files to be packed into the package", Required = false)]
        public IEnumerable<string> AdditionalFiles { get; set; }

        [Option("name",
            HelpText = "The name of this package",
            Required = true)]
        public string Name { get; set; }

        [Option("description",
            HelpText = "The description of this package",
            Required = true)]
        public string Description { get; set; }

        [Option("version", HelpText = "The version of this package", Required = true)]
        public string Version { get; set; }

        [Option("website",
            Default = "",
            HelpText = "The website of this package",
            Required = false)]
        public string Website { get; set; }

        [Option("dependencies",
            HelpText = "The dependency strings of this package",
            Required = false)]
        public IEnumerable<string> Dependencies { get; set; }

        [Option("icon",
            Default = "./icon.png",
            HelpText = "The icon file for this package",
            Required = true)]
        public string IconFile { get; set; }

        [Option("readme",
            HelpText = "The README text for this package",
            SetName = "ReadMe")]
        public string ReadMeText { get; set; }

        [Option("readmefile",
            HelpText = "The README file for this package",
            SetName = "ReadMe")]
        public string ReadMeFile { get; set; }
    }
}
