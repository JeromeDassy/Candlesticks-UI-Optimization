using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDatesTimes : MonoBehaviour
{
    static DisplayDatesTimes instance;
    public static DisplayDatesTimes Instance { get { if (!instance) { instance = FindObjectOfType<DisplayDatesTimes>(); } return instance; } }

    [SerializeField] private Text oldestDate;
    [SerializeField] private Text newestDate;

    /// <summary>
    /// Display and update the oldest and newest date for the first and last visible candles on screen
    /// </summary>
    public void SetStartAndEndDate(List<Candlestick> listItems)
    {
        Candlestick[] sorted = listItems.OrderBy(v => v.PosVector2.x).ToArray();

        string oldest = sorted[0].OHLCData.Time;
        string newest = sorted[sorted.Length - 1].OHLCData.Time;

        if (oldest.Contains(','))
        {
            string[] oldestTimesDataArray = oldest.Trim().Split(","[0]);
            string[] newestTimesDataArray = newest.Trim().Split(","[0]);

            oldest = oldestTimesDataArray[0];
            newest = newestTimesDataArray[newestTimesDataArray.Length - 1];
        }

        oldestDate.text = oldest;
        newestDate.text = newest;
    }
}
