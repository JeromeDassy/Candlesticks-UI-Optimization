using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    static Tooltip instance;
    public static Tooltip Instance { get { if (!instance) { instance = FindObjectOfType<Tooltip>(); } return instance; } }

    [SerializeField] private GameObject TooltipGO;
    [SerializeField] private RectTransform RT_TooltipGO;
    [SerializeField] private RectTransform RT_LOW;
    [SerializeField] private RectTransform RT_OPEN;
    [SerializeField] private RectTransform RT_CLOSE;
    [SerializeField] private RectTransform RT_HIGH;

    [SerializeField] private Text T_LOW;
    [SerializeField] private Text T_OPEN;
    [SerializeField] private Text T_CLOSE;
    [SerializeField] private Text T_HIGH;
    
    public void Init(Vector2 pos, OHLCData ohlcData)
    {
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(pos);
        screenPos = new Vector3(screenPos.x + 1920, screenPos.y + 1080, screenPos.z);
        RT_TooltipGO.anchoredPosition = screenPos;

        T_LOW.text = ohlcData.Low.ToString();
        T_OPEN.text = ohlcData.Open.ToString();
        T_CLOSE.text = ohlcData.Close.ToString();
        T_HIGH.text = ohlcData.High.ToString();

        //if (RightHalf())
        //    RT_TooltipGO.anchoredPosition = new Vector2(24, RT_TooltipGO.anchoredPosition.y);
        //else
        //    RT_TooltipGO.anchoredPosition = new Vector2(78, RT_TooltipGO.anchoredPosition.y);

        TooltipGO.SetActive(true);
    }

    public void Hide()
    {
        TooltipGO.SetActive(false);
    }

    bool RightHalf()
    {
        return Input.mousePosition.x > Screen.width / 2.0f;
    }

    bool BottomHalf()
    {
        return Input.mousePosition.y > Screen.height / 2.0f;
    }
}
