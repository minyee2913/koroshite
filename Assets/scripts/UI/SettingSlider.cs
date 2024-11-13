using UnityEngine;
using UnityEngine.UI;

public class SettingSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] Text text;
    public bool asPercentage;

    // Update is called once per frame
    void Update()
    {
        if (asPercentage) {
            text.text = Mathf.Floor((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * 100).ToString() + "%";
        } else {
            text.text = slider.value.ToString();
        }
    }
}
