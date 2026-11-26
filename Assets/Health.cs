using System.Collections; // 引入集合相关类型（当前脚本没有直接使用）。
using System.Collections.Generic; // 引入泛型集合类型（当前脚本没有直接使用）。
using UnityEngine; // 提供 MonoBehaviour、GameObject 等 Unity 类型。
using Photon.Pun;
using Unity.VisualScripting; // 提供 PunRPC 特性，用于标记可通过 Photon RPC 调用的方法。
using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;

// 角色生命值组件，挂在可受伤的游戏对象上。
public class Health : MonoBehaviour
{
    // 当前生命值，可在 Unity Inspector 中设置。
    public int health;
    // 标记这个 Health 所属的角色是否由本机玩家控制；public 字段也会出现在 Inspector 中。
    // 新角色生成时由 RoomManager 设为 true，其他客户端上的同一角色副本保持默认的 false。
    public bool IsLocalPlayer;

    public RectTransform healthBar;
    private float originalHealthBarSize;

    private void Start()
    {
        originalHealthBarSize = healthBar.sizeDelta.x;
    }

    private void Update()
    {
        // healthBar.sizeDelta = new Vector2(x:originalHealthBarSize * health /100f,y:healthBar.sizeDelta.y);
    }

    [Header("UI")]
    public TextMeshProUGUI healthText;

    // 允许 Photon 通过 RPC 调用下面的 TakeDamage 方法。
    [PunRPC]
    // 接收本次伤害值，并更新生命值。
    public void TakeDamage(int _damage)
    {
        //检查1
        Debug.Log($"TakeDamage: object={name}, damage={_damage}, healthBefore={health}, IsLocalPlayer={IsLocalPlayer}, IsMine={GetComponent<PhotonView>().IsMine}");
        // 从当前生命值中扣除传入的伤害值。
        health -= _damage;
        healthBar.sizeDelta = new Vector2(x:originalHealthBarSize * health /100f,y:healthBar.sizeDelta.y);

        //health转换为string格式
        healthText.text = health.ToString();

        //检查2
        Debug.Log($"After damage: object={name}, health={health}");
        // 生命值降到零或以下时，执行销毁。
        if (health <= 0)
        {
            // 只允许本机拥有的阵亡角色触发复活，避免每个客户端都生成一个新玩家。
            if (IsLocalPlayer)
            {
                // 通过静态 instance 找到场景中的 RoomManager，再请求生成新角色。
                // 注意：当前 RoomManager 只有 SpawnPlayer()，没有 RespawnPlayer()，此处会产生编译错误。
                RoomManager.instance.SpawnPlayer();

                RoomManager.instance.deaths ++;//increase death by 1
                RoomManager.instance.SetHashes();//sync
                // 销毁当前客户端上的这个游戏对象。
            }

            PhotonNetwork.Destroy(gameObject);//Destroy(gameObject); 
        }

    }
}

