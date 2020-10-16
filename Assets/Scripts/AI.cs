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
    bool playing = true;
    GameSession gS;

    public struct MoveEvaluation {
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
        gS = FindObjectOfType<GameSession>();
    }

    public MoveEvaluation Minimax(State[][] grid, bool maxPlayer, int turn, int alpha, int beta, Vector2Int lastMove, State activePlayer)
    {
        State nextPlayer = maxPlayer ? State.O : State.X;
        /*print("Turn : " + turn + ". Current Player = " + activePlayer);
        print("Grid: ");
        PrintGrid(grid);*/
        //print("Starting turn " + turn + " as " + activePlayer + ", maxPlayer = " + maxPlayer);
        State winner = CheckForWinner(grid, lastMove, nextPlayer);
        MoveEvaluation moveEvaluation;
        if (turn == 9 && winner == State.blank)        //next player is also previous player
        {
            State lastMomentWinner = FillLastSlot(grid);
            //print("Filled Grid: ");
            //PrintGrid(grid);
            if (lastMomentWinner == Player)
            {
                moveEvaluation = new MoveEvaluation(1, lastMove.x, lastMove.y);
              //  print("Evaluation for this position is :" + moveEvaluation.moveEval);
            }
            else
            {
                moveEvaluation = new MoveEvaluation(turn - 10, lastMove.x, lastMove.y);
              //  print("Evaluation for this position is :" + moveEvaluation.moveEval);
            }
            return moveEvaluation;
        }
        if (winner == Player)
        {
            moveEvaluation = new MoveEvaluation(11 - turn, lastMove.x, lastMove.y);
            //print("Player wins this potition with evaluation :" + moveEvaluation.moveEval);
            return moveEvaluation;
        }
        if(winner == AIPlayer)
        {
            moveEvaluation = new MoveEvaluation(turn - 11, lastMove.x, lastMove.y);
            //print("AI wins this position with evaluation :" + moveEvaluation.moveEval);
            return moveEvaluation;
        }
        else
        {
            List<State[][]> children = new List<State[][]>();
            List<Vector2Int> childrenMoves = new List<Vector2Int>();
            FindChildren(grid, activePlayer, children, childrenMoves);

            if (maxPlayer)
            {
                MoveEvaluation maxEval = new MoveEvaluation(int.MinValue, -1, -1);

                foreach (var c in children.Zip(childrenMoves, Tuple.Create))
                {
                //    print("Turn :" + turn +" ChildGrid: ");
                //    PrintGrid(c.Item1);
                    MoveEvaluation mE = Minimax(c.Item1, !maxPlayer, turn + 1, alpha, beta, c.Item2, nextPlayer);         
                    if (mE.moveEval > maxEval.moveEval)
                    {
                        /*print("Turn " + turn + " Player choosing to play move : " + mE.x + "," + mE.y + " because it has moveEval :" + mE.moveEval);
                        print("Previous maxEval was : " + maxEval.moveEval + ". Playing in grid :");
                        PrintGrid(grid);*/
                        maxEval.moveEval = mE.moveEval;
                        maxEval.x = c.Item2.x;
                        maxEval.y = c.Item2.y;
                        alpha = Mathf.Max(alpha, maxEval.moveEval);
                        if (alpha >= beta) return maxEval;
                    }
                }
                //print("Turn " + turn + " Player returning evaluation for move" + maxEval.x +"," + maxEval.y + " is :" + maxEval.moveEval);
                return maxEval;
            }
            else
            {
                MoveEvaluation minEval = new MoveEvaluation(int.MaxValue, -1, -1);
                foreach (var c in children.Zip(childrenMoves, Tuple.Create))
                {
                    //print("ChildGrid: ");
                    //PrintGrid(c.Item1);
                    MoveEvaluation mE = Minimax(c.Item1, !maxPlayer, turn + 1, alpha, beta, c.Item2, nextPlayer);
                    if (mE.moveEval < minEval.moveEval)
                    {
                        //print("Turn " + turn + " AI choosing to play move : " + mE.x + "," + mE.y + " because it has moveEval :" + mE.moveEval);
                        //print("Previous minEval was : " + minEval.moveEval + ". Playing in grid :");
                       // PrintGrid(grid);
                        minEval.moveEval = mE.moveEval;
                        minEval.x = c.Item2.x;
                        minEval.y = c.Item2.y;
                        beta = Mathf.Max(beta, minEval.moveEval);
                        if (alpha >= beta) return minEval;
                    }
                }
                //print("Turn " + turn + " AI returning evaluation for this position is :" + minEval.moveEval);
                return minEval;
            }
        }
    }

    private State FillLastSlot(State[][] grid)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (grid[i][j] == State.blank)
                {
                    //print("grid[" + i + "][" + j + "] = " + grid[i][j]);
                    grid[i][j] = State.X;
                    State winner = CheckForWinner(grid, new Vector2Int(i, j), State.X);
                    return winner;
                }
            }
        }
        return State.blank;
    }

    private void FindChildren(State[][] grid, State activePlayer, List<State[][]> children, List<Vector2Int>childrenMoves)
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

    private static void ChildGrid(State[][] grid, State[][] tempGrid)
    {
        for (int i = 0; i < 3; i++)
        {
            tempGrid[i] = new State[3];
            grid[i].CopyTo(tempGrid[i], 0);
        }
    }

    public Vector2Int AITurn(State[][] grid, int turn, Vector2Int lastMove)
    {
        MoveEvaluation move;
        if (!gS.GetPlaying()) return lastMove;
        else
        {
            move = Minimax(grid, false, turn + 1, int.MinValue, int.MaxValue, lastMove, AIPlayer);
            //print("AI has decided to play : " + move.x + " " + move.y);
            grid[move.x][move.y] = AIPlayer;
            GetComponent<GridManager>().transform.Find("Slot " + move.x + "," + move.y).transform.Find("O").gameObject.SetActive(true);
        }
        return new Vector2Int(move.x, move.y);
    }

    public State CheckForWinner(State[][] currentGrid, Vector2Int lastMove, State lastPlayer)
    {
        //check col
        for (int i = 0; i < 3; i++)
        {
            if (currentGrid[lastMove.x][i] != lastPlayer)
                break;
            if (i == 2)
            {
                //print("Game Over");
                //printGrid(currentGrid);
                return lastPlayer;
            }

        }

        //check row
        for (int i = 0; i < 3; i++)
        {
            if (currentGrid[i][lastMove.y] != lastPlayer)
                break;
            if (i == 2)
            {
                //print("Game Over");
                //printGrid(currentGrid);
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
                    //print("Game Over");
                    //printGrid(currentGrid);
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
                    //print("Game Over");
                    //printGrid(currentGrid);
                    return lastPlayer;
                }
            }
        }

        return State.blank;
    }

    private void PrintGrid(State[][] grid)
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







        /*
    public MoveEvaluation Minimax(State[][] grid, int depth, bool maxPlayer, Vector2Int lastMove)
    {
        playing = true;
        timesCalled++;
        //print(maxPlayer);
        State[][] tempgrid = new State[3][];
        for(int i = 0; i < 3; i++)
        {
            tempgrid[i] = new State[3];
        }


        State lastPlayer = maxPlayer ? Player : AIPlayer;
        State winner;

        
        winner = CheckForWinner(grid, lastMove, lastPlayer);
        if (winner != State.blank)
        {
            //printGrid(grid);
            if (winner == Player)
            {
                print("Player wins at depth : " + depth);
                printGrid(grid);
                return new MoveEvaluation(1, lastMove.x, lastMove.y);
            }
            else if(winner == AIPlayer)
            {
                print("AI wins at depth : " + depth);
                printGrid(grid);
                return new MoveEvaluation(-1, lastMove.x, lastMove.y);
            }
        }
        if (depth == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {

                    if (grid[i][j] == State.blank)
                    {
                        grid[i][j] = State.X;
                        //print("reached 0 depth");
                        //printGrid(grid);
                        return new MoveEvaluation(-1, lastMove.x, lastMove.y);       //if 0 depth, player X plays (in this case, human player) and win or lose, ok outcome for AI if actual player does their best
                    }
                }
            }
        }
        //print("Maximizing player :" + maxPlayer + " at depth : " + depth);
        if (maxPlayer)
        {
            return MaximizingPlayer(grid, depth, maxPlayer, tempgrid);
        }

        else
        {
            return MinimizingPlayer(grid, depth, maxPlayer, tempgrid);
        }

    }

    private MoveEvaluation MinimizingPlayer(State[][] grid, int depth, bool maxPlayer, State[][] tempgrid)
    {
        MoveEvaluation minEval = new MoveEvaluation(1, -1, -1);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                grid[i].CopyTo(tempgrid[i], 0);
                if (tempgrid[i][j] == State.blank)
                {
                    if (depth == 1)
                    {
                        printGrid(tempgrid);
                        print("Turns to");
                    }
                    tempgrid[i][j] = AIPlayer;
                    if(depth == 1)
                    {
                        printGrid(tempgrid);
                        print("_____________________________________________");
                    }
                    MoveEvaluation mE = Minimax(tempgrid, depth - 1, !maxPlayer, new Vector2Int(i, j));
                    //print("Move evaluation has returned from depth " + (depth - 1) + " : " + mE.moveEval);
                    if (minEval.moveEval > mE.moveEval)
                    {
                        //print("And it is better than AI's already selected move :" + minEval.moveEval + " , " + minEval.x + " , " + minEval.y);
                        minEval.moveEval = mE.moveEval;
                        minEval.x = i;
                        minEval.y = j;
                        //print("New move :" + minEval.moveEval + " , " + minEval.x + " , " + minEval.y);
                        return minEval;
                    }
                    //else print("But AI's already selected move move was better :" + minEval.moveEval + " , " + minEval.x + " , " + minEval.y);
                }
            }
        }
        return minEval;
    }

    private MoveEvaluation MaximizingPlayer(State[][] grid, int depth, bool maxPlayer, State[][] tempgrid)
    {
        MoveEvaluation maxEval = new MoveEvaluation(-1, -1, -1);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                grid[i].CopyTo(tempgrid[i], 0);
                if (tempgrid[i][j] == State.blank)
                {
                    if (depth == 1)
                    {
                        printGrid(tempgrid);
                        print("Turns to");
                    }
                    tempgrid[i][j] = Player;
                    if (depth == 1)
                    {
                        printGrid(tempgrid);
                        print("_____________________________________________");
                    }
                    //printGrid(tempgrid);
                    //print("_____________________________________________");
                    MoveEvaluation mE = Minimax(tempgrid, depth - 1, !maxPlayer, new Vector2Int(i, j));
                    //print("Move evaluation has returned from depth " + (depth - 1) + " : " + mE.moveEval + " , " + maxEval.x + " , " + maxEval.y);
                    if (maxEval.moveEval < mE.moveEval)
                    {
                        //print("And it is better than Player's already selected move :" + maxEval.moveEval + " , " + maxEval.x + " , " + maxEval.y);
                        maxEval.moveEval = mE.moveEval;
                        maxEval.x = i;
                        maxEval.y = j;
                        //print("New move :" + maxEval.moveEval + " , " + maxEval.x + " , " + maxEval.y);
                        return maxEval;
                    }
                    //else print("But Player's already selected move move was better :" + maxEval.moveEval + " , " + maxEval.x + " , " + maxEval.y);

                }
            }
        }
        //print("\n \n");
        return maxEval;
    }

    public void AITurn(State[][] grid, int turn, Vector2Int lastMove)
    {
        print("AI turn has begun, depth is " + (9 - turn));
        if (!playing) return;
        timesCalled = 0;
        //printGrid(grid);
        MoveEvaluation playMove = Minimax(grid, (9 - turn), false, lastMove);
        if (playMove.x == lastMove.x && playMove.y == lastMove.y)
        {
            print("No more moves to play");
            return;
        }
        //print("AI wants to play " + playMove.x + "," + playMove.y + " that has State: " + grid[playMove.x][playMove.y]);
        grid[playMove.x][playMove.y] = AIPlayer;
        //printGrid(grid);
        GetComponent<GridManager>().ActivateSlot(playMove.x, playMove.y);
        if (CheckForWinner(grid, new Vector2Int(playMove.x, playMove.y), AIPlayer) != State.blank)
        {
            playing = false;
            return;
        }
        //transform.Find("Slot " + playMove.x.ToString()+ playMove.y.ToString()).Find(AIPlayer.ToString()).gameObject.SetActive(true);
    }
    */
}
