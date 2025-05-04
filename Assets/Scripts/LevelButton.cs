using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public int levelIndex; // Level keberapa tombol ini
    public string sceneName;
    

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();

        // Hanya aktif jika level telah terbuka
        if (levelIndex <= DataManager.instance.unlockedLevel)
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }

        button.onClick.AddListener(ShowLevelPopup);
    }

    void ShowLevelPopup()
    {
        LevelLoader.instance.selectedLevelIndex = levelIndex; // Simpan level yang dipilih
    }

    public void ScenePuzzel()
    {
        SceneManager.LoadScene(sceneName);
    }
}
