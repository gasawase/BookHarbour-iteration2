using EpubParser.Extensions;
using EpubParser.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using static EpubParser.Models.Enums;


namespace EpubParser.Services
{
    public class APIDataFetcherService
    {
        private readonly SQLiteService _sqliteService;
        private readonly HttpClient _httpClient = new HttpClient();
        private static string? _apiKey;
        public APIDataFetcherService(SQLiteService sqliteService)
        {
            _sqliteService = sqliteService;

            //_apiKey = Environment.GetEnvironmentVariable("GOOGLE_BOOKS_API_KEY");
            _apiKey = "AIzaSyAQDZEjw0Ly015iekdLMq44DlyvSDyK_YI";

        } // enabling the HTTP service

        public async Task GetDataFromGoogle(EpubMetadataModel book, bool hasISBN)
        {
            // JsonSerializerSettings serializerOptions = new JsonSerializerSettings { Formatting = Formatting.Indented }; only needed if we're writing to json later

            // need to parse and replace the spaces and stuff for symbols that are going to be accepted in the url

            Debug.Log($"Getting info for {book.Title} by {book.Authors.FirstOrDefault()}");

            string googleURL = CreateGoogleURL(book, hasISBN);

            string response = await GetWithRetryAsync(googleURL.ToString());

            GoogleBooksResponse parsedResponse = new GoogleBooksResponse();
            parsedResponse = JsonConvert.DeserializeObject<GoogleBooksResponse>(response);

            if (parsedResponse == null || parsedResponse.TotalItems == 0 || parsedResponse.Items.Count == 0)
            {
                // get google results again, maybe make recursive?
                Debug.Log($"Could not enrich: {book.Title} by {book.Authors.FirstOrDefault()}");
                return;
            }
            else
            {
                if (!hasISBN && parsedResponse.Items[0].VolumeInfo.IndustryIdentifiers != null)
                {
                    foreach (GoogleBooksIndustryIdentifier identifier in parsedResponse.Items[0].VolumeInfo.IndustryIdentifiers)
                    {
                        if (identifier.Type.ToLowerInvariant().Trim() == "ISBN_13".ToLowerInvariant().Trim())
                        {
                            book.ISBN = identifier.Identifier.ToString();
                        }
                    }
                }
                int pageCount = parsedResponse.Items[0].VolumeInfo.PageCount;
                if ((book.PageCount.Equals("0") || book.PageCount.Equals(string.Empty)) && pageCount > 0)
                {
                    book.PageCount = pageCount.ToString();
                    book.PageCountSource = ePageCountSource.GoogleBooksAPI.ToString();
                    //TODO: get data from HardcoverAPI if the response is null
                }
                else if (pageCount == 0 && hasISBN) // we need to call this function again but without the ISBN since we got nothing from searching for the ISBN
                {
                    EpubMetadataModel fallbackBook = book;
                    fallbackBook.ISBN = string.Empty;
                    await GetDataFromGoogle(fallbackBook, false);
                }
                else
                {
                    Debug.Log($"No page count found for {book.Title} by {book.Authors.First()}");
                    book.PageCount = "0";
                    //book.PageCountSource = ePageCountSource.Epub.ToString(); // TODO: need to make sure that we can actually GET a pagecount from some epubs; if not, we need to change this enum

                }
                string description = parsedResponse.Items[0].VolumeInfo.Description ?? string.Empty;
                if (book.Description.Equals(string.Empty) && !description.Equals(string.Empty))
                {
                    book.Description = description;
                }
                string publishedDate = parsedResponse.Items[0].VolumeInfo.PublishedDate ?? string.Empty;
                book.PublishedDate = publishedDate;

                double averageRating = parsedResponse.Items[0].VolumeInfo.AverageRating;
                if (book.AverageRating == 0 && averageRating != 0)
                {
                    book.AverageRating = averageRating;
                }

                int ratingsCount = parsedResponse.Items[0].VolumeInfo.RatingsCount;
                if (book.RatingsCount == 0 && ratingsCount != 0)
                {
                    book.RatingsCount = ratingsCount;
                }

                //_sqliteService.UpdateBookInDatabase(book);
            }
        }

        private static string CreateGoogleURL(EpubMetadataModel book, bool hasISBN)
        {
            string isbn = book.ISBN; // get this from the json
            string languageLimiter = book.Language; // get this from the json but put "" if nothing
            string titleLimiter = book.Title;
            string authorLimiter = book.Authors.FirstOrDefault();

            StringBuilder googleURL = new StringBuilder();
            googleURL.Append("https://www.googleapis.com/books/v1/volumes?q=");

            if (hasISBN)
            {
                googleURL.Append("isbn:").Append(Uri.EscapeDataString(isbn));
            }
            else
            {
                googleURL.Append("+intitle:").Append(Uri.EscapeDataString(titleLimiter))
                    .Append("+inauthor:").Append(Uri.EscapeDataString(authorLimiter))
                    .Append("&maxResults=1");
            }
            googleURL.Append("&langRestrict=")
                     .Append(languageLimiter)
                     .Append("&key=")
                     .Append(_apiKey);

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
                    else if (request.responseCode == 503 || request.responseCode == 429)
                    {
                        // retry path
                        attempt++;
                        if (attempt >= maxRetries)
                        {
                            Debug.Log($"Failed after {maxRetries} attempts: {url}");
                        }
                        int waitSeconds = (int)Math.Pow(2, attempt);
                        Debug.Log($"Rate limited, waiting {waitSeconds}s before retry {attempt}/{maxRetries}...");
                        await Task.Delay(waitSeconds * 1000);
                    }
                    else
                    {
                        attempt++;
                        if (attempt >= maxRetries)
                        {
                            Debug.Log($"Other response code recieved: {request.responseCode} -- {request.error}");
                        }
                        int waitSeconds = (int)Math.Pow(2, attempt);
                        Debug.Log($"Rate limited, waiting {waitSeconds}s before retry {attempt}/{maxRetries}...");
                        await Task.Delay(waitSeconds * 1000);
                    }
                }
            }
            throw new HttpRequestException("Max retries exceeded");
        }

        //TODO once you get all the basics set up
        public async Task GetDataFromHardcover()
        {

        }
    }
}
