using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    State p1Type;
    State p2Type;
    int turn = 0;
    bool playing = true;
    AI ai;
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject escMenu;
    GridManager gridManager;
    State winner;

    private void Start()
    {
        ai = FindObjectOfType<AI>();
        gridManager = FindObjectOfType<GridManager>();
    }

    private void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!escMenu.activeSelf) escMenu.SetActive(true);
            else escMenu.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public int GetTurn()
    {
        return turn;
    }

    public State NewTurn(State[][] grid, Vector2Int lastMove, State activePlayer)
    {
        print(grid[0][0] + " " + " " + lastMove.x + " " + lastMove.y + " " + activePlayer);
        winner = ai.CheckForWinner(grid, lastMove, activePlayer);
        turn++;
        if (turn == 9 || winner != State.blank) GameOver();
        return (turn % 2 == 0) ? State.X : State.O;
    }

    public bool GetPlaying()
    {
        return playing;
    }

    public void GameOver()
    {
        playing = false;
        if (winner == State.blank) canvas.transform.Find("Tie").gameObject.SetActive(true);
        else if(winner == State.X) canvas.transform.Find("YouWon").gameObject.SetActive(true);
        else if (winner == State.O) canvas.transform.Find("YouLost").gameObject.SetActive(true);
    }

    public void SetPlayer1Type(State pT)
    {
        p1Type = pT;
    }

    public void SetPlayer2Type(State pT)
    {
        p2Type = pT;
    }

    public State GetPlayer1Type()
    {
        return p1Type;
    }

    public State GetPlayer2Type()
    {
        return p2Type;
    }


}
