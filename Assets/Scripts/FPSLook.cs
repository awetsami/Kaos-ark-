using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;

public class FPSLook : NetworkBehaviour
{
    [Header("Kamera Ayarları")]
    public float mouseSensitivity = 0.1f;

    [Header("Kamera Sallanması (Headbob)")]
    public float bobbingSpeed = 14f;
    public float bobbingAmount = 0.12f;

    private Camera playerCamera;
    private Rigidbody playerRb;
    private float xRotation = 0f;
    private float timer = 0f;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner) return;

        playerCamera = Camera.main;
        playerRb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!base.IsOwner || playerCamera == null) return;

        if (Mouse.current != null)
        {
            float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity;
            float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.Rotate(Vector3.up * mouseX);
        }
    }

    void LateUpdate()
    {
        if (!base.IsOwner || playerCamera == null) return;

        Vector3 baseEyePosition = transform.position + (Vector3.up * 0.8f) + (transform.forward * 0.4f);

        // YENİ SİSTEM: linearVelocity üzerinden hız kontrolü
        float horizontalSpeed = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z).magnitude;
        float waveSlice = 0f;

        if (horizontalSpeed > 0.5f)
        {
            timer += Time.deltaTime * bobbingSpeed;
            waveSlice = Mathf.Sin(timer);
        }
        else
        {
            timer = 0f;
        }

        Vector3 finalEyePosition = baseEyePosition;

        if (waveSlice != 0)
        {
            float translateChange = waveSlice * bobbingAmount;
            finalEyePosition.y += translateChange;
        }

        playerCamera.transform.position = finalEyePosition;
        playerCamera.transform.rotation = Quaternion.Euler(xRotation, transform.eulerAngles.y, 0f);
    }
}