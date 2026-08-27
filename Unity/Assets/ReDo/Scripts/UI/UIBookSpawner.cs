using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIBookSpawner : UIHandler
{
    public DataProcessing perstistentDataHolder;

    public Dictionary<string, Book> locBookDict;

    public GameObject bookPrefabsParent;


    //private RectTransform bookDisplayContent;
    // Start is called before the first frame update
    void Start()
    {
        perstistentDataHolder = GameObject.FindGameObjectsWithTag("GameManager").First().GetComponent<DataProcessing>();
        locBookDict = new Dictionary<string, Book>();
        bookPrefabsParent = (GameObject) Resources.Load("BookPrefabParent");
        // Event Subscriptions
        perstistentDataHolder.BooksFinishedLoadingAction += OnBooksFinishedLoading;

    }

    void OnBooksFinishedLoading()
    {
        // clear books on each load and reset the list?
        locBookDict = perstistentDataHolder.bookDictionary;
        // instantiate the book prefabs
        // go through the book list and instantiate each book UI
        foreach (Book bookInstance in locBookDict.Values)
        {
            InstantiateBook(bookInstance);
            
        }
        // TODO: change to instantiating asynchronously
    }

    void InstantiateBook(Book bookInstance)
    {
        UIBookData uiBookData = bookPrefabsParent.GetComponent<PrefabParentScript>().UIGameObject.GetComponent<UIBookData>();
        TMP_Text title_txt = uiBookData.title_textbox;
        TMP_Text author_txt = uiBookData.author_textbox;
        // get title, cover, and author and set them
        title_txt.text = bookInstance.title;
        uiBookData.bookTitle = bookInstance.title;
        author_txt.text = bookInstance.pageCount.ToString();
        uiBookData.pageCount = bookInstance.pageCount.ToString();
        GameObject locBookGO = Instantiate(bookPrefabsParent, bookDisplayContent.transform); // the actual game object
        Debug.Log(bookInstance.title);
    }
    
    void ClearBookList()
    {
        locBookDict.Clear();
    }
}
