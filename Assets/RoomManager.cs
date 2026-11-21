using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(message:"Connecting...");

        PhotonNetwork.ConnectUsingSettings();//connect to master server
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log(message:"Connected to server");

        PhotonNetwork.JoinLobby();// request to join lobby
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        PhotonNetwork.JoinOrCreateRoom(roomName:"Test",roomOptions:null,typedLobby:null);

        Debug.Log(message:"We are connected and in a room now.");
    }
}
