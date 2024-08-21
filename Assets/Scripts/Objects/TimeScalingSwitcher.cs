using UnityEngine;
using UnityEngine.UI;

public class TimeScalingSwitcher : MonoBehaviour
{
    static TimeScalingSwitcher instance;
    public static TimeScalingSwitcher Instance { get { if (!instance) { instance = FindObjectOfType<TimeScalingSwitcher>(); } return instance; } }

    [SerializeField] private Button[] timeScaleButtons;

    private ListManager ListManager => ListManager.Instance;
    private ResolutionScaler ResolutionScaler => ResolutionScaler.Instance;

    public void SetTimeScaleLevelButtons()
    {
        for (int i = 0; i < timeScaleButtons.Length-1; i++)
        {
            int count = OHLCDataHolder.GetOHLCDataListCount(timeScale: i);
            int threshold = ResolutionScaler.GetMaxVisibleCandles / 4;

            timeScaleButtons[i].interactable = (count > threshold);
        }
        SetButtonActiveAndSelected(0);
    }

    /// <summary>
    /// Get the Time Scale value => 0 = Default(Seconds) ; 1 = minutes ; 2 = hours ; 3 = days ; 4 = weeks ; 5 = months
    /// </summary>
    /// <param name="timeScale">from 0 to 5</param>
    public void SetTimeScale(int timeScale)
    {
        ListManager.SetTimeScale(timeScale);
        SetButtonActiveAndSelected(timeScale);
    }

    private void SetButtonActiveAndSelected(int index)
    {
        timeScaleButtons[index].image.color = Color.cyan;

        for (int i = 0; i < timeScaleButtons.Length - 1; i++)
        {
            if (i == index)
                continue;
            timeScaleButtons[i].image.color = Color.white;
        }
    }
}
