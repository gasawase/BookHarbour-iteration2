using Assets.Scripts.Extensions;
using Assets.Scripts.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static Assets.Scripts.Models.Enums;


namespace Assets.Scripts.Services
{
    public class APIDataFetcherService
    {
        private readonly SQLiteService _sqliteService;
        private static string? _apiKey;
        public APIDataFetcherService(SQLiteService sqliteService)
        {
            _sqliteService = sqliteService;
            TextAsset keyFile = Resources.Load<TextAsset>("Secrets/GoogleBooksApiKey");
            if (keyFile != null) { _apiKey = keyFile.text.Trim(); }
            else { Debug.LogWarning("No Google Books API key found; using unauthenticated requests."); }
        } // enabling the HTTP service

        public async Task GetDataFromGoogle(EpubMetadataModel book, eSearchMode startMode)
        {
            if (HasPageCount(book)) { book.PageCountSource = ePageCountSource.Epub.ToString(); }

            bool enriched = false;
            eSearchMode mode = startMode;

            while (mode != eSearchMode.None)
            {
                GoogleBooksItem match = await FindMatch(book, mode);
                if (match != null)
                {
                    ApplyMatch(book, match);
                    enriched = true;
                }
                if (enriched && (HasPageCount(book) || mode != eSearchMode.ISBN))
                {
                    // we have the book and either have a page count or only the ISBN needs to be retried
                    break;
                }
                mode = NextMode(mode);
            }
            if (!enriched)
            {
                Debug.Log($"Could not enrich {book.Title}.");
            }
            if (!HasPageCount(book))
            {
                // TODO: Hardcover goes here
                Debug.Log($"No page count found for {book.Title}.");
                book.PageCount = "0";
                book.PageCountSource = ePageCountSource.None.ToString();
            }
        }

