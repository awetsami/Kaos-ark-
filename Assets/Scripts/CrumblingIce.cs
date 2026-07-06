using UnityEngine;
using FishNet.Object;
using System.Collections;

public class CrumblingIce : NetworkBehaviour
{
    [Header("Kırılma Ayarları")]
    public float breakTime = 5f;
    public float withdrawUnlockTime = 2f; // Kaç saniye durunca para çekilebilir olacak

    private bool isTriggered = false;

    [Server]
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>() != null)
        {
            if (!isTriggered)
            {
                isTriggered = true;
                StartCoroutine(BreakRoutine());
            }
        }
    }

    // YENİ: Gladyatör buzdan atladığı an Kasa'nın para çekmesini kilitler
    [Server]
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>() != null)
        {
            if (ArenaManager.Instance != null && ArenaManager.Instance.isGameActive.Value)
            {
                ArenaManager.Instance.canCashOut.Value = false;
            }
        }
    }

    private IEnumerator BreakRoutine()
    {
        // 2 saniye bekle
        yield return new WaitForSeconds(withdrawUnlockTime);

        // Parayı çekilebilir hale getir
        if (ArenaManager.Instance != null)
        {
            ArenaManager.Instance.BlockBecameWithdrawableServer();
        }

        // Kalan süreyi bekle (Örn: 5 - 2 = 3 saniye daha)
        yield return new WaitForSeconds(breakTime - withdrawUnlockTime);

        // Buz kırılınca da para çekmeyi kilitler
        if (ArenaManager.Instance != null)
        {
            ArenaManager.Instance.canCashOut.Value = false;
        }

        RpcDisableBlock();
    }

    [ObserversRpc]
    private void RpcDisableBlock()
    {
        gameObject.SetActive(false);
    }
}