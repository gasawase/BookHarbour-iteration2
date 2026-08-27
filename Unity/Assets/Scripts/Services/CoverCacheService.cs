using System.IO;
using System.Threading.Tasks;

namespace Assets.Scripts.Services
{
    public class CoverCacheService
    {
        private readonly string _coverCacheDirectory;

        public CoverCacheService(string persistentDataPath)
        {
            _coverCacheDirectory = Path.Combine(persistentDataPath, "Covers");
            if (!File.Exists(_coverCacheDirectory))
            {
                Directory.CreateDirectory(_coverCacheDirectory);
            }
        }

        public string GetCachedCoverPath(string bookId, string fileExtension)
        {
            return Path.Combine(_coverCacheDirectory, $"{bookId}{fileExtension}");
        }

        public bool IsCoverCached(string bookId, string fileExtension)
        {
            return File.Exists(GetCachedCoverPath(bookId, fileExtension));
        }

        public async Task<string> CacheCoverAsync(string bookId, byte[] coverBytes, string fileExtension)
        {
            string cachedPath = GetCachedCoverPath(bookId, fileExtension);

            if (!File.Exists(cachedPath))
            {
                await File.WriteAllBytesAsync(cachedPath, coverBytes);
            }

            return cachedPath;
        }
    }
}
