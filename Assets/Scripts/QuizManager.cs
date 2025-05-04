using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    public int nextScene;
    public int previousScene;
    public GameObject victoryPanel;
    public GameObject losePanel;
    public GameObject[] panelQuiz;
    public void RightAswer()
    {
        victoryPanel.SetActive(true);
    }

    void Start()
    {
        PanelSpawn();
    }

    public void WrongAswer()
    {
        losePanel.SetActive(true);
    }

    public void BackToLevel()
    {
        SceneManager.LoadScene(previousScene);
    }

    public void NextLevel()
    {
        int previousPuzzleIndex = SceneManager.GetActiveScene().buildIndex - 1; // Asumsikan quiz selalu setelah puzzle
        DataManager.instance.UnlockNextLevel(previousPuzzleIndex);
        SceneManager.LoadScene(nextScene); // scene peta
    }

    public void PanelSpawn()
    {
        int panelToSpawn = Random.Range(0, panelQuiz.Length);
        panelQuiz[panelToSpawn].SetActive(true);
    }
}
