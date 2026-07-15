using System.Collections;
using UnityEngine;
public class FootstepController : MonoBehaviour
{
    [Header("발소리 설정")]
    [SerializeField] private float defaultWalkDuration = 0.5f;
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private float walkTimer;
    public bool isForceStopped = false;
    
    [Header("이상 현상 설정")]
    private bool isMuted = false;
    private bool isDoubleSound = false;

    void Start()
    {
        walkTimer = defaultWalkDuration / 2f;
    }

    public void CalculateAndPlayFootstep(bool isMoving, float speedRatio = 1f) // 발자국 주기를 계산하는 함수
    {
        if (isForceStopped)
            return;

        if(!isMoving)
        {
            walkTimer = defaultWalkDuration / 2f;
            return;
        }

        walkTimer -= Time.deltaTime;
        if(walkTimer <= 0f)
        {
            PlaySoundLogic();
            walkTimer = defaultWalkDuration / speedRatio;
        }
    }
    public void StopFootsteps() // 캐릭터 걷기를 중단시키는 함수
    {
        isForceStopped = true;
        StopAllCoroutines();
    }

    private void PlaySoundLogic() // 발자국 소리를 재생하는 함수
    {
        if (isMuted || walkSound == null || SoundManager.instance == null)
            return;

        SoundManager.instance.PlaySFX(walkSound, 0.5f);
        if(isDoubleSound)
            StartCoroutine(PlayDoubleSoundRoutine());
        
    }

    private IEnumerator PlayDoubleSoundRoutine(float delay = 0.75f) // 발자국 소리가 두번 들리는 이상현상을 위한 코루틴
    {
        yield return new WaitForSeconds(delay);
        if (SoundManager.instance != null)
            SoundManager.instance.PlaySFX(walkSound, 0.5f);   
    }

    public void SetAbnormalStatus(bool mute, bool doubleSound)
    {
        isForceStopped = false;
        isMuted = mute;
        isDoubleSound = doubleSound;
    }
}
