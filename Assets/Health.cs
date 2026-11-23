using System.Collections; // 引入集合相关类型（当前脚本没有直接使用）。
using System.Collections.Generic; // 引入泛型集合类型（当前脚本没有直接使用）。
using UnityEngine; // 提供 MonoBehaviour、GameObject 等 Unity 类型。
using Photon.Pun;
using Unity.VisualScripting; // 提供 PunRPC 特性，用于标记可通过 Photon RPC 调用的方法。

using TMPro;

// 角色生命值组件，挂在可受伤的游戏对象上。
public class Health : MonoBehaviour
{
    // 当前生命值，可在 Unity Inspector 中设置。
    public int health;


    [Header("UI")]
    public TextMeshProUGUI healthText;

    // 允许 Photon 通过 RPC 调用下面的 TakeDamage 方法。
    [PunRPC]
    // 接收本次伤害值，并更新生命值。
    public void TakeDamage(int _damage)
    {
        // 从当前生命值中扣除传入的伤害值。
        health -= _damage;
        //health转换为string格式
        healthText.text = health.ToString();
        // 生命值降到零或以下时，执行销毁。
        if (health <= 0)
        {
            // 销毁当前客户端上的这个游戏对象。
            Destroy(gameObject);
        }

    }
}

