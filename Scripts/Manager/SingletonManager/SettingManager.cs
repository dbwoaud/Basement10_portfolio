using System;
using System.IO;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
    public static SettingManager instance;
    public static event Action<GameSetting> OnSettingsApplied;

    public GameSetting Current { get; private set; }

    private const string FileName = "settings.json";
    private const string TempExtension = ".tmp";
    private const string BackupExtension = ".bak";

    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) 
            return;

        GameObject go = new GameObject("[SettingsManager]");
        go.AddComponent<SettingManager>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }


    public void Load()
    {
        Current = ReadFromDisk() ?? new GameSetting();
        Current.Validate();
    }

    private GameSetting ReadFromDisk()
    {
        if (!File.Exists(SavePath)) return null;

        try
        {
            string json = File.ReadAllText(SavePath);
            if (string.IsNullOrWhiteSpace(json)) return null;

            GameSetting loaded = JsonUtility.FromJson<GameSetting>(json);
            if (loaded == null) 
                return null;

            return Migrate(loaded);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[설정] 파일을 읽지 못해 기본값으로 시작합니다. ({e.Message})");
            BackupBrokenFile();
            return null;
        }
    }

    private GameSetting Migrate(GameSetting loaded)
    {
        if (loaded.version == GameSetting.CurrentVersion) return loaded;

        Debug.Log($"[설정] 저장 버전 {loaded.version} → {GameSetting.CurrentVersion} 변환");
        loaded.version = GameSetting.CurrentVersion;
        return loaded;
    }

    private void BackupBrokenFile()
    {
        try
        {
            string backup = SavePath + BackupExtension;
            if (File.Exists(backup)) File.Delete(backup);
            File.Move(SavePath, backup);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[설정] 손상 파일 백업 실패: {e.Message}");
        }
    }

    public bool Commit(GameSetting draft)
    {
        if (draft == null) return false;

        Current = draft.Clone();
        Current.Validate();

        bool saved = WriteToDisk();

        OnSettingsApplied?.Invoke(Current);

        return saved;
    }

    private bool WriteToDisk()
    {
        string tempPath = SavePath + TempExtension;

        try
        {
            File.WriteAllText(tempPath, JsonUtility.ToJson(Current, true));

            if (File.Exists(SavePath)) File.Delete(SavePath);
            File.Move(tempPath, SavePath);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[설정] 저장에 실패했습니다: {e.Message}");

            try { if (File.Exists(tempPath)) File.Delete(tempPath); }
            catch { /* 정리 실패는 무시 */ }

            return false;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/설정/저장 파일 삭제")]
    private static void DeleteSaveFile()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log($"[설정] 삭제 완료: {SavePath}");
        }
        else
        {
            Debug.Log("[설정] 저장 파일이 없습니다.");
        }
    }

    [UnityEditor.MenuItem("Tools/설정/저장 폴더 열기")]
    private static void OpenSaveFolder()
    {
        UnityEditor.EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
#endif
}