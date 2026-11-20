using UnityEngine;
using UnityEngine.SceneManagement;


public class CharacterSelect : MonoBehaviour
{
    public void SelectRanged()
    {
        Debug.Log("Ranged Seçildi!");
        PlayerPrefs.SetInt("CharacterType", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("level_01");
    }
    public void SelectMelee()
    {
        Debug.Log("Melee Seçildi!");
        PlayerPrefs.SetInt("CharacterType", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("level_01");
    }
}