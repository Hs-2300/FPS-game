using UnityEngine;//提供输入、向量和物理相关类型

public class Movement : MonoBehaviour
{
    public float walkSpeed = 4f;//默认速度
    public float maxVelocityChange = 10f;//每次物理更新中，X,Z允许的最大速度变化

    private Vector2 input;  //保存最近一次输入的移动
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //GetAxisRaw() 读取未经平滑处理的轴值，按键响应更直接。
        input = new Vector2
        (x:Input.GetAxisRaw("Horizontal"),  //水平轴, A/D
        y:Input.GetAxisRaw("Vertical"));    //竖直轴, W/S

        input.Normalize();//长度归一化为1

    }

    void FixedUpdate()//Called by Unity everytime there is a physics Update, 固定步长
    {
        rb.AddForce(CalculateMovement(walkSpeed), ForceMode.VelocityChange);
    }

    Vector3 CalculateMovement(float _speed)
    {
        Vector3 targetVelocity = new Vector3(input.x, y:0,z:input.y);//竖直平面mapping到水平
        targetVelocity = transform.TransformDirection(targetVelocity);//局部坐标改变为世界坐标，朝向运动的方向offset

        targetVelocity *= _speed;//direction*scale

        Vector3 velocity= rb.velocity;

        if (input.magnitude > 0.5f)//移动条件
        {
            Vector3 velocityChange = targetVelocity - velocity;

            velocityChange.x = Mathf.Clamp(value:velocityChange.x,min: -maxVelocityChange, maxVelocityChange);//限制函数
            velocityChange.z = Mathf.Clamp(value:velocityChange.z,min: -maxVelocityChange, maxVelocityChange);//限制函数
            velocityChange.y=0;

            return velocityChange;
        }

        else
        {
            return new Vector3();
        }

    }

}
