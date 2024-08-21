using UnityEngine;
using UnityEngine.UI;

public class PriceLine : MonoBehaviour
{
    public RectTransform rt;
    [SerializeField] private Text priceText;

    public void SetText(float value)
    {
        priceText.text = value.ToString();
    }
}
