using UnityEngine;
using UnityEngine.UI;

public abstract class CandlestickElement : MonoBehaviour
{
    private CandlestickManager _CandlestickManager;

    public OHLCData OHLCData { get; set; }

    //Since it's in the hierarchy of prefab, it's more optimized to link it directly in the editor
    public RectTransform rtCandlestickGroup;
    [SerializeField] protected Image _imageBody;
    [SerializeField] protected RectTransform _rtWick;
    [SerializeField] protected RectTransform _rtBody;

    public RectTransform GetRT_Wick() { return _rtWick; }
    public RectTransform GetRT_Body() { return _rtBody; }
    public Vector2 PosVector2 { get { return rtCandlestickGroup.anchoredPosition; } }

    public void InitCandlestick(Color color, OHLCData ohlcData)
    {
        OHLCData = ohlcData;
        _imageBody.color = color;
        SetPosition(ohlcData);
    }

    /// <summary>
    /// Display Tooltip with data on mouse enter candlestick
    /// </summary>
    public void onEnter()
    {
        if (_CandlestickManager != null)
            _CandlestickManager.Tooltip.Init(_rtBody.transform.position, OHLCData);//, _rtBody.transform);
    }

    /// <summary>
    /// Hide tool tip on mouse exit candlestick
    /// </summary>
    public void onExit()
    {
        if (_CandlestickManager != null)
            _CandlestickManager.Tooltip.Hide();
    }

    protected virtual void Awake()
    {
        _CandlestickManager = CandlestickManager.Instance;

        if (_CandlestickManager != null)
        {
            _CandlestickManager.AddElement(this);

            Init();

            RefreshWidth(_CandlestickManager);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_CandlestickManager != null)
        {
            _CandlestickManager.RemoveElement(this);
        }
    }

    public virtual void Init()
    {

    }

    private void SetPosition(OHLCData ohlcData)
    {
        float min = _CandlestickManager.GetMinValueOnScreen;

        rtCandlestickGroup.offsetMin = new Vector2(rtCandlestickGroup.offsetMin.x, 0);
        rtCandlestickGroup.offsetMax = new Vector2(rtCandlestickGroup.offsetMin.x, ohlcData.High - min);

        _rtWick.offsetMin = new Vector2(_rtWick.offsetMin.x, ohlcData.Low - min);
        _rtWick.offsetMax = new Vector2(_rtWick.offsetMax.x, ohlcData.High - min);

        _rtBody.offsetMin = new Vector2(_rtBody.offsetMin.x, ohlcData.Open - min);
        _rtBody.offsetMax = new Vector2(_rtBody.offsetMax.x, ohlcData.Close - min);

        if(_CandlestickManager != null)
            RefreshWidth(_CandlestickManager);
    }

    public abstract void RefreshWidth(CandlestickManager candlestickManager);
}
