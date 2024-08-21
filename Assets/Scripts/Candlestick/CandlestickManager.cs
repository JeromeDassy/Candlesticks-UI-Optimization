using System.Collections.Generic;
using UnityEngine;

public class CandlestickManager : MonoBehaviour
{
    static CandlestickManager instance;
    public static CandlestickManager Instance { get { if (!instance) { instance = FindObjectOfType<CandlestickManager>(); } return instance; } }

    [Range(1, 16)]
    public float bodyWidth= 12;
    [Range(1, 4)]
    public float wickWidth = 3;

    private int _divider = 1;
    public int Divider { get { return _divider; }}

    public Tooltip Tooltip => Tooltip.Instance;
    public float GetMinValueOnScreen { get { return ListManager.GetLowestValueOnScreenLate; } }
    private ListManager ListManager => ListManager.Instance;

    HashSet<CandlestickElement> _elements = new HashSet<CandlestickElement>();

    public void SetCandlestickWidth(float width) 
    {
        wickWidth = 1;
        bodyWidth = width * 3;
        UpdateCandlestickWidth();
    }

    private void UpdateCandlestickWidth()
    {
        foreach (var element in _elements)
            element.RefreshWidth(this);
    }

    public void AddElement(CandlestickElement candlestickElement)
    {
        _elements.Add(candlestickElement);
    }

    public void RemoveElement(CandlestickElement candlestickElement)
    {
        _elements.Remove(candlestickElement);
    }
}
