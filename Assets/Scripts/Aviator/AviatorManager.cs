using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;

public class AviatorManager : NetworkBehaviour
{
    public static AviatorManager Instance;

    [Header("Oyun Durumu")]
    public readonly SyncVar<bool> isFlightActive = new SyncVar<bool>(false);
    public readonly SyncVar<float> currentMultiplier = new SyncVar<float>(1.00f);
    public readonly SyncVar<int> currentBet = new SyncVar<int>(0);

    [Header("Çarpan Hız Ayarları (Inspector'dan Ayarla)")]
    [Tooltip("W tuşuna basarken çarpan saniyede ne kadar artsın? (Örn: 0.4)")]
    public float multiplierIncreaseSpeed = 0.4f;
    [Tooltip("S tuşuna basarken çarpan saniyede ne kadar düşsün? (Örn: 0.08)")]
    public float multiplierDecreaseSpeed = 0.08f;

    private string inputBetString = "";

    [Header("Simülasyon Bağlantıları")]
    public Transform pilotSpawnPoint;
    public Transform casinoReturnPoint;

    [Tooltip("Sahnede uçan Aviator_Plane objesi")]
    public NetworkObject aviatorPlaneObject;

    private NetworkObject activePilotNetworkObject;

    void Awake()
    {
        Instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void HandleFlightInputServer(float inputY)
    {
        if (!isFlightActive.Value) return;

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
        if (isFlightActive.Value || currentBet.Value <= 0 || conn == null) return;

        if (ArenaManager.Instance != null)
        {
            if (ArenaManager.Instance.playerMoney.Value < currentBet.Value) return;
            // BAŞLANGIÇTA PARAYI KASADAN DÜŞÜYORUZ (Risk alındı)
            ArenaManager.Instance.playerMoney.Value -= currentBet.Value;
        }

        currentMultiplier.Value = 1.00f;
        isFlightActive.Value = true;
        activePilotNetworkObject = conn.FirstObject;

        if (aviatorPlaneObject != null)
        {
            aviatorPlaneObject.GiveOwnership(conn);
        }

        TargetSetFlightMode(conn, activePilotNetworkObject, pilotSpawnPoint.position, true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CashOutServer()
    {
        if (!isFlightActive.Value) return;

        // DIŞARIDAKİ OYUNCU ZAMANINDA ÇEKERSE KAZANÇ EKLENİR
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

        // PİLOT ÇARPARSA HİÇBİR ŞEY EKLENMEZ, PARA YANAR!
        EndFlightServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndFlightServer()
    {
        isFlightActive.Value = false;
        currentBet.Value = 0;
        inputBetString = "";

        if (aviatorPlaneObject != null && aviatorPlaneObject.Owner != null)
        {
            aviatorPlaneObject.RemoveOwnership();
        }

        if (activePilotNetworkObject != null)
        {
            TargetSetFlightMode(activePilotNetworkObject.Owner, activePilotNetworkObject, casinoReturnPoint != null ? casinoReturnPoint.position : Vector3.zero, false);
            activePilotNetworkObject = null;
        }

        if (aviatorPlaneObject != null)
        {
            aviatorPlaneObject.transform.position = new Vector3(aviatorPlaneObject.transform.position.x, 0, aviatorPlaneObject.transform.position.z);
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
            playerObj.transform.SetParent(null);

            if (rb != null) { rb.isKinematic = false; rb.linearVelocity = Vector3.zero; }
            if (pm != null) pm.enabled = true;

            if (cockpitCam != null) cockpitCam.SetActive(false);
            if (playerFPSCam != null) playerFPSCam.gameObject.SetActive(true);

            playerObj.transform.position = targetPos;
        }
    }

    [ServerRpc(RequireOwnership = false)] public void AppendBetDigitServer(int digit) { if (isFlightActive.Value) return; inputBetString += digit.ToString(); if (int.TryParse(inputBetString, out int p)) currentBet.Value = p; }
    [ServerRpc(RequireOwnership = false)] public void BackspaceBetServer() { if (isFlightActive.Value || string.IsNullOrEmpty(inputBetString)) return; inputBetString = inputBetString.Substring(0, inputBetString.Length - 1); currentBet.Value = inputBetString == "" ? 0 : int.Parse(inputBetString); }
    [ServerRpc(RequireOwnership = false)] public void ClearBetServer() { if (isFlightActive.Value) return; inputBetString = ""; currentBet.Value = 0; }
}