using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Normal Hareket Ayarları")]
    public float moveForce = 60f;
    public float maxSpeed = 8f;
    public float jumpForce = 7.5f;
    public float gravityMultiplier = 2.5f;

    [Header("Sinsice Yürüme & Ani Fren Ayarları")]
    public float sneakForce = 20f;       // Ctrl basılıyken itme gücü (Çok daha az)
    public float sneakMaxSpeed = 2.5f;   // Ctrl basılıyken ulaşabileceği maks hız
    public float brakeSharpness = 12f;   // İnerji/Kaymayı ne kadar keskin yutacağı (Yüksek değer = Yere çakılma)

    private Rigidbody rb;
    private Collider col;
    private bool isGrounded;
    private Vector3 moveDirection;
    private bool jumpPushed;
    private bool isSneaking;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (!base.IsOwner) return;

        isGrounded = CheckGrounded();

        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
            if (Keyboard.current.dKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed) moveX -= 1f;

            // SOL CTRL tuşuna basılıp basılmadığını kontrol ediyoruz
            isSneaking = Keyboard.current.leftCtrlKey.isPressed;

            // Sinsice yürürken (Ctrl basılıyken) zıplamayı engelliyoruz (Minecraft mantığı)
            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isSneaking)
            {
                jumpPushed = true;
            }
        }

        moveDirection = (transform.right * moveX + transform.forward * moveZ).normalized;
    }

    void FixedUpdate()
    {
        if (!base.IsOwner) return;

        if (jumpPushed)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            jumpPushed = false;
        }

        // Değerleri Ctrl tuşunun durumuna göre dinamik olarak seçiyoruz
        float activeForce = isSneaking ? sneakForce : moveForce;
        float activeMaxSpeed = isSneaking ? sneakMaxSpeed : maxSpeed;

        // ANİ FREN MEKANİĞİ: Eğer oyuncu yerdeyse ve Ctrl'ye basıyorsa, mevcut kayma hızını anında sönümlüyoruz
        if (isSneaking && isGrounded)
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            // Mevcut hızı sıfıra doğru çok keskin bir şekilde çeker (Yere çakılma hissi)
            flatVel = Vector3.Lerp(flatVel, Vector3.zero, Time.fixedDeltaTime * brakeSharpness);
            rb.linearVelocity = new Vector3(flatVel.x, rb.linearVelocity.y, flatVel.z);
        }

        // Hareketi uygula
        rb.AddForce(moveDirection * activeForce, ForceMode.Acceleration);

        // Hız Sınırlayıcı (Seçilen moda göre: 8f veya 2.5f)
        Vector3 currentFlatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (currentFlatVel.magnitude > activeMaxSpeed)
        {
            Vector3 limitedVel = currentFlatVel.normalized * activeMaxSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }

        // Tok Düşüş
        rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
    }

    private bool CheckGrounded()
    {
        Vector3 checkPosition = new Vector3(col.bounds.center.x, col.bounds.min.y + 0.05f, col.bounds.center.z);
        Collider[] colliders = Physics.OverlapSphere(checkPosition, 0.1f);

        foreach (var c in colliders)
        {
            if (c.GetComponentInParent<PlayerMovement>() == null && !c.isTrigger)
            {
                return true;
            }
        }
        return false;
    }
}