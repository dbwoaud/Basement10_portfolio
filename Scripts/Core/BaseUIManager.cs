using UnityEngine;

public abstract class BaseUIManager<T> : MonoBehaviour where T : BaseUIManager<T>
{
    private static T instance;

    public static T Instance => instance;
    public static bool HasInstance => instance != null;

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this as T;

        AutoBindUI();
        InitializeUI();
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    protected abstract void AutoBindUI(); // UI 요소를 할당하는 함수

    protected virtual void InitializeUI() { } // UI를 초기화하는 함수

    protected static void PlayButtonSound() // 버튼 소리를 재생하는 함수
    {
        if (SoundManager.HasInstance)
            SoundManager.Instance.PlayButtonSound();
    }
}