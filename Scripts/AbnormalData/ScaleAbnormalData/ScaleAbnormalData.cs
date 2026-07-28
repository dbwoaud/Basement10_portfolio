using UnityEngine;
using System.Collections.Generic;

public enum ScaleMode { Instant, Gradual } // 크기 이상현상 종류

[CreateAssetMenu(fileName = "ScaleAbnormalData", menuName = "Abnormal/ScaleType")]
public class ScaleAbnormalData : AbnormalData
{
    [System.Serializable]
    public struct ScaleInfo
    {
        public string targetObjectName; // 크기를 바꿀 오브젝트 이름
        public Vector3 targetScale; // 목표 크기
        public ScaleMode scaleMode; // 크기 이상현상 종류
        public float duration; // 크기 변경 시간
    }

    [Header("크기 변경 대상 오브젝트 리스트")]
    public List<ScaleInfo> scaleList = new List<ScaleInfo>();


    public override void ApplyAbnormal(GameObject mapRoot) // 이상 현상을 적용하는 함수
    {
        foreach(ScaleInfo scaleInfo in scaleList)
        {
            Transform target = FindTarget(mapRoot, scaleInfo.targetObjectName);
            if (target == null) 
                continue;

            if (scaleInfo.scaleMode == ScaleMode.Instant)
                ApplyInstantMode(scaleInfo, target);

            else if (scaleInfo.scaleMode == ScaleMode.Gradual)
                ApplyGradualMode(scaleInfo, target);
        }
    }

    private static void ApplyInstantMode(ScaleInfo scaleInfo, Transform target) // 크기가 즉시 커지는 이상현상을 적용하는 함수
    {
        target.localScale = scaleInfo.targetScale;
    }

    private static void ApplyGradualMode(ScaleInfo scaleInfo, Transform target) // 크기가 점점 커지는 이상현상을 적용하는 함수
    {
        if (!target.gameObject.TryGetComponent<ObjectScaler>(out ObjectScaler scaler))
            scaler = target.gameObject.AddComponent<ObjectScaler>();

        float time = Mathf.Max(scaleInfo.duration, 0.1f);
        scaler.StartScaling(scaleInfo.targetScale, time);
    }
}
