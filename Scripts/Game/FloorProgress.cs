using System;
using System.Collections.Generic;

public class FloorProgress
{
    private readonly HashSet<int> visitedFloors = new HashSet<int>(); // 층 방문 정보

    public int StartFloor { get; } // 시작 층
    public int TargetFloor { get; } // 목표 층
    public int CurrentFloor { get; private set; } // 현재 층
    public bool IsReturningFromFailure { get; private set; } // 플레이어 선택 오답 여부

    public bool IsCleared => CurrentFloor == TargetFloor; // 게임 클리어 여부

    public FloorProgress(int startFloor, int targetFloor) // 게임 시작 시 층을 설정하는 함수
    {
        if (startFloor < targetFloor)
            throw new ArgumentException("startFloor cannot be less than targetFloor.");
        
        StartFloor = startFloor;
        TargetFloor = targetFloor;
        Reset();
    }


    public void Reset() // 게임 시작 시 층 설정을 초기화하는 함수
    {
        CurrentFloor = StartFloor;
        IsReturningFromFailure = false;
        visitedFloors.Clear();
    }

    public bool Submit(TriggerType choice, bool hasAbnormal) // 플레이어 선택에 따른 게임 로직을 설정하는 함수
    {
        if (IsCleared)
            return false;

        bool isCorrect = FloorRule.IsCorrect(choice, hasAbnormal);
        CurrentFloor = FloorRule.DecideNextMap(CurrentFloor, StartFloor, isCorrect);
        IsReturningFromFailure = !isCorrect;
        return isCorrect;
    }

    public bool TryMarkVisited() // 최초 방문 층을 저장하는 함수
    {
        if (CurrentFloor < TargetFloor || CurrentFloor > StartFloor)
            return false;
        
        if (visitedFloors.Contains(CurrentFloor))
            return false;
       
        visitedFloors.Add(CurrentFloor);
        return true;
    }

    public bool ConsumeReturningFlag() // 오답 선택 시 플레이어 선택 오답 여부을 초기화하는 함수
    {
        if (IsReturningFromFailure)
        {
            IsReturningFromFailure = false;
            return true;
        }
        return false;
    }

    public bool HasVisited(int floor) // 현재 층을 방문한 기록이 있는지 확인하는 함수
    {
        return visitedFloors.Contains(floor);
    }
}
