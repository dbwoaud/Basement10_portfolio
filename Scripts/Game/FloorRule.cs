using System;

public static class FloorRule
{
    public readonly struct MapInfo // 맵 생성 정보 
    {
        public bool UseFinalMap { get; } // 마지막 맵 생성 여부
        public bool AllowAbnormal { get; } // 이상현상 적용 여부

        public MapInfo(bool useFinalMap, bool allowAbnormal)
        {
            UseFinalMap = useFinalMap;
            AllowAbnormal = allowAbnormal;
        }
    }

    public static bool IsCorrect(TriggerType choice, bool hasAbnormal) // 이상현상 여부에 따른 게임 정답을 확인하는 함수
    {
        if (hasAbnormal)
            return choice == TriggerType.Return;
        
        else
            return choice == TriggerType.Exit;
    }

    public static int DecideNextMap(int currentFloor, int startFloor, bool isCorrect) // 플레이어 선택에 따른 층을 결정하는 함수
    {
        return isCorrect ? currentFloor - 1 : startFloor;
    }

    public static MapInfo ChoiceMap(int currentFloor, int startFloor, int targetFloor, bool isEndingScene) // 층에 따른 맵 정보를 선택하는 함수
    {
        if (isEndingScene)
            return new MapInfo(false, false);
        if (currentFloor == startFloor)
            return new MapInfo(false, false);
        if (currentFloor > targetFloor)
            return new MapInfo(false, true);

        return new MapInfo(true, false);
    }
}
