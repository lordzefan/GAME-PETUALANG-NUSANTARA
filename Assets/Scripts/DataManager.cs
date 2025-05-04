using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    public int score;
    public int unlockedLevel = 1; // hanya level 1 yang terbuka di awal

    [System.Serializable]
    class SaveData
    {
        public int score;
        public int unlockedLevel;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        // LoadProgress();
    }

    public void SaveProgress()
    {
        SaveData data = new SaveData();
        data.score = score;
        data.unlockedLevel = unlockedLevel;

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/progress.json", json);
        Debug.Log("Progress saved");
    }

    public void LoadProgress()
    {
        string path = Application.persistentDataPath + "/progress.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            score = data.score;
            unlockedLevel = data.unlockedLevel;
            Debug.Log("Progress loaded");
        }
        else
        {
            Debug.Log("No progress file found, starting fresh.");
        }
    }

    // Panggil ini saat player menyelesaikan level dan kembali ke peta
    public void UnlockNextLevel(int levelJustCompleted)
    {
        if (unlockedLevel <= levelJustCompleted)
        {
            unlockedLevel = levelJustCompleted + 1;
            SaveProgress();
        }
    }
}
