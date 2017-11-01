using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NXLDownloader
{
    public class ProductDetails
    {
        public bool DeactiveDiffPatch;
        public string EULA;
        public PatcherConfig PatcherConfig;
        public string Name;
        public string DisplayName;
        public Dictionary<string, bool> SupportedArch;
        public Dictionary<string, bool> SupportedOS;
        public LauncherConfig LaunchConfig;
        public string BoxArt;
        public string Thumbnail;
        public string MaintenanceMessage;
        public string RequiredDiskSpace;
        public string StatusMessage;
        public bool Maintenance;
        public bool AutoUpdateSupported;
        public string UninstallCommand;
        public bool Public;
        public string ManifestURL;
        [JsonProperty(PropertyName = "product_no")]
        public int ProductNumber;
        [JsonProperty(PropertyName = "service_code")]
        public string ServiceCode;
        public Dictionary<string, Dictionary<string, string>> Branches;
        public string[] PredefinedInstallScripts;
    }
}
