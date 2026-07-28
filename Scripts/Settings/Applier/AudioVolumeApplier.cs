using UnityEngine;
using UnityEngine.Audio;

public class AudioVolumeApplier : SettingApplierBase
{
    private const float MinDecibel = -80f;
    private const float MinLinear = 0.0001f;

    [Header("오디오 믹서")]
    [SerializeField] private AudioMixer mixer;

    [Header("노출 파라미터 이름")]
    [SerializeField] private string masterParameter = "MasterVolume";
    [SerializeField] private string bgmParameter = "BGMVolume";
    [SerializeField] private string sfxParameter = "SFXVolume";


    protected override void Apply(GameSetting settings) // 게임 설정을 적용하는 함수
    {   
        AudioMixer target = GetMixer();

        if (target == null)
            return;

        SetVolume(target, masterParameter, settings.masterVolume);
        SetVolume(target, bgmParameter, settings.bgmVolume);
        SetVolume(target, sfxParameter, settings.sfxVolume);
    }

    private AudioMixer GetMixer()
    {
        if (mixer == null && SoundManager.HasInstance)
            mixer = SoundManager.Instance.Mixer;

        return mixer;
    }

    private void SetVolume(AudioMixer target, string parameter, float linear) // 오디오 볼륨을 설정하는 함수
    {
        if (string.IsNullOrEmpty(parameter))
            return;

        float decibel = linear <= MinLinear
            ? MinDecibel
            : Mathf.Log10(Mathf.Clamp01(linear)) * 20f;

        target.SetFloat(parameter, decibel);
    }
}