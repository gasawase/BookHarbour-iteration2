using Assets.Scripts.Models;
using Assets.Scripts.Services;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class PersistanceManager : MonoBehaviour
{
    public static PersistanceManager Instance;
    public SQLiteService sqliteService { get; private set; }
    public APIDataFetcherService apiFetcher;
    public CoverCacheService coverCacheService;
    string epubFolder = @"C:\Users\ThePa\Documents\GitHub\BookHarbourTempBackend\TestEpubs\SubFolder\OneBook";
    //string epubFolder = @"C:\Users\ThePa\Documents\GitHub\BookHarbourTempBackend\TestEpubs\SubFolder";
    public static Dictionary<string, UIBookData> bookIDToUIBookDataDict;
    //string epubFolder = @"C:\Users\ThePa\Desktop\Epubs";
    public string _dbPath = "";
    public string cachedCoversDirectoryPath;

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
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
        _dbPath = Path.Combine(Application.persistentDataPath, DATABASENAME);
        sqliteService = new SQLiteService(_dbPath);
        apiFetcher = new APIDataFetcherService(sqliteService);
        coverCacheService = new CoverCacheService(Application.persistentDataPath);
        bookIDToUIBookDataDict = new Dictionary<string, UIBookData>();
    }

    public async void ParseBooks()
    {
        ParseProgressReporter progressReporter = new ParseProgressReporter();
        progressReporter.BookParsed += InstantiateUIBook;
        progressReporter.CoverCached += SetButtonImage;
        // later: progressReporter.BookParsed += progressUI.UpdateProgress;
        books = await new EpubMetadataParserService(sqliteService, apiFetcher, progressReporter, coverCacheService)
            .ParseFolderToJSON(epubFolder, true, false);
    }

    // TEMP AND MOVE TO DEDICATED UI HANDLER LATER

    void InstantiateUIBook(EpubMetadataModel bookModel)
    {
        GameObject locBookGO = Instantiate(UIBookObject, UIBookPanel.transform);
        UIBookData uiBookData = locBookGO.GetComponent<UIBookData>();
        if (bookIDToUIBookDataDict.TryAdd(bookModel.bookID.ToString(), uiBookData))
        {
            uiBookData.bookId = bookModel.bookID.ToString();

            //StartCoroutine(TempCoroutine(bookModel, uiBookData));
            //title_txt.text = bookModel.Title;
            Debug.Log($"{bookModel.Title} spawned");
        }
        else
        {
            Debug.Log($"Could not add {bookModel.Title} to internal dictionary of books!");
        }
    }

    public void RestoreBookToPanel(string bookId)
    {
        EpubMetadataModel bookInfo = new EpubMetadataModel();
        bookInfo = sqliteService.SelectLiteBookInfo(bookId);
        InstantiateUIBook(bookInfo);
        SetButtonImage(bookId, bookInfo.CoverImageHref);
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

    IEnumerator TempCoroutine(EpubMetadataModel bookModel, UIBookData uiBookData)
    {
        TMP_Text title_txt = uiBookData.title_textbox;
        yield return null;
        title_txt.text = bookModel.Title;

    }

    void SetButtonImage(string bookId, string cachedCoverPath)
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

    #region Constants
    private const string FOLDERNOTFOUNDEXCEPTION = "Folder not found at {0}";
    private const string DATABASENAME = "database.db"; // in case I change the database name later
    #endregion
}
