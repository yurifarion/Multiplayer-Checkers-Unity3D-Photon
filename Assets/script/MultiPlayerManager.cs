using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MultiPlayerManager : MonoBehaviourPunCallbacks
{
	private CheckersBoard board;
	//Waiting for other players UI
	public GameObject waiting_gobj;
	
	void Start(){
		board = GetComponent<CheckersBoard>();
	}
	void Update(){
		
	}
	public void StopWaiting(){
		 waiting_gobj.SetActive(false);
	}
    //Check if another player entered our Room
	public override void OnPlayerEnteredRoom(Photon.Realtime.Player other)
	{
	   Debug.Log("player join");
		StopWaiting();
	   //if another player enter the room we will need to add its pieces to our board after a delay
	   StartCoroutine(AddEnemyPiece());
   }
	   IEnumerator AddEnemyPiece(){
		   //Delay of 0.1f seconds
		   yield return new WaitForSeconds(2f);
		   board.AddEnemyPiecesTolist(GameObject.FindGameObjectsWithTag("BlackPiece"));
		   //since the creator of the room is always white, i know that the enemy will be always black
	   }
}
