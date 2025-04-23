using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thunder : ItemBase
{
    public override void ItemEffect(GameObject objectToEffect)
    {
        foreach (Transform child in objectToEffect.transform)
        {
            if (child.transform.position == transform.position)
            {
                Destroy(child.gameObject);
            }
        } 
    }
}
