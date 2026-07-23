using System;
using System.Collections;
using UnityEngine;

public class FootstepController : MonoBehaviour
{
    [Header("발소리 설정")]
    [SerializeField] private AudioClip walkSound;
    [SerializeField] private float defaultWalkDuration = 0.5f;
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private float doubleSoundDelay = 0.75f;

    [Header("상태")]
    [SerializeField] private float walkTimer;
    [SerializeField] private bool isForceStopped;
    [SerializeField] private bool isMuted;
    [SerializeField] private bool isDoubleSound;


    void Start()
    {
        walkTimer = defaultWalkDuration / 2f;
    }

    public void CalculateAndPlayFootstep(bool isMoving, float speedRatio = 1f) // 발자국 주기를 계산하고 발자국 소리를 재생하는 함수
    {
        if (isForceStopped || !isMoving)
        {
            walkTimer = defaultWalkDuration / 2f;
            return;
        }

        if (walkTimer > 0f)
            return;

        PlayFootstep();
        walkTimer = defaultWalkDuration / Mathf.Max(speedRatio, 0.01f);
    }

    private void PlayFootstep() // 발자국 소리를 재생하는 함수
    {
        if (isMuted || walkSound == null || !SoundManager.HasInstance)
            return;

        SoundManager.Instance.PlaySFX(walkSound, volume);

        if (isDoubleSound)
            StartCoroutine(PlayDoubleSoundRoutine());
    }

    private IEnumerator PlayDoubleSoundRoutine(float delay = 0.75f) // 발자국 소리가 두번 들리는 이상현상을 위한 코루틴
    {
        yield return new WaitForSeconds(delay);

        if (SoundManager.HasInstance)
            SoundManager.Instance.PlaySFX(walkSound, volume);
    }

    public void StopFootsteps() // 캐릭터 걷기를 강제로 중단시키는 함수
    {
        isForceStopped = true;
        StopAllCoroutines();
    }

    public void SetAbnormalStatus(bool mute, bool doubleSound) // 이상 현상 상태를 설정하는 함수
    {
        isForceStopped = false;
        isMuted = mute;
        isDoubleSound = doubleSound;
    }
}
