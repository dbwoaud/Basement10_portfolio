using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    [Header("믹서")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("오디오 소스")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource ambienceAudioSource;

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

    public AudioMixer Mixer => mixer;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
            return;

        RouteToMixerGroups();
    }


    private void RouteToMixerGroups() // 믹서 그룹에 연결하는 함수
    {
        AssignGroup(bgmAudioSource, bgmGroup);
        AssignGroup(sfxAudioSource, sfxGroup);
        AssignGroup(ambienceAudioSource, sfxGroup);
    }

    private static void AssignGroup(AudioSource source, AudioMixerGroup group) // 오디오 소스를 그룹에 할당하는 함수
    {
        if (source != null && group != null)
            source.outputAudioMixerGroup = group;
    }

    public void PlayBGM(AudioClip audioClip, float volume = 1.0f) // 배경음악 재생 함수
    {
        if (bgmAudioSource == null || audioClip == null)
            return;

        if (bgmAudioSource.clip == audioClip && bgmAudioSource.isPlaying) 
            return;

        bgmAudioSource.Stop();
        bgmAudioSource.volume = Mathf.Clamp01(volume);
        bgmAudioSource.clip = audioClip;
        bgmAudioSource.Play();        
    }

    public void PlaySFX(AudioClip audioClip, float volume = 1.0f) // 효과음 재생 함수
    {
        if (sfxAudioSource == null || audioClip == null)
            return;

        sfxAudioSource.PlayOneShot(audioClip, Mathf.Clamp01(volume));
    }

    public void PlayAmbience(AudioClip audioClip, float volume = 1.0f) // 현장음 재생 함수
    {
        if (ambienceAudioSource == null || audioClip == null)
            return;

        if (ambienceAudioSource.clip == audioClip && ambienceAudioSource.isPlaying)
            return;

        ambienceAudioSource.Stop();
        ambienceAudioSource.clip = audioClip;
        ambienceAudioSource.volume = Mathf.Clamp01(volume);
        ambienceAudioSource.loop = true;
        ambienceAudioSource.Play();
    }

    public void PlayButtonSound() // 버튼 클릭 소리 재생 함수
    {
        PlaySFX(elevatorButtonSound);
    }

    public void PlayElevatorDoorSound() // 엘리베이터 문 소리 재생 함수
    {
        PlaySFX(elevatorDoorSound);
    }

    public void PlayElevatorFinishSound() // 엘리베이터 정지 소리 재생 함수 
    {
        PlaySFX(elevatorFinishSound);
    }

    public void PlayElevatorMovingSound()
    { 
        PlayAmbience(elevatorMovingSound); 
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

    public void StopAmbience() // 현장음 중지 함수
    {
        if (ambienceAudioSource != null)
            ambienceAudioSource.Stop();
    }

    public void StopAllSound() // 모든 소리 중지 함수
    {
        StopBGM();
        StopSFX();
        StopAmbience();
    }
}
