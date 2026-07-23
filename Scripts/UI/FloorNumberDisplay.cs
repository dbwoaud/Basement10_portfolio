using System.Collections.Generic;
using UnityEngine;

public class FloorNumberDisplay : MonoBehaviour
{
    [Header("숫자 오브젝트 설정")]
    [SerializeField] private GameObject[] numberPrefabs = new GameObject[10];

    [Header("배치 설정")]
    [SerializeField] private float space = 0.5f;
    [SerializeField] private Vector3 numberScale = new Vector3(3f, 3f, 3f);
    [SerializeField] private bool centerAlign = true;
    [SerializeField] private int maxDigits = 2;

    private GameObject[][] slots;
    private readonly List<int> digitBuffer = new List<int>(4);
    private bool isInitialized;


    private void Awake()
    {
        BuildPool();
    }

    private void BuildPool() // 오브젝트 풀을 빌드하는 함수
    {
        if (isInitialized)
            return;

        if (numberPrefabs == null || numberPrefabs.Length < 10)
            return;
        
        slots = new GameObject[maxDigits][];

        for (int slot = 0; slot < maxDigits; slot++)
        {
            slots[slot] = new GameObject[10];
            for (int digit = 0; digit < 10; digit++)
            {
                if (numberPrefabs[digit] == null)
                    continue;

                GameObject obj = Instantiate(numberPrefabs[digit], transform);
                obj.name = $"Slot{slot}_Digit{digit}";
                obj.transform.localScale = numberScale;
                obj.SetActive(false);
                slots[slot][digit] = obj;
            }
        }

        isInitialized = true;
    }

    public void SetFloorNumber(int floor) // 현재 층을 씬에 배치하는 함수
    {
        BuildPool();

        if (!isInitialized)
            return;

        ResetFloorNumber();
        GetDigits(floor, digitBuffer);

        int count = Mathf.Min(digitBuffer.Count, maxDigits);
        float totalWidth = (count - 1) * space;
        float startX = centerAlign ? totalWidth / 2f : 0f;

        for (int i = 0; i < count; i++)
        {
            GameObject obj = slots[i][digitBuffer[i]];
            if (obj == null)
                continue;

            obj.transform.localPosition = new Vector3(startX - (i * space), 0f, 0f);
            obj.SetActive(true);
        }
    }

    public void ResetFloorNumber() // 현재 층 숫자를 리셋하는 함수
    {
        if (!isInitialized)
            return;

        for (int slot = 0; slot < slots.Length; slot++)
        {
            for (int digit = 0; digit < 10; digit++)
            {
                if (slots[slot][digit] != null)
                    slots[slot][digit].SetActive(false);
            }
        }
    }

    private static void GetDigits(int number, List<int> result) // 현재 층 표시에 필요한 정수를 배열에 저장하는 함수
    {
        result.Clear();
        int temp = Mathf.Abs(number);
        if (temp == 0)
        {
            result.Add(0);
            return;
        }

        while (temp > 0)
        {
            result.Add(temp % 10);
            temp /= 10;
        }

        result.Reverse();
    }
}