        private static string CreateGoogleURL(EpubMetadataModel book, eSearchMode searchMode)
        {
            string isbn = book.ISBN; // get this from the json
            string languageLimiter = book.Language; // get this from the json but put "" if nothing
            string titleLimiter = book.Title;
            string authorLimiter = book.Authors.FirstOrDefault();

            StringBuilder googleURL = new StringBuilder();
            googleURL.Append("https://www.googleapis.com/books/v1/volumes?q=");

            switch (searchMode)
            {
                case eSearchMode.ISBN:
                    googleURL.Append("isbn:").Append(Uri.EscapeDataString(isbn));
                    break;
                case eSearchMode.TitleAndAuthor:
                    googleURL.Append("+intitle:").Append(Uri.EscapeDataString(titleLimiter))
                        .Append("+inauthor:").Append(Uri.EscapeDataString(authorLimiter))
                        .Append("&maxResults=1");
                    break;
                case eSearchMode.TitleOnly:
                    googleURL.Append("+intitle:").Append(Uri.EscapeDataString(titleLimiter));
                    break;
                case eSearchMode.AuthorOnly:
                    googleURL.Append("+inauthor:").Append(Uri.EscapeDataString(authorLimiter));
                    break;
                default:
                    break;
            }
            if (!languageLimiter.IsNullOrWhiteSpace())
            {
                googleURL.Append("&langRestrict=")
                    .Append(languageLimiter);
            }
            if (!_apiKey.IsNullOrWhiteSpace())
            {
                googleURL.Append("&key=")
                    .Append(_apiKey);
            }

            return googleURL.ToString();
        }
        private async Task<string> GetWithRetryAsync(string url, int maxRetries = 3)
        {
            int attempt = 0;
            while (attempt < maxRetries)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url)) // the actual request
                {
                    await request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        return request.downloadHandler.text;
                    }
                    else
                    {
                        attempt += 1;
                        string body = request.downloadHandler.text ?? string.Empty;
                        Debug.LogWarning($"Attempt {attempt}/{maxRetries} failed: HTTP {request.responseCode} - {request.error} | {body}");

                        if (attempt >= maxRetries)
                        {
                            Debug.LogError($"Error: trying to attempt a web request past the amount of max retries allowed | {url} ");
                            break;
                        }
                        else
                        {
                            int waitSeconds = (int)Math.Pow(2, attempt);
                            Debug.Log($"Waiting {waitSeconds}s before retry {attempt}/{maxRetries}...");
                            await Task.Delay(waitSeconds * 1000);
                        }

                    }
                }
            }
            throw new HttpRequestException("Max retries exceeded");
        }

        private static eSearchMode NextMode(eSearchMode currentMode)
        {
            switch (currentMode)
            {
                case eSearchMode.ISBN:
                    return eSearchMode.TitleAndAuthor;
                case eSearchMode.TitleAndAuthor:
                    return eSearchMode.TitleOnly;
                case eSearchMode.TitleOnly:
                    return eSearchMode.AuthorOnly;
                default:
                    return eSearchMode.None;
            }
        }

        private async Task<GoogleBooksItem> FindMatch(EpubMetadataModel book, eSearchMode mode)
        {
            if (mode == eSearchMode.ISBN && book.ISBN.IsNullOrWhiteSpace()) { return null; }
            if (book.Authors.FirstOrDefault().IsNullOrWhiteSpace() && (mode == eSearchMode.TitleAndAuthor || mode == eSearchMode.AuthorOnly)) { return null; }

            Debug.Log($"Trying {mode.ToString()} for {book.Title}...");
            string url = CreateGoogleURL(book, mode);
            string response = string.Empty;
            try
            {
                response = await GetWithRetryAsync(url);
            }
            catch (HttpRequestException ex)
            {
                Debug.Log($"{mode} lookup failed: {ex.Message}.");
                return null;
            }

            GoogleBooksResponse parsedResponse = JsonConvert.DeserializeObject<GoogleBooksResponse>(response);
            if (parsedResponse == null || parsedResponse.Items.Count == 0) { return null; }

            if (mode == eSearchMode.TitleOnly || mode == eSearchMode.AuthorOnly)
            {
                for (int i = 0; i < parsedResponse.Items.Count; i++)
                {
                    if (parsedResponse.Items[i].VolumeInfo.Title != null
                        && parsedResponse.Items[i].VolumeInfo.Title.ToLowerInvariant().Trim().Contains(book.Title.ToLowerInvariant().Trim()))
                    {
                        return parsedResponse.Items[i];
                    }
                }
                return null;
            }
            else { return parsedResponse.Items[0]; }
        }

        private static void ApplyMatch(EpubMetadataModel book, GoogleBooksItem match)
        {
            GoogleBooksVolumeInfo volume = match.VolumeInfo;
            // try to find the ISBN if we don't have one already
            if (book.ISBN.IsNullOrWhiteSpace() && volume.IndustryIdentifiers != null)
            {
                foreach (GoogleBooksIndustryIdentifier identifier in volume.IndustryIdentifiers)
                {
                    if (string.Equals(identifier.Type, "ISBN_13", StringComparison.OrdinalIgnoreCase))
                    {
                        book.ISBN = identifier.Identifier.ToString();
                        break;
                    }
                }
            }
            // if the book doesn't have a page count but the google books found one
            if (!HasPageCount(book) && volume.PageCount > 0)
            {
                book.PageCount = volume.PageCount.ToString();
                book.PageCountSource = ePageCountSource.GoogleBooksAPI.ToString();
            }
            // replace the book description with the volume description if it's empty
            string description = volume.Description ?? string.Empty;
            if (book.Description.Equals(string.Empty) && !description.Equals(string.Empty))
            {
                book.Description = description;
            }
            // replace the book published date with the volume published date if it's empty
            string publishedDate = volume.PublishedDate ?? string.Empty;
            if (book.PublishedDate.Equals(string.Empty) && !publishedDate.Equals(string.Empty))
            {
                book.PublishedDate = publishedDate;
            }
            // replace the book average rating with the volume average rating if it's empty
            double averageRating = volume.AverageRating;
            if (book.AverageRating == 0 && averageRating != 0)
            {
                book.AverageRating = averageRating;
            }
            // replace the book number of ratings with the volume number of ratings if it's empty
            int ratingsCount = volume.RatingsCount;
            if (book.RatingsCount == 0 && ratingsCount != 0)
            {
                book.RatingsCount = ratingsCount;
            }
        }

        private static bool HasPageCount(EpubMetadataModel book)
        {
            return !(book.PageCount.IsNullOrWhiteSpace() || book.PageCount == "0");
        }

        //TODO once you get all the basics set up
        //public async Task GetDataFromHardcover()
        //{

        //}
    }
}
