using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;

namespace MapleStoryFullDownloaderNXL
{
    public class FileEntry
    {
        [JsonProperty(PropertyName = "fsize")]
        public long FileSize;
        [JsonProperty(PropertyName = "mtime")]
        public int ModifiedTime;
        [JsonProperty(PropertyName = "objects")]
        public List<string> ChunkHashes;
        [JsonProperty(PropertyName = "objects_fsize")]
        public List<string> ChunkSizesStrings;
        public int[] ChunkSizes { get => ChunkSizesStrings.Select(c => int.Parse(c)).ToArray();  }

        public IEnumerable<Task<Tuple<int, byte[]>>> Download(string productId)
        {
            int position = 0;
            for (int i = 0; i < ChunkHashes.Count; ++i)
            {
                string hash = ChunkHashes[i];
                int hashSize = ChunkSizes[i];
                yield return DownloadChunk(productId, hash, hashSize, position);
                position += hashSize;
            }
        }

        public static async Task<Tuple<int, byte[]>> DownloadChunk(string productId, string chunkHash, int expectedSize, int position)
        {
            using (HttpClient client = new HttpClient())
            {
                string chunkPath = $"https://download2.nexon.net/Game/nxl/games/{productId}/{productId}/{chunkHash.Substring(0, 2)}/{chunkHash}";

                bool wrongData = false;
                int retry = 0;
                do
                {
                    byte[] data = await client.GetByteArrayAsync(chunkPath);
                    try
                    {
                        byte[] decompressedData = Program.Decompress(data);
                        wrongData = decompressedData.Length != expectedSize;
                        return new Tuple<int, byte[]>(position, decompressedData);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Error decompressing chunk {chunkHash} from {chunkPath} ({data.Length} vs {expectedSize}), trying again.");
                        if (retry >= 5) throw;
                    }
                } while (!wrongData && retry++ < 5);
            }

            return null;
        }
    }
}
