using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class BombManager : MonoBehaviour
{
    public static event Action<List<Vector3>> OnBombExploded;
    public static event Action<Vector3> PositionSpawnBomb;
    
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Tilemap tileMap1;
    [SerializeField] private Tilemap tileMap2;
    private float _timeToSpawn;
    
    private Transform _targetTileMap;
    private GameObject _bombClone;
    
    private void Start()
    {
        ScheduleNextSpawn();
    }

    private void ScheduleNextSpawn()
    {
        _timeToSpawn = Random.Range(60f, 70f);
        Invoke(nameof(SpawnBomb), _timeToSpawn); 
    }
    
    private void SpawnBomb()
    {
        int player1Score = PlayerController.Instance.score;
        int player2Score = BotController.Instance.score;
        
        float locationY = Random.Range(-10, -3);
        float locationX;
        _bombClone = Instantiate(bombPrefab);
        if (player1Score > player2Score)
        {
            locationX = Random.Range(4, 9);
            _targetTileMap = tileMap1.transform;
        }
        else
        {
            locationX = Random.Range(17, 22);
            _targetTileMap = tileMap2.transform;
        }

        Vector3 pos = new Vector3(locationX, locationY, transform.position.z);
        _bombClone.transform.position = pos;
        PositionSpawnBomb?.Invoke(pos);
        ScheduleNextSpawn();
    }
    
    private void OnEnable()
    {
        BombController.PositionBombExploded += HandleBombExplosion;
    }

    private void OnDisable()
    {
        BombController.PositionBombExploded -= HandleBombExplosion;
    }

    private void HandleBombExplosion(Vector3 pos, int target)
    {
        if (target == 1)
            _targetTileMap = tileMap1.transform;
        else
            _targetTileMap = tileMap2.transform;
        
        DestroyMap(pos);
    }

    private void DestroyMap(Vector3 pos)
    {
        float x = Mathf.FloorToInt(pos.x) + 0.5f;
        float y = Mathf.FloorToInt(pos.y) + 0.5f;
        
        List<Vector3>destroyedPositions = new List<Vector3>();
        
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Vector3 location = new Vector3(x + i, y + j, 0); 
                destroyedPositions.Add(location);
            }
        }
        
        for (int k = 0; k < _targetTileMap.childCount; k++)
        {
            Transform plantClone = _targetTileMap.GetChild(k);        
            foreach (var direction in destroyedPositions )
            {
                if(plantClone.position == direction)
                {
                    Destroy(plantClone.gameObject);
                    MapManager.Instance.PlantInMap.Remove(direction);
                    MapManager.Instance.DirtInMap.Remove(direction);
                }
            }
        }

        if (_targetTileMap == tileMap2.transform)
        {
            OnBombExploded?.Invoke(destroyedPositions);
        }
    }
    
}
