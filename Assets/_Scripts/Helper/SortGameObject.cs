using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortGameObject
{
    public static void SortChildrenByName(Transform parent)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in parent)
        {
            children.Add(child);
        }
        children.Sort((a, b) => 
        {
            if (!Mathf.Approximately(a.position.x, b.position.x))
                return a.position.x.CompareTo(b.position.x);
            if (!Mathf.Approximately(a.position.y, b.position.y))
                return a.position.y.CompareTo(b.position.y);
            return a.position.z.CompareTo(b.position.z);
        });
        
        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }
    }
}
