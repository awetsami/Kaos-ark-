using UnityEngine;
using TMPro;

public class MachineUI : MonoBehaviour
{
    public TextMeshProUGUI machineStatsText; // Makinenin üzerindeki TMPro objesini bağla

    void Update()
    {
        if (ArenaManager.Instance == null) return;

        if (!ArenaManager.Instance.isGameActive.Value)
        {
            // Oyun başlamadan önceki sade görünüm
            machineStatsText.text = $"<align=center><size=120%>BRETING PANEL</size></align>\n\n" +
                                     $"GİRİLEN BAHİS: {ArenaManager.Instance.currentBet.Value}$\n" +
                                     $"HEDEF ZORLUK: {ArenaManager.Instance.targetMultiplier.Value}x";
        }
        else
        {
            // Oyun aktifken canlı Crash / Aviator takibi
            string cashOutStatus = ArenaManager.Instance.canCashOut.Value
                ? "<color=green>CASH OUT AKTİF!</color>"
                : "<color=red>Buzda Bekleniyor...</color>";

            machineStatsText.text = $"<align=center><size=120%>CANLI ARENA</size></align>\n\n" +
                                     $"ANLIK ÇARPAN: {ArenaManager.Instance.currentMultiplier.Value}x\n" +
                                     $"BİRİKEN ÖDÜL: {ArenaManager.Instance.currentReward.Value}$\n" +
                                     $"DURUM: {cashOutStatus}";
        }
    }
}