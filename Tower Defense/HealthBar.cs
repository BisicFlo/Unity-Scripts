using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

    [SerializeField] private Slider slider;

    private void Start() {
        if (slider == null) slider = GetComponent<Slider>();
    }

    public void SetHealth(int health) {
        slider.value = health;
    }
    public void SetMaxHealth(int newMaxHealth) {
        slider.maxValue = newMaxHealth;
    }
}
