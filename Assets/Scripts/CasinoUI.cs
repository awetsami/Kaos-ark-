using UnityEngine;
using TMPro;

public class CasinoUI : MonoBehaviour
{
    public TextMeshProUGUI statsText;

    void Update()
    {
        if (ArenaManager.Instance == null) return;

        // Ekrana crash oyunlarındaki gibi anlık durumu yazdırıyoruz
        string status = ArenaManager.Instance.isGameActive.Value
            ? (ArenaManager.Instance.canCashOut.Value ? "<color=green>PARAYI ÇEK (CASH OUT) AKTİF!</color>" : "<color=red>Buzda Bekleniyor...</color>")
            : "Oyun Beklemede";

        statsText.text = $"CÜZDANIZ: {ArenaManager.Instance.playerMoney.Value}$\n" +
                         $"GİRİLEN BAHİS: {ArenaManager.Instance.currentBet.Value}$\n" +
                         $"HEDEF ÇARPAN ZORLUĞU: {ArenaManager.Instance.targetMultiplier.Value}x\n" +
                         $"---------------------\n" +
                         $"ANLIK ÇARPAN: {ArenaManager.Instance.currentMultiplier.Value}x\n" +
                         $"BİRİKEN ÖDÜL: {ArenaManager.Instance.currentReward.Value}$\n" +
                         $"DURUM: {status}";
    }
}