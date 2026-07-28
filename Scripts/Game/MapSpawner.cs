using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [Header("맵 생성 설정")]
    [SerializeField] private GameObject normalMapPrefab; // 일반 맵 프리팹
    [SerializeField] private GameObject finalMapPrefab; // 마지막 맵 프리팹
    [SerializeField] private Vector3 finalMapOffset = new Vector3(0f, 0f, -5f); // 마지막 맵 생성 오프셋
    
    public GameObject CurrentMap { get; private set; } // 현재맵
    public AbnormalData CurrentAbnormal { get; private set; } // 현재 이상현상
    public bool HasAbnormal => CurrentAbnormal != null; // 현재 이상현상 여부


    public void Spawn(FloorRule.MapInfo plan, Transform spawnPoint) // 맵을 생성하는 함수
    {
        Clear();
        Vector3 spawnPos = spawnPoint.position;
        if (plan.UseFinalMap)
            spawnPos += finalMapOffset;
        
        GameObject prefabToSpawn = plan.UseFinalMap ? finalMapPrefab : normalMapPrefab;
        if (prefabToSpawn == null)
        {
#if UNITY_EDITOR
            Debug.LogError("MapSpawner: 생성할 맵 프리팹이 할당되지 않았습니다.", this);
#endif
            return;
        }

        CurrentMap = Instantiate(prefabToSpawn, spawnPos, spawnPoint.rotation);
        if (plan.AllowAbnormal)
        {
            if (SpawnAbnormalManager.HasInstance)
            {
                SpawnAbnormalManager.Instance.mapRoot = CurrentMap;
                CurrentAbnormal = SpawnAbnormalManager.Instance.SelectAbnormal();
            }
        }
        else
            CurrentAbnormal = null;
    }

    public void Clear() // 현재 맵을 초기화하는 함수
    {
        if (CurrentMap != null)
        {
            CurrentMap.SetActive(false);
            Destroy(CurrentMap);
            CurrentMap = null;
        }
        CurrentAbnormal = null;
    }

    public void UpdateFloorDisplay(int floor, bool visible) // 현재 층을 맵에 생성하는 함수
    {
        if (CurrentMap == null)
            return;

        FloorNumberDisplay display = CurrentMap.GetComponentInChildren<FloorNumberDisplay>();
        if (display != null)
        {
            if (visible)
                display.SetFloorNumber(floor);
            else 
                display.ResetFloorNumber();
        }
    }
}
