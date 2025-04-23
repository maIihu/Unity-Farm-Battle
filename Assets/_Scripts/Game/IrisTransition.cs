using System;
using UnityEngine;
using System.Collections;

public class IrisTransition : MonoBehaviour
{
    public Material irisMaterial;
    public float duration = 1f;
    
    private void Start()
    {
        StartCoroutine(SequenceIrisAnimations());
    }

    private IEnumerator SequenceIrisAnimations()
    {
        yield return StartCoroutine(AnimateIris(1f, 0f));
        yield return StartCoroutine(AnimateIris(0f, 1f));
        gameObject.SetActive(false);
        //GameManager.Instance.ChangeState(GameState.Playing);
    }
    
    private IEnumerator AnimateIris(float start, float end)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float value = Mathf.Lerp(start, end, t / duration);
            irisMaterial.SetFloat("_Radius", value);
            yield return null;
        }
        irisMaterial.SetFloat("_Radius", end);
    }
}