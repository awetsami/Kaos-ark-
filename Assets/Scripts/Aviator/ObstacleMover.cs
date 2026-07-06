using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public float currentMoveSpeed = 40f;
    private float deactivationZ = -20f;

    void Update()
    {
        transform.Translate(Vector3.back * currentMoveSpeed * Time.deltaTime, Space.World);

        if (transform.position.z < deactivationZ)
        {
            gameObject.SetActive(false);
        }
    }

    // UÇAKLA ÇARPIŞMA SENSÖRÜ
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlaneController>() != null)
        {
            if (AviatorManager.Instance != null && AviatorManager.Instance.isFlightActive.Value)
            {
                // Çarpışma anında sunucuya oyunu sonlandır komutu gönder (Bahis yanar)
                AviatorManager.Instance.CrashServer();
            }
        }
    }
}