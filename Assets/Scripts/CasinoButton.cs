using UnityEngine;
using System.Collections;

public class CasinoButton : MonoBehaviour
{
    public enum ButtonType { Digit, Clear, Backspace, Multiplier, Confirm, TestWin, TestLose, CashOut }

    [Header("Butonun Görevi")]
    public ButtonType type;
    public int value;

    [Header("Görsel Geri Bildirim (Animasyon)")]
    // Butonun kendi lokal ekseninde ne kadar içeri göçeceği. Y: -0.05 genelde masadaki butonlar için harikadır. 
    // Duvara asarsan ve Z ekseninde hareket etmesini istersen bunu (0, 0, -0.05) yapabilirsin.
    public Vector3 pressDirection = new Vector3(0, -0.05f, 0);
    public float pressSpeed = 0.15f; // İnip çıkma hızı

    private Vector3 originalLocalPos;
    private bool isAnimating = false;

    void Awake()
    {
        // Oyun başladığında butonun orijinal konumunu hafızaya alıyoruz
        originalLocalPos = transform.localPosition;
    }

    // Lazer çarptığında PlayerRaycast bu fonksiyonu tetikleyecek
    public void AnimatePress()
    {
        if (!isAnimating)
        {
            StartCoroutine(PressRoutine());
        }
    }

    private IEnumerator PressRoutine()
    {
        isAnimating = true;
        Vector3 targetPos = originalLocalPos + pressDirection;

        // Aşağı / İçeri çökme
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / (pressSpeed / 2);
            transform.localPosition = Vector3.Lerp(originalLocalPos, targetPos, t);
            yield return null; // Bir sonraki frame'i bekle
        }

        // Yukarı / Dışarı çıkma
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / (pressSpeed / 2);
            transform.localPosition = Vector3.Lerp(targetPos, originalLocalPos, t);
            yield return null;
        }

        // Pozisyonu kesin olarak sıfırla ki kayma olmasın
        transform.localPosition = originalLocalPos;
        isAnimating = false;
    }
}