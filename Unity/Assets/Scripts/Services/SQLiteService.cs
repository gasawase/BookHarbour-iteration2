using Assets.Scripts.Models;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using Unity.VisualScripting;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.VirtualTexturing;
using static Assets.Scripts.Models.Enums;
using static Unity.VisualScripting.AnnotationUtility;
using static UnityEngine.Audio.ControlContext;

namespace Assets.Scripts.Services
{
    public class SQLiteService
    {
        private readonly string _filePath;
        public SQLiteService(string filePath)
        {
            _filePath = filePath;
            InitializeDatabase();
        }
        public void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(_filePath)) // so this connection only lives for the duration of this using
            {
                string booksSql = "CREATE TABLE IF NOT EXISTS books (BookID TEXT PRIMARY KEY, ISBN TEXT, Title TEXT, Authors TEXT, Publishers TEXT, " +
                    "Language TEXT, Description TEXT, Subjects TEXT, Series TEXT, SeriesIndex int, EpubVersion TEXT, PageCount int, " +
                    "AverageRating REAL, RatingsCount INT, ThumbnailURL TEXT, CoverImageHref TEXT, FilePath TEXT, FileName TEXT, " +
                    "FileSizeBytes int, DatePublished TEXT, DateAdded TEXT, PageCountSource TEXT, OpenLibraryKey TEXT);";
                string userBookDataSql = "CREATE TABLE IF NOT EXISTS user_book_data (BookID TEXT PRIMARY KEY, ReadingStatus TEXT, PersonalRating int, Review TEXT, DateStarted DATE, " +
                    "DateFinished DATE, TotalMinutesRead int, CurrentCFI TEXT, " +
                    "CONSTRAINT fk_UserBookData_Books FOREIGN KEY (BookID) REFERENCES books(BookID));";
                string shelvesSql = "CREATE TABLE IF NOT EXISTS shelves (ShelfID TEXT PRIMARY KEY, Name TEXT, ShelfType TEXT, DateCreated DATE, SortOrder int);"; // shelf_type: distinguishes user-curated shelves from system-generated ones so you don't let the user manually reorder a genre shelf
                string shelfPositionsSql = "CREATE TABLE IF NOT EXISTS shelf_positions (ShelfID TEXT, BookID TEXT UNIQUE, SlotIndex int, DatePlaced DATE, " +
                    "PRIMARY KEY (BookID, ShelfID), CONSTRAINT fk_ShelfPositions_Shelves FOREIGN KEY (ShelfID) REFERENCES shelves(ShelfID), CONSTRAINT fk_ShelfPositions_Books FOREIGN KEY (BookID) REFERENCES books(BookID));";
                string tagsSql = "CREATE TABLE IF NOT EXISTS tags (TagID TEXT PRIMARY KEY, Name TEXT UNIQUE, Color TEXT);";
                string bookTagsSql = "CREATE TABLE IF NOT EXISTS book_tags (BookID TEXT, TagID TEXT, " +
                    "PRIMARY KEY (BookID, TagID), CONSTRAINT fk_BookTags_Books FOREIGN KEY (BookID) REFERENCES books(BookID), CONSTRAINT fk_BookTags_Tags FOREIGN KEY (TagID) REFERENCES tags(TagID));";
                string highlightsSql = "CREATE TABLE IF NOT EXISTS highlights (HighlightID TEXT PRIMARY KEY, BookID TEXT, CFIStart TEXT, CFIEnd TEXT, Color TEXT, " +
                    "HighlightedText TEXT, DateCreated DATE, " +
                    "CONSTRAINT fk_Highlights_Books FOREIGN KEY (BookID) REFERENCES books(BookID));";
                string annotationsSql = "CREATE TABLE IF NOT EXISTS annotations (AnnotationID TEXT PRIMARY KEY, BookID TEXT, Type TEXT, CFI TEXT, CFIEnd TEXT, Content TEXT, " +
                    "DateCreated DATE, DateUpdated DATE, " +
                    "CONSTRAINT fk_Annotations_Books FOREIGN KEY (BookID) REFERENCES books(BookID));";
                string bookSeriesSql = "CREATE TABLE IF NOT EXISTS book_series (BookID TEXT, SeriesName TEXT, Position REAL, " +
                    "PRIMARY KEY (BookID, SeriesName), CONSTRAINT fk_BookSeries_Books FOREIGN KEY (BookID) REFERENCES books(BookID));";

                /// Things worth noting about this design
                /// authors, publishers, and subjects are stored as JSON arrays in TEXT fields — SQLite doesn't have an array type, and this avoids needing extra junction tables for simple lists you'll mostly read whole
                /// highlights and annotations are separate tables because highlights are visual(color, selected text) while annotations are content(notes, bookmarks) — keeping them separate makes querying cleaner
                /// shelf_type on shelves distinguishes user - curated shelves from system - generated ones so you never accidentally let the user manually reorder a genre shelf
                /// page_count_source tracks where the page count came from so you know which ones are estimates vs exact

                connection.Execute(booksSql);
                connection.Execute(userBookDataSql);
                connection.Execute(shelvesSql);
                connection.Execute(shelfPositionsSql);
                connection.Execute(tagsSql);
                connection.Execute(bookTagsSql);
                connection.Execute(highlightsSql);
                connection.Execute(annotationsSql);
                connection.Execute(bookSeriesSql);
            }
        }
        public void InsertBookIntoDatabase(EpubMetadataModel bookModel)
        {
            using (var connection = new SQLiteConnection(_filePath)) // so this connection only lives for the duration of this using
            {
                string authorsJson = JsonConvert.SerializeObject(bookModel.Authors);
                string publishersJson = JsonConvert.SerializeObject(bookModel.Publishers);
                string subjectsJson = JsonConvert.SerializeObject(bookModel.Subjects);

                // insert into books
                string sql = "INSERT OR IGNORE INTO books (BookID, ISBN, Title, Authors, Publishers, Language, Description, Subjects, " +
                    " EpubVersion, PageCount, AverageRating, RatingsCount, CoverImageHref, FilePath, FileName, FileSizeBytes, DatePublished, DateAdded, PageCountSource) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, " +
                           "?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?); ";
                connection.Execute(sql, bookModel.bookID.ToString(), bookModel.ISBN, bookModel.Title, authorsJson, publishersJson, bookModel.Language, bookModel.Description, subjectsJson, bookModel.EpubVersion, bookModel.PageCount,
                    bookModel.AverageRating, bookModel.RatingsCount, bookModel.CoverImageHref, bookModel.FilePath, bookModel.FileName, bookModel.FileSizeBytes, FormatDates(bookModel.PublishedDate), FormatDates(DateTime.Today.Date.ToString()), bookModel.PageCountSource);
                // insert a default row into user_book_data for that same book_id to test foreign keying
                string sqlDefData = "INSERT OR IGNORE INTO user_book_data (BookID, ReadingStatus, TotalMinutesRead) " +
                    "VALUES (?, ?, ?)";
                connection.Execute(sqlDefData, bookModel.bookID.ToString(), eReadingStatus.Unread.ToString(), 0);

                if (bookModel.SeriesMemberships != null && bookModel.SeriesMemberships.Count > 0)
                {
                    InsertBookSeriesIntoDatabase(bookModel.bookID.ToString(), bookModel.SeriesMemberships);
                }
            }
        }

        public void InsertBookSeriesIntoDatabase(string bookId, List<BookSeriesEntry> bookSeriesList)
        {
            using (var connection = new SQLiteConnection(_filePath)) // so this connection only lives for the duration of this using
            {
                string sqlSeries = "INSERT OR IGNORE INTO book_series (BookID, SeriesName, Position) VALUES (?, ?, ?)";
                foreach (BookSeriesEntry series in bookSeriesList)
                {
                    connection.Execute(sqlSeries, bookId, series.SeriesName, series.Position);
                }
            }
        }

        public void InsertShelfIntoDatabase(ShelfModel shelfModel)
        {
            using (var connection = new SQLiteConnection(_filePath))
            {
                string sql = "INSERT OR IGNORE INTO shelves (ShelfID, Name, ShelfType, DateCreated, SortOrder) " +
                    "VALUES (?, ?, ?, ?, ?); ";
                connection.Execute(sql, shelfModel.shelfId, shelfModel.shelfName, shelfModel.shelfType, FormatDates(DateTime.Now.Date.ToString()), shelfModel.sortOrder);
            }
        }

        // for now, this is unused; we'll need to reuse this for when the user can edit the book information
        public void UpdateBookInDatabase(EpubMetadataModel bookModel)
        {
            Guid guidFromTitle = UUIDHelper.GenerateDeterministicGuid($"{bookModel.Title}::{bookModel.Authors.FirstOrDefault()}".ToLowerInvariant().Trim());
            Guid guidFromISBN = UUIDHelper.GenerateDeterministicGuid(bookModel.ISBN);

            string sql = "UPDATE books SET PageCount = ?, ISBN = ? WHERE BookID = ? OR BookID = ?";
            //string sql = "SELECT * FROM books WHERE BookID = @TitleGuid OR BookID = @ISBNGuid";

            using (var connection = new SQLiteConnection(_filePath))
            {
                // get the current object in the database to compare it to what we have currently? we'll have to update the BookID, PageCount, PageCountSource, PublishedDate, and Description at the moment

                // see if we have an entry already in the database by the guid from the title, the guid from the isbn, or from the title
                connection.Execute(sql, bookModel.PageCount, bookModel.ISBN, guidFromTitle.ToString(), guidFromISBN.ToString());
                //var affectedRows = connection.QueryFirst(sql, new { TitleGuid = guidFromTitle.ToString(), ISBNGuid = guidFromISBN.ToString() });
            }
        }

        public void UpdateCoverImagePath(string bookId, string cachedCoverPath)
        {
            string sql = "UPDATE books SET CoverImageHref = ? WHERE BookID = ?";

            using (var connection = new SQLiteConnection(_filePath))
            {
                connection.Execute(sql, cachedCoverPath, bookId);
            }
        }

        public void UpdateBookLocation(string bookId, string shelfId, int? slotIndex, DateTime datePlaced)
        {
            Debug.Log($"bookId = {bookId}, shelfId = {shelfId}, slotIndex = {slotIndex}, datePlaced = {FormatDates(datePlaced.ToString())}");
            string sql = "INSERT INTO shelf_positions (BookID, ShelfID, SlotIndex, DatePlaced)" +
                "VALUES (?, ?, ?, ?) " +
                "ON CONFLICT(BookID) DO UPDATE SET " +
                    "ShelfID = excluded.ShelfID, SlotIndex = excluded.SlotIndex, DatePlaced = excluded.DatePlaced;";
            Debug.Log(sql);
            using ( var connection = new SQLiteConnection(_filePath))
            {
                connection.Execute(sql, bookId, shelfId, slotIndex, FormatDates(datePlaced.ToString()));
            }
        }

        public string FormatDates(string oldDateAsString)
        {
            if(string.IsNullOrWhiteSpace(oldDateAsString))
            {
                throw new ArgumentException("Date string cannot be null or empty.", nameof(oldDateAsString));
            }

            oldDateAsString = oldDateAsString.Trim();

            // Month only case i.e. 2022-08: default to the first of the month
            if (DateTime.TryParseExact(oldDateAsString, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime monthOnly))
            {
                return monthOnly.ToString("yyy-MM-dd");
            }

            // ISO 8601 with offset i.e. 2022-01-31T00:00:00+00:00
            if (DateTimeOffset.TryParse(oldDateAsString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset withOffset))
            {
                return withOffset.Date.ToString("yyyy-MM-dd");
            }

            // Normal date i.e. 2022-05-17; probably don't need to format but just to be safe
            if (DateTime.TryParseExact(oldDateAsString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime plainDate))
            {
                return plainDate.ToString("yyyy-MM-dd");
            }

            throw new FormatException($"Unrecognized date format: '{oldDateAsString}'");
        }
    }
}
