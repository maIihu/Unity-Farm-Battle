using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; } 

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Tilemap tileMap;
    
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private Vector2 _moveInput;
    private Rigidbody2D _rb;
    private Transform _pickCell;
    private Collider2D _currentBomb;
    
    private bool _isTouchingBomb;
    private bool _isRaining;
    private bool _isSorted;    
    private bool _moveOut;
    
    private float _sowDelay;
    private float _lastDigTime = -1f;

    public bool isShopping;
    public int score;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;

        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
    }
    
    private void Rain(int obj)
    {
        if (obj == 1)
            _isRaining = !_isRaining;
    }
    
    private void Start()
    {
        _sowDelay = 0.25f;
        _pickCell = transform.GetChild(0);
    }

    private void Update()
    {
        if (GameManager.Instance.currentState == GameState.Playing)
        {
            MoveOutGarden();
        
            if (!_moveOut)
            {
                _pickCell.position = new Vector3((int)(transform.position.x) + 0.5f, 
                    (int)(transform.position.y) - 0.5f, transform.position.z);
                
                if (_isTouchingBomb && Input.GetKeyDown(KeyCode.Space))
                    ThrowBomb();

                if (Input.GetKey(KeyCode.Space))
                    Plant();
            }
        
            InputHandle();
        
            if(isShopping)
                PauseAnimation();

            SortPlantWithPosition();
        
            if (_isRaining)
                MapManager.Instance.BuffGrowTime(tileMap);   
        }
    }

    private void MoveOutGarden()
    {
        if (transform.position.y >= 0)
        {
            if (!_moveOut) 
            {
                _pickCell.gameObject.SetActive(false);
                _moveOut = true;
            }
        }
        else
        {
            if (_moveOut) 
            {
                _pickCell.gameObject.SetActive(true);
                _moveOut = false;
            }
        }
    }
    
    private void SortPlantWithPosition()
    {
        if (tileMap.transform.childCount == 144 && !_isSorted)
        {
            SortGameObject.SortChildrenByName(tileMap.transform);
            _isSorted = true; 
        }
        else if (tileMap.transform.childCount != 144)
            _isSorted = false; 
    }
    
    private void FixedUpdate()
    {
        _rb.velocity = _moveInput * moveSpeed;
    }
    
    private void PauseAnimation()
    {
        _animator.SetTrigger("Shopping");
        _moveInput = Vector2.zero;
        _animator.SetFloat("Speed", 0);
    }
    
    private void Plant()
    {
        bool hasDug = MapManager.Instance.Dig(_pickCell.position, tileMap);
        if (hasDug)
        {
            _lastDigTime = Time.time;
        }
        else if (Time.time - _lastDigTime >= _sowDelay) 
        {
            MapManager.Instance.Sow(_pickCell.position);
        }

        MapManager.Instance.Harvest(_pickCell.position, tileMap, ref score); 
    }
    
    private void ThrowBomb()
    {
        float x = Random.Range(14, 22);
        float y = Random.Range(-10, -2);
        _currentBomb.GetComponent<BombController>().ThrowingBomb(new Vector3(x, y, 0));
    }
    
    private void InputHandle()
    {
        _moveInput = Vector2.zero;
        
        if (Input.GetKey(KeyCode.A))
            _moveInput.x = -1f;
        if (Input.GetKey(KeyCode.D))
            _moveInput.x = 1f;
        if (Input.GetKey(KeyCode.W))
            _moveInput.y = 1f;
        if (Input.GetKey(KeyCode.S))
            _moveInput.y = -1f;
        
        SetAnimation();
        FlipCharacter();
        
        _moveInput.Normalize();
    }

    private void SetAnimation()
    {
        _animator.SetFloat("Horizontal", _moveInput.x);
        _animator.SetFloat("Vertical", _moveInput.y);
        _animator.SetFloat("Speed", _moveInput.magnitude);
    }

    private void FlipCharacter()
    {
        if (_moveInput.x > 0)
        {
            _spriteRenderer.flipX = false;
        }
        else if (_moveInput.x < 0)
        {
            _spriteRenderer.flipX = true;
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Bomb"))
        {
            _isTouchingBomb = true;
            _currentBomb = other;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bomb"))
        {
            _isTouchingBomb = false;
            _currentBomb = null;
        }
    }
    private void MouseEatPlant(Vector3 obj)
    {
        if (MapManager.Instance.DirtInMap.ContainsKey(obj))
            MapManager.Instance.DirtInMap.Remove(obj);
    }

    private void DirtDestroyed(List<Vector3> obj)
    {
        MapManager mapM = MapManager.Instance;
        foreach (var plantPos in obj)
        {
            if(mapM.DirtInMap.ContainsKey(plantPos))
                mapM.DirtInMap.Remove(plantPos);
            if(mapM.PlantInMap.ContainsKey(plantPos))
                mapM.PlantInMap.Remove(plantPos);
        }
    }
    
    private void PlantDestroyed(List<Vector3> obj)
    {
        MapManager mapM = MapManager.Instance;
        foreach (var plantPos in obj)
        { 
            if(mapM.PlantInMap.ContainsKey(plantPos))
                mapM.PlantInMap.Remove(plantPos);
        }
    }

    private void OnEnable()
    {
        ItemEffectManager.StartRain += Rain;
        BombManager.OnBombExploded += DirtDestroyed;
        ItemEffectManager.DestroyDirtMap1 += DirtDestroyed;
        Mouse.Plant1Destroyed += MouseEatPlant;
        ItemEffectManager.DestroyPlantMap1 += PlantDestroyed;
    }
    
    private void OnDestroy()
    {
        ItemEffectManager.StartRain -= Rain;
        BombManager.OnBombExploded -= DirtDestroyed;
        ItemEffectManager.DestroyDirtMap1 -= DirtDestroyed;
        Mouse.Plant1Destroyed -= MouseEatPlant;
        ItemEffectManager.DestroyPlantMap1 -= PlantDestroyed;
    }
    
}
