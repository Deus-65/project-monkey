using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;
    [SerializeField]private InputAction Test2;

    private void OnEnable()
    {
        Test2.Enable();
    }

    private void OnDisable()
    {
        Test2.Disable();
    }


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update()
    {
        /* get mouse input old way that doesn't work
        float mouseX = Input.GetAxisRaw("mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("mouse Y") * Time.deltaTime * sensY;
        */

        // Get mouse delta using the New Input System

        // Mouse yerine doðrudan oluþturduðumuz InputAction'ý okuyoruz.
        // Bu bize bir Vector2 (x ve y yönleri) verecek.
        Vector2 lookInput = Test2.ReadValue<Vector2>();
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // X ve Y eksenlerini ayýrýyoruz
        float lookX = lookInput.x * Time.deltaTime * sensX;
        float lookY = lookInput.y * Time.deltaTime * sensY;
        float mouseX = mouseDelta.x * Time.deltaTime * sensX;
        float mouseY = mouseDelta.y * Time.deltaTime * sensY;


        yRotation += lookX;

        xRotation -= lookY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

    }
}
