using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;

public class PlayerRaycast : NetworkBehaviour
{
    public float interactDistance = 5f;
    private Camera playerCamera;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner) return;
        playerCamera = Camera.main;
    }

    void Update()
    {
        if (!base.IsOwner || playerCamera == null) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                // 1. İHTİMAL: PENGUEN MAKİNESİ BUTONLARI (CasinoButton)
                if (hit.collider.TryGetComponent(out CasinoButton casinoBtn))
                {
                    if (ArenaManager.Instance != null)
                    {
                        // BUTONA TIKLANDIĞINDA GÖRSEL ANİMASYONU BAŞLAT!
                        casinoBtn.AnimatePress();

                        switch (casinoBtn.type)
                        {
                            case CasinoButton.ButtonType.Digit:
                                ArenaManager.Instance.AppendBetDigitServer(casinoBtn.value);
                                break;
                            case CasinoButton.ButtonType.Clear:
                                ArenaManager.Instance.ClearBetServer();
                                break;
                            case CasinoButton.ButtonType.Backspace:
                                ArenaManager.Instance.BackspaceBetServer();
                                break;
                            case CasinoButton.ButtonType.Multiplier:
                                ArenaManager.Instance.SetMultiplierServer(casinoBtn.value);
                                break;
                            case CasinoButton.ButtonType.Confirm:
                                ArenaManager.Instance.ConfirmAndStartGameServer();
                                break;
                            case CasinoButton.ButtonType.CashOut:
                                ArenaManager.Instance.CashOutServer();
                                break;
                            case CasinoButton.ButtonType.TestWin:
                                ArenaManager.Instance.EndGameServer(true);
                                break;
                            case CasinoButton.ButtonType.TestLose:
                                ArenaManager.Instance.EndGameServer(false);
                                break;
                        }
                    }
                }

                // 2. İHTİMAL: AVIATOR MAKİNESİ BUTONLARI (AviatorButton)
                else if (hit.collider.TryGetComponent(out AviatorButton aviatorBtn))
                {
                    // Aviator butonları basılma animasyonunu ve Server iletişimini kendi içindeki Interact() fonksiyonunda çözer.
                    aviatorBtn.Interact();
                }
            }
        }
    }
}