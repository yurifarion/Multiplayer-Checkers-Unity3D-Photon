using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class createAndJoinRoom : MonoBehaviourPunCallbacks
{
    public InputField createInput;
    public InputField joinInput;

	//quick match UI gameObjects
	public GameObject findMatchBtn;
	public GameObject searchingPnl;
	
	public void FindMatch(){
		findMatchBtn.SetActive(false);
		searchingPnl.SetActive(true);
		
		//Try to join a random room
		PhotonNetwork.JoinRandomRoom();
		Debug.Log("Searching for a random room");
		
	}
	public override void OnJoinRandomFailed(short returnCode,string message){
		Debug.Log("Cound not find room - creating a room");
		MakeRoom();
		
	}
	void MakeRoom(){
		int randomRoomName = Random.Range(0,5000);
		RoomOptions roomOptions = new RoomOptions()
		{
			IsVisible = true,
			IsOpen = true,
			MaxPlayers = 2
		};
		PhotonNetwork.CreateRoom("RoomName_"+randomRoomName,roomOptions);
		Debug.Log("Room Created, Waiting For another Player");
	}
	public void StopSearch(){
		findMatchBtn.SetActive(true);
		searchingPnl.SetActive(false);
		
		PhotonNetwork.LeaveRoom();
		Debug.Log("Stopped, Back to Menu");
	}
    public void CreateRoom()
    {
		RoomOptions roomOptions = new RoomOptions()
		{
			MaxPlayers = 2
		};
        PhotonNetwork.CreateRoom(createInput.text,roomOptions);
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("SampleScene");
    }
	
}
