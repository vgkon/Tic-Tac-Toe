using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    State[][] grid; //;

    State activePlayer = State.X;
    //int turn = 0;
    GameSession gS;
    // Start is called before the first frame update
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

    // Update is called once per frame
    private void Update()
    {
        //ProcessTurn();  //player goes first (x)
        ProcessInput();
    }

    private void ProcessTurn()
    {
        //activePlayer = (turn % 2 == 0) ? State.X : State.O;
    }

    private void ProcessInput()
    {
        if (!gS.GetPlaying()) return;
        RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
        foreach (RaycastHit hit in hits)
        {
            Slot target = hit.transform.GetComponent<Slot>();
            if (target == null) continue;

            if (Input.GetMouseButtonDown(0) && target.GetIsPlaceable())
            {
                Vector2Int lastMove = Place(target);
                activePlayer = gS.NewTurn(grid, lastMove, activePlayer);

                if (gS.GetPlaying())
                {
                    lastMove = GetComponent<AI>().AITurn(grid, gS.GetTurn(), lastMove);
                    activePlayer = gS.NewTurn(grid, lastMove, activePlayer);
                }
                
            }
        }
    }

    private Vector2Int Place(Slot target)
    {
        Vector2Int lastMove = new Vector2Int((int)target.transform.localPosition.x, (int)target.transform.localPosition.y);
        //print(target.transform.localPosition.x + " " + target.transform.localPosition.y);
        var childToSetActive = target.transform.Find(activePlayer.ToString());

        childToSetActive.gameObject.SetActive(true);
        target.Unplaceable();
        print("Placing on " + lastMove.x + "," + lastMove.y);
        grid[lastMove.x][lastMove.y] = activePlayer;
        for (int j = 2; j > -1; j--)
        {
            string row = " ";
            for (int i = 0; i < 3; i++)
            {
                row += " " + i + " " + j + ":" + grid[i][j].ToString();
            }
            //print(row);
        }

        return lastMove;
    }

    public void ActivateSlot(int x, int y)
    {
        string slotName = "Slot " + x.ToString() +","+ y.ToString();
        print(slotName);
        Slot slotToActivate = transform.Find(slotName).GetComponent<Slot>();
        Transform stateToActivate = slotToActivate.transform.Find("O");
        stateToActivate.gameObject.SetActive(true);
        slotToActivate.Unplaceable() ;
        //transform.Find("Slot " + playMove.x.ToString() + playMove.y.ToString()).Find(AIPlayer.ToString()).gameObject.SetActive(true);
    }

    private static Ray GetMouseRay()
    {
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }
}
