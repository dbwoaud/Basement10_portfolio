using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DeleteAbnormalData", menuName = "Abnormal/DeleteType")]
public class DeleteAbnormalData : AbnormalData
{
    [Header("삭제 대상 오브젝트 리스트")]
    public List<string> targetObjectNames = new List<string>();


    public override void ApplyAbnormal(GameObject mapRoot) // 이상현상을 적용하는 함수
    {
        foreach(string targetName in targetObjectNames)
        {
            Transform target = FindTarget(mapRoot, targetName);
            if (target != null)
                target.gameObject.SetActive(false);
        }
    }
}
