using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool isWhite;
    public bool isKing;


    public bool validMove(Piece[,]board, int x1, int y1, int x2, int y2)
    {
        //prevent piece to stack upon oneanother
        if (board[x2, y2] != null)
            return false;


        // how many tiles(2d array coordinates) did the player move ? (both in x,y columns)?

        int deltaMove = Mathf.Abs(x1 - x2); // how many tiles you jump in x
        int deltaMoveY = y2 - y1;


        if(isWhite || isKing)
        {
            if(deltaMove == 1) //normal jump in x
            {
                if (deltaMoveY == 1) //nomal jump in y
                    return true;
            }
            else if(deltaMove == 2) //jumping over another piece in x (i.e kill move)
            {
                if (deltaMoveY == 2)// jumping over another piece in y (i.e kill move)  
                {
                    Piece p=board[ (x1+x2)/2 , (y1 + y2)/2 ];
                    if (p != null && p.isWhite != isWhite)
                        return true;

                }
            }
        }
        //for black team
        if ( !isWhite || isKing)
        {
            if (deltaMove == 1) //normal jump in x
            {
                if (deltaMoveY == -1) //nomal jump in y
                    return true;
            }
            else if (deltaMove == 2) //jumping over another piece in x (i.e kill move)
            {
                if (deltaMoveY == -2)// jumping over another piece in y (i.e kill move)  
                {
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null && p.isWhite != isWhite)
                        return true;

                }
            }
        }

        return false;
    }

    public bool isForcedToMove(Piece[,] board,int x,int y)
    { 
        // for white piece or king
     if(isWhite || isKing)
        {
            //top left
            if (x >= 2 && y <= 5)
            {
                Piece p = board[x - 1, y + 1];
                // if there's a piece and it's not the same color as ours
                if (p != null && p.isWhite != isWhite)
                {
                    // check landing possibility after jump
                    if (board[x - 2, y + 2] == null)
                    {
                        return true;
                    }
                }
            }
            //top right
            if (x <= 5 && y <= 5)
            {
                Piece p = board[x + 1, y + 1];
                // if there's a piece and it's not the same color as ours
                if (p != null && p.isWhite != isWhite)
                {
                    // check landing possibility after jump
                    if (board[x + 2, y + 2] == null)
                    {
                        return true;
                    }
                }
            }
        }

        // for black piece or king
        if (!isWhite || isKing)
        {
            //Bottom left
            if (x >= 2 && y >= 2)
            { 
                Piece p = board[x - 1, y - 1];
                // if there's a piece and it's not the same color as ours
                if (p != null && p.isWhite != isWhite)
                {
                    // check landing possibility after jump
                    if (board[x - 2, y - 2] == null)
                    {
                        return true;
                    }
                }
            }
            //Bottom right
            if (x <= 5 && y >= 2)
            {
                Piece p = board[x + 1, y - 1];
                // if there's a piece and it's not the same color as ours
                if (p != null && p.isWhite != isWhite)
                {
                    // check landing possibility after jump
                    if (board[x + 2, y - 2] == null)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
