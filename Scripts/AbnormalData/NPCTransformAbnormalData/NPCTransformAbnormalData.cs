using UnityEngine;

[CreateAssetMenu(fileName = "NPCTransformAbnormalData", menuName = "Abnormal/NPCTransformType")]
public class NPCTransformAbnormalData : AbnormalData
{
    [Header("목표 세팅")]
    public string targetName;

    [Header("블렌드 셰이프 설정")]
    public string smileBlendShapeName = "Smile";
    public float smileTargetWeight = 100f;

    public override void ApplyAbnormal(GameObject mapRoot)
    {
        Transform target = FindTarget(mapRoot, targetName);
        if (target == null)
            return;

        SkinnedMeshRenderer skinRenderer = target.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinRenderer == null)
            return;

        int blendShapeIndex = skinRenderer.sharedMesh.GetBlendShapeIndex(smileBlendShapeName);
        if (blendShapeIndex == -1)
            return;

        skinRenderer.SetBlendShapeWeight(blendShapeIndex, smileTargetWeight);
    }
}