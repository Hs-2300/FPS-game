using System.Collections; // 引入集合相关类型（当前脚本没有直接使用）。
using System.Collections.Generic; // 引入泛型集合类型（当前脚本没有直接使用）。
using Photon.Pun; // 提供 PhotonView 和 RpcTarget 等 Photon 网络类型。
using TMPro;
using UnityEngine; // 提供 MonoBehaviour、Camera、Ray、Physics 和输入接口。

// 武器组件：读取开火输入，并用射线检测命中目标。
public class Weapon : MonoBehaviour
{
    // 每次命中造成的伤害值。
    public int damage;

    // 发射射线所依据的摄像机。
    public Camera camera;

    // 每秒允许开火的次数。
    public float fireRate;
    //命中特效
    [Header("VFX")]
    public GameObject hitVFX;
    // 距离下一次可开火还剩多少秒。
    private float nextFire;

    [Header("Ammo")]
    public int mag = 5;//how many magazine we start with
    public int ammo = 30;
    public int magAmmo = 30;//amount of ammo in each mag

    [Header("UI")]
    public TextMeshProUGUI magText;
    public TextMeshProUGUI ammoText;
    [Header("Animation")]
    public Animation animation;
    public AnimationClip reload;

    void Start()
    {
        magText.text = mag.ToString();
        ammoText.text = ammo + "/" +magAmmo;
    }
    
    // Unity 每帧调用 Update，处理冷却计时和开火输入。
    void Update()
    {
        // 冷却尚未结束时，按本帧耗时递减。
        if (nextFire > 0)
            // Time.deltaTime 是上一帧到当前帧的时间间隔。
            nextFire -= Time.deltaTime;
        
        // 按住 Fire1（通常是鼠标左键）且冷却结束时开火。
        if (Input.GetButton("Fire1") && nextFire <=0 && ammo>0 && animation.isPlaying == false)
        {
            // 将每秒开火次数换算为两次开火之间的秒数。
            nextFire = 1 /fireRate;

            ammo --;

            magText.text = mag.ToString();
            ammoText.text = ammo + "/" +magAmmo;

            // 执行一次命中检测。
            Fire();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }
    void Reload()
    {
        animation.Play(reload.name);
        if (mag > 0)
        {
            mag --;

            ammo = magAmmo;
        }
        magText.text = mag.ToString();
        ammoText.text = ammo + "/" +magAmmo;
    }
    
    
    // 从摄像机位置沿摄像机正前方发出射线。
    void Fire()
    {
        // 构造射线：起点是摄像机位置，方向是摄像机朝向。
        Ray ray = new Ray(
            origin:camera.transform.position, 
            direction:camera.transform.forward);

        // 用于保存射线命中的对象、位置等信息。
        RaycastHit hit;

        // 在射线前方 100 个 Unity 单位内检测碰撞。
        if (Physics.Raycast(
            ray.origin,
            ray.direction,
            out hit,
            maxDistance: 100f))
        {
            //射线命中的位置，生成一个所有房间玩家都能看到的命中特效
            PhotonNetwork.Instantiate(hitVFX.name,hit.point,Quaternion.identity);
            // 只处理命中对象本身带有 Health 组件的情况。
            if (hit.transform.gameObject.GetComponent<Health>())
            {
                // 对命中对象的 PhotonView 向所有客户端发送 TakeDamage RPC。
                hit.transform.gameObject.GetComponent<PhotonView>().RPC("TakeDamage",RpcTarget.All,damage);
            }
        }
    }
}
