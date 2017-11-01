using System;
using System.Collections.Generic;
using System.Text;

namespace NXLDownloader
{
    public class LauncherConfig
    {
        public bool EnableSDKInjection;
        public string WorkingDir;
        public string EXEPath;
        public string[] args;
        public bool RequireUAC;
    }
}
