using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBookData : MonoBehaviour
{
    [SerializeField] public Sprite coverImg_Sprite;
    [SerializeField] public TMP_Text title_textbox;
    [SerializeField] public TMP_Text author_textbox;
    [SerializeField] public RectTransform panel;
    [SerializeField] public GameObject horizontalTitleAndImage_go;
    public string bookTitle;
    public string pageCount;
    //[SerializeField] public GameObject imageAndTitleOnly_go;
    // Start is called before the first frame update
    void Start()
    {
        //title_textbox = title_go.GetComponent<TextMeshPro>();
        //author_textbox = author_go.GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetImageAndTitleOnly()
    {

    }
}
