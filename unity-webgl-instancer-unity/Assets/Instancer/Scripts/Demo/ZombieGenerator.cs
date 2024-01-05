using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieGenerator : MonoBehaviour
{
    private const int MAX_ZOMBIE_COUNT = 1000;

    [SerializeField] private GameObject _zombiePrefab;
    private List<ZombieBehaviour> _zombiesPool = new List<ZombieBehaviour>(MAX_ZOMBIE_COUNT);

    private const float SPAWN_TIME = 0.03f;
    private const float SPAWN_RADIUS = 30f;

    private float _lastSpawnTime = 0f;

    private void Start()
    {
        for (int i = 0; i < MAX_ZOMBIE_COUNT; i++)
        {
            GameObject zombie = Instantiate(_zombiePrefab, transform);
            zombie.SetActive(false);
            _zombiesPool.Add(zombie.GetComponent<ZombieBehaviour>());
        }
        SpawnZombie(100);
    }

    private void Update()
    {
        if (Time.time - _lastSpawnTime > SPAWN_TIME)
        {
            _lastSpawnTime = Time.time;
            SpawnZombie(10);
        }
    }

    private void SpawnZombie(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float theta = Random.Range(0f, Mathf.PI * 2f);
            Vector3 spawnPosition = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)) * SPAWN_RADIUS;
            spawnPosition += Player.position;

            ZombieBehaviour zombie = _zombiesPool.Find(z => !z.gameObject.activeSelf);
            // if (zombie == null)
            // {
            //     GameObject zombieObject = Instantiate(_zombiePrefab, spawnPosition, Quaternion.identity, transform);
            //     zombie = zombieObject.GetComponent<ZombieBehaviour>();
            //     _zombiesPool.Add(zombie);
            // }

            if (zombie != null)
            {
                zombie.Spawn(spawnPosition);
                zombie.gameObject.SetActive(true);
            }
        }
    }
}
