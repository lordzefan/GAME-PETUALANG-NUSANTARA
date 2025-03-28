using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject backgroundPanel;
    public GameObject victoryPanel;
    public GameObject losePanel;
    public int goal;
    public int move;
    public int points;
    public bool isGameEnded;
    public TMP_Text pointText;
    public TMP_Text goalText;
    public TMP_Text moveText;

    private void Awake()
    {
        instance = this;
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
        SceneManager.LoadScene(0);
    }

    // attached to a button to change scene when losing
    public void LoseGame()
    {
        SceneManager.LoadScene(0);
    }
    
}
