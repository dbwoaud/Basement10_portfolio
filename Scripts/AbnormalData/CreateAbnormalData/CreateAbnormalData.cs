using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Create Abnormal", menuName = "Abnormal/CreateType")]
public class CreateAbnormalData : AbnormalData
{
    [System.Serializable]
    public struct SpawnInfo
    {
        public GameObject spawnGameObject; // 생성할 오브젝트
        public Vector3 spawnPosition; // 생성할 위치
        public Vector3 spawnRotation; // 생성할 방향
        public Vector3 spawnScale; // 생성할 크기
    }

    [Header("생성 대상 오브젝트 리스트 ")]
    public List<SpawnInfo> spawnList = new List<SpawnInfo>();

    public override void ApplyAbnormal(GameObject mapRoot) // 이상현상을 적용하는 함수
    {
        foreach(SpawnInfo spawnInfo in spawnList)
        {
            if (spawnInfo.spawnGameObject == null)
                continue;

            GameObject instance = Instantiate(spawnInfo.spawnGameObject, mapRoot.transform);
            instance.transform.localPosition = spawnInfo.spawnPosition;
            instance.transform.localEulerAngles = spawnInfo.spawnRotation;
            instance.transform.localScale = spawnInfo.spawnScale;
        }
    }
}
