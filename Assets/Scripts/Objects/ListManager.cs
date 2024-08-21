using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Create, display manage and update the candlesticks in the chart
/// </summary>
public class ListManager : MonoBehaviour
{
    #region Variables

    static ListManager instance;
    public static ListManager Instance { get { if (!instance) { instance = FindObjectOfType<ListManager>(); } return instance; } }

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform Container;
    [SerializeField] private RectTransform Prefab;
    [SerializeField] private RectMask2D Mask;

    [SerializeField] private Text textCount;
    [SerializeField] private Slider globalSlider;
    [SerializeField] private Scrollbar hScrollbar;

    private RectTransform _maskRT;
    private int _num = -1;
    private int _numVisible;
    private int _numBuffer = 2;
    private int _resolution = 0;
    private int _numItems = 0;
    private int _maxCandlesCount = 0;
    private int _timeScale = 0;
    private float _spacing = 0;
    private float _containerHalfSize;
    private float _prefabSize;
    private float _lowest;
    private float _highest;

    private Vector3 _startPos;
    private Vector3 _offsetVec;
    private List<RectTransform> _listItemRect = new List<RectTransform>();
    private List<Candlestick> _listItems = new List<Candlestick>();
    private List<Candlestick> _candlestickPool = new List<Candlestick>();

    private DrawAverageLine DrawAverageLine => DrawAverageLine.Instance;
    private DisplayDatesTimes DisplayDatesTimes => DisplayDatesTimes.Instance;
    private CandlestickManager CandlestickManager => CandlestickManager.Instance;
    private ResolutionScaler ResolutionScaler => ResolutionScaler.Instance;
    private TimeScalingSwitcher TimeScalingSwitcher => TimeScalingSwitcher.Instance;
    private DisplayPrices DisplayPrices => DisplayPrices.Instance;
    // colors for the candlesticks
    private Color _colDOWN = new Color(0.9490196f, 0.4509804f, 0.4392157f, 1f);
    private Color _colUP = new Color(0.3843137f, 0.7098039f, 0.5607843f, 1f);

    private int GetTimeScale { get { return _timeScale; } }
    public float GetLowestValueOnScreenLate { get { return _lowest; } }
    public float GetMinYScaleValue { get { return (_maskRT.rect.height - 22) / (GetLowestAndHighestValueInScreen().max - _lowest); } }

    #endregion

    private void Start()
    {
        scrollRect.onValueChanged.AddListener((value) => ValueHasChanged(value.x));
    }

    #region Public Function that can be from another script or a unity event

    /// <summary>
    /// Get the Time Scale value => 0 = Default(Seconds) ; 1 = minutes ; 2 = hours ; 3 = days ; 4 = weeks ; 5 = months
    /// </summary>
    /// <param name="timeScale">from 0 to 5</param>
    public void SetTimeScale(int timeScale)
    {
        _timeScale = timeScale;
        globalSlider.minValue = -(OHLCDataHolder.GetMaxScale(timeScale));
        globalSlider.value = 0;
        GenerateNewListFromTimeScale();
    }

    public void GenerateNewListWithItemsCount()
    {
        int value = _num = -1;
        if(textCount.text != string.Empty || int.TryParse(textCount.text, out value))
            _num = int.Parse(textCount.text);
        _maxCandlesCount = ResolutionScaler.GetMaxVisibleCandles;

        ClearList();
        if (textCount.text != string.Empty || _num > 0)
            OHLCDataHolder.GenerateOHCLData(_num, _maxCandlesCount);
        else
        {
            OHLCDataHolder.LoadOHCLData(_maxCandlesCount);
            _num = OHLCDataHolder.GetOHLCDataListCount(resolution: 0, timeScale: GetTimeScale);
        }
        InitList();

        globalSlider.minValue = -(OHLCDataHolder.GetMaxScale(GetTimeScale));
        globalSlider.value = 0;
        TimeScalingSwitcher.SetTimeScaleLevelButtons();
    }

    public void GenerateNewListFromTimeScale()
    {
        _maxCandlesCount = ResolutionScaler.GetMaxVisibleCandles;

        ClearList();        
        _num = OHLCDataHolder.GetOHLCDataListCount(resolution: 0, timeScale: GetTimeScale);
        InitList();

        globalSlider.minValue = -(OHLCDataHolder.GetMaxScale(GetTimeScale));
        globalSlider.value = 0;
    }

