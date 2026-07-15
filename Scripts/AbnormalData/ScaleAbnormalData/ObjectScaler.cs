using System.Collections;
using UnityEngine;

public class ObjectScaler : MonoBehaviour
{
    public void StartScaling(Vector3 targetScale, float duration) // 크기 조절을 수행하는 함수
    {
        StopAllCoroutines();
        StartCoroutine(ScaleRoutine(targetScale, duration));
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale, float duration) // 크기 조절을 수행하는 코루틴
    {
        Vector3 startScale = transform.localScale;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
        Destroy(this);
    }
}
