using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInfo : MonoBehaviour
{
    public static PlayerInfo PI;

    public bool isWhite;

    private PhotonView PV;
    private void Start()
    {
        PV = GetComponent<PhotonView>();

        if (PV.IsMine)
        {
            PV.RPC("RPC_AddCharacter", RpcTarget.AllBuffered,isWhite);
        }
    }
   
    [PunRPC]
    void RPC_AddCharacter(bool w)
    {
        isWhite = w;

    }
}
