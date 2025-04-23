using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Mouse : ItemBase
{
    private Dictionary<Vector3, Plant> _plants;
    private Plant _targetPlant;
    private Vector3 _plantPos;
    private GameObject _tileMap;
    private Animator _anim;
    
    private float _moveSpeed;
    private bool _moveToPlant;

    public static event Action<Vector3> Plant1Destroyed;
    public static event Action<Vector3> Plant2Destroyed;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    private void Start()
    {
        _moveSpeed = 6f;
        _plants = new Dictionary<Vector3, Plant>();
        FindAllPlantsCanEat();
    }

    private void Update()
    {
        if (_plants.Count > 0 && !_moveToPlant)
        {
            FindRandomPlant();
        }

        if (_moveToPlant && _targetPlant)
        {
            StartCoroutine(MoveAndEat());
        }
    }

    private void FindRandomPlant()
    {
        int randomIndex = Random.Range(0, _plants.Count);
        _targetPlant = _plants.ElementAt(randomIndex).Value;
        
        if (_targetPlant)
        {
            _moveToPlant = true;
            _plantPos = _targetPlant.transform.position;
        }
    }

    private IEnumerator MoveAndEat()
    {
        transform.position = Vector3.MoveTowards(transform.position, _plantPos, _moveSpeed * Time.deltaTime);
        Flip();

        if (Vector3.Distance(transform.position, _plantPos) < 0.1f)
        {
            if (_targetPlant)
            {
                _anim.SetTrigger("Eating");
                _plants.Remove(_plantPos);
                Destroy(_targetPlant.gameObject);
                yield return new WaitForSeconds(1f);
                
                if(_tileMap.name == "Garden1")
                    Plant1Destroyed?.Invoke(_plantPos);
                
                if(_tileMap.name == "Garden2")
                    Plant2Destroyed?.Invoke(_plantPos);
            }
            _targetPlant = null;
            _moveToPlant = false;
            _plantPos = new Vector3();
        }
    }

    private void Flip()
    {
        Vector3 direction = _plantPos - transform.position;

        if (direction.x > 0.01f)
        {
            transform.localScale = new Vector3(1, 1, 1); 
        }
        else if (direction.x < -0.01f)
        {
            transform.localScale = new Vector3(-1, 1, 1); 
        }
    }
    
    private void FindAllPlantsCanEat()
    {
        foreach (Transform child in _tileMap.transform)
        {
            if (child.childCount > 0)
            {
                Transform plant = child.GetChild(0);
                _plants.Add(plant.position, plant.GetComponent<Plant>());
            }
        }
    }
    
    public override void ItemEffect(GameObject objectToEffect)
    {
        _tileMap = objectToEffect;
    }
}
