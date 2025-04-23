using System;
using UnityEngine;

public class BombController : MonoBehaviour
{
    public static event Action<Vector3, int> PositionBombExploded;
    public static event Action<Vector3> BotHasBomb;
    
    [SerializeField] private float timeToExplode = 6f;
    [SerializeField] private float moveSpeed = 10f;

    private Vector3 _lastPosition;
    private Vector3 _targetPosition;
    private bool _isMoving;
    
    
    private void Start()
    {
        Invoke(nameof(Explode), timeToExplode);
    }

    public void ThrowingBomb(Vector3 destination)
    { 
        _targetPosition = destination;
        _isMoving = true;
    }

    private void Update()
    {
        if (_isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _targetPosition) < 0.001f)
            {
                StopMoving();
            }
        }
        
    }

    private void StopMoving()
    {
        _isMoving = false;
        if (transform.position.x is >= 13f and <= 25f && transform.position.y is >= -12 and <= 0)
        {
            _lastPosition = transform.position;
            BotHasBomb?.Invoke(_lastPosition);
        }
    }

    private void Explode()
    {
        _lastPosition = transform.position;
        int tileMap = 0;
        
        if (transform.position.x is >= 13f and <= 25 && transform.position.y is >= -12 and <= 0)
            tileMap = 2;
        if (transform.position.x is >= 1f and <= 12 && transform.position.y is >= -12 and <= 0)
            tileMap = 1;
            
        PositionBombExploded?.Invoke(_lastPosition, tileMap);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fence"))
        {
            gameObject.GetComponent<CircleCollider2D>().isTrigger = false;
            Invoke(nameof(EnableTrigger), 0.2f);
        }
    }

    private void EnableTrigger()
    {
        gameObject.GetComponent<CircleCollider2D>().isTrigger = true;
    }

}