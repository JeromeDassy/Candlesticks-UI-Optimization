using System.Collections.Generic;
using UnityEngine;

public class DisplayPrices : MonoBehaviour
{
    static DisplayPrices instance;
    public static DisplayPrices Instance { get { if (!instance) { instance = FindObjectOfType<DisplayPrices>(); } return instance; } }

    [SerializeField] private RectTransform Container;
    [SerializeField] private RectTransform Prefab;

    private GameObject _lowestPrice;
    private GameObject _middlePrice;
    private GameObject _highestPrice;

    private List<PriceLine> _priceLineList = new List<PriceLine>();

    private float PrefabHalfHeight { get { return Prefab.rect.height/2; } }

    public void UpdatePricePerVisibleCandlesOnScreen(float max, float min)//To optimize if enough time
    {
        //int gap = (int)(max - min);

        //for (int i = 0; i < gap; i++)
        //{

        //}

        float middleLine = (min + max) / 2;

        if(_lowestPrice == null)
        {
            _lowestPrice = Instantiate(Prefab.gameObject, Container.transform);
            _lowestPrice.name = "LowestPriceVisible";
            _priceLineList.Add(_lowestPrice.GetComponent<PriceLine>());
            _priceLineList[0].rt.anchoredPosition = new Vector2(0, PrefabHalfHeight);//low
        }

        if (_middlePrice == null)
        {
            _middlePrice = Instantiate(Prefab.gameObject, Container.transform);
            _middlePrice.name = "LowestPriceVisible";
            _priceLineList.Add(_middlePrice.GetComponent<PriceLine>());
            _priceLineList[1].rt.anchoredPosition = new Vector2(0, Container.rect.height/2);//low
        }

        if (_highestPrice == null)
        {
            _highestPrice = Instantiate(Prefab.gameObject, Container.transform);
            _highestPrice.name = "HighestPriceVisible";
            _priceLineList.Add(_highestPrice.GetComponent<PriceLine>());
            _priceLineList[_priceLineList.Count - 1].rt.anchoredPosition = new Vector2(0, Container.rect.height - PrefabHalfHeight);//high
        }

        _priceLineList[0].SetText(max);
        _priceLineList[1].SetText(middleLine);
        _priceLineList[_priceLineList.Count - 1].SetText(min);
        
    }
}
