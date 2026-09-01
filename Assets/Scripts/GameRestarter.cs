using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ξαναφορτώνει τη σκηνή από την αρχή.
/// </summary>
public class GameRestarter : MonoBehaviour
{
    public void RestartGame()
    {
        // Σε περίπτωση που κάτι είχε παγώσει τον χρόνο
        Time.timeScale = 1f;

        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}
