using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;

public class ObstacleManager : NetworkBehaviour
{
    [Header("Pool (Havuz) Ayarları")]
    public GameObject obstaclePrefab;
    public int poolSize = 30;
    private List<GameObject> obstaclePool;

    [Header("Dinamik Hızlanma Ayarları")]
    public float startSpawnInterval = 3.5f;
    public float minSpawnInterval = 0.6f;
    private float timer = 0f;

    public float minY = -3.0f;
    public float maxY = 3.0f;

    [Header("Müfettiş (Inspector) Boyut Ayarları")]
    public float rockWidth = 2.0f;
    public float rockHeight = 1.0f;

    private float lastSpawnY = 0f;

    // Sadece sunucu ayaklandığında havuzu oluşturur (Hayalet obje oluşumunu engeller)
    public override void OnStartServer()
    {
        base.OnStartServer();
        obstaclePool = new List<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(obstaclePrefab, this.transform);

            // Objeyi ağa sok ve anında uykuya al (Havuza gönder)
            // false parametresi objenin tamamen silinmesini engeller, yeniden kullanıma hazır bekletir
            ServerManager.Spawn(obj);
            ServerManager.Despawn(obj, DespawnType.Pool);

            obstaclePool.Add(obj);
        }
    }

    void Update()
    {
        // Güvenlik Kilidi: Sadece sunucu zamanı sayıp kaya üretebilir
        if (!IsServerInitialized) return;

        if (AviatorManager.Instance == null || !AviatorManager.Instance.isFlightActive.Value) return;

        float currentMultiplier = AviatorManager.Instance.currentMultiplier.Value;
        float currentInterval = Mathf.Clamp(startSpawnInterval - (currentMultiplier * 0.3f), minSpawnInterval, startSpawnInterval);

        timer += Time.deltaTime;

        if (timer >= currentInterval)
        {
            SpawnSingleObstacle();
            timer = 0f;
        }
    }

    private void SpawnSingleObstacle()
    {
        GameObject rock = GetPooledObstacle();

        if (rock != null)
        {
            rock.transform.localScale = new Vector3(rockWidth, rockHeight, 1f);

            float spawnY = (lastSpawnY <= 0f) ? maxY : minY;
            lastSpawnY = spawnY;
            float adjustedSpawnY = (spawnY == maxY) ? maxY - (rockHeight / 2f) : minY + (rockHeight / 2f);

            rock.transform.position = this.transform.position + new Vector3(0, adjustedSpawnY, 0);

            // Havuzdan çekilen kayayı ağda görünür hale getir (Kameralar artık bunu görebilir)
            ServerManager.Spawn(rock);
        }
    }

    private GameObject GetPooledObstacle()
    {
        for (int i = 0; i < obstaclePool.Count; i++)
        {
            NetworkObject nob = obstaclePool[i].GetComponent<NetworkObject>();
            // Eğer obje ağda aktif değilse (havuzda uyuyorsa) onu seç
            if (nob != null && !nob.IsSpawned)
            {
                return obstaclePool[i];
            }
        }
        return null;
    }
}