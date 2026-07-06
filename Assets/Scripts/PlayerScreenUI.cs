using UnityEngine;
using TMPro;

public class PlayerScreenUI : MonoBehaviour
{
    public TextMeshProUGUI walletText; // Sağ üst köşedeki metni buraya bağla

    void Update()
    {
        if (ArenaManager.Instance == null) return;

        // Dünyadaki herkes için ortak olan parayı gösterir
        walletText.text = $"<color=#66FF66>ORTAK KASA:</color> {ArenaManager.Instance.playerMoney.Value}$";
    }
}