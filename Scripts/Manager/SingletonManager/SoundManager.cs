using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    [Header("�ͼ�")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("����� �ҽ�")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource ambienceAudioSource;

    [Header("����� Ŭ��")]
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


    private void RouteToMixerGroups() // �ͼ� �׷쿡 �����ϴ� �Լ�
    {
        AssignGroup(bgmAudioSource, bgmGroup);
        AssignGroup(sfxAudioSource, sfxGroup);
        AssignGroup(ambienceAudioSource, sfxGroup);
    }

    private static void AssignGroup(AudioSource source, AudioMixerGroup group) // ����� �ҽ��� �׷쿡 �Ҵ��ϴ� �Լ�
    {
        if (source != null && group != null)
            source.outputAudioMixerGroup = group;
    }

    public void PlayBGM(AudioClip audioClip, float volume = 1.0f) // ������� ��� �Լ�
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

    public void PlaySFX(AudioClip audioClip, float volume = 1.0f) // ȿ���� ��� �Լ�
    {
        if (sfxAudioSource == null || audioClip == null)
            return;

        sfxAudioSource.PlayOneShot(audioClip, Mathf.Clamp01(volume));
    }

    public void PlayAmbience(AudioClip audioClip, float volume = 1.0f) // ������ ��� �Լ�
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

    public void PlayButtonSound() // ��ư Ŭ�� �Ҹ� ��� �Լ�
    {
        PlaySFX(elevatorButtonSound);
    }

    public void PlayElevatorDoorSound() // ���������� �� �Ҹ� ��� �Լ�
    {
        PlaySFX(elevatorDoorSound);
    }

    public void PlayElevatorFinishSound() // ���������� ���� �Ҹ� ��� �Լ� 
    {
        PlaySFX(elevatorFinishSound);
    }

    public void PlayElevatorMovingSound()
    { 
        PlayAmbience(elevatorMovingSound); 
    }

    public void StopBGM() // ������� ���� �Լ�
    {
        if(bgmAudioSource != null)
            bgmAudioSource.Stop();
    }

    public void StopSFX() // ȿ���� ���� �Լ�
    {
        if (sfxAudioSource != null)
            sfxAudioSource.Stop();
    }

    public void StopAmbience() // ������ ���� �Լ�
    {
        if (ambienceAudioSource != null)
            ambienceAudioSource.Stop();
    }

    public void StopAllSound() // ��� �Ҹ� ���� �Լ�
    {
        StopBGM();
        StopSFX();
        StopAmbience();
    }

    public void PauseGameplay()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
            bgmAudioSource.Pause();

        if (ambienceAudioSource != null && ambienceAudioSource.isPlaying)
            ambienceAudioSource.Pause();
    }

    public void ResumeGameplay()
    {
        if (bgmAudioSource != null)
            bgmAudioSource.UnPause();

        if (ambienceAudioSource != null)
            ambienceAudioSource.UnPause();
    }
}
