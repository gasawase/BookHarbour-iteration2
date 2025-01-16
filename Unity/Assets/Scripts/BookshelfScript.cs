using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookshelfScript : MonoBehaviour
{
    [SerializeField] public int shelfCount;
    [SerializeField] public int intShelfHeight;
    [SerializeField] public int intShelfWidth;

    public int GetShelfCount()
    {
        return shelfCount;
    }

    public void SetShelfCount(int outsideShelfCount)
    {
        shelfCount = outsideShelfCount;
    }

    public int GetShelfHeight()
    {
        return intShelfHeight;
    }

    public void SetShelfHeight(int outsideShelfHeight)
    {
        intShelfHeight = outsideShelfHeight;
    }

    public int GetShelfWidth()
    {
        return intShelfWidth;
    }

    public void SetShelfWidth(int outsideShelfWidth)
    {
        intShelfWidth = outsideShelfWidth;
    }
}
