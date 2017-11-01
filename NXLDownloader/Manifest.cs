﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NXLDownloader
{
    public class Manifest
    {
        public decimal BuildTime;
        public DateTime BuiltAt { get => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds((double)BuildTime).ToLocalTime(); }
        [JsonProperty(PropertyName = "filepath_encoding")]
        public string FilePathEncodingName;
        public Encoding FilePathEncoding
        {
            get {
                if (FilePathEncodingName.Equals("utf16", StringComparison.CurrentCultureIgnoreCase))
                    return Encoding.Unicode;
                return Encoding.ASCII;
            }
        }
        public string Platform;
        public string Product;
        [JsonProperty(PropertyName = "total_compressed_size")]
        public long TotalCompressedSize;
        [JsonProperty(PropertyName = "total_objects")]
        public long TotalObjects;
        [JsonProperty(PropertyName = "total_uncompressed_size")]
        public long TotalUncompressedSize;
        public string Version;
        public Dictionary<string, FileEntry> Files;
        public Dictionary<string, FileEntry> RealFileNames {
            get => Files.ToDictionary(
                c => FilePathEncoding.GetString(Convert.FromBase64CharArray(c.Key.ToCharArray(), 0, c.Key.Length)).TrimStart((char)65279), 
                c => c.Value
            );
        }

        public static Manifest Parse(byte[] data)
        {
            using (MemoryStream result = new MemoryStream(Program.Decompress(data)))
            using (StreamReader reader = new StreamReader(result, Encoding.UTF8))
            {
                string uncompressed = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<Manifest>(uncompressed);
            }
        }
    }
}
