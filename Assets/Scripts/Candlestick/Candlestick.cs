using UnityEngine;

/// <summary>
/// A Candlestick prefab that contain the body and wick images, as well as all the prices value
/// </summary>
public class Candlestick : CandlestickElement
{
    public override void Init()
    {
        base.Init();
    }

    public override void RefreshWidth(CandlestickManager candlestickManager)
    {
        BodyWidth = candlestickManager.bodyWidth;
        WickWidth = candlestickManager.wickWidth;
    }

    protected float BodyWidth { get => rtCandlestickGroup.sizeDelta.x; set => rtCandlestickGroup.sizeDelta = new Vector2(value, rtCandlestickGroup.sizeDelta.y); }
    protected float WickWidth { get => _rtWick.sizeDelta.x; set => _rtWick.sizeDelta = new Vector2(value, _rtWick.sizeDelta.y); }
}
