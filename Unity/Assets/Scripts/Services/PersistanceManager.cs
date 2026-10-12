using Assets.Scripts.Models;
using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-100)] // make sure that this runs before everything else
public class PersistanceManager : MonoBehaviour
{
    public static PersistanceManager Instance;
    public SQLiteService sqliteService { get; private set; }
    public APIDataFetcherService apiFetcher;
    public CoverCacheService coverCacheService;
    internal ShelfLayoutService shelfLayoutService;
    internal SpawningService spawningService;
    //string epubFolder = @"C:\Users\ThePa\Documents\GitHub\BookHarbourTempBackend\TestEpubs\SubFolder\OneBook";
    string epubFolder = @"C:\Users\ThePa\Documents\GitHub\BookHarbourTempBackend\TestEpubs\SubFolder";
    public static Dictionary<string, UIBookData> bookIDToUIBookDataDict;
    //string epubFolder = @"C:\Users\ThePa\Desktop\Epubs";
    public string _dbPath = "";
    public string cachedCoversDirectoryPath;
    public UIBookData uiBookData;
    public List<BookshelfModel> shelvesInScene;
    public event Action PersistenceManagerReady;

    #region CONSTANTS
    public const float ITEMPADDING = 0f;
    public const float MAGNET = 0.015f; // how close to its neighbor does it have to be before it snaps flush next to it, example is 1.5cm
    #endregion

    List<EpubMetadataModel> books;

    [SerializeField]
    GameObject UIBookObject;
    [SerializeField]
    GameObject UIBookPanel;

    private void Awake()
    {
        Debug.Log(Application.persistentDataPath);
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeScene();
        OnSceneLoaded();
        PersistenceManagerReady?.Invoke();
    }

    public void InitializeScene()
    {
        _dbPath = Path.Combine(Application.persistentDataPath, DATABASENAME);
        sqliteService = new SQLiteService(_dbPath);
        apiFetcher = new APIDataFetcherService(sqliteService);
        coverCacheService = new CoverCacheService(Application.persistentDataPath);
        if (this.TryGetComponent<SpawningService>(out SpawningService gotSpawningService))
        {
            spawningService = gotSpawningService;
        }
        shelfLayoutService = new ShelfLayoutService(sqliteService, spawningService);
        bookIDToUIBookDataDict = new Dictionary<string, UIBookData>();
        shelvesInScene = new List<BookshelfModel>();
    }

    public void OnSceneLoaded() // should be called once the scene is loaded; the end of this method should result in the user being shown the scenes
    {

    }

    public async void ParseBooks()
    {
        ProgressReporter progressReporter = new ProgressReporter();
        progressReporter.BookParsed += OnBookParsed;
        progressReporter.CoverCached += SetButtonImage;
        // later: progressReporter.BookParsed += progressUI.UpdateProgress;
        books = await new EpubMetadataParserService(sqliteService, apiFetcher, progressReporter, coverCacheService)
            .ParseFolderToJSON(epubFolder, true, false);
    }

    // TEMP AND MOVE TO DEDICATED UI HANDLER LATER

    public void OnBookParsed(EpubMetadataModel bookModel)
    {
        string bookID = bookModel.bookID.ToString();
        if (bookIDToUIBookDataDict.ContainsKey(bookID)) { return; } // already on the panel?
        if (!string.IsNullOrEmpty(sqliteService.SelectShelfByObjectID(bookModel.bookID.ToString()))) { return; }
        InstantiateUIBook(bookModel);
    }

    public UIBookData InstantiateUIBook(EpubMetadataModel bookModel)
    {
        string bookID = bookModel?.bookID.ToString();
        if (bookIDToUIBookDataDict.TryGetValue(bookID, out UIBookData existing)) { return existing; }

        GameObject locBookGO = Instantiate(UIBookObject, UIBookPanel.transform);
        uiBookData = locBookGO.GetComponent<UIBookData>();
        uiBookData.bookId = bookID;
        bookIDToUIBookDataDict.Add(bookID, uiBookData);
        return uiBookData;
    }

    public void RestoreBookToPanel(string bookId)
    {
        EpubMetadataModel bookInfo = new EpubMetadataModel();
        if (bookIDToUIBookDataDict.ContainsKey(bookId)) { return; }
        bookInfo = sqliteService.SelectLiteBookInfo(bookId);
        if (bookInfo == null) { throw new System.Exception("Book info not found"); }
        UIBookData createdUIBook = InstantiateUIBook(bookInfo);
        SetButtonImage(bookId, bookInfo.CoverImageHref, createdUIBook);
    }

    public void RemoveBookEntryFromUIBookDataDict(string bookId)
    {
        try
        {
            if (bookIDToUIBookDataDict.TryGetValue(bookId, out UIBookData value))
            {
                bookIDToUIBookDataDict.Remove(bookId);
            }
            else
            {
                Debug.Log($"Book with ID {bookId} is not in local dictionary of available books in the UI.");
            }
        }
        catch
        {

        }
    }
    void SetButtonImage(string bookId, string cachedCoverPath, UIBookData uiBookData)
    {
        if (bookIDToUIBookDataDict.TryGetValue(bookId, out UIBookData uiBookDataFound))
        {
            uiBookDataFound.book_button.image.sprite = LoadImageAsSprite(cachedCoverPath, bookId);

        }
    }

    public static Texture2D LoadImage(string path, string bookId)
    {
        if (File.Exists(path))
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);

            return tex;
        }
        else
        {
            EpubMetadataModel bookInfo = PersistanceManager.Instance.sqliteService.SelectLiteBookInfo(bookId);
            if (bookIDToUIBookDataDict.TryGetValue(bookId, out UIBookData uiBookDataFound))
            {
                TMP_Text title_txt = uiBookDataFound.title_textbox;
                title_txt.text = bookInfo.Title;
            }
            return null;
        }
    }

    public static Sprite LoadImageAsSprite(string path, string bookId)
    {
        Sprite sprite = Sprite.Create(LoadImage(path, bookId), new Rect(0.0f, 0.0f, LoadImage(path, bookId).width,
        LoadImage(path, bookId).height), new Vector2(0.5f, 0.5f), 100.0f);

        return sprite;
    }

    public void ResetAllObjectsOnAllShelves()
    {
        foreach (BookshelfModel bookshelf in shelvesInScene)
        {
            ClearBookshelf(bookshelf);
        }
    }

    public void ClearBookshelf(BookshelfModel bookshelf)
    {
        if (bookshelf == null)
        {
            Debug.LogWarning($"Bookshelf not registered. Skipping...");
            return;
        }

        foreach (ShelfModel shelf in bookshelf.shelves)
        {
            if (shelf != null) { shelfLayoutService.ClearShelf(shelf.shelfId); }
        }
    }

    public void RegisterShelf(BookshelfModel shelf)
    {
        if (!shelvesInScene.Contains(shelf))
        {
            shelvesInScene.Add(shelf);
        }
    }

    #region Constants
    private const string FOLDERNOTFOUNDEXCEPTION = "Folder not found at {0}";
    private const string DATABASENAME = "database.db"; // in case I change the database name later
    #endregion
}
