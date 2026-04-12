using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public Transform orientation;


    // Yer Kontrol
    public float groundDrag;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;
   
    // Zıplama 
    [Header("Jumping")]
    public float jumpForce;
    public float jumpHeight;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    // koşu için Yeni Input Action
    private InputAction sprintAction;
    [SerializeField] private InputAction sprintActionGamepad;
   


    // Zıplama için Yeni Input Action
    private InputAction jumpAction;
    [SerializeField]private InputAction jumpActionGamepad;

    // Hareket Input Action'ları
    private InputAction moveAction;
    [SerializeField] private InputAction Test;

    float horizontalInput;
    float verticalInput;
    bool isJumpingInput; // Zıplama tuşuna basılıp basılmadığını tutar
    bool isSprintingInput; 

    Vector3 moveDirection;
    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        air
    }

    private void Awake()
    {

        // Kod üzerinden WASD ve Ok tuşları
        moveAction = new InputAction("Move", binding: "2DVector");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        // Zıplama aksiyonunu kod üzerinden oluşturuyoruz (İstersen Inspector'dan da bağlayabilirsin)
        if (jumpAction == null || jumpAction.bindings.Count == 0)
        {
            jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
            // (deneme amaçlı de aktif hale getirildi)
            // jumpAction.AddBinding("<Gamepad>/buttonSouth"); // Kontrolcüdeki X/A tuşu
        }

        if (sprintAction == null || sprintAction.bindings.Count == 0)
        {
            sprintAction = new InputAction("sprint", binding: "<Keyboard>/LeftShift");
        }

    }

    private void OnEnable()
    {
        //klavye için
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        // gamepad için
        Test?.Enable();
        jumpActionGamepad?.Enable();
        sprintActionGamepad?.Enable();

        moveAction.performed += OnMovementInput;
        moveAction.canceled += OnMovementInput;
               
        // Zıplama tuşuna basıldığını dinliyoruz
        jumpAction.performed += ctx => isJumpingInput = true;
        jumpAction.canceled += ctx => isJumpingInput = false;

        sprintAction.performed += ctx => isSprintingInput = true;
        sprintAction.canceled += ctx => isSprintingInput = false;

        if (Test != null)
        {
            Test.performed += OnMovementInput;
            Test.canceled += OnMovementInput;
        }

        if (jumpActionGamepad != null)
        {
            jumpActionGamepad.performed += ctx => isJumpingInput = true;
            jumpActionGamepad.canceled += ctx => isJumpingInput = false;
        }
        
        if (sprintActionGamepad != null)
        {
            sprintActionGamepad.performed += ctx => isSprintingInput = true;
            sprintActionGamepad.canceled += ctx => isSprintingInput = false;
        }

    }

    private void OnDisable()
    {
        moveAction.performed -= OnMovementInput;
        moveAction.canceled -= OnMovementInput;
        jumpAction.performed -= ctx => isJumpingInput = true;
        jumpAction.canceled -= ctx => isJumpingInput = false;
        sprintAction.performed -= ctx => isSprintingInput = true;
        sprintAction.canceled -= ctx => isSprintingInput = false;
        
        if (Test != null)
        {
            Test.performed -= OnMovementInput;
            Test.canceled -= OnMovementInput;           
        }

        if (jumpActionGamepad != null)
        {
            jumpActionGamepad.performed -= ctx => isJumpingInput = true;
            jumpActionGamepad.canceled -= ctx => isJumpingInput = false;
        }

        if (sprintActionGamepad != null)
        {
            sprintActionGamepad.performed -= ctx => isSprintingInput = true;
            sprintActionGamepad.canceled -= ctx => isSprintingInput = false;
        }

        // klavye için
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        // gamepad için
        Test?.Disable();
        jumpActionGamepad?.Disable();
        sprintActionGamepad?.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        jumpForce = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
        readyToJump = true; // Oyun başladığında zıplamaya hazırız
    }

    private void Update()
    {
        // Ground Check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        
        StateHandler();

        // Handle drag
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        // Zıplama Kontrolü (Yeni Sistem)
        if (isJumpingInput && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown); // Yazım hatası düzeltildi: reserJump -> ResetJump
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        SpeedControl(); 
       
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        Vector2 inputVector = context.ReadValue<Vector2>();
        horizontalInput = inputVector.x;
        verticalInput = inputVector.y;
    }
 
    private void StateHandler()
    {
        // Sprinting mode
        if (grounded && isSprintingInput)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Yerdeyken
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        // Havadayken
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // HATA DÜZELTİLDİ: Y hızını sıfırlıyoruz, X ve Z'yi koruyoruz
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
   
}
