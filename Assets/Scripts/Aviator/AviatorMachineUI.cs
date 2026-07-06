using UnityEngine;
using TMPro;

public class AviatorMachineUI : MonoBehaviour
{
    public TextMeshProUGUI aviatorStatsText;

    void Update()
    {
        if (AviatorManager.Instance == null) return;

        // 1. DURUM: GERİ SAYIM AKTİFSE
        if (AviatorManager.Instance.isCountingDown.Value)
        {
            aviatorStatsText.text = $"<align=center><size=150%><color=#FFA500>UÇUŞA HAZIRLAN</color></size></align>\n\n" +
                                    $"<align=center><size=250%>{AviatorManager.Instance.countdownTimer.Value}</size></align>";
        }
        // 2. DURUM: BEKLEME MODUNDAYSA (Oyun başlamadıysa)
        else if (!AviatorManager.Instance.isFlightActive.Value)
        {
            aviatorStatsText.text = $"<align=center><size=130%><color=#FFCC00>AVIATOR SYSTEM</color></size></align>\n\n" +
                                     $"GİRİLEN BAHİS: {AviatorManager.Instance.currentBet.Value}$\n" +
                                     $"<size=80%><color=white>BAHİS GİRİN VE START'A BASIN</color></size>";
        }
        // 3. DURUM: UÇUŞ BAŞLADIYSA (Havadaysak)
        else
        {
            int liveReward = Mathf.FloorToInt(AviatorManager.Instance.currentBet.Value * AviatorManager.Instance.currentMultiplier.Value);

            aviatorStatsText.text = $"<align=center><size=140%><color=#33FF33>UÇAK HAVADA!</color></size></align>\n\n" +
                                     $"<align=center><size=180%>{AviatorManager.Instance.currentMultiplier.Value.ToString("F2")}x</size></align>\n" +
                                     $"ANLIK KAZANÇ: {liveReward}$";
        }
    }
}