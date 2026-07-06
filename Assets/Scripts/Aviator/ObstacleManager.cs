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

    void Awake()
    {
        obstaclePool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(obstaclePrefab);
            obj.transform.SetParent(this.transform);
            obj.SetActive(false);
            obstaclePool.Add(obj);
        }
    }

    void Update()
    {
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
            rock.SetActive(true);

            base.Spawn(rock);
        }
    }

    private GameObject GetPooledObstacle()
    {
        for (int i = 0; i < obstaclePool.Count; i++)
        {
            if (!obstaclePool[i].activeInHierarchy) return obstaclePool[i];
        }
        return null;
    }
}