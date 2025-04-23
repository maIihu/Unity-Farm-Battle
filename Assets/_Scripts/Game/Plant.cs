using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Plant : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    
    public bool isReadyToHarvest;
    public bool checkBuff;
    public int growTimer;
    
    private void Start()
    {
        StartCoroutine(Grow());
    }
    
    private IEnumerator Grow()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[0];
        for (int i = 1; i < sprites.Length; i++)
        {
            yield return new WaitForSeconds(growTimer);
            gameObject.GetComponent<SpriteRenderer>().sprite = sprites[i];
        }
        
        isReadyToHarvest = true;
        //Debug.Log("Cây " + this.transform.position + " đã chín");
        yield return new WaitUntil(() => !isReadyToHarvest); 
        
    }

    public void Harvest()
    {
        //Debug.Log("Đã thu hoạch");
        isReadyToHarvest = false;
        StartCoroutine(Grow()); 
    }
}
