using System;
using System.Collections.Generic;

public class FloorProgress
{
    private readonly HashSet<int> visitedFloors = new HashSet<int>();

    public int StartFloor { get; }
    public int TargetFloor { get; }
    public int CurrentFloor { get; private set; }
    public bool IsReturningFromFailure { get; private set; }

    public bool IsCleared => CurrentFloor == TargetFloor;

    public FloorProgress(int startFloor, int targetFloor)
    {
        if (startFloor < targetFloor)
        {
            throw new ArgumentException("startFloor cannot be less than targetFloor.");
        }
        StartFloor = startFloor;
        TargetFloor = targetFloor;
        Reset();
    }

    public void Reset()
    {
        CurrentFloor = StartFloor;
        IsReturningFromFailure = false;
        visitedFloors.Clear();
    }

    public bool Submit(TriggerType choice, bool hasAbnormal)
    {
        if (IsCleared)
        {
            return false;
        }

        bool isCorrect = FloorRule.IsCorrect(choice, hasAbnormal);
        CurrentFloor = FloorRule.ResolveNextFloor(CurrentFloor, StartFloor, isCorrect);
        IsReturningFromFailure = !isCorrect;
        return isCorrect;
    }

    public bool TryMarkVisited()
    {
        if (CurrentFloor < TargetFloor || CurrentFloor > StartFloor)
        {
            return false;
        }

        if (visitedFloors.Contains(CurrentFloor))
        {
            return false;
        }

        visitedFloors.Add(CurrentFloor);
        return true;
    }

    public bool ConsumeReturningFlag()
    {
        if (IsReturningFromFailure)
        {
            IsReturningFromFailure = false;
            return true;
        }
        return false;
    }

    public bool HasVisited(int floor)
    {
        return visitedFloors.Contains(floor);
    }
}
