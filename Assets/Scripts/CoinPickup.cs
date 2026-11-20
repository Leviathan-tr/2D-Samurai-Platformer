using UnityEngine;
public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int scoreValue = 1;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        ScoreManager scoreManager =
            FindAnyObjectByType<ScoreManager>();
        if(scoreManager != null )
        {
            scoreManager.AddScore(scoreValue);
        }
        Destroy(gameObject);
    }
        
    }
