using Networking;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform aCamTransform;
    [SerializeField] private float walkSpeed = 6.0f;
    [SerializeField] private float jumpHeight = 3.0f;
    [SerializeField] private float gravity = -13.0f;
    [SerializeField] private float groundDistance;
    [SerializeField] private LayerMask groundMask;
    [SerializeField][Range(0.0f, 0.5f)] private float moveSmoothTime = 0.3f;

    private float velocityY;

    private Vector2 currentDir = Vector2.zero;
    private Vector2 currentDirVelocity = Vector2.zero;
    private Vector3 velocity;

    private Vector2 movementDirection;
    public bool isGrounded;
    public bool jump;
    // ReSharper disable once InconsistentNaming
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }
    
    private void FixedUpdate()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            controller.stepOffset = 0.3f;
            if (velocity.y < 0f)
            {
                velocity.y = -1f;
            }

            if (jump)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            controller.stepOffset = 0f;
        }

        currentDir = Vector2.SmoothDamp(currentDir, movementDirection, ref currentDirVelocity, moveSmoothTime);

        Vector3 _move = _transform.right * currentDir.x + _transform.forward * currentDir.y;
        controller.Move(_move * (walkSpeed * Time.deltaTime));

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        ServerSend.PlayerMovement(player.id, _transform.position, transform.rotation);
    }

    public void Look(float _xRotationOfCamera, Vector3 _bodyRotation)
    {
        aCamTransform.localEulerAngles = Vector3.right * _xRotationOfCamera;
        transform.Rotate(_bodyRotation);
    }

    public void GetMovementInput(Vector2 _input, bool _jump, Quaternion _rotation)
    {
        movementDirection = _input;
        jump = _jump;
        _transform.rotation = _rotation;
    }
    
    public void Teleport(Vector3 _pos)
    {
        controller.enabled = false;
        transform.position = _pos;
        controller.enabled = true;
    }
}
