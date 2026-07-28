using System;
using System.IO;
using UnityEngine;

public class SettingManager : Singleton<SettingManager>
{
    public static event Action<GameSetting> OnSettingsApplied;

    public GameSetting Current { get; private set; }

    private const string FileName = "settings.json";
    private const string TempExtension = ".tmp";
    private const string BackupExtension = ".bak";

    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap() // 초기 설정을 수행하는 함수
    {
        if (HasInstance)
            return;

        new GameObject("SettingManager").AddComponent<SettingManager>();
    }

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        Load();
    }

    private void Start()
    {
        OnSettingsApplied?.Invoke(Current);
    }

    public void Load() // 설정 정보를 불러오는 함수
    {
        Current = ReadFromDisk() ?? GameSetting.CreateDefault();
        Current.Validate();
    }

    private GameSetting ReadFromDisk() // 디스크에서 설정 정보를 불러오는 함수
    {
        if (!File.Exists(SavePath))
            return null;

        try
        {
            string json = File.ReadAllText(SavePath);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            GameSetting loaded = JsonUtility.FromJson<GameSetting>(json);

            if (loaded == null)
                return null;

            return Migrate(loaded);
        }
        catch
        {
            QuarantineBrokenFile();
            return null;
        }
    }

    private GameSetting Migrate(GameSetting loaded) // 설정 버전을 업데이트하는 함수
    {
        if (loaded.version == GameSetting.CurrentVersion)
            return loaded;

        if (loaded.version > GameSetting.CurrentVersion)
            return null;

        int guard = 0;

        while (loaded.version < GameSetting.CurrentVersion)
        {
            int before = loaded.version;

            switch (loaded.version)
            {
                case 1:
                    loaded.resolutionIndex = -1;
                    loaded.displayModeIndex = ToDisplayModeIndex(Screen.fullScreenMode);
                    loaded.languageCode = GameLanguages.SetLanguageOnSystem(Application.systemLanguage);
                    loaded.version = 2;
                    break;

                default:
                    return null;
            }

            if (++guard > 16)
                return null;
        }

        return loaded;
    }

    private static int ToDisplayModeIndex(FullScreenMode mode) // 화면 모드 설정 인덱스를 반환하는 함수
    {
        for (int i = 0; i < DisplayOptions.DisplayModes.Length; i++)
        {
            if (DisplayOptions.DisplayModes[i] == mode)
                return i;
        }

        return 0;
    }

    private void QuarantineBrokenFile() // 깨진 파일을 삭제하는 함수
    {
        try
        {
            string broken = SavePath + ".broken";
            TryDelete(broken);
            File.Move(SavePath, broken);
        }
        catch
        {

        }
    }

    public bool Commit(GameSetting draft) // 게임 설정 정보를 저장하는 함수
    {
        if (draft == null)
            return false;

        Current = draft.Clone();
        Current.Validate();

        bool saved = WriteToDisk();
        OnSettingsApplied?.Invoke(Current);
        return saved;
    }

    private bool WriteToDisk() // 설정 정보를 디스크에 저장하는 함수
    {
        string tempPath = SavePath + TempExtension;
        string backupPath = SavePath + BackupExtension;

        try
        {
            File.WriteAllText(tempPath, JsonUtility.ToJson(Current, true));

            if (File.Exists(SavePath))
                File.Replace(tempPath, SavePath, backupPath, ignoreMetadataErrors: true);
            else
                File.Move(tempPath, SavePath);

            return true;
        }
        catch
        {
            TryDelete(tempPath);
            return false;
        }
    }

    private static void TryDelete(string path) // 파일 삭제를 시도하는 함수
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {

        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/설정/저장 파일 삭제")]
    private static void DeleteSaveFile()
    {
        TryDelete(SavePath);
        TryDelete(SavePath + TempExtension);
        TryDelete(SavePath + BackupExtension);
    }

    [UnityEditor.MenuItem("Tools/설정/저장 폴더 열기")]
    private static void OpenSaveFolder()
    {
        UnityEditor.EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
#endif
}