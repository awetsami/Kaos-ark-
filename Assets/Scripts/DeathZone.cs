using UnityEngine;
using FishNet.Object;

public class DeathZone : NetworkBehaviour
{
    public Transform casinoReturnPoint;

    [Server]
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>() != null)
        {
            // Fiziği ve hızı sıfırla ki kumarhanede uçmasın
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // EĞER oyun hala aktifse (Kasa parayı çekemeden düşmüşse) = KAYBETTİ
            if (ArenaManager.Instance != null && ArenaManager.Instance.isGameActive.Value)
            {
                Debug.Log("<color=red>Gladyatör düştü! Bahis kaybedildi.</color>");
                ArenaManager.Instance.EndGameServer(false);
            }

            // Oyun bitmişse (Kaybettiği için VEYA Kasa parayı çekip arenayı sildiği için) güvenlice ışınla
            if (casinoReturnPoint != null)
            {
                collision.gameObject.transform.position = casinoReturnPoint.position;
            }
            else
            {
                collision.gameObject.transform.position = new Vector3(0, 2f, 0);
            }
        }
    }
}