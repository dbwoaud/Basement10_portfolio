using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DeleteAbnormalData", menuName = "Abnormal/DeleteType")]
public class DeleteAbnormalData : AbnormalData
{
    [Header("삭제할 타겟 이름 목록")]
    public List<string> targetObjectNames = new List<string>();

    public override void ApplyAbnormal(GameObject mapRoot)
    {
        foreach(string targetName in targetObjectNames)
        {
            Transform target = FindTarget(mapRoot, targetName);
            if (target != null)
                target.gameObject.SetActive(false);
        }
    }
}
