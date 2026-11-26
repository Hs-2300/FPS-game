using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;//

public class RoomManager : MonoBehaviourPunCallbacks
{
    // 保存当前场景中 RoomManager 的共享引用，供 Health 等脚本通过 RoomManager.instance 访问。
    public static RoomManager instance;
    
    public GameObject player;//Object to spawn
    // 在 Inspector 中为下一个公开字段添加可见间距，不影响运行逻辑。
    [Space]
    public Transform[] SpawnPoints;
    // 再添加一段 Inspector 间距，便于区分出生点和连接画面摄像机。
    [Space]
    // 指向进入房间前使用的 RoomCam 游戏对象，由场景中的 Inspector 引用赋值。
    public GameObject roomCam;

    [Space]
    public GameObject nameUI;
    public GameObject connectingUI;

    private string nickname = "Unnamed";

    public string roomNameToJoin = "Test";

    [HideInInspector]
    public int kills = 0;
    public int deaths = 0;


    // Unity 创建并启用此组件时调用 Awake，通常早于 Start。
    void Awake()
    {
        // 将当前组件实例存入静态字段，使其他脚本能找到这个 RoomManager。
        instance = this;
    }

    public void ChangeNickname(string _name)
    {
        nickname = _name;
    }

    public void JoinRoomButtonPressed()
    {
        Debug.Log(message:"Connecting...");

        PhotonNetwork.ConnectUsingSettings();//request connection to master server

        nameUI.SetActive(false);
        connectingUI.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        
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

        Debug.Log(message:"We are in the lobby.");
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log(message:"We are connected and in a room now.");

        // 加入房间后关闭等待画面摄像机及其子对象，让玩家摄像机接管画面。
        roomCam.SetActive(false);
        
        // 首次进入房间时生成由本机控制的玩家角色。
        SpawnPlayer();
    }

    // 生成一个网络玩家，并启用这个客户端对新角色的控制和本地身份标记。
    public void SpawnPlayer()
        {
            Transform spawnPoint = SpawnPoints[UnityEngine.Random.Range(0,SpawnPoints.Length)];
            
            // PhotonNetwork.Instantiate 会从 Resources 中按预制体名称加载，并同步创建给房间成员。
            GameObject _player = PhotonNetwork.Instantiate
            // player.name 是要查找的预制体名称；SpawnPoint.position 是世界坐标出生点；
            // Quaternion.identity 表示不额外旋转，沿用预制体的默认朝向。
            (player.name,   //player.name提供prefab名称，需要Photon能找到
            spawnPoint.position,    //生成的时候的世界坐标
            Quaternion.identity);   //使用默认旋转，不使用额外旋转

            // 获取新角色的 PlayerSetup，启用其移动组件和玩家摄像机。
            _player.GetComponent<PlayerSetup>().IsLocalPlayer();//can only be called by local player
            // 将本机新角色的 Health 标为本地角色；后续死亡时据此决定是否触发复活。
            _player.GetComponent<Health>().IsLocalPlayer = true;
            _player.GetComponent<PhotonView>().RPC(methodName:"SetNickName", RpcTarget.AllBuffered, nickname);
            PhotonNetwork.LocalPlayer.NickName = nickname;
        }

        public void SetHashes()
    {
        try
        {
            Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;

            hash["kills"] = kills;
            hash["deaths"] = deaths;

            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
        catch
        {
            //Do nothing
        }
    }

}
