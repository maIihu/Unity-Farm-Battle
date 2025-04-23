using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemBase : MonoBehaviour
{
    public void DestroyItem()
    {
        Destroy(gameObject);
    }

    public abstract void ItemEffect(GameObject objectToEffect);
}
