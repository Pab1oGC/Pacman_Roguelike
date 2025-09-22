using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ToastView : MonoBehaviour
{
    [Header("Refs")]
    public CanvasGroup canvasGroup;
    public Image background;
    public Text label;

    public void SetText(string txt) { if (label) label.text = txt; }
    public void SetColor(Color c) { if (background) background.color = c; }
}