    public void SetWidthAndSpacing(float value)
    {        
        if (value >= 0)
        {
            CandlestickManager.SetCandlestickWidth(value + 1);
            _spacing = value;
            UpdateList();
        }
        else
        {
            int intValue = Mathf.FloorToInt(value);
            int res = Mathf.Abs(intValue);

            float minSize = res == OHLCDataHolder.GetMaxScale(GetTimeScale) ? (float)_maxCandlesCount/(OHLCDataHolder.GetOHLCDataListCount(resolution: res, timeScale: GetTimeScale) + _numBuffer + 2) : 1;

            value = Extentions.ConvertRange(intValue, intValue + 1, minSize, 2, value);

            CandlestickManager.SetCandlestickWidth(value);
            _spacing = 0;

            UpdateList(res);
        }
    }

    #endregion

    #region Create Update or clear the chart
    private void ClearList()
    {
        _listItemRect.Clear();
        _listItems.Clear();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void InitList()
    {
        Container.anchoredPosition3D = new Vector3(0, 0, 0);

        _maskRT = Mask.GetComponent<RectTransform>();

        Vector2 prefabScale = new Vector2(CandlestickManager.bodyWidth, OHLCDataHolder.GetLowestAndHighestValue().max-22);
        _prefabSize = prefabScale.x + _spacing;

        Container.sizeDelta = (new Vector2(_prefabSize * _num, prefabScale.y));
        _containerHalfSize = Container.rect.size.x * 0.5f;

        _numVisible = Mathf.CeilToInt(_maskRT.rect.size.x / _prefabSize);

        _offsetVec = Vector3.right;
        _startPos = Container.anchoredPosition3D - (_offsetVec * _containerHalfSize) + (_offsetVec * (prefabScale.x * 0.5f));
        _numItems = Mathf.Min(_num, _numVisible + _numBuffer);

        for (int i = 0; i < _numItems; i++)
        {
            GameObject obj = Instantiate(Prefab.gameObject, Container.transform);
            obj.name = obj.name + "-" + i.ToString();
            Candlestick m = obj.GetComponentInChildren<Candlestick>();
            RectTransform rt = m.rtCandlestickGroup;
            rt.anchoredPosition3D = _startPos + (_offsetVec * i * _prefabSize);
            _listItemRect.Add(rt);
            obj.SetActive(true);

            _listItems.Add(m);

            OHLCData? ohlcData = GetDataInList(i);
            Color color = ohlcData.Value.isRising ? _colUP : _colDOWN;

            m.InitCandlestick(color, ohlcData.Value);
        }

        Container.anchoredPosition3D += _offsetVec * (_containerHalfSize - (_maskRT.rect.size.x * 0.5f));
        SetContainerScale(GetMinYScaleValue);
    }

    private void UpdateList(int resolution = 0)
    {
        _num = OHLCDataHolder.GetOHLCDataListCount(resolution: resolution, timeScale: GetTimeScale);

        _resolution = resolution;

        Container.anchoredPosition3D = new Vector3(0, 0, 0);

        Vector2 prefabScale = new Vector2(CandlestickManager.bodyWidth, OHLCDataHolder.GetLowestAndHighestValue().max - 22);
        _prefabSize = prefabScale.x + _spacing;

        Container.sizeDelta = new Vector2(_prefabSize * _num, prefabScale.y);
        _containerHalfSize = Container.rect.size.x * 0.5f;

        _numVisible = Mathf.CeilToInt(_maskRT.rect.size.x / _prefabSize);

        _offsetVec = Vector3.right;
        _startPos = Container.anchoredPosition3D - (_offsetVec * _containerHalfSize) + (_offsetVec * (prefabScale.x * 0.5f));
        _numItems = Mathf.Min(_num, _numVisible + _numBuffer);

        if (_listItems.Count < _numItems)//Scaling increase
        {
            UpdateVisibleCandlestick(resolution);

            for (int i = _listItems.Count; i < _numItems; i++)//Add new candlestick
            {
                if (_candlestickPool.Count == 0)
                    break;

                Candlestick m = _candlestickPool.Last();
                _candlestickPool.RemoveAt(_candlestickPool.Count-1);
                RectTransform rt = m.rtCandlestickGroup;
                rt.anchoredPosition3D = _startPos + (_offsetVec * i * _prefabSize);
                _listItemRect.Add(rt);

                _listItems.Add(m);

                OHLCData? ohlcData = GetDataInList(i, resolution);
                Color color = ohlcData.Value.isRising ? _colUP : _colDOWN;

                m.InitCandlestick(color, ohlcData.Value);
                m.gameObject.SetActive(true);
            }
        }
        else//Scaling decrease
        {
            UpdateVisibleCandlestick(resolution);

            for (int i = _listItems.Count - 1; i >= _numItems; i--)//Hide Unecessary Candlesticks
            {
                Candlestick m = _listItems[i];
                if (m != null)
                    hideItemByIndex(i);
            }
        }

        Container.anchoredPosition3D += _offsetVec * (_containerHalfSize - (_maskRT.rect.size.x * 0.5f));
        SetContainerScale(GetMinYScaleValue);
    }

    /// <summary>
    /// Update visible candlestick in order to optimize the framerates
    /// </summary>
    private void UpdateVisibleCandlestick(int resolution)
    {
        for (int i = 0; i < _listItems.Count; i++)
        {
            Candlestick m = _listItems[i];

            m.rtCandlestickGroup.anchoredPosition3D = _startPos + (_offsetVec * i * _prefabSize);

            OHLCData? ohlcData = GetDataInList(i, resolution);

            Color color = ohlcData.Value.isRising ? _colUP : _colDOWN;

            m.InitCandlestick(color, ohlcData.Value);
        }
    }

    /// <summary>
    /// Hide the current candlestick in order to re-use them later and optimize the framterate
    /// </summary>
    /// <param name="index">Index of candlestick to hide</param>
    private void hideItemByIndex(int index)
    {
        Candlestick m = _listItems[index];
        _candlestickPool.Add(m);
        _listItemRect.RemoveAt(index);
        _listItems.RemoveAt(index);
        m.gameObject.SetActive(false);
    }

    private void moveItemByIndex(RectTransform item, int index)
    {
        item.anchoredPosition3D = _startPos + (_offsetVec * index * _prefabSize);
    }

    private void ReorderItemsByPos(float normPos)
    {
        int numOutOfView = Mathf.CeilToInt(normPos * (_num - _numVisible));   //number of elements beyond the left boundary (or top)
        int firstIndex = Mathf.Max(0, numOutOfView - _numBuffer);   //index of first element beyond the left boundary (or top)
        int originalIndex = firstIndex % _numItems;

        int newIndex = firstIndex;
        for (int i = originalIndex; i < _numItems; i++)
        {
            moveItemByIndex(_listItemRect[i], newIndex);

            OHLCData? ohlcData = GetDataInList(newIndex, _resolution);
            Color color = ohlcData.Value.isRising ? _colUP : _colDOWN;

            _listItems[i].InitCandlestick(color, ohlcData.Value);
            newIndex++;
        }
        for (int i = 0; i < originalIndex; i++)
        {
            moveItemByIndex(_listItemRect[i], newIndex);

            OHLCData? ohlcData = GetDataInList(newIndex, _resolution);
            Color color = ohlcData.Value.isRising ? _colUP : _colDOWN;

            _listItems[i].InitCandlestick(color, ohlcData.Value);
            newIndex++;
        }

        Container.sizeDelta = new Vector2(Container.sizeDelta.x, 4000);

        SetContainerScale(GetMinYScaleValue);
        DrawAverageLine.AverageLine(_listItems);
        DisplayDatesTimes.SetStartAndEndDate(_listItems);
        DisplayPrices.UpdatePricePerVisibleCandlesOnScreen(_lowest, _highest);
    }

    #endregion

    #region Data list details

    private OHLCData GetDataInList(int i, int resolution = 0)
    {
        return OHLCDataHolder.GetOHLCDatas(i, resolution: resolution, timeScale: GetTimeScale);
    }

    private void SetContainerScale(float value)
    {
        Container.localScale = new Vector3(1, value, 1);
    }

    private (float min, float max) GetLowestAndHighestValueInScreen()
    {
        Candlestick[] sourceArray = _listItems.ToArray();

        float max = _listItems.Max(y => y.OHLCData.High);
        float min = _listItems.Min(y => y.OHLCData.Low);

        _lowest = min;
        _highest = max;
        return (min, max);
    }

    private void ValueHasChanged(float value, bool hasBeenUpdatedByScaling = false)
    {
        ReorderItemsByPos(value);     
    }

    #endregion
}
