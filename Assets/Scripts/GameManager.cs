using System.Collections;
using System.Collections.Generic;
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
        
        highScoreText.text = "Best Score: "+ highScore.ToString();
    }

    public void Initialize(int _move, int _goal)
    {

    }


    // Update is called once per frame
    void Update()
    {
        pointText.text = "Points: "+ points.ToString();
        goalText.text = "Goal: "+ goal.ToString();
        moveText.text = "move: "+ move.ToString();
        scoreText.text = "Score: "+ DataManager.instance.score.ToString();
    }

    public void ProcessTurn(int _pointsToGain, bool _subtractMove)
    {
        points += _pointsToGain;
        if (_subtractMove)
            move--;
        if (points >= goal)
        {
            //you won the game
            isGameEnded = true;
            //Display victory screen
            backgroundPanel.SetActive(true);
            victoryPanel.SetActive(true);
            PotionBoard.Instace.potionParent.SetActive(false);
            DataManager.instance.score = points * move;
            SetScore();
            return;
        }

        if (move == 0)
        {
            //lose the game
            isGameEnded = true;
            backgroundPanel.SetActive(true);
            losePanel.SetActive(true);
            PotionBoard.Instace.potionParent.SetActive(false);
            return;
        }
    }

    // attached to a button to change scene when winning
    public void WinGame()
    {
        SaveScore();
    }

    public int SetScore()
    {
        if (DataManager.instance.score > highScore)
        {
            highScore = DataManager.instance.score;
        }
        return highScore;
    }

    // attached to a button to change scene when losing
    public void LoseGame()
    {
        SceneManager.LoadScene(0);
    }
    
    [System.Serializable]
    class SaveData
    {
        public int highScore;
    }

    public void SaveScore()
    {
        SaveData data = new SaveData();
        data.highScore= highScore;

        string json = JsonUtility.ToJson(data);
  
        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
        Debug.Log("file ke simpan");
    }

    public void LoadScore()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            highScore = data.highScore;
            Debug.Log("file ke keload");
        }
    }

}
