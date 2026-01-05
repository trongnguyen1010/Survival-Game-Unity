using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject enemyPrefab;
        public float spawnTimer;
        public float spawnInterval;
        public int enemiesPerWave;
        public int spawnedEnemyCount;

    }

    public List<Wave> waves;
    public int waveNubmer;
    public Transform minPos;
    public Transform maxPos;

    // Update is called once per frame
    void Update()
    {

        if (PlayerController.Instance.gameObject.activeSelf)
        {
            waves[waveNubmer].spawnTimer += Time.deltaTime;
            if (waves[waveNubmer].spawnTimer>= waves[waveNubmer].spawnInterval)
            {
                waves[waveNubmer].spawnTimer = 0;
                SpawnEnemy();
            }
            if (waves[waveNubmer].spawnedEnemyCount >= waves[waveNubmer].enemiesPerWave)
            {
                waves[waveNubmer].spawnedEnemyCount = 0;
                if (waves[waveNubmer].spawnInterval > 0.15f)
                {
                    waves[waveNubmer].spawnInterval *= 0.8f;
                }
                waveNubmer++;
            }
            if(waveNubmer >= waves.Count)
            {
                waveNubmer=0;
            }
        }
    }

    private void SpawnEnemy()
    {
        Instantiate(waves[waveNubmer].enemyPrefab, RandomSpawnPoint(), transform.rotation);
        waves[waveNubmer].spawnedEnemyCount++;
    }

    private Vector2 RandomSpawnPoint()
    {
        Vector2 spawnPoint;
        if (Random.Range(0f, 1f) > 0.5)
        {
            spawnPoint.x = Random.Range(minPos.position.x, maxPos.position.x);
            if (Random.Range(0f, 1f) > 0.5)
            {
                spawnPoint.y = minPos.position.y;
            }
            else{
                spawnPoint.y = maxPos.position.y;
            }
        }else{
            spawnPoint.y = Random.Range(minPos.position.y, maxPos.position.y);
            if (Random.Range(0f, 1f) > 0.5)
            {
                spawnPoint.x = minPos.position.x;
            }else{
                spawnPoint.x = maxPos.position.x;
            }
        }

        return spawnPoint;
    }
}
