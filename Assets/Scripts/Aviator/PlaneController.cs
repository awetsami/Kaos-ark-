using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlaneController : NetworkBehaviour
{
    [Header("Uçuş Kuvvet Ayarları")]
    public float flySpeed = 6f;
    public float minY = -3.5f;
    public float maxY = 3.5f;

    [Header("Görsel Bağlantılar")]
    public GameObject cockpitCamera;

    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.freezeRotation = true;
        }

        if (cockpitCamera != null) cockpitCamera.SetActive(false);
    }

    void Update()
    {
        if (!base.IsOwner) return;
        if (AviatorManager.Instance == null || !AviatorManager.Instance.isFlightActive.Value) return;

        float inputY = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) inputY += 1f;
            if (Keyboard.current.sKey.isPressed) inputY -= 1f;
        }

        transform.localPosition += new Vector3(0f, inputY * flySpeed * Time.deltaTime, 0f);

        float clampedY = Mathf.Clamp(transform.localPosition.y, minY, maxY);
        transform.localPosition = new Vector3(transform.localPosition.x, clampedY, transform.localPosition.z);

        AviatorManager.Instance.HandleFlightInputServer(inputY);
    }
}