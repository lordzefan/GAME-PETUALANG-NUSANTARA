using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance;
    public int selectedLevelIndex = 1;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        instance = this;
    }

    public void LoadSelectedLevel()
    {
        // Kamu bisa atur manual mapping levelIndex ke nama scene
        string sceneName = "Level" + selectedLevelIndex;
        SceneManager.LoadScene(sceneName);
    }
}
