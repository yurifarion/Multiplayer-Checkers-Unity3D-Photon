using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using System.IO;
using System.Net;
using UnityEngine;

public class Server : MonoBehaviour
{
   public int port =  6321;
   
   private List<ServerClient> clients;
   private List<ServerClient> disconnectList;
   
   private TcpListener server;
   private bool serverStarted;
   
   public void Init()
   {
	   DontDestroyOnLoad(gameObject);
	   clients = new List<ServerClient>();
	   disconnectList = new List<ServerClient>();
	   
	   try
	   {
		   server = new TcpListener(IPAddress.Any,port);
		   server.Start();
		   StartListening();
		   serverStarted = true;
	   }
	   catch(Exception e)
	   {
		   Debug.Log("Socket error: "+ e.Message);
	   }
   }
   private void Update()
   {
	   if(!serverStarted) return;
	   
	   foreach(ServerClient c in clients){
		   //Is a client conncted ?
		   if(!IsConnected(c.tcp)){
			   c.tcp.Close();
			   disconnectList.Add(c);
			   continue;
		   }
		   else{
			   NetworkStream s = c.tcp.GetStream();
			   if(s.DataAvailable){
				   StreamReader reader = new StreamReader(s,true);
				   string data = reader.ReadLine();
				   
				   if(data != null) OnIncomingData(c,data);
			   }
		   }
	   }
	   for( int i = 0; i < disconnectList.Count -1;++i){
		   
		   //Say to player that someon  has disconnected
		   clients.Remove(disconnectList[i]);
		   disconnectList.RemoveAt(i);
	   }
   }
   private void StartListening(){
	   server.BeginAcceptTcpClient(AcceptTcpClient,server);
   }
   private void AcceptTcpClient(IAsyncResult ar)
   {
	   TcpListener listener = (TcpListener)ar.AsyncState;
	   ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
	   clients.Add(sc);
	   
	   StartListening();
	   Debug.Log("Somebody has connected");
   }
   private void OnIncomingData(ServerClient c,string data)
   {
	   Debug.Log(c.clientName +" ; "+ data);
   }
   private bool IsConnected(TcpClient c){
	   try{
		   if(c != null && c.Client != null && c.Client.Connected)
		   {
			   if(c.Client.Poll(0,SelectMode.SelectRead)) 
				   return !(c.Client.Receive(new byte[1],SocketFlags.Peek)==0);
			   
			   return true;
		   }
		   else return false;
	   }
	   catch{
		   return false;
	   }
   }
}

public class ServerClient
{
	public string clientName;
	public TcpClient tcp;
	
	public ServerClient(TcpClient tcp)
	{
		this.tcp = tcp;
	}
}
