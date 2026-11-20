using UnityEngine;
using UnityEngine.UI;
public class HealthBarUI : MonoBehaviour
{
    public Slider hpSlider;
    public PlayerController player;
    private void Update()
    {
        hpSlider.value = player.currentHp;
    }
}