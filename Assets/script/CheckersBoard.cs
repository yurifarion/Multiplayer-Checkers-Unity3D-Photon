using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class CheckersBoard : MonoBehaviour
{
	public Piece[,] pieces = new Piece[8,8];
    public GameObject whitePiecePrefab;
	public GameObject blackPiecePrefab;
	
	
	
	public bool isWhite;
	private bool isWhiteTurn = true;
	private bool hasKilled;
	private bool isGameFinished = false;
	
	
	private Piece selectedPiece;
	private List<Piece> forcedPieces = new List<Piece>();
	private Vector2 mouseOver;
	private Vector2 startDrag;
	private Vector2 endDrag;
	
	//User interface
	public Text ui_log;
	
	//this variable will if the piece is being dragged by the mouse
	private bool dragging = false;
	
	public void Start(){
		isWhiteTurn = true;
		StartCoroutine(GenerateBoard());
	}
	void Update(){
		//Check if the game is won or lost
		if(CheckVictory() == "none")isGameFinished = false;
		else if(!isGameFinished){
			isGameFinished = true;
			Debug.Log("The team "+CheckVictory()+" won");
		}
		//if game is still running
		if(!isGameFinished){
			//Update whos time it is
			
			ui_log.text = "Turn:"+((isWhiteTurn)?"White":"Black");
			UpdateMouseOver();
			if((isWhite)?isWhiteTurn:!isWhiteTurn)
			{
				DraggingPiece();
				int x = (int)mouseOver.x;
				int y = (int)mouseOver.y;
				
				
				//Mouse Button Down
				if(Input.GetMouseButtonDown(0)){
					dragging = true;
					SelectPiece(x,y);
				}
				
				//Mouse Button Up
				if(Input.GetMouseButtonUp(0)){
					dragging = false;
					TryMove((int)startDrag.x,(int)startDrag.y,x,y);
				}
			}
		}
		else{
			ui_log.text = "Game finished "+CheckVictory()+" won";
		}
		// draw a matrix of the map 
		if(Input.GetKeyDown(KeyCode.P)) DrawMatrix();
		
	}
	private void DraggingPiece(){
		
		//There is some selected piece and the mouse is being pressed
		if(selectedPiece != null && dragging){
			RaycastHit hit;
			if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit,25.0f,LayerMask.GetMask("Board"))){
		
				selectedPiece.gameObject.transform.position = hit.point + Vector3.up;
			}
		}
	}
	private void UpdateMouseOver(){
		
		//safety checks
		if(!Camera.main){
			Debug.Log("Unable to find main camera");
			return;
		}
		//Check if mouse is over the board and then update the mouse coordinates
		RaycastHit hit;
		if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit,25.0f,LayerMask.GetMask("Board"))){
				
				float pieceOffset = 0.5f;
				mouseOver.x = hit.point.x + 0.5f;
				mouseOver.x = (int)mouseOver.x;
				
				mouseOver.y = hit.point.z + 0.5f;
				mouseOver.y = (int)mouseOver.y;
				
				
		}
		else{
			mouseOver.x =-1;
			mouseOver.y =-1;
			
		}
	}
	private void TryMove(int x1,int y1,int x2, int y2){
		
		forcedPieces = ScanForPossibleMove();
		//Multiplayer support
		startDrag = new Vector2(x1,y1);
		endDrag = new Vector2(x2,y2);
		selectedPiece = pieces[x1,y1];
		//check if we are out of bounds
		if(x2<0 || x2 >= 8 || y2 <0 || y2 >= 8){
			if(selectedPiece != null)
			{
				MovePiece(selectedPiece,x1,y1);
				startDrag = Vector2.zero;
				selectedPiece = null;
				return;
			}
		}
		if(selectedPiece != null){
			//it has not move
			if(endDrag == startDrag){
				MovePiece(selectedPiece,x1,y1);
				startDrag = Vector2.zero;
				selectedPiece = null;
				return;
			}
			//check if its a valid move
			if(selectedPiece.ValidMove(pieces,x1,y1,x2,y2)){
				//Did we kill anything
				if(Mathf.Abs(x2-x1) == 2){
					Piece p = pieces[(x1+x2)/2,(y1+y2)/2];
					if(p != null)
					{
						
						GetComponent<PhotonView>().RPC("KillPiece", RpcTarget.AllBuffered,(x1+x2)/2,(y1+y2)/2);
						hasKilled = true;
					}
					
				}
				//were we supposed to kill anything
				if(forcedPieces.Count != 0 && !hasKilled ){
					MovePiece(selectedPiece,x1,y1);
					startDrag = Vector2.zero;
					selectedPiece = null;
					return;
				}
				
				//It will update the board in every client
				GetComponent<PhotonView>().RPC("UpdateBoard", RpcTarget.AllBuffered,x1,y1,x2,y2);
				MovePiece(selectedPiece,x2,y2);
				
				EndTurn();
			}
			else{
				MovePiece(selectedPiece,x1,y1);
				startDrag = Vector2.zero;
				selectedPiece = null;
				return;
			}
		}
		//if there a selected pieces
	}
	void DrawMatrix(){
		Debug.Log("--------------");
		for(int i = 0; i < 8; ++i){
			string line = "";
			for(int j = 0 ; j < 8; j++){
				
				if(pieces[i,j] == null) line+="-";
				else{
					if(pieces[i,j].isWhite) line+="O";
					else if(!pieces[i,j].isWhite) line+="X";
				}
			}
			Debug.Log(line);
			
		}
		Debug.Log("--------------");
	}
	void EndTurn(){
		int x = (int)endDrag.x;
		int y = (int)endDrag.y;
		
		//promoting to king state
		if(selectedPiece != null)
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
		selectedPiece = null;
		startDrag = Vector2.zero;
		
		if(ScanForPossibleMove(selectedPiece,x,y).Count != 0 && hasKilled) return;
		
		
		//make sure to send that information only in the owned View
			Debug.Log("End Turn Mine");
			GetComponent<PhotonView>().RPC("changeTurn", RpcTarget.AllBuffered,!isWhiteTurn);
		
		hasKilled = false;
		//CheckVictory();
	}
	//update killed pieces based in the board coordinates
	[PunRPC]
	void KillPiece(int x, int y){
		Destroy(pieces[x,y].gameObject);
		pieces[x,y] = null;
	}
	//This function will change the Turn for all the clients
	[PunRPC]
	void changeTurn(bool turn){
		Debug.Log("Changing turns to is White:"+isWhiteTurn+"To"+turn);
		isWhiteTurn = turn;
	}
	[PunRPC]
	void UpdateBoard(int x1, int y1,int x2, int y2){
		pieces[x2,y2] = pieces[x1,y1];
		pieces[x1,y1] = null;
	}
	//Returns White, Black or none, to show who won or lose and also set Game finished if someone won
	private string CheckVictory(){
		
		bool hasWhite = true, hasBlack = true;
		//check if it exist black piece
		if(GameObject.FindGameObjectsWithTag("BlackPiece").Length <= 0) hasBlack = false;
		//check if it exist white piece
		if(GameObject.FindGameObjectsWithTag("WhitePiece").Length <= 0) hasWhite = false;
		
	
			if(!hasBlack && hasWhite){
				isGameFinished = true;
				return "White";
			}
			else if(hasBlack && !hasWhite){
				isGameFinished = true;
				return "Black";
			}
			else return "none";
			
		
	}
	private void Victory(bool isWhite)
	{
		
		if(isWhite) Debug.Log("White team has won");
		else Debug.Log("Black team has won");
		
	}
	private List<Piece> ScanForPossibleMove (Piece p, int x, int y)
	{
		forcedPieces = new List<Piece>();
		
		if(pieces[x,y].IsForceToMove(pieces,x,y))
			forcedPieces.Add(pieces[x,y]);
		
		
		return forcedPieces;
		
	}
	private List<Piece> ScanForPossibleMove(){
		forcedPieces = new List<Piece>();
		
		//check all pieces
		for(int i = 0; i < 8; ++i){
			for(int j = 0 ; j < 8; j++){
				if(pieces[i,j] != null && pieces[i,j].isWhite == isWhiteTurn)
				{
					if(pieces[i,j].IsForceToMove(pieces,i,j)){
						forcedPieces.Add(pieces[i,j]);
					}
				}
			}
		}
		return forcedPieces;
	}
	private void SelectPiece(int x, int y){
		//Out of Bounds
		if(x < 0 || x >= pieces.Length || y < 0 || y >= pieces.Length ) return;
		
		Piece p = pieces[x,y];
		if( p != null && p.isWhite == isWhite)
		{
			if(forcedPieces.Count == 0)
			{
				selectedPiece = p;
				startDrag = mouseOver;
				Debug.Log(selectedPiece.name);
			}
			else{
				//look for the piece under our forced pieces list
				if(forcedPieces.Find(fp => fp ==p)==null) return;
				
				selectedPiece = p;
				startDrag = mouseOver;
			}
		}
	}
	// it will create the board
	IEnumerator GenerateBoard(){
		yield return new WaitForSeconds(0.2f);//A little delay to receive informations from the other end
		//create white team if there is not white piece
		if (GameObject.FindGameObjectsWithTag("WhitePiece").Length == 0)
		{
			
			//this is the white team 
			isWhite = true;
			for (int y = 0; y < 3; y++)
			{
				bool oddRow = (y % 2 == 0);
				for (int x = 0; x < 8; x += 2)
				{
					//instantiate piece
					GeneratePiece((oddRow) ? x : x + 1, y);
				}
			}
		}
		else
		{
			AddEnemyPiecesTolist(GameObject.FindGameObjectsWithTag("WhitePiece"));
			//this is black team
			isWhite = false;
			//Change camera angle to the blac team
			GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
			if(cam != null){
				Vector3 newAngle = new Vector3(90,0,180);
				cam.transform.eulerAngles  = newAngle;
			}
			
			//Set stop waiting if we are the visitor
			GetComponent<MultiPlayerManager>().StopWaiting();
			//create black team if there is already a white piece
			for (int y = 7; y > 4; y--)
			{
				bool oddRow = (y % 2 == 0);
				for (int x = 0; x < 8; x += 2)
				{
					//instantiate piece
					GeneratePiece((oddRow) ? x : x + 1, y);
				}
			}
			
		}
	}
	//It will receive the list of GameObject that contains the tag of the other team
	//and then it will add them to its own board
	public void AddEnemyPiecesTolist(GameObject[] ps){
		
		foreach(GameObject o in ps){
			
			if(o.GetComponent<Piece>() != null){
				Piece p = o.GetComponent<Piece>();
				pieces[(int)o.transform.position.x,(int)o.transform.position.z] = p;
				
				
				
			}
		}
		
	}
	private void GeneratePiece(int x, int y){
		bool isPieceWhite = (y > 3) ? false:true;
		Vector3 pos = Vector3.zero;
		string pieceKind = (isPieceWhite) ? whitePiecePrefab.name : blackPiecePrefab.name;
		GameObject o = PhotonNetwork.Instantiate(pieceKind, pos, Quaternion.identity);

		
		o.transform.position = Vector3.zero;//it will move the piece to the center
		Piece p = o.GetComponent<Piece>();
		pieces[x,y] = p;
		MovePiece(p,x,y);
		Debug.Log("Piece:"+pieceKind+" and position"+o.transform.position);
	}
	private void MovePiece(Piece p, int x, int y){
		p.transform.position = (Vector3.right * x) + (Vector3.forward * y);
	}
}
