using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;

    private bool isSprintingInput;
    private bool IsSprinting => canSprint && isSprintingInput && (!useStamina || currentStamina > 0);

    [Header("Functional Optionas")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadBob = true;
    [SerializeField] private bool WillSlideOnSlopes = true;
    [SerializeField] private bool canZoom = true;
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool useFootsteps = true;
    [SerializeField] private bool useStamina = true;
    [SerializeField] private bool canDive = true;
    [SerializeField] private bool canClimb = true;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchingSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8f;
    private float moveSpeed;
    [SerializeField] private float groundAcceleration = 10f;
    [SerializeField] private float airAcceleration = 3f;
    private Vector3 currentLedgeNormal;

    [Header("Look Prameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Health Parameters")]
    [SerializeField] private float maxHelath = 100;
    [SerializeField] private float timeBeforeRegenStarts = 3;
    [SerializeField] private float healthValueIncrement = 1;
    [SerializeField] private float healthTimeIncrement = 0.1f;
    private float currentHealth;
    private Coroutine regeneratingHealth;
    public static Action<float> OnTakeDamage;
    public static Action<float> OnDamage;
    public static Action<float> OnHeal;

    [Header("Stamina Parameters")]
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float staminaUseMultipler = 5;
    [SerializeField] private float timeBeforeStaminaRegenStarts = 5;
    [SerializeField] private float staminaValueIncrement = 2;
    [SerializeField] private float staminaTimeIncrement = 0.1f;
    private float currentStamina;
    private Coroutine regeneratingStamina;
    public static Action<float> OnStaminaChance;

    [Header("Jumping Parameters")]
    [SerializeField] private float maxJumpHeight = 1.5f;
    [SerializeField] private float gravity = 12.0F;
    [SerializeField] private float fallMultiplier = 1.5F;
    [SerializeField] private float lowJumpMultiplier = 2.0F;
    private bool isJumpPressed;

    [Header("Jump Tolerances (coyote and buffer")]
    [SerializeField] private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [SerializeField] private float jumpBufferTime = 0.15f;
    private float jumpBufferTimeCounter;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;


    [Header("Headbob Prameterers")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.11f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYPos = 0;
    private float crouchYPos = 0;
    private float currentYPos = 0;
    private float timer;

    [Header("Zoom Parameters")]
    [SerializeField] private float timeToZoom = 0.3f;
    [SerializeField] private float zoomFOV = 30f;
    private float defaultFOV;
    private Coroutine zoomRoutine;

    [Header("Foot Step Parameters")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultipler = 1.5f;
    [SerializeField] private float sprintStepMultipler = 0.6f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] woodClips = default;
    [SerializeField] private AudioClip[] metalClips = default;
    [SerializeField] private AudioClip[] grassClips = default;
    private float footstepTimer = 0;
    private float getCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultipler : IsSprinting ? baseStepSpeed * sprintStepMultipler : baseStepSpeed;
    // sliding parameters

    private Vector3 hitPointNormal;

    public MovementState state;
    public enum MovementState{ walking, sprinting, air, crouching, dive, hanging, climbing, tail}

    private bool IsSliding
    {
        get 
        {
            
            if(characterController.isGrounded && Physics.Raycast(transform.position,Vector3.down, out RaycastHit slopeHit , 2f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }

    [Header("Interaction")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable currentInteractable; 

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currenInput;

    private float rotationX = 0;

    [Header("Dive Parameters")]
    [SerializeField] private float diveSpeed = 15f;
    [SerializeField] private float diveDuration = 0.25f;
    [SerializeField] private float diveStaminaCost = 25f;

    private bool hasDived;
    private bool isDiving;

    [Header("Edge Climb Parameters")]
    [SerializeField] private float climbRayLength = 0.6f;
    [SerializeField] private float climbRayHeight = 0.4f;
    [SerializeField] private float climbDownRayHeight = 0.6f;
    [SerializeField] private LayerMask climbableLayer;
    [SerializeField] private float climbSpeed = 5f;

    private Vector3 currentLedgePoint;

    private bool isClimbing;


    [Header("Input Actions ")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction lookAction;
    [SerializeField] private InputAction sprintAction;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction crouchAction;
    [SerializeField] private InputAction zoomAction;
    [SerializeField] private InputAction interactAction;
    private Vector2 rawInput;
    private Vector2 mouseInput;

    private void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        crouchYPos = defaultYPos* (crouchHeight / standingHeight);
        currentYPos = defaultYPos;
        defaultFOV = playerCamera.fieldOfView;
        currentHealth = maxHelath;
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        OnTakeDamage += ApplyDamage;

        moveAction?.Enable();
        lookAction?.Enable();
        sprintAction?.Enable();
        jumpAction?.Enable();
        crouchAction?.Enable();
        zoomAction?.Enable();
        interactAction?.Enable();

        moveAction.performed += ctx => rawInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => rawInput = Vector2.zero;

        lookAction.performed += ctx => mouseInput = ctx.ReadValue<Vector2>();
        lookAction.canceled += ctx => mouseInput = Vector2.zero;

        jumpAction.started += ctx => HandelJump();
        jumpAction.canceled += ctx => OnJumpReleased();

        crouchAction.performed += ctx => HandleCrouch();

        interactAction.performed += ctx => HandleInteactionInput();

        sprintAction.performed += OnSprintAction;
        sprintAction.canceled += OnSprintAction;

        zoomAction.started += ctx => ToggleZoomState(true);
        zoomAction.canceled += ctx => ToggleZoomState(false);
    }

    private void OnDisable()
    {
        OnTakeDamage -= ApplyDamage;

        moveAction.performed -= ctx => rawInput = ctx.ReadValue<Vector2>();
        moveAction.canceled -= ctx => rawInput = Vector2.zero;

        lookAction.performed -= ctx => mouseInput = ctx.ReadValue<Vector2>();
        lookAction.canceled -= ctx => mouseInput = Vector2.zero;

        jumpAction.started -= ctx => HandelJump();
        jumpAction.canceled -= ctx => OnJumpReleased();

        crouchAction.performed -= ctx => HandleCrouch();

        interactAction.performed -= ctx => HandleInteactionInput();

        sprintAction.performed -= OnSprintAction;
        sprintAction.canceled -= OnSprintAction;

        zoomAction.started -= ctx => ToggleZoomState(true);
        zoomAction.canceled -= ctx => ToggleZoomState(false);

        moveAction?.Disable();
        lookAction?.Disable();
        sprintAction?.Disable();
        jumpAction?.Disable();
        crouchAction?.Disable();
        zoomAction?.Disable();
        interactAction?.Disable();
    }

    private void Update()
    {
        if (CanMove)
        {
            StateHandler();
            UpdateTimers();
            HandleMouseLook();

            if (!isDiving)
            {
                HandleMovementInput();
            }

            ExecuteJump();

            HandleLedgeDetection();
            HandleHanging();

            if(state == MovementState.walking || state == MovementState.crouching || state == MovementState.sprinting)
            {
                if (canUseHeadBob)
                    HandleHeadBob();

                if (useFootsteps)
                    Handle_Footsteps();
            }
            
            if (canInteract)
                HandleInteractionCheck();
            
            if (useStamina)
                HandleStamina();
                
            ApplyFinalMovements();
        }
    }

    private void StateHandler()
    {
        if (characterController.isGrounded)
        {
            if (isCrouching)
            {
                state = MovementState.crouching;
                moveSpeed = crouchingSpeed;
            }
            else if (IsSprinting)
            {
                state = MovementState.sprinting;
                moveSpeed = sprintSpeed;
            }
            else
            {
                state = MovementState.walking;
                moveSpeed = walkSpeed;
            }
        }
        else
        {
            state = MovementState.air;
        }
    }

    private void UpdateTimers()
    {
        if (characterController.isGrounded) 
        {
            coyoteTimeCounter = coyoteTime;
            hasDived = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        jumpBufferTimeCounter -= Time.deltaTime;
    }

    private void HandleMovementInput()
    {
        Vector2 targetInput = new Vector2(moveSpeed * rawInput.y, moveSpeed* rawInput.x);

        float acceleration = characterController.isGrounded ? groundAcceleration : airAcceleration;

        currenInput = Vector2.Lerp(currenInput, targetInput, acceleration * Time.deltaTime);

        float moveDirectionY = moveDirection.y;

        moveDirection = (transform.TransformDirection(Vector3.forward) * currenInput.x) + (transform.TransformDirection(Vector3.right) * currenInput.y);

        moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook()
    {
        rotationX -= mouseInput.y * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        transform.rotation *= Quaternion.Euler(0, mouseInput.x * lookSpeedX, 0);
    }

    private void HandelJump() 
    {
        if (canJump)
        {
            jumpBufferTimeCounter = jumpBufferTime;
        }
           
    }

    private void ExecuteJump()
    {
        if (!canJump) return;

        if (jumpBufferTimeCounter > 0f && coyoteTimeCounter > 0f) 
        {
            moveDirection.y = Mathf.Sqrt(2f * gravity * maxJumpHeight);

            jumpBufferTimeCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    private void OnJumpReleased()
    {
        if (moveDirection.y > 0)
        {
            moveDirection.y *= 0.5f;
        }
    }

    private void OnSprintAction(InputAction.CallbackContext ctx)
    {
        
        if (ctx.performed)
        {
            isSprintingInput = true; 

           
            if (canDive && state == MovementState.air && !hasDived && (!useStamina || currentStamina >= diveStaminaCost))
            {
                StartCoroutine(PerformDive());
            }
        }
        
        else if (ctx.canceled)
        {
            isSprintingInput = false; 
        }
    }


    private void HandleCrouch()
    {
        // YENÝ: Asýlýyken eðilme tuþuna basarsa tutunmayý býrakýr ve düþer
        if (state == MovementState.hanging) { state = MovementState.air; return; }

        if (!canCrouch || !characterController.isGrounded || duringCrouchAnimation) return;
        StartCoroutine(CrouchStand());
    }

    private void HandleHeadBob()
    {
        if (!characterController.isGrounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                currentYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount: IsSprinting ? sprintBobAmount: walkBobAmount),
                playerCamera.transform.localPosition.z);
        }

    }

    private void HandleStamina()
    {
        if(state == MovementState.sprinting && rawInput.magnitude > 0.1f)
        {
            if (regeneratingStamina != null)
            {
                StopCoroutine(regeneratingStamina);
                regeneratingStamina = null;
            }

            currentStamina -= staminaUseMultipler * Time.deltaTime;

            if (currentStamina < 0)
                currentStamina = 0;

            OnStaminaChance?.Invoke(currentStamina);

            if(currentStamina <= 0)
                canSprint = false;
        }

        if(!IsSprinting && currentStamina < maxStamina && regeneratingStamina ==  null)
        {
            regeneratingStamina = StartCoroutine(RegenerateStamina());
        }
    }

    private void ToggleZoomState(bool isEnter)
    {
        if (!canZoom) return;

        if (zoomRoutine != null) 
        { 
            StopCoroutine (zoomRoutine);
            zoomRoutine = null;
        }

        zoomRoutine = StartCoroutine(ToggleZoom(isEnter));
    }

    private void HandleInteractionCheck()
    {
        if (Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance))
        {
            if(hit.collider.gameObject.layer == 9 && (currentInteractable == null || hit.collider.gameObject.GetEntityId() != currentInteractable.GetEntityId())) 
            {
                hit.collider.TryGetComponent(out currentInteractable);
                
                if(currentInteractable)
                    currentInteractable.OnFocus();
            }
        }
        else if (currentInteractable)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }

    private void HandleInteactionInput()
    {
        if (canInteract && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
        {
            currentInteractable.OnInteract();
        }
    }

    private void Handle_Footsteps()
    {
        if (!characterController.isGrounded) return;
        if (rawInput.magnitude < 0.1f) return;

        footstepTimer -= Time.deltaTime;
        
        if(footstepTimer <= 0)
        {
            if(Physics.Raycast(playerCamera.transform.position,Vector3.down, out RaycastHit hit, 3))
            {
                switch (hit.collider.tag)
                {
                    
                    case "Footsteps/WOOD":
                        footstepAudioSource.PlayOneShot(woodClips[UnityEngine.Random.Range(0, woodClips.Length - 1)]);
                        break;
                    case "Footsteps/METAL":
                        footstepAudioSource.PlayOneShot(metalClips[UnityEngine.Random.Range(0, metalClips.Length - 1)]);
                        break;
                    case "Footsteps/GRASS":
                        footstepAudioSource.PlayOneShot(grassClips[UnityEngine.Random.Range(0, grassClips.Length - 1)]);
                        break;
                    default: 
                        break;
                    
                } 
            }

            footstepTimer = getCurrentOffset;
        }
        
    }

    private void HandleLedgeDetection()
    {
        if (!canClimb || characterController.isGrounded || state == MovementState.dive || state == MovementState.climbing || state == MovementState.hanging) return;

        Vector3 rayStart = transform.position + Vector3.up * climbRayHeight;

        if (Physics.Raycast(rayStart, transform.forward, out RaycastHit forwardHit, climbRayLength, climbableLayer))
        {
            Vector3 downRayStart = forwardHit.point + (transform.forward * 0.05f) + (Vector3.up * climbDownRayHeight);

            if (Physics.Raycast(downRayStart, Vector3.down, out RaycastHit downHit, climbDownRayHeight, climbableLayer))
            {
                currentLedgePoint = downHit.point;
                currentLedgeNormal = forwardHit.normal;
                EvaluateClimbIntention();
            }
        }
    }

    private void HandleHanging()
    {

        if (state != MovementState.hanging) return;

        // Asýlýyken W'ya basarsa güvenle týrman
        if (rawInput.y > 0)
        {
            StartCoroutine(PerformClimb(currentLedgePoint));
        }
    }

    private void EvaluateClimbIntention()
    {
        if (rawInput.y > 0 && IsSprinting)
        {
            StartCoroutine(PerformClimb(currentLedgePoint));
        }
        else 
        {
            StartCoroutine(PerformHang(currentLedgePoint, currentLedgeNormal));
        }
    }

    private void ApplyDamage(float dmg)
    {
        currentHealth -= dmg;
        OnDamage?.Invoke(currentHealth);

        if (currentHealth <= 0)
            KillPlayer();
        else if (regeneratingHealth != null)
            StopCoroutine(regeneratingHealth);

        regeneratingHealth = StartCoroutine(RegenerateHealth());
    }
    
    private void KillPlayer()
    {
        currentHealth = 0;

        if (regeneratingHealth != null)
            StopCoroutine(regeneratingHealth);

        print("dead");
    }

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded && state != MovementState.dive && state != MovementState.hanging && state != MovementState.climbing)
        {
            
            if (moveDirection.y < 0)
            {
                moveDirection.y -= gravity * fallMultiplier * Time.deltaTime;
            }
            else if (moveDirection.y > 0 && !jumpAction.IsPressed()) 
            {
                moveDirection.y -= gravity * lowJumpMultiplier * Time.deltaTime;
            }
            else
            {
                moveDirection.y -= gravity * Time.deltaTime;
            }
        }
        else if (characterController.isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f;
        }

        if (WillSlideOnSlopes && IsSliding)
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;

        characterController.Move(moveDirection * Time.deltaTime);

    }

    private IEnumerator CrouchStand()
    {
        if(isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up,1f))
            yield break;

        duringCrouchAnimation = true;
        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;
        float targetCamHeight = isCrouching ? defaultYPos : crouchYPos;
        float startCamHeight = currentYPos;

        while (timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            currentYPos = Mathf.Lerp(startCamHeight, targetCamHeight, timeElapsed / timeToCrouch);
            playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x,currentYPos,playerCamera.transform.localPosition.z);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }

    private IEnumerator ToggleZoom(bool isEnter)
    {
        float targetFOV = isEnter ? zoomFOV : defaultFOV;
        float startingFOV = playerCamera.fieldOfView;
        float timeElapsed = 0;

        while(timeElapsed < timeToZoom)
        {
            playerCamera.fieldOfView = Mathf.Lerp(startingFOV, targetFOV, timeElapsed / timeToZoom);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        playerCamera.fieldOfView = targetFOV;
        zoomRoutine = null;
    }
    private IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(timeBeforeRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(healthTimeIncrement);

        while(currentHealth <= maxHelath)
        {
            currentHealth += healthValueIncrement;

            if(currentHealth > maxHelath)
                currentHealth = maxHelath;

            OnHeal?.Invoke(currentHealth);
            yield return timeToWait;
        }

        regeneratingHealth = null;
    }

    private IEnumerator RegenerateStamina()
    {
        yield return new WaitForSeconds(timeBeforeStaminaRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);

        while (currentStamina < maxStamina)
        {

            if(currentStamina > 0)
                canSprint = true;

            currentStamina += staminaValueIncrement;

            if (currentStamina > maxStamina)
                currentStamina = maxStamina;

            OnStaminaChance?.Invoke(currentStamina);

            yield return timeToWait;
        }

        regeneratingStamina = null;
    }

    private IEnumerator PerformDive()
    {
        isDiving = true;
        hasDived = true;

        if (useStamina)
        {
            currentStamina -= diveStaminaCost;
            OnStaminaChance?.Invoke(currentStamina);
        }
        

        Vector3 diveDirection = (transform.forward *currenInput.x + transform.right * currenInput.y).normalized;

        if (diveDirection == Vector3.zero)
            diveDirection = transform.forward;

        float startTime = Time.time;

        while (Time.time < startTime + diveDuration)
        {
            moveDirection = diveDirection * diveSpeed;
            moveDirection.y = 0;

            yield return null;
        }

        isDiving = false;
    }

    private IEnumerator PerformClimb(Vector3 targetPosition)
    {
        state = MovementState.climbing;

        Vector3 finalPosition = targetPosition + new Vector3(0, characterController.height / 2f,0);

        while (Vector3.Distance(transform.position, finalPosition) > 0.1f)
        {
            Vector3 climpDirection = (finalPosition - transform.position).normalized;
            characterController.Move(climpDirection* climbSpeed * Time.deltaTime);
            yield return null;
        }

        state = MovementState.walking;
    }

    private IEnumerator PerformHang(Vector3 targetPosition, Vector3 wallNormal)
    {
        state = MovementState.hanging;
        moveDirection = Vector3.zero;

        // Maymunun durmasý gereken yer: Kenarýn biraz aþaðýsý ve duvarýn dýþ cephesi
        Vector3 hangOffset = (wallNormal * 0.15f) - new Vector3(0, characterController.height / 1.5f, 0);
        Vector3 hangPosition = targetPosition + hangOffset;

        float timeElapsed = 0;
        float snapTime = 0.15f; // Havadan duvara mýknatýs gibi çekilme süresi

        while (timeElapsed < snapTime)
        {
            // Karakteri havada donduðu yerden alýp, tam kenara yumuþakça oturt
            Vector3 snapDirection = (hangPosition - transform.position);
            characterController.Move(snapDirection * 10f * Time.deltaTime);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
}
