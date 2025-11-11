using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBookData : MonoBehaviour
{
    [SerializeField] public RawImage coverImg_rawImg;
    [SerializeField] public TMP_Text title_textbox;
    [SerializeField] public TMP_Text author_textbox;
    [SerializeField] public RectTransform panel;
    [SerializeField] public GameObject horizontalTitleAndImage_go;
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
