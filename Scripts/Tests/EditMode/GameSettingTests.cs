using NUnit.Framework;
using UnityEngine;

public class GameSettingTests
{
    [Test]
    public void Validate는_볼륨을_영과_일_사이로_보정한다()
    {
        GameSetting settings = new GameSetting
        {
            masterVolume = 3.5f,
            bgmVolume = -2f,
            sfxVolume = 0.5f
        };

        settings.Validate();

        Assert.AreEqual(1f, settings.masterVolume, "마스터 볼륨 3.5f는 1f로 보정되어야 합니다.");
        Assert.AreEqual(0f, settings.bgmVolume, "BGM 볼륨 -2f는 0f로 보정되어야 합니다.");
        Assert.AreEqual(0.5f, settings.sfxVolume, "SFX 볼륨 0.5f는 그대로 유지되어야 합니다.");
    }

    [Test]
    public void Validate는_마우스_감도를_허용_범위로_보정한다()
    {
        GameSetting settings = new GameSetting { mouseSensitivity = 999f };
        settings.Validate();

        Assert.AreEqual(GameSetting.MaxSensitivity, settings.mouseSensitivity, "최대 감도를 초과하는 999f는 MaxSensitivity로 보정되어야 합니다.");

        settings.mouseSensitivity = 0f;
        settings.Validate();

        Assert.AreEqual(GameSetting.MinSensitivity, settings.mouseSensitivity, "최소 감도 미만인 0f는 MinSensitivity로 보정되어야 합니다.");
    }

    [Test]
    public void Validate는_현재_해상도를_뜻하는_음수를_보존한다()
    {
        GameSetting settings = new GameSetting { resolutionIndex = -1 };
        settings.Validate();

        Assert.AreEqual(-1, settings.resolutionIndex, "-1은 '현재 화면 사용'을 뜻하는 유효한 값이므로 보존되어야 합니다.");
    }

    [Test]
    public void Validate는_지원하지_않는_로케일_코드를_한국어로_되돌린다()
    {
        GameSetting settings = new GameSetting { languageCode = "fr" };
        settings.Validate();

        Assert.AreEqual(GameLanguages.Korean, settings.languageCode, "지원하지 않는 로케일 코드인 'fr'은 한국어('ko')로 복구되어야 합니다.");
    }

    [Test]
    public void Validate는_빈_로케일_코드를_한국어로_되돌린다()
    {
        GameSetting settings1 = new GameSetting { languageCode = null };
        settings1.Validate();
        Assert.AreEqual(GameLanguages.Korean, settings1.languageCode, "null 로케일 코드는 한국어('ko')로 복구되어야 합니다.");

        GameSetting settings2 = new GameSetting { languageCode = "" };
        settings2.Validate();
        Assert.AreEqual(GameLanguages.Korean, settings2.languageCode, "빈 로케일 코드는 한국어('ko')로 복구되어야 합니다.");
    }

    [Test]
    public void Clone은_원본과_독립적인_사본을_만든다()
    {
        GameSetting original = new GameSetting { bgmVolume = 0.3f };
        GameSetting copy = original.Clone();

        copy.bgmVolume = 0.9f;

        Assert.AreEqual(0.3f, original.bgmVolume, "Clone을 통해 생성된 사본의 값을 수정해도 원본 값은 유지되어야 합니다.");
    }

    [Test]
    public void IsSameAs는_언어만_달라도_거짓이다()
    {
        GameSetting a = new GameSetting();
        GameSetting b = a.Clone();

        Assert.IsTrue(a.IsSameAs(b), "동일하게 클론한 객체는 IsSameAs가 참이어야 합니다.");

        b.languageCode = GameLanguages.Japanese;
        Assert.IsFalse(a.IsSameAs(b), "언어 코드가 다르면 IsSameAs가 거짓이어야 합니다.");
    }
}

public class GameLanguagesTests
{
    [Test]
    public void 지원_언어는_네_개다()
    {
        Assert.AreEqual(4, GameLanguages.Supported.Length, "지원 언어(GameLanguages.Supported)의 길이는 4개여야 합니다.");
    }

    [Test]
    public void 시스템_언어가_지원_목록에_없으면_영어로_떨어진다()
    {
        Assert.AreEqual(GameLanguages.Korean,
            GameLanguages.SetLanguageOnSystem(SystemLanguage.Korean),
            "시스템 언어가 한국어일 때는 'ko'여야 합니다.");

        Assert.AreEqual(GameLanguages.Japanese,
            GameLanguages.SetLanguageOnSystem(SystemLanguage.Japanese),
            "시스템 언어가 일본어일 때는 'ja'여야 합니다.");

        Assert.AreEqual(GameLanguages.ChineseSimplified,
            GameLanguages.SetLanguageOnSystem(SystemLanguage.ChineseSimplified),
            "시스템 언어가 중국어 간체일 때는 'zh-Hans'여야 합니다.");

        Assert.AreEqual(GameLanguages.English,
            GameLanguages.SetLanguageOnSystem(SystemLanguage.French),
            "번역 지원이 없는 프랑스어(French) 등은 영어('en')로 안내되어야 합니다.");
    }

    [Test]
    public void 로케일_코드_지원_여부를_판별한다()
    {
        Assert.IsTrue(GameLanguages.IsSupported("ko"), "ko는 지원되어야 합니다.");
        Assert.IsTrue(GameLanguages.IsSupported("zh-Hans"), "zh-Hans는 지원되어야 합니다.");
        Assert.IsFalse(GameLanguages.IsSupported("zh-Hant"), "zh-Hant는 지원되지 않아야 합니다.");
        Assert.IsFalse(GameLanguages.IsSupported(""), "빈 문자열은 지원되지 않아야 합니다.");
        Assert.IsFalse(GameLanguages.IsSupported(null), "null은 지원되지 않아야 합니다.");
    }
}