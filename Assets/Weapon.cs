using System.Collections; // 引入集合相关类型（当前脚本没有直接使用）。
using System.Collections.Generic; // 引入泛型集合类型（当前脚本没有直接使用）。
using Photon.Pun; // 提供 PhotonView 和 RpcTarget 等 Photon 网络类型。
using TMPro;
using Unity.VisualScripting; // 当前脚本没有直接使用此命名空间中的类型。
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

    // 在 Inspector 中将下面的后坐力参数归入“Recoil Settings”标题。
    [Header("Recoil Settings")]
    // 这两行目前被注释掉，recoilPercent 不会成为字段，也不会参与计算。
    // [Range(0,1)]//slider 0-1
    // public float recoilPercent = 0.3f;
    // 在 Inspector 中把 recoverPercent 显示为 0 到 2 的滑块。
    [Range(0,2)]
    // 用开火间隔的倍数计算回位平滑时间；数值越大，回位通常越慢。
    public float recoverPercent = 0.7f;
    // 仅在 Inspector 中增加字段间距。
    [Space]
    // 后坐时相对初始位置沿局部 Y 轴向上移动的距离。
    public float recoilUp = 0.03f;
    // 后坐时相对初始位置沿局部 Z 轴向后移动的距离。
    public float recoilBack = 0.05f;

    // 保存枪在开始运行时的局部位置，作为回位目标和后坐偏移的基准。
    private Vector3 originalPosition;
    // SmoothDamp 用来保存上一帧的移动速度；ref 使函数能够更新它。
    private Vector3 recoilVelocity = Vector3.zero;

    // 枪向后坐目标移动时使用的平滑时间。
    private float recoilLength;
    // 枪从后坐位置返回初始位置时使用的平滑时间。
    private float recoverLength;

    // true 表示当前处于后坐阶段，Update 会调用 Recoil。
    private bool recoiling;
    // true 表示当前处于回位阶段，Update 会调用 Recovering。
    private bool recovering;

    void Start()
    {
        magText.text = mag.ToString();
        ammoText.text = ammo + "/" +magAmmo;

        // 使用局部位置，因为后续修改的也是 transform.localPosition。
        originalPosition = transform.localPosition;

        // 后坐平滑时间设为 0，意味着希望快速到达后坐目标。
        recoilLength = 0;
        // 1 / fireRate 是两次射击的间隔，再乘回位比例得到回位时间。
        recoverLength = 1 / fireRate * recoverPercent;
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

        // 本帧按下 R 且备用弹匣数不是负数时，调用换弹方法。
        if (Input.GetKeyDown(KeyCode.R) && mag >= 0)
        {
            Reload();
        }
        // 处于后坐阶段时，每帧向后坐目标移动。
        if (recoiling)
        {
            Recoil();
        }
        // 处于回位阶段时，每帧向初始位置移动。
        if (recovering)
        {
            Recovering();
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
        // 每次真正开火时启动后坐阶段。
        recoiling = true;
        // 新一发射击会打断此前可能正在进行的回位。
        recovering = false;
        
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

    // 计算本次后坐的目标位置，并让枪向该位置移动。
    void Recoil()
    {
        // x 保持初始值，y 增加向上偏移，z 减去向后偏移；x:/y:/z: 是命名参数。
        Vector3 finalPosition =  new Vector3(
            x:originalPosition.x, 
            y:originalPosition.y + recoilUp, 
            z:originalPosition.z - recoilBack);
        // SmoothDamp 返回这一帧的平滑位置，并通过 ref recoilVelocity 更新移动速度。
        transform.localPosition = Vector3.SmoothDamp(
            // 当前位置是插值起点。
            current:transform.localPosition, 
            // finalPosition 是要抵达的后坐位置。
            target:finalPosition, 
            // ref 表示按引用传入速度，函数能把计算结果写回字段。
            ref recoilVelocity, 
            // 最后一个位置参数是平滑时间 recoilLength。
            recoilLength);
        // 只有位置与目标完全相等时，才结束后坐并开始回位。
        if (transform.localPosition == finalPosition)
        {
            // 关闭后坐阶段。
            recoiling =  false;
            // 开启回位阶段。
            recovering = true;
        }
    }

    // 后坐结束后，逐帧将枪移回最初的局部位置。
    void Recovering()
    {
        // 回位目标就是 Start 中保存的初始局部位置。
        Vector3 finalPosition =  originalPosition;
        // 使用 recoverLength 作为平滑时间，复用 recoilVelocity 维持移动连续性。
        transform.localPosition = Vector3.SmoothDamp(
            current:transform.localPosition, 
            target:finalPosition, 
            ref recoilVelocity, 
            recoverLength);
        // 只有精确回到初始位置时，才关闭两个状态标记。
        if (transform.localPosition == finalPosition)
        {
            // 确保后坐状态关闭。
            recoiling =  false;
            // 回位完成，不再更新枪的位置。
            recovering = false;
        }
    }
}
