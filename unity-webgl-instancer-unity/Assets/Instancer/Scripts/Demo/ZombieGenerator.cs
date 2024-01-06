using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;

public class ZombieGenerator : MonoBehaviour
{
    public static ZombieGenerator instance { get; private set; }

    [SerializeField] private GameObject _saveManager;
    private int _killCount = 0;

    private const int MAX_ZOMBIE_COUNT = 1000;

    [SerializeField] private GameObject _zombiePrefab;
    private List<ZombieBehaviour> _zombiesPool = new List<ZombieBehaviour>(MAX_ZOMBIE_COUNT);

    private const float SPAWN_TIME = 0.03f;
    private const float SPAWN_RADIUS = 30f;

    private float _lastSpawnTime = 0f;

    private Delegate _customEvHandler;

    private void Awake()
    {
        instance = this;
        _customEvHandler = VisualScriptingUtility.AddCustomEventListener(gameObject, HandleCustomEvent);
    }
    private void OnDestroy()
    {
        VisualScriptingUtility.RemoveCustomEventListener(gameObject, _customEvHandler);
    }
    private void HandleCustomEvent(string message, object[] args)
    {
        switch (message)
        {
            case "Initialized":
                UpdateKillCount(addCount: (int)args[0]); // current score + loaded scores
                break;
            default:
                Debug.LogWarning("received unknown message: " + message);
                break;
        }
    }

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
            SpawnZombie(20);
        }
    }

    private void SpawnZombie(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float theta = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
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

    public static void AddKill()
    {
        instance.UpdateKillCount(1);
    }
    private void UpdateKillCount(int addCount)
    {
        _killCount += addCount;
        UIManager.UpdateKillCount(_killCount);
        VisualScriptingUtility.TriggerCustomEvent(_saveManager, "SaveScore", new object[] { _killCount });
    }
}
