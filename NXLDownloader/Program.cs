using MoreLinq;
using Newtonsoft.Json;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Readers.GZip;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace MapleStoryFullDownloaderNXL
{
    class Program
    {
        static ConcurrentQueue<string> consoleMessages = new ConcurrentQueue<string>();
        public static void Log(string message) => consoleMessages.Enqueue(message);
        static void Main(string[] args) => GetStuff();

        static void GetStuff()
        {
            string downloadId = "10100";

            // Set the log handler for when hash mismatches or wrong sizes
            FileEntry.Log = Log;

            HttpClient client = new HttpClient();
            // Download the full list of Nexon games
            ProductSummary[] summaries = JsonConvert.DeserializeObject<ProductSummary[]>(client.GetStringAsync("http://nxl.nxfs.nexon.com/games/regions/1.json").Result);
            // We only care about MapleStory
            ProductSummary MapleStorySummary = summaries.FirstOrDefault(c => c.ProductId == downloadId);
            // Download the full product info of MapleStory
            Product MapleStory = JsonConvert.DeserializeObject<Product>(client.GetStringAsync($"http://api.nexon.io/products/{downloadId}").Result);
            // Get the manifest's hash
            string latestManifestHash = client.GetStringAsync(MapleStory.Details.ManifestURL).Result;
            // Download the manifest for MapleStory's files
            byte[] ManifestCompressed = client.GetByteArrayAsync($"https://download2.nexon.net/Game/nxl/games/{MapleStory.ProductId}/{latestManifestHash}").Result;
            client.Dispose();
            // Parse the manifest
            Manifest manifest = Manifest.Parse(ManifestCompressed);

            // Ensure the output directory exists
            string output = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), downloadId);
            if (!Directory.Exists(output)) Directory.CreateDirectory(output);

            Dictionary<string, FileEntry> FileNames = manifest.RealFileNames;
            // Build the directory tree before we start downloading
            KeyValuePair<string, FileEntry>[] directories = FileNames.Where(c => c.Value.ChunkHashes.First().Equals("__DIR__")).ToArray();

            foreach (KeyValuePair<string, FileEntry> directory in directories)
            {
                string subDirectory = Path.Combine(output, directory.Key);
                if (File.Exists(subDirectory)) File.Delete(subDirectory);
                if (!Directory.Exists(subDirectory)) Directory.CreateDirectory(subDirectory);
            }

            bool running = true;
            // Handle the console messages in its own thread so as to prevent any locking or messages being written at the same time
            Thread consoleQueue = new Thread(() =>
            {
                string message = null;
                while (running || consoleMessages.TryDequeue(out message))
                {
                    do
                    {
                        if (message != null) Console.WriteLine(message);
                        Thread.Sleep(1);
                    } while (consoleMessages.TryDequeue(out message));
                }
            });

            consoleQueue.Start();

            // Download all of the files
            //foreach (KeyValuePair<string, FileEntry> file in )
            Parallel.ForEach(FileNames.Where(c => !directories.Contains(c)), file =>
            {
                string filePath = Path.Combine(output, file.Key);
                Log($"Starting download of {file.Key}");

                long position = 0;
                // Get all of the chunks in their own threads
                long writtenSize = file.Value.ChunkHashes.Select(hash =>
                {
                    Tuple<long, byte[]> chunk = null;
                    int index = file.Value.ChunkHashes.IndexOf(hash); // Which chunk are we downloading
                    long size = file.Value.ChunkSizes[index]; // How big is the chunk
                    long realSize = 0;
                    do
                    {
                        chunk = FileEntry.DownloadChunk(manifest.Product, hash, size, position); // Download the chunk
                        realSize = chunk.Item2.Length;

                        using (FileStream fileOut = File.OpenWrite(filePath)) // Reusing the same FileStream seems to cause memory issues, so make a new one
                        {
                            fileOut.Position = chunk.Item1; // The chunk's offset
                            fileOut.Write(chunk.Item2, 0, chunk.Item2.Length); // Write the chunk data to the file
                            Log($"Wrote 0x{chunk.Item2.Length.ToString("X")} at 0x{chunk.Item1.ToString("X")} to {file.Key}");
                            fileOut.Flush(); // Flush it out and dispose of the FileStream
                        }
                    } while (chunk == null);

                    position += realSize;

                    chunk = null;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true); // Try to GC if possible

                    return realSize;
                }).Sum(); // Get the sum of the chunked data

                if (writtenSize != file.Value.FileSize)
                    Log($"ERROR, mismatch written and expected size");
                Log($"{file.Key} Total: {writtenSize} Expected: {file.Value.FileSize}");
            });

            // Exit out of the console message processor
            running = false;
            consoleQueue.Join(); // Wait for console processor to exit
        }

        public static byte[] Decompress(byte[] data)
        {
            using (MemoryStream str = new MemoryStream(data))
                return Decompress(str);
        }

        public static byte[] Decompress(Stream str)
        {
            using (MemoryStream result = new MemoryStream())
            using (ZlibStream deflate = new ZlibStream(str, SharpCompress.Compressors.CompressionMode.Decompress, true))
            {
                deflate.CopyTo(result);

                result.Position = 0;
                return result.ToArray();
            }
        }
    }
}
