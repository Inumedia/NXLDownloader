using Ionic.Zlib;
using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MapleStoryFullDownloaderNXL
{
    class Program
    {
        static void Main(string[] args) => Console.WriteLine(GetStuff().Result);

        static async Task<string> GetStuff()
        { 
            HttpClient client = new HttpClient();
            ProductSummary[] summaries = JsonConvert.DeserializeObject<ProductSummary[]>(await client.GetStringAsync("http://nxl.nxfs.nexon.com/games/regions/1.json"));
            ProductSummary MapleStorySummary = summaries.First(c => c.ProductId == "10100");
            Product MapleStory = JsonConvert.DeserializeObject<Product>(await client.GetStringAsync(MapleStorySummary.ProductLink));
            string latestManifestHash = await client.GetStringAsync(MapleStory.Details.ManifestURL);
            byte[] ManifestZip = await client.GetByteArrayAsync($"https://download2.nexon.net/Game/nxl/games/10100/{latestManifestHash}");
            client.Dispose();
            Manifest manifest = Manifest.Parse(ManifestZip);

            string output = Path.Combine(Environment.CurrentDirectory, "Output");
            if (!Directory.Exists(output)) Directory.CreateDirectory(output);

            ConcurrentQueue<Tuple<int, byte[]>> chunks = new ConcurrentQueue<Tuple<int, byte[]>>();
            Dictionary<string, FileEntry> FileNames = manifest.RealFileNames;
            foreach (KeyValuePair<string, FileEntry> file in FileNames)
            {
                Console.WriteLine(file.Key);

                if (file.Value.ChunkHashes.First().Equals("__DIR__", StringComparison.CurrentCultureIgnoreCase))
                {
                    string subDirectory = Path.Combine(output, file.Key);
                    if (File.Exists(subDirectory)) File.Delete(subDirectory);
                    if (!Directory.Exists(subDirectory)) Directory.CreateDirectory(subDirectory);
                    continue;
                }

                string filePath = Path.Combine(output, file.Key);

                Task<int> writtenSize = Task.WhenAll(file.Value.ChunkHashes.Batch(Math.Max(1, file.Value.ChunkHashes.Count / Environment.ProcessorCount)).AsParallel().Select(c =>
                {
                    return c.Select(async hash =>
                    {
                        int index = file.Value.ChunkHashes.IndexOf(hash);
                        int size = file.Value.ChunkSizes[index];
                        int position = file.Value.ChunkSizes.Take(index).Sum();
                        Tuple<int, byte[]> chunkToQueue = await FileEntry.DownloadChunk(manifest.Product, hash, size, position);
                        chunks.Enqueue(chunkToQueue);
                        chunkToQueue = null;
                        return size;
                    });
                }).SelectMany(c => c)).ContinueWith(c => c.Result.Sum());

                Tuple<int, byte[]> chunk = null;
                while (!writtenSize.IsCompleted || chunks.TryDequeue(out chunk))
                {
                    do
                    {
                        if (chunk == null) continue;
                        using (FileStream fileOut = File.OpenWrite(filePath))
                        {
                            fileOut.Position = chunk.Item1;
                            fileOut.Write(chunk.Item2, 0, chunk.Item2.Length);
                            Console.WriteLine($"Wrote 0x{chunk.Item2.Length.ToString("X")} at 0x{chunk.Item1.ToString("X")}");
                            fileOut.Flush();
                        }
                        chunk = null;
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, false);
                    } while (chunks.TryDequeue(out chunk));
                    System.Threading.Thread.Sleep(1);
                }

                Console.WriteLine($"{file.Key} Total: {writtenSize.Result} Expected: {file.Value.FileSize}");
            }

            return "Hello World";
        }

        public static byte[] Decompress(byte[] data)
        {
            using (MemoryStream str = new MemoryStream(data))
                return Decompress(str);
        }

        public static byte[] Decompress(Stream str)
        {
            using (MemoryStream result = new MemoryStream())
            using (ZlibStream gzip = new ZlibStream(result, Ionic.Zlib.CompressionMode.Decompress))
            {
                byte[] buffer = new byte[1024];
                int len;
                while ((len = str.Read(buffer, 0, buffer.Length)) > 0)
                    gzip.Write(buffer, 0, len);
                gzip.Flush();

                result.Position = 0;
                return result.ToArray();
            }
        }
    }
}
