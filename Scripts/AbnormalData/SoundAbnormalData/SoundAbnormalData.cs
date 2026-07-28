using UnityEngine;

public enum TargetType { Player, NPC } // 이상현상 대상
public enum SoundMode { Mute, Double } // 사운드 이상현상 종류

[CreateAssetMenu(fileName = "SoundAbnormalData", menuName = "Abnormal/SoundType")]
public class SoundAbnormalData : AbnormalData
{
    [Header("사운드 이상 현상 설정")]
    public TargetType targetType; 
    public SoundMode soundMode;
    public string targetName;


    public override void ApplyAbnormal(GameObject mapRoot) // 이상 현상을 적용하는 함수
    {
        bool isMute = (soundMode == SoundMode.Mute);
        bool isDouble = (soundMode == SoundMode.Double);

        if (targetType == TargetType.Player)
            ApplyToPlayer(isMute, isDouble);
        
        else
            ApplyToNPC(isMute, isDouble);
    }

    private static void ApplyToPlayer(bool isMute, bool isDouble) // 플레이어의 사운드 이상 현상을 적용하는 함수
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player == null)
            return;

        if (player.TryGetComponent(out FootstepController footstep))
            footstep.SetAbnormalStatus(isMute, isDouble);
    }

    private static void ApplyToNPC(bool isMute, bool isDouble) // NPC의 사운드 이상 현상을 적용하는 함수
    {
        NPCMovement npc = FindAnyObjectByType<NPCMovement>();
        if (npc == null)
            return;

        if (npc.TryGetComponent(out FootstepController footstep))
            footstep.SetAbnormalStatus(isMute, isDouble);
    }
}