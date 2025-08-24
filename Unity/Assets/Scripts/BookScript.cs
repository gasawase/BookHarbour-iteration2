using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookScript : MonoBehaviour
{
    public BookModel bookModel;
    public string BookTitle;
    public string PageCount;
    public string BookScaleFactor;

    [SerializeField] public GameObject BookPrefab;
    // Start is called before the first frame update
    void Start()
    {
        TESTINGStartRandomBook();
        BookTitle = this.bookModel.BookTitle;
        PageCount = this.bookModel.BookPageCount.ToString();
        BookScaleFactor = this.bookModel.BookScaleFactor.ToString();
    }

    void TESTINGStartRandomBook()
    {
        bookModel = new BookModel()
        {
            BookTitle = "Book Title",
            BookISBN = 12345678,
            BookPageCount = 500
        };
        bookModel.BookScaleFactor = SetBookSize(bookModel.BookPageCount);
    }
    
    public Vector3 SetBookSize(int pageCount) // returns the scale factor
    {
        //BoxCollider thisBoxCollider = this.GetComponent<BoxCollider>();
        Renderer thisRenderer = BookPrefab.GetComponentInChildren<Renderer>();

        Vector3 ogSize = thisRenderer.bounds.size;
        float ogSizeX = ogSize.x;
        
        // using the equation S=mP + b where S = spine width, m = growth rate (so the spine width per page),
        // P = page count, and b = fixed width from the page thickness
        // if we assume that the size of the bookPrefab has 300 pages and that is our base, along with altering numbers to fit the
        // unity world scale (1/1000) we get
        // S = 0.0006P + 0.02 so we're going to use that
        float newSizeX = ((0.0006f * pageCount) + 0.02f);
        
        Vector3 scaleFactor = new Vector3(
            newSizeX / ogSizeX,
            ogSize.y,
            ogSize.z);
        
        // apply the scaling factor to the book
        
        //this.gameObject.transform.localScale = scaleFactor;
        Debug.Log($"The scale factor is {scaleFactor}");
        return scaleFactor;
    }

    public void SetBookScaleFactor()
    {
        Debug.Log($"Setting the BookScaleFactor: {bookModel.BookScaleFactor}");
        BookPrefab.transform.localScale = bookModel.BookScaleFactor;
        Debug.Log($"Book local scale: {BookPrefab.transform.localScale}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
