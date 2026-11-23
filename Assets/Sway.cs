using System.Collections; // 引入非泛型集合命名空间；此脚本当前未使用其中的类型。
using System.Collections.Generic; // 引入泛型集合命名空间；此脚本当前未使用其中的类型。
using System.IO; // 引入文件输入输出命名空间；此脚本当前未使用其中的类型。
using System.IO.Compression; // 引入压缩相关命名空间；此脚本当前未使用其中的类型。
using Unity.VisualScripting; // 引入 Unity Visual Scripting 命名空间；此脚本当前未使用其中的类型。
using UnityEngine; // 引入 MonoBehaviour、Vector2、Vector3、Mathf 和 Time 等 Unity 类型。

// Sway 是挂在游戏对象上的组件，用鼠标移动量产生局部位置晃动。
public class Sway : MonoBehaviour
{
    // 在 Inspector 中为下面的设置字段显示“Settings”标题。
    [Header("Settings")]
    // 鼠标输入在每个方向上的最大晃动量；public 字段可在 Inspector 中调整。
    public float swayClamp = 0.09f;
    
    // 在 Inspector 中给相邻字段增加间距。
    [Space]
    // 位置插值速度系数：值越大，跟随目标位置越快。
    public float smoothing = 3f;

    // 记录物体开始时的局部位置，作为晃动的中心；private 字段不在 Inspector 中显示。
    private Vector3 origin;
    // Unity 在组件启用后的第一帧 Update 之前调用 Start。
    void Start()
    {
        // 保存相对于父对象的初始位置。
        origin = transform.localPosition;
    }

    // Unity 每帧调用 Update，根据鼠标移动更新目标位置。
    void Update()
    {
        // 读取本帧鼠标水平和竖直方向的原始输入，并组成二维向量。
        Vector2 input = new Vector2(
            // x: 是构造函数参数名；Mouse X 是 Unity 输入轴名称。
            x:Input.GetAxisRaw("Mouse X"),
            // y: 是构造函数参数名；Mouse Y 是 Unity 输入轴名称。
            y:Input.GetAxisRaw("Mouse Y"));

        // 将水平输入限制在 -swayClamp 到 swayClamp 之间。
        input.x = Mathf.Clamp(value:input.x, min:-swayClamp,max:swayClamp);
        // 将竖直输入限制在 -swayClamp 到 swayClamp 之间。
        input.y = Mathf.Clamp(value:input.y, min:-swayClamp,max:swayClamp);

        // 将二维输入变为局部位移；负号让晃动方向与鼠标移动方向相反，z 保持 0。
        Vector3 target = new Vector3(-input.x, -input.y, z:0);

        // 从当前位置向“初始位置 + 目标位移”平滑移动；deltaTime 使速度随帧时长调整。
        transform.localPosition = Vector3.Lerp(a:transform.localPosition,b:target+origin, t:Time.deltaTime * smoothing);
    }
}
