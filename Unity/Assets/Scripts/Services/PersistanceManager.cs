using EpubParser.Models;
using EpubParser.Services;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PersistanceManager : MonoBehaviour
{
    public static PersistanceManager Instance;
    public SQLiteService sqliteService;
    public APIDataFetcherService apiFetcher;
    public CoverCacheService coverCacheService;
    string epubFolder = @"C:\Users\ThePa\Documents\GitHub\BookHarbourTempBackend\TestEpubs\SubFolder";
    //string epubFolder = @"C:\Users\ThePa\Desktop\Epubs";

    List<EpubMetadataModel> books;

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
        sqliteService = new SQLiteService(Path.Combine(Application.persistentDataPath, DATABASENAME));
        apiFetcher = new APIDataFetcherService(sqliteService);
        coverCacheService = new CoverCacheService(Application.persistentDataPath);
    }

    public async void ParseBooks()
    {
        ParseProgressReporter progressReporter = new ParseProgressReporter();
        progressReporter.BookParsed += InstantiateBook;
        // later: progressReporter.BookParsed += progressUI.UpdateProgress;
        books = await new EpubMetadataParserService(sqliteService, apiFetcher, progressReporter, coverCacheService)
            .ParseFolderToJSON(epubFolder, true, false);
    }

    // TEMP AND MOVE TO DEDICATED UI HANDLER LATER

    [SerializeField]
    GameObject UIBookObject;
    [SerializeField]
    GameObject UIBookPanel;

    void InstantiateBook(EpubMetadataModel bookModel)
    {
        GameObject locBookGO = Instantiate(UIBookObject, UIBookPanel.transform);
        UIBookData uiBookData = locBookGO.GetComponent<UIBookData>();

        StartCoroutine(TempCoroutine(bookModel, uiBookData));
        //title_txt.text = bookModel.Title;

        Debug.Log($"{bookModel.Title} spawned");
        
    }

    IEnumerator TempCoroutine(EpubMetadataModel bookModel, UIBookData uiBookData)
    {
        TMP_Text title_txt = uiBookData.title_textbox;
        Sprite locSprite = uiBookData.coverImg_Sprite;
        yield return null;
        title_txt.text = bookModel.Title;
        locSprite = LoadImageAsSprite(bookModel.CoverImageHref);
    }

    public static Texture2D LoadImage(string path)
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
            return null;
        }
    }

    public static Sprite LoadImageAsSprite(string path)
    {
        Sprite sprite = Sprite.Create(LoadImage(path), new Rect(0.0f, 0.0f, LoadImage(path).width,
        LoadImage(path).height), new Vector2(0.5f, 0.5f), 100.0f);

        return sprite;
    }

    #region Constants
    private const string FOLDERNOTFOUNDEXCEPTION = "Folder not found at {0}";
    private const string DATABASENAME = "database.db"; // in case I change the database name later
    #endregion
}
