using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "ScaleAbnormalData", menuName = "Abnormal/ScaleType")]
public class ScaleAbnormalData : AbnormalData
{
    public enum ScaleMode { Instant, Gradual }
    
    [System.Serializable]
    public struct ScaleInfo
    {
        public string targetObjectName; // 크기를 바꿀 오브젝트 이름
        public Vector3 targetScale; // 목표 크기
        public ScaleMode scaleMode; // 크기 변경 방식
        public float duration; // 크기 변경 시간
    }

    [Header("크기 변경 설정 리스트")]
    public List<ScaleInfo> scaleList = new List<ScaleInfo>();

    public override void ApplyAbnormal(GameObject mapRoot)
    {
        foreach(ScaleInfo scaleInfo in scaleList)
        {
            Transform target = FindTarget(mapRoot, scaleInfo.targetObjectName);

            if (target == null) 
                continue;

            if (scaleInfo.scaleMode == ScaleMode.Instant)
                target.localScale = scaleInfo.targetScale;
            
            else if (scaleInfo.scaleMode == ScaleMode.Gradual)
            {
                if (!target.gameObject.TryGetComponent<ObjectScaler>(out ObjectScaler scaler))
                    scaler = target.gameObject.AddComponent<ObjectScaler>();

                float time = Mathf.Max(scaleInfo.duration, 0.1f);
                scaler.StartScaling(scaleInfo.targetScale, time);
            }
        }
    }
}
