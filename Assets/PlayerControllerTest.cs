using UnityEngine;

public class PlayerControllerTest : MonoBehaviour
{
    [SerializeField] Transform groundCheck;
    [SerializeField] float walkSpeed = 6.0f;
    [SerializeField] float jumpHeight = 3.0f;
    [SerializeField] float gravity = -13.0f;
    [SerializeField] private float groundDistance;
    [SerializeField] private LayerMask groundMask;
    [SerializeField][Range(0.0f, 0.5f)] float moveSmoothTime = 0.3f;

    CharacterController controller;

    public Vector3 velocity;

    Vector2 currentDir = Vector2.zero;
    Vector2 currentDirVelocity = Vector2.zero;
    float velocityY;
    
    Vector2 movementInput;
    public bool isGrounded;
    public bool jump;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        UpdateMovement();
    }

    void UpdateMovement()
    {
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            controller.stepOffset = 0.3f;
            if (velocity.y < 0f)
            {
                velocity.y = -1f;
            }
        }
        else
        {
            controller.stepOffset = 0f;
        }


        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -1f;
        }
        
        currentDir = Vector2.SmoothDamp(currentDir, movementInput, ref currentDirVelocity, moveSmoothTime);

        Vector3 _move = transform.right * currentDir.x + transform.forward * currentDir.y;
        controller.Move(_move * walkSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.Space))
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }            
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
