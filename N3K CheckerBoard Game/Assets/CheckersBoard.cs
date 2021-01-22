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

    public bool isWhite;
    private bool isWhiteTurn;
    private bool hasKilled;

    private List<Piece> forcedPieces;




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
        if (p != null && p.isWhite == isWhite)
        {
            if (forcedPieces.Count == 0)
            {
                selectedPiece = p;
                startDrag = mouseOver;
                Debug.Log(selectedPiece.name);
            }
            else
            {
                if (forcedPieces.Find(fp => fp == p) == null)
                    return;

                selectedPiece = p;
                startDrag = mouseOver;
            }
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
        forcedPieces = new List<Piece>();
        GenerateBoard();
    }// Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        UpdateMouseOver();
        //Debug.Log(mouseOver);

        if((isWhite)? isWhiteTurn : !isWhiteTurn)
        { 
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

        forcedPieces = ScanForPossibleMove();

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
                if (Mathf.Abs(x2 - x1) == 2)
                {
                    Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null)
                    {
                        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null; //delete that piece from array
                        Destroy(p.gameObject); //delete that piece from scene .
                        hasKilled = true;
                    }
                }
                //block all mooves if we have a kill move possible 
                // were we supposed to kill anyone and we didn't then cancel all other moves except kill move
                if(forcedPieces.Count!=0 && !hasKilled)
                {
                    // cancel the move
                    MovePiece(selectedPiece, x1, y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    return;
                }

                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                MovePiece(selectedPiece, x2, y2);

                //endturn
                EndTurn();
                
                //----------------------rotating camera from white player to black player--------
                //Camera.main.transform.position+= new Vector3(0,0,11);
                //Camera.main.transform.RotateAround(Camera.main.transform.position,Vector3.up,180);

            }
            //if the move is invalid snap the piece back to it's place.
            else 
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }
        }



    }
    private void EndTurn()
    {
        //keeping track of where we landed during the turn

        int x = (int)endDrag.x;
        int y = (int)endDrag.y;

        // making king condition
        if (selectedPiece != null)
        {
            if(selectedPiece.isWhite && !selectedPiece.isKing && y == 7)
            {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
            else if(!selectedPiece.isWhite && !selectedPiece.isKing && y == 0)
            {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
        }


        selectedPiece = null; // we are done moving the piece that we selected
        startDrag = Vector2.zero;
        if (ScanForPossibleMove(x, y).Count != 0 && hasKilled)
            return;



        isWhiteTurn = !isWhiteTurn; 
        isWhite = !isWhite;
        hasKilled = false;
        
        CheckVictory();

        ScanForPossibleMove();
    }

    private void CheckVictory()
    {
        Piece[] ps = FindObjectsOfType<Piece>();
        bool hasWhite = false; bool hasBlack = false;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].isWhite)
                hasWhite = true;
            else
                hasBlack = true;

        }

        if (!hasWhite)
            Victory(false);
        if (!hasBlack)
            Victory(true);
    }

    private void Victory(bool isWhite)
    {
        if (isWhite)
            Debug.Log("White plyaer has won");
       else
            Debug.Log("Black Player has won");
    }


    // a function that returns list of all pieces that are forced to move
    
    private List<Piece> ScanForPossibleMove()
    {
        forcedPieces = new List<Piece>();
        // go thru all piecs
        for(int i=0; i < 8; i++)
        {
            for(int j=0; j < 8; j++)
            {
                if (pieces[i, j] != null && pieces[i, j].isWhite == isWhiteTurn)
                    if (pieces[i, j].isForcedToMove(pieces, i, j))
                        forcedPieces.Add(pieces[i, j]);
            }
        }

        return forcedPieces;
    }

    private List<Piece> ScanForPossibleMove(int x,int y)
    {
        forcedPieces = new List<Piece>();
        if (pieces[x, y].isForcedToMove(pieces, x, y))
            forcedPieces.Add(pieces[x, y]);

        return forcedPieces;
    }




}




















