using UnityEngine;
using TMPro;

public class AviatorMachineUI : MonoBehaviour
{
    public TextMeshProUGUI aviatorStatsText;

    void Update()
    {
        if (AviatorManager.Instance == null) return;

        if (!AviatorManager.Instance.isFlightActive.Value)
        {
            aviatorStatsText.text = $"<align=center><size=130%><color=#FFCC00>AVIATOR SYSTEM</color></size></align>\n\n" +
                                     $"GİRİLEN BAHİS: {AviatorManager.Instance.currentBet.Value}$\n" +
                                     $"<size=80%><color=white>BAHİS GİRİN VE START'A BASIN</color></size>";
        }
        else
        {
            int liveReward = Mathf.FloorToInt(AviatorManager.Instance.currentBet.Value * AviatorManager.Instance.currentMultiplier.Value);

            aviatorStatsText.text = $"<align=center><size=140%><color=#33FF33>UÇAK HAVADA!</color></size></align>\n\n" +
                                     $"<align=center><size=180%>{AviatorManager.Instance.currentMultiplier.Value.ToString("F2")}x</size></align>\n" +
                                     $"ANLIK KAZANÇ: {liveReward}$";
        }
    }
}