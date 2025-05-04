using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject backgroundPanel;
    public GameObject victoryPanel;
    public GameObject losePanel;

    public int goal;
    public int move;
    public int points;
    public int highScore;
    public bool isGameEnded;

    public int nextScene;     // Index scene peta/berikutnya
    public int previousScene; // Index scene untuk mengulang
    public int quizScene;

    public TMP_Text pointText;
    public TMP_Text goalText;
    public TMP_Text moveText;
    public TMP_Text scoreText;
    public TMP_Text highScoreText;

    private void Awake()
    {
        instance = this;
        LoadScore();
    }

    void Start()
    {
        highScoreText.text = "Best Score: " + highScore.ToString();
    }

    void Update()
    {
        pointText.text = "Points: " + points.ToString();
        goalText.text = "Goal: " + goal.ToString();
        moveText.text = "Move: " + move.ToString();
        scoreText.text = "Score: " + DataManager.instance.score.ToString();
    }

    public void ProcessTurn(int _pointsToGain, bool _subtractMove)
    {
        points += _pointsToGain;
        if (_subtractMove) move--;

        if (points >= goal)
        {
            isGameEnded = true;
            backgroundPanel.SetActive(true);
            victoryPanel.SetActive(true);
            PotionBoard.Instace.potionParent.SetActive(false);

            DataManager.instance.score = points * move;
            SetScore();

            return;
        }

        if (move <= 0)
        {
            isGameEnded = true;
            backgroundPanel.SetActive(true);
            losePanel.SetActive(true);
            PotionBoard.Instace.potionParent.SetActive(false);
        }
    }

    public void WinGame()
    {
        SceneManager.LoadScene(quizScene);
    }

    public void BackToLobby()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void LoseGame()
    {
        SceneManager.LoadScene(previousScene);
    }

    public int SetScore()
    {
        if (DataManager.instance.score > highScore)
        {
            highScore = DataManager.instance.score;
        }
        return highScore;
    }

    // public void GoToTrivia()
    // {
    //     SceneManager.LoadScene(9);
    // }

    [System.Serializable]
    class SaveData
    {
        public int highScore;
    }

    public void SaveScore()
    {
        SaveData data = new SaveData();
        data.highScore = highScore;

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
        Debug.Log("Score saved");
    }

    public void LoadScore()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            highScore = data.highScore;
            Debug.Log("Score loaded");
        }
    }
}
