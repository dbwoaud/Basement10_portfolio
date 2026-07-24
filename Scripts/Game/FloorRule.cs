using System;

public static class FloorRule
{
    public readonly struct MapPlan
    {
        public bool UseFinalMap { get; }
        public bool AllowAbnormal { get; }

        public MapPlan(bool useFinalMap, bool allowAbnormal)
        {
            UseFinalMap = useFinalMap;
            AllowAbnormal = allowAbnormal;
        }
    }

    public static bool IsCorrect(TriggerType choice, bool hasAbnormal)
    {
        if (hasAbnormal)
        {
            return choice == TriggerType.Return;
        }
        else
        {
            return choice == TriggerType.Exit;
        }
    }

    public static int ResolveNextFloor(int currentFloor, int startFloor, bool isCorrect)
    {
        return isCorrect ? currentFloor - 1 : startFloor;
    }

    public static MapPlan ResolveMapPlan(int currentFloor, int startFloor, int targetFloor, bool isEndingScene)
    {
        if (isEndingScene)
        {
            return new MapPlan(false, false);
        }
        if (currentFloor == startFloor)
        {
            return new MapPlan(false, false);
        }
        if (currentFloor > targetFloor)
        {
            return new MapPlan(false, true);
        }
        return new MapPlan(true, false);
    }
}
