using UnityEngine;

public enum TargetType { Player, NPC }
public enum SoundMode { Mute, Double }

[CreateAssetMenu(fileName = "SoundAbnormalData", menuName = "Abnormal/SoundType")]
public class SoundAbnormalData : AbnormalData
{

    [Header("사운드 이상 현상 설정")]
    public TargetType targetType; 
    public SoundMode soundMode;
    public string targetName;


    public override void ApplyAbnormal(GameObject mapRoot)
    {
        bool isMute = (soundMode == SoundMode.Mute);
        bool isDouble = (soundMode == SoundMode.Double);

        if (targetType == TargetType.Player)
        {
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                FootstepController fc = player.GetComponent<FootstepController>();
                if (fc != null)
                    fc.SetAbnormalStatus(isMute, isDouble);
            }
        }

        else 
        {
            NPCMovement npc = FindFirstObjectByType<NPCMovement>();
            if (npc != null)
            {
                FootstepController fc = npc.GetComponent<FootstepController>();
                if (fc != null)
                    npc.SetAbnormalStatus(isMute, isDouble);
            }     
        }
    }
}


