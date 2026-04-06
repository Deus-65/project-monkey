using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public Transform orientation;

    // Yeni Input Sistemi için Action tanýmlamasý
    private InputAction moveAction;
   
    // controlleri kolayca ayarlamak için bir yöntem 
    [SerializeField]private InputAction Test;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    private void Awake()
    {
        // Kod üzerinden WASD ve Ok tuþlarý için bir 2D Vector (Yön) aksiyonu oluþturuyoruz
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
    }

    private void OnEnable()
    {
        // Script aktif olduðunda Input Action'ý açýyoruz
        moveAction.Enable();
        Test.Enable();

        // Event'lere abone oluyoruz (Subscription)
        // performed: Tuþa basýldýðýnda veya deðer deðiþtiðinde çalýþýr
        // canceled: Tuþ býrakýldýðýnda çalýþýr (deðerleri sýfýrlamak için gereklidir)
        moveAction.performed += OnMovementInput;
        moveAction.canceled += OnMovementInput;
        Test.performed += OnMovementInput;
        Test.canceled += OnMovementInput;
    }

    private void OnDisable()
    {
        // Script kapanýrsa veya obje yok olursa hafýza sýzýntýsýný (memory leak) önlemek için abonelikten çýkýyoruz
        moveAction.performed -= OnMovementInput;
        moveAction.canceled -= OnMovementInput;
        Test.performed -= OnMovementInput;
        Test.canceled -= OnMovementInput;
        
        moveAction.Disable();
        Test.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Artýk Update içinde MyInput() çaðýrmamýza gerek yok!
    // Sadece Fizik iþlemleri için FixedUpdate kalýyor.
    private void FixedUpdate()
    {
        MovePlayer();
    }

    // --- CALLBACK CONTEXT KULLANAN FONKSÝYON ---
    private void OnMovementInput(InputAction.CallbackContext context)
    {
        // context.ReadValue ile 2D Vector deðerini okuyoruz (örneðin W'ye basarsak (0, 1) gelir)
        Vector2 inputVector = context.ReadValue<Vector2>();

        horizontalInput = inputVector.x;
        verticalInput = inputVector.y;
    }

    private void MovePlayer()
    {
        // Hareket yönünü hesapla
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Kuvvet uygula
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }




    /*
    eski sisteme göre kod 

    [Header("Movement")]
    public float moveSpeed;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }
    
    // Update is called once per frame
    private void Update()
    {
        MyInput();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizantal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f,ForceMode.Force);
    }
    */
}
