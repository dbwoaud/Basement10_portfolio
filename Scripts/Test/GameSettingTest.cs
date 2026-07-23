using NUnit.Framework;
using UnityEngine;

public class GameSettingTests
{
    [Test]
    public void Validate는_볼륨을_0에서_1_사이로_보정한다()
    {
        GameSetting settings = new GameSetting
        {
            masterVolume = 3.5f,
            bgmVolume = -2f,
            sfxVolume = 0.5f
        };

        settings.Validate();

        Assert.AreEqual(1f, settings.masterVolume);
        Assert.AreEqual(0f, settings.bgmVolume);
        Assert.AreEqual(0.5f, settings.sfxVolume);
    }

    [Test]
    public void Validate는_마우스_감도를_허용_범위로_보정한다()
    {
        GameSetting settings = new GameSetting { mouseSensitivity = 999f };
        settings.Validate();

        Assert.AreEqual(GameSetting.MaxSensitivity, settings.mouseSensitivity);

        settings.mouseSensitivity = 0f;
        settings.Validate();

        Assert.AreEqual(GameSetting.MinSensitivity, settings.mouseSensitivity);
    }

    [Test]
    public void Validate는_현재_해상도를_뜻하는_음수를_보존한다()
    {
        GameSetting settings = new GameSetting { resolutionIndex = -1 };
        settings.Validate();

        Assert.AreEqual(-1, settings.resolutionIndex,
            "-1은 '현재 화면 사용'을 뜻하는 유효한 값이다.");
    }

    [Test]
    public void Validate는_지원하지_않는_로케일_코드를_한국어로_되돌린다()
    {
        GameSetting settings = new GameSetting { languageCode = "fr" };
        settings.Validate();

        Assert.AreEqual(GameLanguages.Korean, settings.languageCode);
    }

    [Test]
    public void Validate는_빈_로케일_코드를_한국어로_되돌린다()
    {
        GameSetting settings = new GameSetting { languageCode = null };
        settings.Validate();

        Assert.AreEqual(GameLanguages.Korean, settings.languageCode);
    }

    [Test]
    public void Clone은_원본과_독립적인_사본을_만든다()
    {
        GameSetting original = new GameSetting { bgmVolume = 0.3f };
        GameSetting copy = original.Clone();

        copy.bgmVolume = 0.9f;

        Assert.AreEqual(0.3f, original.bgmVolume);
    }

    [Test]
    public void IsSameAs는_언어만_달라도_거짓이다()
    {
        GameSetting a = new GameSetting();
        GameSetting b = a.Clone();

        Assert.IsTrue(a.IsSameAs(b));

        b.languageCode = GameLanguages.Japanese;
        Assert.IsFalse(a.IsSameAs(b));
    }

    [Test]
    public void IsDisplayChangedFrom은_화면_항목만_본다()
    {
        GameSetting a = new GameSetting();
        GameSetting b = a.Clone();

        b.bgmVolume = 0.1f;
        Assert.IsFalse(a.IsDisplayChangedFrom(b), "볼륨 변경은 화면 변경이 아니다.");

        b.displayModeIndex = 2;
        Assert.IsTrue(a.IsDisplayChangedFrom(b));
    }
}

public class GameLanguagesTests
{
    [Test]
    public void 지원_언어는_네_개다()
    {
        Assert.AreEqual(4, GameLanguages.Supported.Length);
    }

    [Test]
    public void 시스템_언어가_지원_목록에_없으면_영어로_떨어진다()
    {
        Assert.AreEqual(GameLanguages.Korean,
            GameLanguages.FromSystemLanguage(SystemLanguage.Korean));

        Assert.AreEqual(GameLanguages.Japanese,
            GameLanguages.FromSystemLanguage(SystemLanguage.Japanese));

        Assert.AreEqual(GameLanguages.ChineseSimplified,
            GameLanguages.FromSystemLanguage(SystemLanguage.ChineseSimplified));

        Assert.AreEqual(GameLanguages.English,
            GameLanguages.FromSystemLanguage(SystemLanguage.French),
            "번역이 없는 언어는 한국어가 아니라 영어로 안내한다.");
    }

    [Test]
    public void 로케일_코드_지원_여부를_판별한다()
    {
        Assert.IsTrue(GameLanguages.IsSupported("ko"));
        Assert.IsTrue(GameLanguages.IsSupported("zh-Hans"));
        Assert.IsFalse(GameLanguages.IsSupported("zh-Hant"));
        Assert.IsFalse(GameLanguages.IsSupported(""));
        Assert.IsFalse(GameLanguages.IsSupported(null));
    }
}