using UnityEngine;

public class XAxisScaling : MonoBehaviour
{
    private ListManager ListManager => ListManager.Instance;

    public void SetXScaling(float value)
    {
        ListManager.SetWidthAndSpacing(value);
    }
}
