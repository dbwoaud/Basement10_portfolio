using System.Collections.Generic;
using UnityEngine;

public class SpawnAbnormalManager : Singleton<SpawnAbnormalManager>
{
    [SerializeField] private List<AbnormalData> abnormalDatas = new List<AbnormalData>();
    [SerializeField] [Range(0f, 1f)] private float AbnormalRate = 0.5f;

    [HideInInspector] public GameObject mapRoot;
    public AbnormalData currentAbnormal {  get; private set; }

    public AbnormalData SelectAbnormal() // 이상 현상을 선택하는 함수
    {
        if (abnormalDatas.Count == 0 || mapRoot == null) 
            return null;
        
        bool isAbnormal = Random.value <= AbnormalRate;

        if (!isAbnormal)
            return null;
        
        int abnormalIndex = Random.Range(0, abnormalDatas.Count);
        currentAbnormal = abnormalDatas[abnormalIndex];
        currentAbnormal.ApplyAbnormal(mapRoot);

        return currentAbnormal;
    }
}
