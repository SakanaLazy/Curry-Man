using UnityEngine;
using UnityEngine.UI;

public class SuperMeterUI : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;

    public void SetFill(float value)
    {
        if (fillImage)
            fillImage.fillAmount = Mathf.Clamp01(value);
    }
}
