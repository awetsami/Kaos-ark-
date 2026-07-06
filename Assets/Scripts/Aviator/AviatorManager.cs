using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using System.Collections; // Coroutine (Geri sayım) için eklendi

public class AviatorManager : NetworkBehaviour
{
    public static AviatorManager Instance;

    [Header("Oyun Durumu")]
    public readonly SyncVar<bool> isFlightActive = new SyncVar<bool>(false);

    // YENİ: Geri Sayım Değişkenleri
    public readonly SyncVar<bool> isCountingDown = new SyncVar<bool>(false);
    public readonly SyncVar<int> countdownTimer = new SyncVar<int>(3);

    public readonly SyncVar<float> currentMultiplier = new SyncVar<float>(1.00f);
    public readonly SyncVar<int> currentBet = new SyncVar<int>(0);

    [Header("Çarpan Hız Ayarları")]
    public float multiplierIncreaseSpeed = 0.4f;
    public float multiplierDecreaseSpeed = 0.08f;

    private string inputBetString = "";

    [Header("Simülasyon Bağlantıları")]
    public Transform pilotSpawnPoint;
    public Transform casinoReturnPoint;

    public NetworkObject aviatorPlaneObject;
    private NetworkObject activePilotNetworkObject;

    // YENİ: Uçağın Tüneldeki Orijinal Pozisyonu
    private Vector3 originalPlaneLocalPos;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (aviatorPlaneObject != null)
        {
            originalPlaneLocalPos = aviatorPlaneObject.transform.localPosition;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void HandleFlightInputServer(float inputY)
    {
        // Geri sayım varken çarpanın değişmesini engelle
        if (!isFlightActive.Value || isCountingDown.Value) return;

        if (inputY > 0f)
        {
            currentMultiplier.Value += multiplierIncreaseSpeed * Time.deltaTime;
        }
        else if (inputY < 0f)
        {
            currentMultiplier.Value -= multiplierDecreaseSpeed * Time.deltaTime;
            if (currentMultiplier.Value < 1.00f) currentMultiplier.Value = 1.00f;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartFlightServer(NetworkConnection conn = null)
    {
        if (isFlightActive.Value || isCountingDown.Value || currentBet.Value <= 0 || conn == null) return;

        if (ArenaManager.Instance != null)
        {
            if (ArenaManager.Instance.playerMoney.Value < currentBet.Value) return;
            ArenaManager.Instance.playerMoney.Value -= currentBet.Value;
        }

        currentMultiplier.Value = 1.00f;
        activePilotNetworkObject = conn.FirstObject;

        if (aviatorPlaneObject != null)
        {
            aviatorPlaneObject.transform.localPosition = originalPlaneLocalPos;
            aviatorPlaneObject.GiveOwnership(conn);
        }

        TargetSetFlightMode(conn, activePilotNetworkObject, pilotSpawnPoint.position, true);

        // YENİ: 3 Saniyelik Geri Sayımı Başlat
        StartCoroutine(FlightCountdownRoutine());
    }

    private IEnumerator FlightCountdownRoutine()
    {
        isCountingDown.Value = true;
        countdownTimer.Value = 3;

        while (countdownTimer.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            countdownTimer.Value--;
        }

        isCountingDown.Value = false;
        isFlightActive.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void CashOutServer()
    {
        if (!isFlightActive.Value || isCountingDown.Value) return;

        int finalReward = Mathf.FloorToInt(currentBet.Value * currentMultiplier.Value);

        if (ArenaManager.Instance != null)
        {
            ArenaManager.Instance.playerMoney.Value += finalReward;
        }

        EndFlightServer();
    }

    [ServerRpc(RequireOwnership = false)]
    public void CrashServer()
    {
        if (!isFlightActive.Value) return;
        EndFlightServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndFlightServer()
    {
        isFlightActive.Value = false;
        isCountingDown.Value = false;
        currentBet.Value = 0;
        inputBetString = "";

        if (aviatorPlaneObject != null && aviatorPlaneObject.Owner != null)
        {
            aviatorPlaneObject.RemoveOwnership();
        }

        if (activePilotNetworkObject != null)
        {
            // ÖNEMLİ: Işınlamadan önce uçaktan koparıyoruz
            activePilotNetworkObject.transform.SetParent(null);

            TargetSetFlightMode(activePilotNetworkObject.Owner, activePilotNetworkObject, casinoReturnPoint != null ? casinoReturnPoint.position : Vector3.zero, false);
            activePilotNetworkObject = null;
        }

        if (aviatorPlaneObject != null)
        {
            // ÖNEMLİ: Uçağı global sıfıra değil, yerel orijinal noktasına gönderiyoruz
            aviatorPlaneObject.transform.localPosition = originalPlaneLocalPos;
        }
    }

    [TargetRpc]
    private void TargetSetFlightMode(NetworkConnection conn, NetworkObject playerObj, Vector3 targetPos, bool isFlying)
    {
        if (playerObj == null) return;

        Rigidbody rb = playerObj.GetComponent<Rigidbody>();
        PlayerMovement pm = playerObj.GetComponent<PlayerMovement>();
        Camera playerFPSCam = playerObj.GetComponentInChildren<Camera>(true);

        PlaneController plane = aviatorPlaneObject != null ? aviatorPlaneObject.GetComponent<PlaneController>() : null;
        GameObject cockpitCam = plane != null ? plane.cockpitCamera : null;

        if (isFlying)
        {
            if (rb != null) { rb.linearVelocity = Vector3.zero; rb.isKinematic = true; }
            if (pm != null) pm.enabled = false;

            if (playerFPSCam != null) playerFPSCam.gameObject.SetActive(false);
            if (cockpitCam != null) cockpitCam.SetActive(true);

            playerObj.transform.position = targetPos;
            if (aviatorPlaneObject != null) playerObj.transform.SetParent(aviatorPlaneObject.transform, true);
        }
        else
        {
            // YENİ: KARAKTERİ DİMDİK AYAĞA KALDIRMA (Hacıyatmaz Bug'ı Çözümü)
            playerObj.transform.rotation = Quaternion.Euler(0, 0, 0);

            if (rb != null) { rb.isKinematic = false; rb.linearVelocity = Vector3.zero; }
            if (pm != null) pm.enabled = true;

            if (cockpitCam != null) cockpitCam.SetActive(false);
            if (playerFPSCam != null) playerFPSCam.gameObject.SetActive(true);

            playerObj.transform.position = targetPos;
        }
    }

    [ServerRpc(RequireOwnership = false)] public void AppendBetDigitServer(int digit) { if (isFlightActive.Value || isCountingDown.Value) return; inputBetString += digit.ToString(); if (int.TryParse(inputBetString, out int p)) currentBet.Value = p; }
    [ServerRpc(RequireOwnership = false)] public void BackspaceBetServer() { if (isFlightActive.Value || isCountingDown.Value || string.IsNullOrEmpty(inputBetString)) return; inputBetString = inputBetString.Substring(0, inputBetString.Length - 1); currentBet.Value = inputBetString == "" ? 0 : int.Parse(inputBetString); }
    [ServerRpc(RequireOwnership = false)] public void ClearBetServer() { if (isFlightActive.Value || isCountingDown.Value) return; inputBetString = ""; currentBet.Value = 0; }
}