using EpubParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace EpubParser.Services
{
    public class EpubMetadataParserService
    {
        private readonly SQLiteService _sqliteService;
        private readonly APIDataFetcherService _apidataFetcherService;
        private readonly IParseProgressReporter _progressReporter;
        private readonly CoverCacheService _coverCacheService;
        public EpubMetadataParserService(SQLiteService sqliteService, APIDataFetcherService apidataFetcherService, IParseProgressReporter progressReporter, CoverCacheService coverCacheService)
        {
            _sqliteService = sqliteService;
            _apidataFetcherService = apidataFetcherService;
            _progressReporter = progressReporter;
            _coverCacheService = coverCacheService;
        }

        public async Task<List<EpubMetadataModel>> ParseFolderToJSON(string folderPath, bool recursive = false, bool usesCalibre = false)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException(string.Format(FOLDERNOTFOUNDEXCEPTION, folderPath));
            }
            string searchPattern = usesCalibre ? "*.opf" : "*.epub";
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            string[] dataFiles = Directory.GetFiles(folderPath, searchPattern, searchOption);

            List<EpubMetadataModel> results = new List<EpubMetadataModel>();
            List<ParseError> errors = new List<ParseError>();


            foreach (var file in dataFiles)
            {
                try
                {
                    EpubMetadataModel meta = usesCalibre ? ParseOpf(file, usesCalibre) : ParseEpub(file, usesCalibre);
                    bool hasISBN = !String.IsNullOrEmpty(meta.ISBN);
                    await _apidataFetcherService.GetDataFromGoogle(meta, hasISBN);
                    _sqliteService.InsertBookIntoDatabase(meta);
                    results.Add(meta);
                    _progressReporter.ReportBookParsed(meta);
                }
                catch (Exception ex)
                {
                    var error = new ParseError { FilePath = file, Message = ex.Message };
                    errors.Add(new ParseError { FilePath = file, Message = ex.Message });
                    _progressReporter.ReportError(error);
                    // TODO: need to do some other handling for when you get a 503 error; I'm finding that it's happening when I do too many requests in a certain amount of time
                }
            }

            if (errors.Count > 0)
            {
                Debug.Log($"{errors.Count} file(s) had errors (see 'errors' array in JSON).");
            }

            return results;
        }
        private EpubMetadataModel ParseOpf(string opfFilePath, bool usesCalibre)
        {
            using var stream = File.OpenRead(opfFilePath);
            string folder = Path.GetDirectoryName(opfFilePath) ?? string.Empty;
            string filePath = Directory.GetFiles(folder, "*.epub").FirstOrDefault() ?? opfFilePath;
            string opfDirectory = Path.GetDirectoryName(opfFilePath)?.Replace('\\', '/') ?? string.Empty;
            Debug.Log($"opfDirectory: {opfDirectory}");
            return ParseOpfStream(stream, filePath, usesCalibre, opfDirectory, null); // TODO: need to fix this to get the actual epub path
        }
        public EpubMetadataModel ParseEpub(string epubPath, bool usesCalibre)
        {
            using var archive = ZipFile.OpenRead(epubPath); // we use "using" so that we handle and release the Zip file automatically even if there's an exception

            // get the OPF file path from where the container.xml points to 
            ZipArchiveEntry containerEntry = archive.GetEntry("META-INF/container.xml")
                ?? throw new InvalidDataException(MISSINGCONTAINERXML);

            string opfPath = ReadOpfPathFromContainer(containerEntry);
            string opfDirectory = Path.GetDirectoryName(opfPath)?.Replace('\\', '/') ?? string.Empty;
            // Open the OPF file with all the metadata
            var opfEntry = archive.GetEntry(opfPath)
                ?? throw new InvalidDataException(string.Format(MISSINGOPFFILE, opfPath));

            using var stream = opfEntry.Open();

            return ParseOpfStream(stream, epubPath, usesCalibre, opfDirectory, archive);
        }
        private string ReadOpfPathFromContainer(ZipArchiveEntry containerEntry)
        {
            using var filestream = containerEntry.Open();
            var doc = XDocument.Load(filestream);

            XNamespace ns = "urn:oasis:names:tc:opendocument:xmlns:container";
            var rootfile = doc.Descendants(ns + "rootfile").FirstOrDefault()
                ?? doc.Descendants("rootfile").FirstOrDefault() // if there is no namespace
                ?? throw new InvalidDataException(NOROOTFILEFOUND);

            return rootfile.Attribute("full-path")?.Value
                ?? throw new InvalidDataException(MISSINGFULLPATHATTRIBUTE);
        }

        private EpubMetadataModel ParseOpfStream(Stream? stream, string epubFilePath, bool usesCalibre, string opfDirectory, ZipArchive? archive)
        {
            // CoverCacheService coverCacheService = new CoverCacheService(Application.persistentDataPath);
            if (stream == null) throw new ArgumentNullException(NULLSTREAMEXCEPTION);
            var doc = XDocument.Load(stream);

            // account for the Dublin Core and opf namespaces
            XNamespace dc = "http://purl.org/dc/elements/1.1/";
            XNamespace opf = "http://www.idpf.org/2007/opf";

            var metadataFromOPF = doc.Descendants(opf + "metadata").FirstOrDefault()
                ?? doc.Descendants("metadata").FirstOrDefault();

            string Get(XName name) =>
                metadataFromOPF?.Element(name)?.Value?.Trim() ?? string.Empty;

            IEnumerable<string> GetAll(XName name) =>
                metadataFromOPF?.Elements(name).Select(e => e.Value.Trim()).ToList()
                ?? Enumerable.Empty<string>();

            // handling the different attributes of the metadata
            var package = doc.Root; // maybe quit here if they can't find anything so we don't have to do the package? for a null check below
                                    // first, the uuid
            string isbn = "";
            var UIDRefName = package?.Attribute("unique-identifier")?.Value;
            var identifierElements = metadataFromOPF?.Elements(dc + "identifier");
            var uid = identifierElements
                .FirstOrDefault(e => e.Attribute(opf + "scheme")?.Value == "UUID"
                    || (UIDRefName != null
                        && e.Attribute("{http://www.w3.org/XML/1998/namespace}id")?.Value == UIDRefName
                            || e.Attribute("id")?.Value == UIDRefName))?.Value?.Trim() ?? Get(dc + "identifier");
            //var elements = metadataFromOPF?.Elements(dc + "identifier");

            isbn = metadataFromOPF?.Elements(dc + "identifier").FirstOrDefault(e => e.Attribute(opf + "scheme")?.Value.Equals("ISBN", StringComparison.OrdinalIgnoreCase) == true)?.Value?.Trim() ?? string.Empty;

            if (uid.StartsWith("978") || uid.StartsWith("979"))
            {
                isbn = uid;
            }

            // series / reading order; this isn't included in Dublin Core
            string series = string.Empty;
            string seriesIndex = string.Empty;
            var metaMeta = metadataFromOPF?.Elements(opf + "meta").ToList()
                ?? metadataFromOPF?.Elements("meta").ToList()
                ?? new List<XElement>();

            foreach (var m in metaMeta)
            {
                string nameAttr = m.Attribute("name")?.Value ?? string.Empty;
                string contentAttr = m.Attribute("content")?.Value ?? string.Empty;

                if (nameAttr.Equals("calibre:series", StringComparison.OrdinalIgnoreCase))
                    series = contentAttr;
                else if (nameAttr.Equals("calibre:series_index", StringComparison.OrdinalIgnoreCase))
                    seriesIndex = contentAttr;
            }

            string coverHref = string.Empty;

            // get the cover image from the manifest so the first item with the id "cover-image" or properties="cover-image" (i think could be cover later)
            if (usesCalibre)
            {
                // TODO: we need to figure out how to get the manifest working for the chapters eventually
                var metaGuide = package?.Elements(opf + "guide") ?? package?.Elements("guide");
                coverHref = metaGuide?.Elements(opf + "reference")
                    .Concat(metaGuide?.Elements("reference") ?? Enumerable.Empty<XElement>())
                    .FirstOrDefault(e => e.Attribute("type")?.Value?.Contains("cover", StringComparison.OrdinalIgnoreCase) == true)
                    ?.Attribute("href")?.Value ?? string.Empty;
            }
            else
            {
                var manifest = doc.Descendants(opf + "manifest").FirstOrDefault()
                    ?? doc.Descendants("manifest").FirstOrDefault();
                coverHref = manifest?.Elements(opf + "item")
                    .Concat(manifest?.Elements("item") ?? Enumerable.Empty<XElement>())
                    .FirstOrDefault(e => e.Attribute("id")?.Value?.Contains("cover", StringComparison.OrdinalIgnoreCase) == true || e.Attribute("properties")?.Value?.Contains("cover-image") == true)
                    ?.Attribute("href")?.Value ?? string.Empty;
            }

            Guid bookGuid;
            // generate the UUID from the ISBN or the title+author
            if (!String.IsNullOrEmpty(isbn))
            {
                bookGuid = UUIDHelper.GenerateDeterministicGuid(isbn);
            }
            else
            {
                string titleAuthStr = $"{Get(dc + "title")}::{GetAll(dc + "creator").ToList().FirstOrDefault()}".ToLowerInvariant().Trim();
                bookGuid = UUIDHelper.GenerateDeterministicGuid(titleAuthStr);
            }

            string resolvedCoverPath = string.IsNullOrEmpty(opfDirectory) ? coverHref : $"{opfDirectory}/{coverHref}";
            byte[]? coverBytes = null;

            if (archive != null) // we're using calibre and don't need to unzip things
            {
                ZipArchiveEntry? coverEntry = archive.GetEntry(resolvedCoverPath);
                if (coverEntry != null)
                {
                    using var coverStream = coverEntry.Open();
                    using var memoryStream = new MemoryStream();
                    coverStream.CopyTo(memoryStream);
                    coverBytes = memoryStream.ToArray();
                }
            }
            else
            {
                if (File.Exists(resolvedCoverPath))
                {
                    coverBytes = File.ReadAllBytes(resolvedCoverPath);
                }
            }

            if (coverBytes != null)
            {
                // fire-and-forget: caching/DB-update/reporting run independently,
                // metadata parsing doesn't wait on them
                _ = CacheAndReportCoverAsync(bookGuid, coverBytes, Path.GetExtension(coverHref));
            }

            Debug.Log($"{Get(dc + "title")} | FilePath: {Directory.GetParent(epubFilePath)} | FileName: {Path.GetFileName(epubFilePath)} | resolvedCoverPath: {resolvedCoverPath} | path root: {Path.GetPathRoot(epubFilePath)}");

            return new EpubMetadataModel
            {
                bookID = bookGuid,
                FilePath = epubFilePath,
                FileName = Path.GetFileName(epubFilePath),
                FileSizeBytes = new FileInfo(epubFilePath).Length,

                // Core Dublin Core fields
                Title = Get(dc + "title"),
                Authors = GetAll(dc + "creator").ToList(),
                Publishers = GetAll(dc + "publisher").ToList(),
                Language = LanguageCleaner(Get(dc + "language")),
                Identifier = uid,
                ISBN = isbn,
                PageCount = "0",
                Description = Get(dc + "description"),
                Subjects = GetAll(dc + "subject").ToList(),
                Rights = Get(dc + "rights"),
                PublishedDate = Get(dc + "date"),
                Type = Get(dc + "type"),
                Format = Get(dc + "format"),
                Source = Get(dc + "source"),
                AverageRating = double.TryParse(Get(dc + "averageRating"), out double avgRating) ? avgRating : 0.0,
                RatingsCount = int.TryParse(Get(dc + "ratingsCount"), out int ratCount) ? ratCount : 0,
                //Relation        = Get(dc + "relation"),
                //Coverage        = Get(dc + "coverage"),

                // Series info (Calibre-style)
                Series = series,
                SeriesIndex = seriesIndex,

                // Packaging info
                EpubVersion = package?.Attribute("version")?.Value ?? string.Empty,
                CoverImageHref = resolvedCoverPath,
            };
        }
        private string LanguageCleaner(string dirtyLanguage)
        {
            if (dirtyLanguage == null) { return dirtyLanguage; }
            string cleanedLanguage = string.Empty;
            switch (dirtyLanguage)
            {
                case string s when s.StartsWith("en", StringComparison.OrdinalIgnoreCase):
                    cleanedLanguage = "en";
                    break;
                default:
                    cleanedLanguage = dirtyLanguage;
                    break;
            }
            return cleanedLanguage;
        }

        private async Task CacheAndReportCoverAsync(Guid bookId, byte[] coverBytes, string extension)
        {
            try
            {
                string cachedPath = await _coverCacheService.CacheCoverAsync(bookId.ToString(), coverBytes, extension);
                _sqliteService.UpdateCoverImagePath(bookId.ToString(), cachedPath);
                _progressReporter.ReportCoverCached(bookId.ToString(), cachedPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Cover caching failed for {bookId}: {ex.Message}");
            }
        }

        #region Constants
        private const string FOLDERNOTFOUNDEXCEPTION = "Folder not found at {0}";
        private const string MISSINGCONTAINERXML = "Missing META-INF/container.xml. Not a valid ePub.";
        private const string NOROOTFILEFOUND = "container.xml has no <rootfile> element.";
        private const string MISSINGFULLPATHATTRIBUTE = "container.xml <rootfile> is missing full-path attribute.";
        private const string MISSINGOPFFILE = "OPF file not found at path: {0}";
        private const string NULLSTREAMEXCEPTION = "Stream is null.";
        #endregion
    }
}
