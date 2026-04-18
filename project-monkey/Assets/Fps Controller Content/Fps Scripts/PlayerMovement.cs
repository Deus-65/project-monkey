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

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYscale;
    private float startYscale;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;


    // crouch için Yeni Input Action
    private InputAction crouchAction;
    [SerializeField] private InputAction crouchActionGamepad;

    // koşu için Yeni Input Action
    private InputAction sprintAction;
    [SerializeField] private InputAction sprintActionGamepad;
   
    // Zıplama için Yeni Input Action
    private InputAction jumpAction;
    [SerializeField]private InputAction jumpActionGamepad;

    // Hareket Input Action'ları
    private InputAction moveAction;
    [SerializeField] private InputAction moveActinGamepad;

    float horizontalInput;
    float verticalInput;
    bool isJumpingInput; // Zıplama tuşuna basılıp basılmadığını tutar
    bool isSprintingInput;
    bool iscrouchingInput;

    Vector3 moveDirection;
    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        air,
        crouching
    }

    private void Awake()
    {

        // Kod üzerinden WASD ve Ok tuşları
        moveAction = new InputAction("Move", binding: "2DVector");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
          
        if (jumpAction == null || jumpAction.bindings.Count == 0)
        {
            jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
        }

        if (sprintAction == null || sprintAction.bindings.Count == 0)
        {
            sprintAction = new InputAction("sprint", binding: "<Keyboard>/LeftShift");
        }

        if (crouchAction == null || crouchAction.bindings.Count == 0)
        {
            crouchAction = new InputAction("crouch", binding: "<Keyboard>/ctrl");
        }

    }

    private void OnEnable()
    {
        //klavye için
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        crouchAction.Enable();
        // gamepad için
        moveActinGamepad?.Enable();
        jumpActionGamepad?.Enable();
        sprintActionGamepad?.Enable();
        crouchActionGamepad?.Enable();

        moveAction.performed += OnMovementInput;
        moveAction.canceled += OnMovementInput;
               
        // Zıplama tuşuna basıldığını dinliyoruz
        jumpAction.performed += ctx => isJumpingInput = true;
        jumpAction.canceled += ctx => isJumpingInput = false;

        sprintAction.performed += ctx => isSprintingInput = true;
        sprintAction.canceled += ctx => isSprintingInput = false;

        crouchAction.performed += OnCrouchPerformed;
        crouchAction.canceled += OnCrouchCanceled;

        if (moveActinGamepad != null)
        {
            moveActinGamepad.performed += OnMovementInput;
            moveActinGamepad.canceled += OnMovementInput;
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
        
        if (crouchActionGamepad != null)
        {
            crouchActionGamepad.performed += OnCrouchPerformed;
            crouchActionGamepad.canceled += OnCrouchCanceled;
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

        crouchAction.performed -= OnCrouchPerformed;
        crouchAction.canceled -= OnCrouchCanceled;

        if (moveActinGamepad != null)
        {
            moveActinGamepad.performed -= OnMovementInput;
            moveActinGamepad.canceled -= OnMovementInput;           
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
        
        if (crouchActionGamepad != null)
        {
            crouchActionGamepad.performed -= OnCrouchPerformed;
            crouchActionGamepad.canceled -= OnCrouchCanceled;
        }

        // klavye için
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        crouchAction.Disable();
        // gamepad için
        moveActinGamepad?.Disable();
        jumpActionGamepad?.Disable();
        sprintActionGamepad?.Disable();
        crouchActionGamepad?.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYscale = transform.localScale.y;

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

        
        if (iscrouchingInput && grounded)
        { 
           transform.localScale = new Vector3(transform.localScale.x,crouchYscale,transform.localScale.z);
           
        }
        else
        {
           transform.localScale = new Vector3(transform.localScale.x, startYscale, transform.localScale.z);
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
        if (grounded && iscrouchingInput)
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        else if (grounded && isSprintingInput)
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

        if (OnSlope() && exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDricetion() * moveSpeed * 20f, ForceMode.Force);

            if(rb.linearVelocity.y > 0) 
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);    
        }

        // Yerdeyken
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        // Havadayken
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        
        // eğimde yer çekimini kapat
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // eğimde hız limiti 
        if (OnSlope() && exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed) 
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        // normal hız limiti
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }

        
    }

    private void Jump()
    {
        // HATA DÜZELTİLDİ: Y hızını sıfırlıyoruz, X ve Z'yi koruyoruz
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse); 
        
        exitingSlope = true;

    }

    private void ResetJump()
    {    
        readyToJump = true;
     
        exitingSlope = false;
    }

    // Tuşa basıldığında çalışacak temiz metot
    private void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        iscrouchingInput = true;

        if (grounded)
        {
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
    }

    // Tuş bırakıldığında çalışacak temiz metot
    private void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        iscrouchingInput = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit , playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0; 
        }
        return false;
    }

    private Vector3 GetSlopeMoveDricetion()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
