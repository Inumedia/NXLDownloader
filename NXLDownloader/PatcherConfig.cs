using System;
using System.Collections.Generic;
using System.Text;

namespace NXLDownloader
{
    public class PatcherConfig
    {
        public string ExePath;
        public string[] Args;
        public string WorkingDir;
        public bool Enable;
        public Dictionary<string, Patch> Patches;
    }
}
