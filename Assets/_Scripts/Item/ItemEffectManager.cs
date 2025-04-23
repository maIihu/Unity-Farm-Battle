using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemEffectManager : MonoBehaviour
{
    [SerializeField] private GameObject tileMap1;
    [SerializeField] private GameObject tileMap2;

    [SerializeField] private List<GameObject> effectPrefabs;

    private Dictionary<string, Transform> _effects;
    private Dictionary<string, GameObject> _effectPrefabs;

    private bool _shieldEffectActive1;
    private bool _shieldEffectActive2;
    
    public static event Action<List<Vector3>> DestroyDirtMap1, DestroyPlantMap1;
    public static event Action<List<Vector3>> DestroyDirtMap2, DestroyPlantMap2;
    public static event Action<int> StartRain;
    
    private void Start()
    {
        _effects = new Dictionary<string, Transform>();
        _effectPrefabs = new Dictionary<string, GameObject>();
        
        foreach (var prefab in effectPrefabs)
        
            _effectPrefabs[prefab.name] = prefab;

        for (int i = 0; i < transform.childCount; i++)
            _effects[transform.GetChild(i).name] = transform.GetChild(i);
    }
    
    public void GetEffect(string itemName, int player)
    {
        switch (itemName)
        {
            case "Thunder":
                ThunderStart(player);
                break;
            case "Rain":
                RainStart(player);
                break;
            case "Tsunami":
                TsunamiStart(player);
                break;
            case "Shield":
                ShieldStart(player);
                break;
            case "Mouse":
                MouseStart(player);
                break;
        }
    }

    private void MouseStart(int player)
    {
        Vector3 position;
        GameObject targetTileMap;
        if (player == 1)
        {
            position = new Vector3(18.5f, -5.5f, 0f);
            targetTileMap = tileMap2;
        }
        else
        {
            position = new Vector3(5.5f, -5.5f, 0f);
            targetTileMap = tileMap1;
        }
        
        GameObject mouse = Instantiate(_effectPrefabs["Mouse"], position, Quaternion.identity, _effects["MouseEffect"]);
        mouse.GetComponent<Mouse>().ItemEffect(targetTileMap);
        Invoke(nameof(MouseEnd), 15);
    }

    private void MouseEnd()
    {
        Destroy(_effects["MouseEffect"].GetChild(0).gameObject);
    }
    
    private void ShieldStart(int player)
    {
        Vector3 position;
        
        if (player == 1)
        {
            _shieldEffectActive1 = true;
            position = new Vector3(6, -6, 0);
        }
        else
        {
            _shieldEffectActive2 = true;
            position = new Vector3(19, -6, 0);
        }
        
        Instantiate(_effectPrefabs["Shield"], position, Quaternion.identity, _effects["ShieldEffect"]);
        
        Invoke(nameof(ShieldEnd), 10f);
    }

    private void ShieldEnd()
    {
        Destroy(_effects["ShieldEffect"].GetChild(0).gameObject);
        _shieldEffectActive1 = false;
        _shieldEffectActive2 = false;
    }

    private void ThunderStart(int player)
    {
        Transform container = _effects["ThunderEffect"];
        for (int i = 0; i < 14; i++)
        {
            float x;  
            float y = Random.Range(-12, 0) + 0.5f;
            if (player == 1)
                x = Random.Range(13, 25) + 0.5f;  
            else
                x = Random.Range(0, 12) + 0.5f;
            
            Instantiate(_effectPrefabs["Thunder"], new Vector3(x, y, 0), Quaternion.identity, container);
        }
        
        if(player == 1)
            Instantiate(_effectPrefabs["Lightning"], new Vector3(19, 0, 0), Quaternion.identity, container);
        else
            Instantiate(_effectPrefabs["Lightning"], new Vector3(6, 0, 0), Quaternion.identity, container);
        
        AudioManager.Instance.PlaySfx(AudioManager.Instance.thunderSoundEffect);

        StartCoroutine(ThunderEnd(player, 1, container));
    }
    
    private IEnumerator ThunderEnd(int player, float time, Transform container)
    {
        yield return new WaitForSeconds(time);
    
        List<Vector3> plants1Destroyed = new List<Vector3>();
        List<Vector3> plants2Destroyed = new List<Vector3>();
        
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            if (!_shieldEffectActive1 && player == 2)
                if(container.GetChild(i).GetComponent<Thunder>() != null)
                {
                    container.GetChild(i).GetComponent<Thunder>().ItemEffect(tileMap1);
                    plants1Destroyed.Add(container.GetChild(i).position);
                }
            
            if(!_shieldEffectActive2 && player == 1)
                if(container.GetChild(i).GetComponent<Thunder>() != null)
                {
                    container.GetChild(i).GetComponent<Thunder>().ItemEffect(tileMap2);
                    plants2Destroyed.Add(container.GetChild(i).position);
                }
            
            Destroy(container.GetChild(i).gameObject);
        }
        
        if(plants1Destroyed.Count > 0)
            DestroyDirtMap1?.Invoke(plants1Destroyed);
        
        if(plants2Destroyed.Count > 0)
            DestroyDirtMap2?.Invoke(plants2Destroyed);
    }
    
    private void RainStart(int player)
    {
        Vector3 position;
        GameObject tileMapTarget;
        
        if(player == 1)
        {
            position = new Vector3(6, 4, 0);
            tileMapTarget = tileMap1;
        }
        else
        {
            position = new Vector3(19, 4, 0);
            tileMapTarget = tileMap2;
        }

        GameObject rain = Instantiate(_effectPrefabs["Rain"], position, Quaternion.Euler(90, 0, 0), _effects["RainEffect"]);

        rain.gameObject.GetComponent<ParticleSystem>().Play();
        //rain.gameObject.GetComponent<Rain>().ItemEffect(tileMapTarget);
        StartRain?.Invoke(player);
        StartCoroutine(RainEnd(tileMapTarget, player, 10f));
    }
    
    private IEnumerator RainEnd(GameObject tileMapTarget, int player, float time)
    {
        yield return new WaitForSeconds(time);
        _effects["RainEffect"].GetChild(0).gameObject.GetComponent<ParticleSystem>().Stop();
        
        StartRain?.Invoke(player);
        
        MapManager.Instance.DeBuffGrowTime(tileMapTarget.transform);
        
        Destroy(_effects["RainEffect"].GetChild(0).gameObject);
    }

    private void TsunamiStart(int player)
    {
        Vector3 position, targetPosition;
        
        if (player == 1)
        {
            position = new Vector3(19, -30, 0);
            targetPosition = new Vector3(19, 4, 0);
        }
        else
        {
            position = new Vector3(6, -30, 0);
            targetPosition = new Vector3(6, 4, 0);
        }
            
        GameObject tsunami = Instantiate(_effectPrefabs["Tsunami"], position, Quaternion.identity, _effects["TsunamiEffect"]);
        AudioManager.Instance.PlaySfx(AudioManager.Instance.tsunamiSoundEffect);
        
        StartCoroutine(MoveTsunami(tsunami, targetPosition, player));
    }

    private IEnumerator MoveTsunami(GameObject tsunami, Vector3 targetPosition, int player)
    {
        float speed = 18f;
        List<Vector3> plantsDestroyed1 = new List<Vector3>();
        List<Vector3> plantsDestroyed2 = new List<Vector3>();
        foreach (Transform child in tileMap1.transform)
            plantsDestroyed1.Add(child.position);
        foreach (Transform child in tileMap2.transform)
            plantsDestroyed2.Add(child.position);
        
        while (Vector3.Distance(tsunami.transform.position, targetPosition) > 0.1)
        {
            tsunami.transform.position =
                Vector3.MoveTowards(tsunami.transform.position, targetPosition, speed * Time.deltaTime);
            
            if (player == 1 && !_shieldEffectActive2)
                tsunami.GetComponent<Tsunami>().ItemEffect(tileMap2);
            if (player == 2 && !_shieldEffectActive1) 
                tsunami.GetComponent<Tsunami>().ItemEffect(tileMap1);
        
            yield return null;
        }
        
        tsunami.transform.position = targetPosition;
        Destroy(tsunami);
        
        if (player == 1 && !_shieldEffectActive2)
            DestroyPlantMap2?.Invoke(plantsDestroyed2);

        if (player == 2 && !_shieldEffectActive1)
            DestroyPlantMap1?.Invoke(plantsDestroyed1);
    }
    
}
