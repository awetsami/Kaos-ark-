using UnityEngine;
using FishNet.Object;
using System.Collections;

public class AviatorButton : NetworkBehaviour
{
    public enum AviatorButtonType { Digit, Clear, Backspace, Start, CashOut }

    [Header("Buton Ayarları")]
    public AviatorButtonType buttonType;
    public int digitValue;

    private bool isPressing = false;
    private Vector3 originalLocalPos;

    void Start()
    {
        originalLocalPos = transform.localPosition;
    }

    public void Interact()
    {
        if (isPressing) return;

        StartCoroutine(PressAnimation());

        if (AviatorManager.Instance == null) return;

        switch (buttonType)
        {
            case AviatorButtonType.Digit: AviatorManager.Instance.AppendBetDigitServer(digitValue); break;
            case AviatorButtonType.Clear: AviatorManager.Instance.ClearBetServer(); break;
            case AviatorButtonType.Backspace: AviatorManager.Instance.BackspaceBetServer(); break;
            case AviatorButtonType.Start: AviatorManager.Instance.StartFlightServer(); break;
            case AviatorButtonType.CashOut: AviatorManager.Instance.CashOutServer(); break;
        }
    }

    private IEnumerator PressAnimation()
    {
        isPressing = true;
        transform.localPosition = originalLocalPos + new Vector3(0, 0, 0.05f);
        yield return new WaitForSeconds(0.15f);
        transform.localPosition = originalLocalPos;
        isPressing = false;
    }
}