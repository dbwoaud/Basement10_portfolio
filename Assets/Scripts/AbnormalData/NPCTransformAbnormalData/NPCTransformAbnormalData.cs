using UnityEngine;

[CreateAssetMenu(fileName = "NPCTransformAbnormalData", menuName = "Abnormal/NPCTransformType")]
public class NPCTransformAbnormalData : AbnormalData
{
    [Header("목표 세팅")]
    public string targetName;

    [Header("비주얼 변경")]
    public GameObject newModelPrefab;
    public RuntimeAnimatorController newController;
    public Avatar newAvatar;

    [Header("상세 설정")]
    public string rootBoneName = "mixamorig:Hips";
    public string defaultAnimState = "NPC_Walking";

    public override void ApplyAbnormal(GameObject mapRoot)
    {
        Transform rootTarget = FindTarget(mapRoot, targetName) ?? GameObject.Find(targetName)?.transform;
        if (rootTarget == null) 
            return;

        Animator targetAnimator = rootTarget.GetComponentInChildren<Animator>();
        if (targetAnimator == null) 
            return;

        Transform bodyTransform = targetAnimator.transform;

        CleanUpOldModel(bodyTransform);
        if (newModelPrefab != null)
            SetupNewModel(bodyTransform, targetAnimator);
    }

    private void CleanUpOldModel(Transform bodyTransform) // 이전 모델을 제거하는 함수
    {
        if (!string.IsNullOrEmpty(rootBoneName))
        {
            Transform oldBone = bodyTransform.Find(rootBoneName);
            if (oldBone != null)
            {
                oldBone.name = "Destroyed_Bone";
                Destroy(oldBone.gameObject);
            }
        }

        SkinnedMeshRenderer oldRenderer = bodyTransform.GetComponentInChildren<SkinnedMeshRenderer>();
        if (oldRenderer != null)
            Destroy(oldRenderer.gameObject);
    }

    private void SetupNewModel(Transform bodyTransform, Animator targetAnimator) // 새로운 모델을 설정하는 함수
    {
        GameObject newModel = Instantiate(newModelPrefab, bodyTransform);
        newModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        Animator childAnimator = newModel.GetComponent<Animator>();
        Avatar avatarToUse = newAvatar != null ? newAvatar : (childAnimator != null ? childAnimator.avatar : null);

        if (childAnimator != null) 
            Destroy(childAnimator);

        targetAnimator.enabled = false;
        if (avatarToUse != null) 
            targetAnimator.avatar = avatarToUse;

        if (newController != null) 
            targetAnimator.runtimeAnimatorController = newController;

        targetAnimator.enabled = true;
        targetAnimator.Rebind();
        targetAnimator.Update(0f);

        if (targetAnimator.layerCount > 0)
            targetAnimator.SetLayerWeight(0, 1f);

        if (!string.IsNullOrEmpty(defaultAnimState))
            targetAnimator.CrossFadeInFixedTime(defaultAnimState, 0.1f);
    }
}