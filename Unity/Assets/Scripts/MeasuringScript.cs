using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MeasuringScript : MonoBehaviour
{
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;
    [SerializeField] private TMP_Text textBox;
    // Start is called before the first frame update

    public void CalculateManualWidth()
    {
        if (leftEdge != null && rightEdge != null)
        {
            float interiorWidth = Vector3.Distance(leftEdge.position, rightEdge.position);
            Debug.Log("Manual Interior Width: " + interiorWidth);
            textBox.text = interiorWidth.ToString();
        }
        else
        {
            Debug.LogError("Left or Right Edge Transform is missing!");
        }
    }
}
