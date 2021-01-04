using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckersBoard : MonoBehaviour
{
    // make a 2d array of pieces(black and white players)

    public Piece[,] pieces = new Piece[8, 8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    private Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);
    private void GenerateBoard()
    {
        // Generate White Team
        for(int y = 0; y < 3; y++)
        {
            //check if the row is odd
            bool oddRow = ( y%2 == 0 );
            for(int x=0; x < 8 ; x += 2 )
            {
                GeneratePiece((oddRow) ? x : x+1 , y);
            }
                   
        }

        // Generate Black Team
        for (int y = 7; y >=5 ; y--)
        {
            //check if the row is odd
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x <= 7; x += 2)
            {
                GeneratePiece((oddRow) ? x : x + 1, y);
            }

        }
    }

    private void GeneratePiece(int x,int y)
    {
        // make piece using prefabs but check whether piece we are instantiating is white /black?
        bool isPieceWhite = (y > 3) ? false : true;
        GameObject go = Instantiate((isPieceWhite) ? whitePiecePrefab : blackPiecePrefab) as GameObject;
        go.transform.SetParent(this.transform);

        //now put the instantiated piece in the 2d array
        Piece p = go.GetComponent<Piece>();
        pieces[x, y] = p;
        //setting the pieces in world space as we have now filled the 2darray
        MovePiece(p, x, y);
    }

    private void MovePiece(Piece p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }




    // Start is called before the first frame update
    void Start()
    {
        GenerateBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
