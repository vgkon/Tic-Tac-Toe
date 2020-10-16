using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public class AI : MonoBehaviour
{
    State AIPlayer = State.O;
    State Player = State.X;
    GameSession gS;

    public struct MoveEvaluation {                                          //move evaluation instances is where move data will be stored during minimax algorithm's stages
        public int moveEval;
        public int x;
        public int y;

        public MoveEvaluation(int mE, int X, int Y)
        {
            moveEval = mE;
            x = X;
            y = Y;
        }
    }

    private void Start()
    {
        gS = FindObjectOfType<GameSession>();                   //let AI know where to look for the GameSession
    }

    public MoveEvaluation Minimax(State[][] grid, bool maxPlayer, int turn, int alpha, int beta, Vector2Int lastMove, State activePlayer)       
    {

        //in the minimax algorithm, the AI(minimizing player) recursively calling the minimax function, searches all possible game positions and
        //tries to minimize player's (maximizing player)score. For every different "stage" it is called, it switches between minimizing and maximizing player
        //making decisions, choosing the optimal move evaluation depending on whose turn it is.



        State nextPlayer = maxPlayer ? State.O : State.X;                                   //next player is the same as previous player
        State winner = CheckForWinner(grid, lastMove, nextPlayer);                          
        MoveEvaluation moveEvaluation;                                                      
        if (turn == 9 && winner == State.blank)                                             //if we have reached the 9th (last turn) without a winner, calculate last move and check again
        {
            State lastMomentWinner = FillLastSlot(grid);
            if (lastMomentWinner == Player)                                                 //if player wins this position return move evaluation (1+turns left)
            {
                moveEvaluation = new MoveEvaluation(1, lastMove.x, lastMove.y);
            }
            else
            {
                moveEvaluation = new MoveEvaluation(turn - 10, lastMove.x, lastMove.y);     // else if AI won return points return move evaluation (-1-turns left)
            }
            return moveEvaluation;
        }
        if (winner == Player)
        {
            moveEvaluation = new MoveEvaluation(11 - turn, lastMove.x, lastMove.y);
            return moveEvaluation;
        }
        if(winner == AIPlayer)
        {
            moveEvaluation = new MoveEvaluation(turn - 11, lastMove.x, lastMove.y);
            return moveEvaluation;
        }
        else
        {                                                                                       //if no one has won this position keep calculating
            List<State[][]> children = new List<State[][]>();                                   //this is where the child positions will be kept
            List<Vector2Int> childrenMoves = new List<Vector2Int>();                            //this is where the moves leading to corresponding child positions will be kept
            FindChildren(grid, activePlayer, children, childrenMoves);

            if (maxPlayer)
            {
                MoveEvaluation maxEval = new MoveEvaluation(int.MinValue, -1, -1);              //set to minimum so that if all moves are bad, they will at least beat the minimum evaluation

                foreach (var c in children.Zip(childrenMoves, Tuple.Create))                    //i need both child position and move so... tuples
                {
                    MoveEvaluation mE = Minimax(c.Item1, !maxPlayer, turn + 1, alpha, beta, c.Item2, nextPlayer);         //recursively search ""tree of positions"
                    if (mE.moveEval > maxEval.moveEval)                                         //if we have found a better move, update the move evaluation 
                    {
                        maxEval.moveEval = mE.moveEval;
                        maxEval.x = c.Item2.x;
                        maxEval.y = c.Item2.y;
                        alpha = Mathf.Max(alpha, maxEval.moveEval);                             //used for pruning. basically means that AI can know if the move they have evaluated up to
                        if (alpha >= beta) return maxEval;                                      //now, is the best it can be, avoid reading any other child states of this branch
                    }
                }
                return maxEval;                                                                 //move evaluation goes all the way up from the bottom node to top node
            }
            else
            {                                                                                   //the exact opposite for AI's turns
                MoveEvaluation minEval = new MoveEvaluation(int.MaxValue, -1, -1);
                foreach (var c in children.Zip(childrenMoves, Tuple.Create))
                {
                    MoveEvaluation mE = Minimax(c.Item1, !maxPlayer, turn + 1, alpha, beta, c.Item2, nextPlayer);
                    if (mE.moveEval < minEval.moveEval)
                    {
                        minEval.moveEval = mE.moveEval;
                        minEval.x = c.Item2.x;
                        minEval.y = c.Item2.y;
                        beta = Mathf.Max(beta, minEval.moveEval);
                        if (alpha >= beta) return minEval;
                    }
                }
                return minEval;
            }
        }
    }

    private State FillLastSlot(State[][] grid)                                                          //self explanatory for last turn
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (grid[i][j] == State.blank)
                {
                    grid[i][j] = State.X;
                    State winner = CheckForWinner(grid, new Vector2Int(i, j), State.X);
                    return winner;
                }
            }
        }
        return State.blank;
    }

    private void FindChildren(State[][] grid, State activePlayer, List<State[][]> children, List<Vector2Int>childrenMoves)      //calculates child positions based on empty cells
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (grid[i][j] == State.blank) { 
                    State[][] childGrid = new State[3][];
                    ChildGrid(grid, childGrid);
                    childGrid[i][j] = activePlayer;
                    children.Add(childGrid);
                    childrenMoves.Add(new Vector2Int(i, j));
                }
            }
        }
    }

    private static void ChildGrid(State[][] grid, State[][] tempGrid)                                       //creates child position for FindChildren
    {
        for (int i = 0; i < 3; i++)
        {
            tempGrid[i] = new State[3];
            grid[i].CopyTo(tempGrid[i], 0);
        }
    }

    public Vector2Int AITurn(State[][] grid, int turn, Vector2Int lastMove)                                 //here from GridManager, after player moves
    {
        MoveEvaluation move;
        if (!gS.GetPlaying()) return lastMove;                                                              //get playing status from GameSession
        else
        {
            move = Minimax(grid, false, turn + 1, int.MinValue, int.MaxValue, lastMove, AIPlayer);          //minimax start
            grid[move.x][move.y] = AIPlayer;                                                                //update grid
            GetComponent<GridManager>().transform.Find("Slot " + move.x + "," + move.y).transform.Find("O").gameObject.SetActive(true);     //actually show move in game
        }
        return new Vector2Int(move.x, move.y);
    }

    public State CheckForWinner(State[][] currentGrid, Vector2Int lastMove, State lastPlayer)
    {
        //check col
        for (int i = 0; i < 3; i++)
        {
            if (currentGrid[lastMove.x][i] != lastPlayer)                                                   //if anything in column doesn't match the last player, game's still on
                break;
            if (i == 2)
            {
                return lastPlayer;
            }

        }

        //check row
        for (int i = 0; i < 3; i++)
        {
            if (currentGrid[i][lastMove.y] != lastPlayer)                                                   //same for row, diag, anti-diag
                break;
            if (i == 2)
            {
                return lastPlayer;
            }

        }

        //check diag
        if(lastMove.x == lastMove.y)
        {
            for(int i = 0; i < 3; i++)
            {
                if (currentGrid[i][i] != lastPlayer)
                    break;
                if (i == 2)
                {
                    return lastPlayer;
                }
            }
        }

        //check anti-diag
        if (lastMove.x + lastMove.y == 2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (currentGrid[i][2-i] != lastPlayer)
                    break;
                if (i == 2)
                {
                    return lastPlayer;
                }
            }
        }

        return State.blank;
    }

    private void PrintGrid(State[][] grid)                                                              //for debugging purposes
    {
        for (int j = 2; j > -1; j--)
        {
            string row = " ";
            for (int i = 0; i < 3; i++)
            {
                row += "\t" + grid[i][j].ToString();
            }
            print(row);
        }
        print("\n");
    }

}
