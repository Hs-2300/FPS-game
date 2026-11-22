using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public GameObject player;//Object to spawn
    public Transform SpawnPoint;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(message:"Connecting...");

        PhotonNetwork.ConnectUsingSettings();//request connection to master server
    }

    public override void OnConnectedToMaster()//override method provided by parent, override call back
    {
        base.OnConnectedToMaster();

        Debug.Log(message:"Connected to server");

        PhotonNetwork.JoinLobby();// request to join lobby
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        PhotonNetwork.JoinOrCreateRoom
        (roomName:"Test",   //if room exist then join, otherwise create
        roomOptions:null,
        typedLobby:null);

        Debug.Log(message:"We are connected and in a room now.");
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log(message:"We are connected and in a room now.");

        //sync 生成一个Photon 网络对象，其他房间成员也能收到这个对象的生成事件
        GameObject _player = PhotonNetwork.Instantiate
        (player.name,   //player.name提供prefab名称，需要Photon能找到
        SpawnPoint.position,    //生成的时候的世界坐标
        Quaternion.identity);   //使用默认旋转，不使用额外旋转
        _player.GetComponent<PlayerSetup>().IsLocalPlayer();//can only be called by local player
    }

}
