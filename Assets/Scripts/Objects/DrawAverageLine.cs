using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DrawAverageLine : MonoBehaviour
{
    static DrawAverageLine instance;
    public static DrawAverageLine Instance { get { if (!instance) { instance = FindObjectOfType<DrawAverageLine>(); } return instance; } }

    [SerializeField] private CanvasScaler scaler;
    [SerializeField] private LineRenderer lineRenderer;

    private int _averagePrecision = 1;
    private List<Candlestick> _listItems = new List<Candlestick>();

    public void SetSMAPrecision (float value)
    {
        _averagePrecision = (int)value;
        AverageLine(_listItems, _averagePrecision);
    }

    public void AverageLine(List<Candlestick> listItems, int averageCount = -1)
    {
        averageCount = averageCount == -1 ? _averagePrecision : averageCount;

        _listItems = listItems;
        Candlestick[] sorted = listItems.OrderBy(v => v.PosVector2.x).ToArray();
        List<Vector3> pos3DList = new List<Vector3>();
        for (int i = 0; i < sorted.Length; i += averageCount)
        {
            List<Vector2> nextXVector = new List<Vector2>();
            Vector2 candlePos = i + averageCount < sorted.Length ? sorted[i].transform.position : sorted[sorted.Length - 1].transform.position;
            for (int y = 1; y < averageCount - 1; y++)
            {
                if (averageCount - 1 < i + y)
                    break;
                nextXVector.Add(sorted[i + y].transform.position);
            }
            float candlesAverageY = nextXVector.Count == 0 ? candlePos.y : nextXVector.Average(y => y.y);

            Vector3 pos = new Vector3(candlePos.x + (scaler.referenceResolution.x / 2), candlesAverageY + scaler.referenceResolution.y, 0);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(pos);
            pos3DList.Add(worldPos);
        }
        bool firstPosHigherThanLast = pos3DList[0].y > pos3DList[pos3DList.Count - 1].y;
        lineRenderer.startColor = lineRenderer.endColor = firstPosHigherThanLast ? Color.red : Color.green;
        lineRenderer.startWidth = lineRenderer.endWidth = 5f;
        lineRenderer.positionCount = pos3DList.Count;

        float currentMax = pos3DList.Max(v => v.y);
        float currentMin = pos3DList.Min(v => v.y);

        for (int i = 0; i < pos3DList.Count; i++)
        {
            Vector3 currentPos = pos3DList[i];
            float correctedYPos = Extentions.ConvertRange(currentMin, currentMax, 80, 2020, currentPos.y);
            Vector3 newPos = new Vector3(currentPos.x, correctedYPos, currentPos.z);

            lineRenderer.SetPosition(i, newPos);
        }
    }
}
