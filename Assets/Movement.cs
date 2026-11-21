using UnityEngine;

public class Movement : MonoBehaviour
{
    public float walkSpeed = 4f;
    public float maxVelocityChange = 10f;

    private Vector2 input;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        input = new Vector2(x:Input.GetAxisRaw("Horizontal"), y:Input.GetAxisRaw("Vertical"));
        input.Normalize();

    }

    void FixedUpdate()//Called by Unity everytime there is a physics Update
    {
        rb.AddForce(CalculateMovement(walkSpeed), ForceMode.VelocityChange);
    }

    Vector3 CalculateMovement(float _speed)
    {
        Vector3 targetVelocity = new Vector3(input.x, y:0,z:input.y);//竖直平面mapping到水平
        targetVelocity = transform.TransformDirection(targetVelocity);//局部坐标改变为世界坐标，朝向运动的方向offset

        targetVelocity *= _speed;//direction*scale

        Vector3 velocity= rb.velocity;

        if (input.magnitude > 0.5f)
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
