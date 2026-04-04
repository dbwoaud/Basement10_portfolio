using System.Collections.Generic;
using UnityEngine;

public class FloorNumberDisplay : MonoBehaviour
{
    [Header("숫자 오브젝트 설정")]
    [SerializeField] private GameObject[] numberPrefabs;
    [SerializeField] private List<GameObject> numberPool = new List<GameObject>();

    [Header("배치 설정")]
    [SerializeField] private float space = 0.5f;
    [SerializeField] private Vector3 numberScale = new Vector3(3f, 3f, 3f);
    [SerializeField] private bool centerAlign = true; 
    public void SetFloorNumber(int floor) // 현재 층을 씬에 배치하는 함수
    {
        ResetFloorNumber();
        List<int> digits = GetDigits(floor);

        PreparePool(digits.Count);

        float totalWidth = (digits.Count - 1) * space;
        float startX = centerAlign ? totalWidth / 2f : 0f;
        for (int i = 0; i < digits.Count; i++)
        {
            int digit = digits[i];
            UpdateNumberObject(i, digit);
            GameObject obj = numberPool[i];
            float posX = startX - (i * space);
            obj.transform.localPosition = new Vector3(posX, 0, 0);
            obj.transform.localScale = numberScale;
            obj.SetActive(true);
        }

        for (int i = digits.Count; i < numberPool.Count; i++)
            numberPool[i].SetActive(false);
    }

    private List<int> GetDigits(int number) // 현재 층에 표현하는데 필요한 숫자 리스트를 반환하는 함수
    {
        List<int> digits = new List<int>();
        if (number == 0) 
            digits.Add(0);

        int temp = Mathf.Abs(number);
        while (temp > 0)
        {
            digits.Add(temp % 10);
            temp /= 10;
        }

        digits.Reverse();
        return digits;
    }

    private void PreparePool(int requiredCount) // 층 숫자 오브젝트를 풀에 저장하는 함수
    {
        while (numberPool.Count < requiredCount)
        {
            GameObject newObj = Instantiate(numberPrefabs[0], transform);
            newObj.SetActive(false);
            numberPool.Add(newObj);
        }
    }

    private void UpdateNumberObject(int index, int digit) // 층 숫자 오브젝트를 업데이트 하는 함수
    {
        if (numberPool[index].name != $"Digit_{digit}(Clone)")
        {
            Destroy(numberPool[index]);
            numberPool[index] = Instantiate(numberPrefabs[digit], transform);
            numberPool[index].name = $"Digit_{digit}(Clone)";
        }
    }

    public void ResetFloorNumber() // 현재 층 숫자를 리셋하는 함수
    {
        foreach (var obj in numberPool)
        {
            if (obj != null) 
                obj.SetActive(false);
        }
    }
}
