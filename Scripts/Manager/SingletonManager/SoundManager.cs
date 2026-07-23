using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [Header("오디오 소스")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource voiceAudioSource;

    [Header("오디오 클립")]
    [SerializeField] private AudioClip elevatorButtonSound;
    [SerializeField] private AudioClip elevatorDoorSound;
    [SerializeField] private AudioClip elevatorMovingSound;
    [SerializeField] private AudioClip elevatorFinishSound;
    [SerializeField] private AudioClip badEndingBGM;
    [SerializeField] private AudioClip eyeOpeningBGM;
    [SerializeField] private AudioClip trueEndingBGM;
    [SerializeField] private AudioClip endingCreditBGM;

    public AudioClip BadEndingBGM => badEndingBGM;
    public AudioClip EyeOpeningBGM => eyeOpeningBGM;
    public AudioClip TrueEndingBGM => trueEndingBGM;
    public AudioClip EndingCreditBGM => endingCreditBGM;

    public void PlayBGM(AudioClip audioClip, float volume = 1.0f) // 배경음악 재생 함수
    {
        if (bgmAudioSource == null || audioClip == null)
            return;

        if (bgmAudioSource.clip == audioClip && bgmAudioSource.isPlaying) 
            return;

        bgmAudioSource.Stop();
        bgmAudioSource.volume = volume;
        bgmAudioSource.clip = audioClip;
        bgmAudioSource.Play();        
    }

    public void PlaySFX(AudioClip audioClip, float volume = 1.0f) // 효과음 재생 함수
    {
        if (sfxAudioSource != null && audioClip != null)
            sfxAudioSource.PlayOneShot(audioClip, sfxAudioSource.volume * volume);
    }

    public void PlayButtonSound() // 버튼 클릭 소리 재생 함수
    {
        PlaySFX(elevatorButtonSound);
    }

    public void PlayElevatorDoorSound() // 엘리베이터 문 소리 재생 함수
    {
        PlaySFX(elevatorDoorSound);
    }

    public void PlayElevatorMovingSound() // 엘리베이터 동작 소리 재생 함수
    {
        PlayBGM(elevatorMovingSound);
    }

    public void PlayElevatorFinishSound() // 엘리베이터 정지 소리 재생 함수 
    {
        PlaySFX(elevatorFinishSound);
    }

    public void PlayVoice(AudioClip audioClip, float volume = 1.0f) // 나레이션 소리 재생 함수
    {
        if (voiceAudioSource == null || audioClip == null)
            return;

        voiceAudioSource.Stop();
        voiceAudioSource.clip = audioClip;
        voiceAudioSource.volume = volume;
        voiceAudioSource.Play();
    }

    public void StopBGM() // 배경음악 중지 함수
    {
        if(bgmAudioSource != null)
            bgmAudioSource.Stop();
    }

    public void StopSFX() // 효과음 중지 함수
    {
        if (sfxAudioSource != null)
            sfxAudioSource.Stop();
    }

    public void StopVoice() // 나레이션 소리 중지 함수
    {
        if (voiceAudioSource != null)
            voiceAudioSource.Stop();
    }

    public void StopAllSound() // 모든 소리 중지 함수
    {
        StopBGM();
        StopSFX();
        StopVoice();
    }
}
