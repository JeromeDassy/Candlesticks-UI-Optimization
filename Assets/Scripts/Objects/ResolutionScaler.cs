using UnityEngine;
using UnityEngine.UI;

public class ResolutionScaler : MonoBehaviour
{
    static ResolutionScaler instance;
    public static ResolutionScaler Instance { get { if (!instance) { instance = FindObjectOfType<ResolutionScaler>(); } return instance; } }

    [SerializeField] private CanvasScaler scaler;
    private int _maxVisibleCandles;

    public int GetMaxVisibleCandles { get { return _maxVisibleCandles; } }

    void Awake()
    {
        GetMaxCandlesVisible();
        Screen.SetResolution((int)scaler.referenceResolution.x, (int)scaler.referenceResolution.y, true);
    }

    private void GetMaxCandlesVisible()//These are the only resolution supported, more than 4k may not run smoothly, less than hd, may look not sharp enough
    {
        if (scaler.referenceResolution.x == 2560)//QHD
            _maxVisibleCandles = 797;
        else if (scaler.referenceResolution.x == 3840)//4k
            _maxVisibleCandles = 1224;
        else//1080p
            _maxVisibleCandles = 584;
    }
}
