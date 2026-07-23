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


    protected override void Apply(GameSetting settings)
    {
        AudioMixer target = ResolveMixer();

        if (target == null)
        {
            Debug.LogWarning("[설정] AudioMixer가 없어 볼륨을 적용하지 못했습니다.");
            return;
        }

        SetVolume(target, masterParameter, settings.masterVolume);
        SetVolume(target, bgmParameter, settings.bgmVolume);
        SetVolume(target, sfxParameter, settings.sfxVolume);
    }

    private AudioMixer ResolveMixer()
    {
        if (mixer == null && SoundManager.HasInstance)
            mixer = SoundManager.instance.Mixer;

        return mixer;
    }

    private static void SetVolume(AudioMixer target, string parameter, float linear)
    {
        if (string.IsNullOrEmpty(parameter))
            return;

        float decibel = linear <= MinLinear
            ? MinDecibel
            : Mathf.Log10(Mathf.Clamp01(linear)) * 20f;

        if (!target.SetFloat(parameter, decibel))
            Debug.LogWarning($"[설정] 믹서 파라미터 '{parameter}'를 찾지 못했습니다.");
    }
}