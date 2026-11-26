using UnityEngine;//提供输入、向量和物理相关类型

public class Movement : MonoBehaviour
{
    public float walkSpeed = 4f;//默认速度
    public float SprintSpeed = 14f;//冲刺
    public float maxVelocityChange = 10f;//每次物理更新中，X,Z允许的最大速度变化
    
    [Space]
    public float aircontrol = 0.5f;

    [Space]
    public float JumpHeight = 30f;//跳跃高度
    private Vector2 input;  //保存最近一次输入的移动
    private Rigidbody rb;
    private bool Sprinting;
    private bool Jumping;
    private bool grounded = false;

    [Header("Animation")]
    public Animation handAnimation;
    public AnimationClip handWalkAnimation;
    public AnimationClip idleAnimation;

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

        Sprinting = Input.GetButton("Sprint");//if sprinting, use sprint speed; otherwise walkspeed
        if (Input.GetKeyDown(KeyCode.LeftShift))
            Debug.Log("Detected: left shift pressed!");

        Jumping = Input.GetButton("Jump");//detect jumping first
        if (Input.GetKeyDown(KeyCode.Space))
            Debug.Log("Detected: Space pressed!");
    }

    private void OnTriggerStay(Collider other)
    {
        grounded = true;
    }
    void FixedUpdate()//Called by Unity everytime there is a physics Update, 固定步长
    {
        if (grounded)
        {   
            if (Jumping)//优先跳跃
            {
               rb.velocity = new Vector3(rb.velocity.x, 
                JumpHeight, 
                rb.velocity.z);
            }
            else if (input.magnitude > 0.5)//没有跳跃有移动
            {
                handAnimation.clip = handWalkAnimation;
                handAnimation.Play();
                //这个语法，满足条件就case1否则case2.Bool condition?Case 1:Case2
                rb.AddForce(CalculateMovement(Sprinting?SprintSpeed:walkSpeed), ForceMode.VelocityChange);
            }
            else//无跳跃无移动
            {
                handAnimation.clip = idleAnimation;
                handAnimation.Play();

                var velocity1 = rb.velocity;
                velocity1 = new Vector3(
                    x:velocity1.x * 0.2f * Time.fixedDeltaTime,
                    velocity1.y,//保留y方向速度，重力继续发挥作用
                    z:velocity1.z * 0.2f * Time.fixedDeltaTime);
                    rb.velocity = velocity1;
            }
        }
        else
        {
            if (input.magnitude > 0.5)//没有跳跃有移动
            {
                //这个语法，满足条件就case1否则case2.Bool condition?Case 1:Case2
                rb.AddForce(
                    CalculateMovement(Sprinting?SprintSpeed:walkSpeed * aircontrol), 
                    ForceMode.VelocityChange);
            }
            else//无跳跃无移动
            {
                var velocity1 = rb.velocity;
                velocity1 = new Vector3(
                    x:velocity1.x * 0.2f * Time.fixedDeltaTime,
                    velocity1.y,//保留y方向速度，重力继续发挥作用
                    z:velocity1.z * 0.2f * Time.fixedDeltaTime);
                    rb.velocity = velocity1;
            }
        }
        grounded = false;//本轮结束，等待下一次落地检测设置为true
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
