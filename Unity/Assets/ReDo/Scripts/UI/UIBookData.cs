using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBookData : MonoBehaviour
{
    [SerializeField] public Button book_button;
    [SerializeField] public TMP_Text title_textbox;
    [SerializeField] public TMP_Text author_textbox;
    [SerializeField] public RectTransform panel;
    [SerializeField] public GameObject horizontalTitleAndImage_go;
    [SerializeField] public GameObject book3dPrefab;
    [SerializeField] public string bookId;
    public string bookTitle;
    public string pageCount;
}
