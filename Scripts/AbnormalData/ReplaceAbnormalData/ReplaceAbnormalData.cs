using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ReplaceAbnormalData", menuName = "Abnormal/ReplaceType")]
public class ReplaceAbnormalData : AbnormalData
{
    [System.Serializable]
    public struct ReplaceInfo
    {
        public string targetObjectName; // 교체되는 오브젝트 이름
        public GameObject newGameObject; // 교체하는 오브젝트 이름
    }

    [Header("교체 대상 오브젝트 리스트")]
    public List<ReplaceInfo> replaceList = new List<ReplaceInfo>();


    public override void ApplyAbnormal(GameObject mapRoot) // 이상현상을 적용하는 함수
    {
        foreach (ReplaceInfo replaceInfo in replaceList)
        {
            Transform oldTarget = FindTarget(mapRoot, replaceInfo.targetObjectName);
            if (oldTarget == null || replaceInfo.newGameObject == null) 
                continue;

            Transform parent = oldTarget.parent;
            Vector3 originalPos = oldTarget.localPosition;
            Quaternion originalRot = oldTarget.localRotation;
            Vector3 originalScale = oldTarget.localScale;                
            oldTarget.gameObject.SetActive(false);

            GameObject instance = Instantiate(replaceInfo.newGameObject, parent);
            instance.transform.SetLocalPositionAndRotation(originalPos,originalRot);
            instance.transform.localScale = originalScale;
        }
    }
}