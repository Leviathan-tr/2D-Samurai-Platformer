using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class ScoreManager : MonoBehaviour
{

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text coinText;
    private int score = 0;
    private int coins = 0;
    private void Start()
    {
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
    }
    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Skor:" + score;
        }
        if (coinText != null)
        {
            coinText.text = "Coin" + coins;
        }
    }
}