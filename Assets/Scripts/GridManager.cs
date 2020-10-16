using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    State[][] grid; 
    State activePlayer = State.X;
    GameSession gS;

    void Start()
    {
        gS = FindObjectOfType<GameSession>();
        grid = new State[3][];
        for(int i = 0; i < 3; i++)
        {
            grid[i] = new State[3];
            for(int j = 0; j < 3; j++)
            {
                grid[i][j] = State.blank;
            }
        }
        
    }

    private void Update()
    {
        ProcessInput();
    }


    private void ProcessInput()
    {
        if (!gS.GetPlaying()) return;
        RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());                              //RaycastAll in case there are objects in front (initial plan was to have rotating camera,
        foreach (RaycastHit hit in hits)                                                    //but turns out it was irrelevant to the game
        {
            Slot target = hit.transform.GetComponent<Slot>();                               //if we have hit a slot (or cell) then activate X or O child object (depending on player)
            if (target == null) continue;                                                   //for the purpose of this demo, there is only one human player for X and one AI player

            if (Input.GetMouseButtonDown(0) && target.GetIsPlaceable())
            {
                Vector2Int lastMove = Place(target);
                activePlayer = gS.NewTurn(grid, lastMove, activePlayer);                    //after playing, advance turn (also checks for winner)

                if (gS.GetPlaying())                                                        //and if still playing, have the AI play
                {
                    lastMove = GetComponent<AI>().AITurn(grid, gS.GetTurn(), lastMove);
                    activePlayer = gS.NewTurn(grid, lastMove, activePlayer);                //and then advance turn
                }
                
            }
        }
    }

    private Vector2Int Place(Slot target)   
    {
        Vector2Int lastMove = new Vector2Int((int)target.transform.localPosition.x, (int)target.transform.localPosition.y);     //enables slot child by name
        var childToSetActive = target.transform.Find(activePlayer.ToString());

        childToSetActive.gameObject.SetActive(true);
        target.Unplaceable();                                                                   //then renders slot unusable so that nobody can play on the same slot twice
        grid[lastMove.x][lastMove.y] = activePlayer;
        

        return lastMove;
    }


    private static Ray GetMouseRay()                        //used for raycasting
    {
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }
}
