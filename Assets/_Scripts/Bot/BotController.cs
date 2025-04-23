using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class BotController : MonoBehaviour
{
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private float moveSpeed = 5f;

    private Transform _pickCell;
    private Plant _targetPlant;
    private List<Vector3> _plantsToDig;
    private List<Vector3> _plantsToSow;
    private Dictionary<Vector3, Plant> _plantsToHarvest;
    private Vector3 _bombPosition;
    
    private bool _dig, _dug;
    private bool _sow, _sowed;
    private bool _harvest;
    private bool _hasBomb, _throwBomb;
    private bool _isRaining;
    private bool _shop, _shopping;
    private bool _timeChecker;
    
    private Coroutine _harvestCoroutine;
    private Coroutine _digCoroutine;
    private Coroutine _sowCoroutine;
    private Coroutine _throwBombCoroutine;
    private Coroutine _shoppingCoroutine;
    
    private int _lastCheckedPoint = 20;

    private Animator _anim;
    private float _lastXDirection = 1f; // sẽ tùy thuộc vào vị trí di chuyển mà xét 
    
    public int score;
    public bool buying;
    public int buyItemIndex;
    
    public static BotController Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _anim = GetComponent<Animator>();
    }

    private void Start()
    {
        _pickCell = transform.GetChild(0);
        _plantsToDig = new List<Vector3>();
        _plantsToSow = new List<Vector3>();
        _plantsToHarvest = new Dictionary<Vector3, Plant>();
        TileCanPlant();
    }

    private void Update()
    {
        if (GameManager.Instance.currentState == GameState.Playing)
        {
            if (Mathf.Approximately((int)Time.time, _lastCheckedPoint) && !_timeChecker)
            {
                _timeChecker = true;
                int index = BotDecisionMaker.Instance.UseItemWithModel();
                if(index >= 0 && index <= 5)
                {
                    _shop = true;
                    buyItemIndex = index;
                }
                _lastCheckedPoint += 20;
                _timeChecker = false;
            }
            StateHandle();
            if (_isRaining)
                MapManager.Instance.BuffGrowTime(tileMap);
            CanHarvestPlant();
        }
    }

    private void StateHandle()
    {
        if (_hasBomb)
        { // uu tien da bom
            if (!_throwBomb)
            {
                ResetState();
                _throwBombCoroutine = StartCoroutine(ThrowBomb());
            }
        }
        else if (!_dug) // dao dat
        { 
            if(!_dig)
            {
                ResetState();
                _digCoroutine = StartCoroutine(Dig());
            }
        }
        else if (!_sowed) // gieo hat
        {
            if(!_sow)
            {
                ResetState();
                _sowCoroutine = StartCoroutine(Sow());
            }
        }
        else if (_shop) // mua vat pham
        {
            if (!_shopping)
            {
                ResetState();
                _shoppingCoroutine = StartCoroutine(Shopping());
            }
        }
        else // thu hoach
        {
            if(_targetPlant == null)
                FindNearestPlant();
            if (!_harvest && _targetPlant)
            {
                ResetState();
                _harvestCoroutine = StartCoroutine(Harvest());
            }
        }
    }
    
    private void ResetState()
    {
        if (_digCoroutine != null)
        {
            StopCoroutine(_digCoroutine);
            _dig = false;
        }
        if(_sowCoroutine != null)
        {
            StopCoroutine(_sowCoroutine);
            _sow = false;
        }
        if(_harvestCoroutine != null)
        {
            StopCoroutine(_harvestCoroutine);
            _harvest = false;
        }
        if(_throwBombCoroutine != null)
        {
            StopCoroutine(_throwBombCoroutine);
            _throwBomb = false;
        }

        if (_shoppingCoroutine != null)
        {
            StopCoroutine(_shoppingCoroutine);
            _shopping = false;
        }
    }
    
    private IEnumerator MoveToTarget(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            Vector3 dir = targetPosition - transform.position;
            
            _anim.SetBool("isMoving", true);
            
            if (Mathf.Abs(dir.x) > 0.01f)
                _lastXDirection = dir.x;
            
            _anim.SetFloat("XDirection", _lastXDirection);
            
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            _pickCell.position = new Vector3((int)(transform.position.x) + 0.5f, 
                (int)(transform.position.y) - 0.5f, transform.position.z);
            yield return null;
        }
        
        _anim.SetBool("isMoving", false);
        _anim.SetFloat("XDirection", _lastXDirection);
        
        transform.position = targetPosition;
        _pickCell.position = new Vector3((int)(transform.position.x) + 0.5f, 
            (int)(transform.position.y) - 0.5f, transform.position.z);
    }

    private IEnumerator Dig()
    {
        _dig = true;
        while (_plantsToDig.Count > 0)
        {
            Vector3 targetPosition = _plantsToDig.OrderBy(p => Vector3.Distance(transform.position, p)).FirstOrDefault();
            yield return MoveToTarget(targetPosition);
            MapManager.Instance.Dig(_pickCell.position, tileMap);
            _plantsToDig.Remove(targetPosition);
            _plantsToSow.Add(targetPosition);
        }
        _dig = false;
        _dug = true;
    }

    private IEnumerator Sow()
    {
        _sow = true;
        while (_plantsToSow.Count > 0)
        {
            Vector3 targetPosition = _plantsToSow.OrderBy(p => Vector3.Distance(transform.position, p)).FirstOrDefault();
            yield return MoveToTarget(targetPosition);
            MapManager.Instance.Sow(_pickCell.position);
            _plantsToSow.Remove(targetPosition);
        }
        _sow = false;
        _sowed = true;
    }

    private void FindNearestPlant()
    {
        if (_plantsToHarvest.Count > 0)
        {
            _targetPlant = _plantsToHarvest.OrderBy(p => Vector3.Distance(transform.position, p.Key)).FirstOrDefault()
                .Value;
        }
    }
    
    private IEnumerator Harvest()
    {
        _harvest = true;
        Vector3 targetPosition = _targetPlant.transform.position;
        yield return MoveToTarget(targetPosition);
        if (_targetPlant != null && _targetPlant.isReadyToHarvest) 
        {
            _targetPlant.Harvest();
            score++;
            _targetPlant = null;
        }
        _plantsToHarvest.Remove(targetPosition);
        _harvest = false;
    }

    private IEnumerator ThrowBomb()
    {
        _throwBomb = true;
        yield return StartCoroutine(MoveToTarget(_bombPosition));
        GameObject bomb = GameObject.FindGameObjectWithTag("Bomb");
        if (bomb)
        {
            float x = Random.Range(2, 10);
            float y = Random.Range(-10, -2);
            bomb.GetComponent<BombController>().ThrowingBomb(new Vector3(x, y, 0));
        }
        _hasBomb = false;
        _throwBomb = false;
    }
    
    private IEnumerator Shopping()
    {
        _shopping = true;
        Vector3 shopPos = new Vector3(18.5f, 1f, 0f);
        yield return MoveToTarget(shopPos);
        yield return new WaitForSeconds(2f);
        buying = true;
        yield return null;
        buying = false;
        _shop = false;
        _shopping = false;
    }
    
    private void CanHarvestPlant()
    {
        for (int i = 0; i < tileMap.transform.childCount; i++)
        {
            Transform child = tileMap.transform.GetChild(i);
            if (child.childCount > 0)
            {
                Plant plant = child.GetChild(0).gameObject.GetComponent<Plant>();
                if (plant != null && plant.isReadyToHarvest && !_plantsToHarvest.ContainsKey(plant.transform.position))
                {
                    _plantsToHarvest.Add(plant.transform.position, plant);
                }
            }
            else
            {
                _plantsToHarvest.Remove(child.position);
            }
        }
    }
    
    private void TileCanPlant()
    { 
        for (int j = -12; j <= -1; j++)
            for (int i = 13; i <= 24; i++)
            {
                float xPos = i + 0.5f;
                float yPos = j + 0.5f;
                Vector3 pos = new Vector3(xPos, yPos, 0f);
                _plantsToDig.Add(pos);
            }
    }
    
    private void HasBomb(Vector3 pos)
    {
        if (!IsInBotArea(pos))
            return;
        _hasBomb = true;
        _bombPosition = pos;
        _harvest = false;
        _targetPlant = null;
    }
    
    private void DirtDestroyed(List<Vector3> dirtDestroyed)
    {
        _dug = false;
        _sowed = false;
        _targetPlant = null;
        
        foreach (var plantPos in dirtDestroyed)
            if(IsInBotArea(plantPos) && !_plantsToDig.Contains(plantPos)) 
                _plantsToDig.Add(plantPos);
        MapManager mapM = MapManager.Instance;
        foreach (var plantPos in _plantsToDig)
        {
            if(mapM.DirtInMap.ContainsKey(plantPos))
                mapM.DirtInMap.Remove(plantPos);
            if(mapM.PlantInMap.ContainsKey(plantPos))
                mapM.PlantInMap.Remove(plantPos);
            
            if(_plantsToHarvest.ContainsKey(plantPos))
                _plantsToHarvest.Remove(plantPos);
        }
    }
    
    private void PlantDestroyed(List<Vector3> plantDestroyed)
    {
        _sowed = false;
        _targetPlant = null;
        
        foreach (var plantPos in plantDestroyed)
            if(IsInBotArea(plantPos) && !_plantsToSow.Contains(plantPos)) 
                _plantsToSow.Add(plantPos);
        MapManager mapM = MapManager.Instance;
        foreach (var plantPos in _plantsToSow)
        {
            if(mapM.PlantInMap.ContainsKey(plantPos))
                mapM.PlantInMap.Remove(plantPos);
            if(_plantsToHarvest.ContainsKey(plantPos))
                _plantsToHarvest.Remove(plantPos);
        }
    }

    private void OnEnable()
    {
        BombController.BotHasBomb += HasBomb;
        BombManager.PositionSpawnBomb += HasBomb;
        BombManager.OnBombExploded += DirtDestroyed;
        
        ItemEffectManager.DestroyDirtMap2 += DirtDestroyed;
        ItemEffectManager.DestroyPlantMap2 += PlantDestroyed;
        ItemEffectManager.StartRain += EffectRain;
        Mouse.Plant2Destroyed += MouseEatPlant;
    }
    
    private void OnDestroy()
    {
        BombController.BotHasBomb -= HasBomb;
        BombManager.PositionSpawnBomb -= HasBomb;
        
        BombManager.OnBombExploded -= DirtDestroyed;
        ItemEffectManager.DestroyDirtMap2 -= DirtDestroyed;
        ItemEffectManager.DestroyPlantMap2 -= PlantDestroyed;
        ItemEffectManager.StartRain -= EffectRain;
        Mouse.Plant2Destroyed -= MouseEatPlant;
    }
    
    private void MouseEatPlant(Vector3 objPos)
    {
        if (_plantsToHarvest.ContainsKey(objPos))
            _plantsToHarvest.Remove(objPos);
        _targetPlant = null;
    }

    private void EffectRain(int targetMap)
    {
        if (targetMap == 2) _isRaining = !_isRaining;
    }
    
    private static bool IsInBotArea(Vector3 pos)
    {
        return pos.x >= 13f && pos.x <= 25f && pos.y >= -12f && pos.y <= 0f;
    }
}
