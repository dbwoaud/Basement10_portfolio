using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [SerializeField] private GameObject normalMapPrefab;
    [SerializeField] private GameObject finalMapPrefab;
    [SerializeField] private Vector3 finalMapOffset = new Vector3(0f, 0f, -5f);

    public GameObject CurrentMap { get; private set; }
    public AbnormalData CurrentAbnormal { get; private set; }
    public bool HasAbnormal => CurrentAbnormal != null;

    public void Spawn(FloorRule.MapPlan plan, Transform spawnPoint)
    {
        Clear();

        Vector3 spawnPos = spawnPoint.position;
        if (plan.UseFinalMap)
        {
            spawnPos += finalMapOffset;
        }

        GameObject prefabToSpawn = plan.UseFinalMap ? finalMapPrefab : normalMapPrefab;
        if (prefabToSpawn == null)
        {
            Debug.LogError("MapSpawner: Prefab to spawn is not assigned.");
            return;
        }

        CurrentMap = Instantiate(prefabToSpawn, spawnPos, spawnPoint.rotation);

        if (plan.AllowAbnormal)
        {
            if (SpawnAbnormalManager.HasInstance)
            {
                SpawnAbnormalManager.Instance.mapRoot = CurrentMap;
                CurrentAbnormal = SpawnAbnormalManager.Instance.SelectAbnormal();
                if (CurrentAbnormal != null)
                {
                    Debug.Log("이상현상 번호: " + CurrentAbnormal.abnormalName + "\n 이상현상 설명: " + CurrentAbnormal.abnormalDescription);
                }
                else
                {
                    Debug.Log("이번 루프에는 이상현상이 발생하지 않습니다.");
                }
            }
        }
        else
        {
            CurrentAbnormal = null;
        }
    }

    public void Clear()
    {
        if (CurrentMap != null)
        {
            Destroy(CurrentMap);
            CurrentMap = null;
        }
        CurrentAbnormal = null;
    }

    public void UpdateFloorDisplay(int floor, bool visible)
    {
        if (CurrentMap == null)
            return;

        FloorNumberDisplay display = CurrentMap.GetComponentInChildren<FloorNumberDisplay>();
        if (display != null)
        {
            if (visible)
            {
                display.SetFloorNumber(floor);
            }
            else
            {
                display.ResetFloorNumber();
            }
        }
    }
}
