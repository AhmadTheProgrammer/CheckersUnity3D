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

    private Vector2 mouseOver; // to keep track of mouse position all time
    private Piece selectedPiece;// for storing the peice that is currently selected
    private Vector2 startDrag;
    private Vector2 endDrag;

    private bool isWhite;
    private bool isWhiteTurn;





    private void GenerateBoard()
    {
        // Generate White Team
        for (int y = 0; y < 3; y++)
        {
            //check if the row is odd
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                GeneratePiece((oddRow) ? x : x + 1, y);
            }

        }

        // Generate Black Team
        for (int y = 7; y >= 5; y--)
        {
            //check if the row is odd
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x <= 7; x += 2)
            {
                GeneratePiece((oddRow) ? x : x + 1, y);
            }

        }
    }//method for generating 2D array.
    private void GeneratePiece(int x, int y)
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
    }//setting the pieces from 2d array to screen
    private void UpdateMouseOver()
    {
        //checking availablity of the camera
        if (!Camera.main)
            Debug.Log("Camera not found");

        // for raycasting you need to have box collider on the object i.e board in our casee
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
            out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }


    }//getting mouse position everyframe using raycasting

    private void SelectPiece(int x, int y)
    {
        //out of bounds check
        if (x < 0 || x >= pieces.Length || y < 0 || y >= pieces.Length)
            return;

        Piece p = pieces[x, y];
        if (p != null)
        {
            selectedPiece = p;
            startDrag = mouseOver;

        }


    }// for selecting the piece in the array
    private void UpdatePieceDrag(Piece p)
    {
        //checking availablity of the camera
        if (!Camera.main)
            Debug.Log("Camera not found");

        // for raycasting you need to have box collider on the object i.e board in our casee
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
            out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }
    } // to highlight selected piece by elevating it above


    void Start()
    {
        isWhiteTurn = true;
        GenerateBoard();
    }// Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        UpdateMouseOver();
        //Debug.Log(mouseOver);

        { //if it's my turn
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;

            if (selectedPiece != null)
                UpdatePieceDrag(selectedPiece);

            if (Input.GetMouseButtonDown(0))
                SelectPiece(x, y);

            if (Input.GetMouseButtonUp(0))
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);

        }

    }

    private void TryMove(int x1, int y1, int x2, int y2)
    {
        // for multiplayer purposes ????
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        // out of bound check
        if (x2 < 0 || x2 >= pieces.Length || y2 < 0 || y2 >= pieces.Length)
        {
            //snap the piece back to its place if player choses wrong position to drop the piece
            //in other words we cancel the move if player tries wrong [x2,y2] in 2d array
            if (selectedPiece != null)
                MovePiece(selectedPiece, x1, y1); // this will snap the piece back to its original place

            startDrag = Vector2.zero;
            selectedPiece = null;
            return;

        }

        if (selectedPiece != null)
        {
            //if piece is selected but not moved yet then again cancel it & put it back to it's place
            if (startDrag == endDrag)
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }

            //if we have selected the piece and it's not null then 
            //check if it's a valid move & (look at method validMove() in Piece class)
            //we just defined rules for a valid move in validMove() method. now here we try to follow them.
            if (selectedPiece.validMove(pieces, x1, y1, x2, y2))
            {
                //did we kill anything? //did we jump>?
                if (Mathf.Abs(x1 - x2) == 2)
                {
                    Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null)
                    {
                        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null; //delete that piece from array
                        Destroy(p.gameObject); //delete that piece from scene .
                    }
                }

                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                MovePiece(selectedPiece, x2, y2);

                //endturn
                EndTurn();
            }
        }



    }
    private void EndTurn()
    {
        selectedPiece = null; // we are done moving the piece that we selected
        startDrag = Vector2.zero;


        isWhiteTurn = !isWhiteTurn; //a line which is to be removed in the future.
        CheckVictory();


    }

    private void CheckVictory()
    {

    }


}




















