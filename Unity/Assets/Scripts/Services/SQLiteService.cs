using EpubParser.Models;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Linq;
using static EpubParser.Models.Enums;

namespace EpubParser.Services
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
                    "FileSizeBytes int, DatePublished DATE, DateAdded DATE, PageCountSource TEXT, OpenLibraryKey TEXT);";
                string userBookDataSql = "CREATE TABLE IF NOT EXISTS user_book_data (BookID TEXT PRIMARY KEY, ReadingStatus TEXT, PersonalRating int, Review TEXT, DateStarted DATE, " +
                    "DateFinished DATE, TotalMinutesRead int, CurrentCFI TEXT, " +
                    "CONSTRAINT fk_UserBookData_Books FOREIGN KEY (BookID) REFERENCES books(BookID));";
                string shelvesSql = "CREATE TABLE IF NOT EXISTS shelves (ShelfID TEXT PRIMARY KEY, Name TEXT, ShelfType TEXT, DateCreated DATE, SortOrder int);";
                string shelfPositionsSql = "CREATE TABLE IF NOT EXISTS shelf_positions (PositionID TEXT PRIMARY KEY, ShelfID TEXT, BookID TEXT, SlotIndex int, DatePlaced DATE, " +
                    "CONSTRAINT fk_ShelfPositions_Shelves FOREIGN KEY (ShelfID) REFERENCES shelves(ShelfID), CONSTRAINT fk_ShelfPositions_Books FOREIGN KEY (BookID) REFERENCES books(BookID));";
                string tagsSql = "CREATE TABLE IF NOT EXISTS tags (TagID TEXT PRIMARY KEY, Name TEXT UNIQUE, Color TEXT);";
                string bookTagsSql = "CREATE TABLE IF NOT EXISTS book_tags (BookID TEXT, TagID TEXT, " +
                    "PRIMARY KEY (BookID, TagID), CONSTRAINT fk_BookTags_Books FOREIGN KEY (BookID) REFERENCES books(BookID), CONSTRAINT fk_BookTags_Tags FOREIGN KEY (TagID) REFERENCES tags(TagID));";
                string highlightsSql = "CREATE TABLE IF NOT EXISTS highlights (HighlightID TEXT PRIMARY KEY, BookID TEXT, CFIStart TEXT, CFIEnd TEXT, Color TEXT, " +
                    "HighlightedText TEXT, DateCreated DATE, " +
                    "CONSTRAINT fk_Highlights_Books FOREIGN KEY (BookID) REFERENCES books(BookID));";
                string annotationsSql = "CREATE TABLE IF NOT EXISTS annotations (AnnotationID TEXT PRIMARY KEY, BookID TEXT, Type TEXT, CFI TEXT, CFIEnd TEXT, Content TEXT, " +
                    "DateCreated DATE, DateUpdated DATE, " +
                    "CONSTRAINT fk_Annotations_Books FOREIGN KEY (BookID) REFERENCES books(BookID));";

                connection.Execute(booksSql);
                connection.Execute(userBookDataSql);
                connection.Execute(shelvesSql);
                connection.Execute(shelfPositionsSql);
                connection.Execute(tagsSql);
                connection.Execute(bookTagsSql);
                connection.Execute(highlightsSql);
                connection.Execute(annotationsSql);
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
                string sql = "INSERT OR IGNORE INTO books (BookID, ISBN, Title, Authors, Publishers, Language, Description, Subjects, Series, SeriesIndex," +
                    " EpubVersion, PageCount, AverageRating, RatingsCount, CoverImageHref, FilePath, FileName, FileSizeBytes, DatePublished, DateAdded, PageCountSource) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?," +
                           " ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?); ";
                connection.Execute(sql, bookModel.bookID.ToString(), bookModel.ISBN, bookModel.Title, authorsJson, publishersJson, bookModel.Language, bookModel.Description, subjectsJson, bookModel.Series, bookModel.SeriesIndex, bookModel.EpubVersion, bookModel.PageCount,
                    bookModel.AverageRating, bookModel.RatingsCount, bookModel.CoverImageHref, bookModel.FilePath, bookModel.FileName, bookModel.FileSizeBytes, bookModel.PublishedDate, DateTime.Today.Date, bookModel.PageCountSource);
                // insert a default row into user_book_data for that same book_id to test foreign keying
                string sqlDefData = "INSERT OR IGNORE INTO user_book_data (BookID, ReadingStatus, TotalMinutesRead) " +
                    "VALUES (?, ?, ?)";
                connection.Execute(sqlDefData, bookModel.bookID.ToString(), eReadingStatus.Unread.ToString(), 0);
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
    }
}
