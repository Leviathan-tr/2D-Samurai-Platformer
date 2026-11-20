using UnityEngine;
public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        PlayerController player =
            other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetCheckpoint(other.transform.position);
            Debug.Log("Checkpoint etkinleþti: " + gameObject.name);
        }
    }
}