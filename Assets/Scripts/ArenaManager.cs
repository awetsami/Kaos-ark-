using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;

public class ArenaManager : NetworkBehaviour
{
    public static ArenaManager Instance;

    [Header("Ekonomi ve Bahis Ayarları")]
    public readonly SyncVar<int> playerMoney = new SyncVar<int>(1000);
    public readonly SyncVar<int> currentBet = new SyncVar<int>(0);
    public readonly SyncVar<int> targetMultiplier = new SyncVar<int>(2);
    public readonly SyncVar<int> difficultyLevel = new SyncVar<int>(1);
    public readonly SyncVar<bool> isGameActive = new SyncVar<bool>(false);

    [Header("Canlı Maç Verileri (Crash)")]
    public readonly SyncVar<int> currentMultiplier = new SyncVar<int>(0);
    public readonly SyncVar<int> currentReward = new SyncVar<int>(0);
    public readonly SyncVar<bool> canCashOut = new SyncVar<bool>(false);

    private string inputBetString = "";

    [Header("Arena Spawn Ayarları")]
    public Transform arenaSpawnPoint;
    public Transform playerArenaSpawnPoint;
    public GameObject[] difficultyArenas;
    private GameObject currentActiveArena;

    void Awake()
    {
        Instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AppendBetDigitServer(int digit)
    {
        if (isGameActive.Value) return;

        if (inputBetString == "0") inputBetString = "";
        inputBetString += digit.ToString();

        if (int.TryParse(inputBetString, out int parsedBet))
        {
            if (parsedBet > playerMoney.Value)
            {
                parsedBet = playerMoney.Value;
                inputBetString = parsedBet.ToString();
            }
            currentBet.Value = parsedBet;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BackspaceBetServer()
    {
        if (isGameActive.Value || string.IsNullOrEmpty(inputBetString)) return;

        inputBetString = inputBetString.Substring(0, inputBetString.Length - 1);

        if (inputBetString == "") currentBet.Value = 0;
        else if (int.TryParse(inputBetString, out int parsedBet)) currentBet.Value = parsedBet;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClearBetServer()
    {
        if (isGameActive.Value) return;
        inputBetString = "";
        currentBet.Value = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetMultiplierServer(int multiplier)
    {
        if (isGameActive.Value) return;

        if (targetMultiplier.Value == multiplier && currentActiveArena != null) return;

        targetMultiplier.Value = multiplier;

        switch (multiplier)
        {
            case 2: difficultyLevel.Value = 1; break;
            case 3: difficultyLevel.Value = 2; break;
            case 4: difficultyLevel.Value = 3; break;
            case 5: difficultyLevel.Value = 4; break;
            case 7: difficultyLevel.Value = 5; break;
            case 10: difficultyLevel.Value = 6; break;
            case 15: difficultyLevel.Value = 7; break;
        }

        SpawnArenaByDifficulty();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ConfirmAndStartGameServer(NetworkConnection conn = null)
    {
        if (isGameActive.Value || currentBet.Value <= 0) return;

        if (currentActiveArena == null)
        {
            SpawnArenaByDifficulty();
        }

        playerMoney.Value -= currentBet.Value;
        currentMultiplier.Value = 0;
        currentReward.Value = 0;
        canCashOut.Value = false;
        isGameActive.Value = true;

        if (conn != null && conn.FirstObject != null)
        {
            PlayerMovement pressingPlayer = conn.FirstObject.GetComponent<PlayerMovement>();
            if (pressingPlayer != null && playerArenaSpawnPoint != null)
            {
                pressingPlayer.transform.position = playerArenaSpawnPoint.position;

                Rigidbody rb = pressingPlayer.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // YENİ SİSTEM: linearVelocity sıfırlama
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BlockBecameWithdrawableServer()
    {
        if (!isGameActive.Value) return;

        if (currentMultiplier.Value < targetMultiplier.Value)
        {
            currentMultiplier.Value++;
            currentReward.Value = currentBet.Value * currentMultiplier.Value;
            canCashOut.Value = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CashOutServer()
    {
        if (!isGameActive.Value || !canCashOut.Value) return;

        playerMoney.Value += currentReward.Value;
        EndGameServer(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndGameServer(bool gladyatorKazandi)
    {
        if (!isGameActive.Value) return;

        currentBet.Value = 0;
        inputBetString = "";
        currentMultiplier.Value = 0;
        currentReward.Value = 0;
        canCashOut.Value = false;
        isGameActive.Value = false;

        SpawnArenaByDifficulty();
    }

    private void SpawnArenaByDifficulty()
    {
        if (currentActiveArena != null)
        {
            ServerManager.Despawn(currentActiveArena);
            Destroy(currentActiveArena);
        }

        int index = difficultyLevel.Value - 1;
        if (difficultyArenas.Length > index && difficultyArenas[index] != null)
        {
            GameObject prefabToSpawn = difficultyArenas[index];
            currentActiveArena = Instantiate(prefabToSpawn, arenaSpawnPoint.position, Quaternion.identity);
            currentActiveArena.transform.localScale = prefabToSpawn.transform.localScale;
            ServerManager.Spawn(currentActiveArena);
        }
    }
}