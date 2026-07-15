using UnityEngine;
using System.Collections.Generic;
public abstract class AbnormalData : ScriptableObject
{
    [Header("기본 정보")]
    public string abnormalName; // 이상 현상 이름
    [TextArea] public string abnormalDescription; // 이상 현상 설명

    public abstract void ApplyAbnormal(GameObject mapRoot); // 이상 현상을 맵에 적용하는 함수

    protected Transform FindTarget(GameObject mapRoot, string targetName) // 이상 현상을 적용할 게임 오브젝트를 탐색하는 함수
    {
        if(string.IsNullOrEmpty(targetName) || mapRoot == null)
            return null;
        
        Stack<Transform> stack = new Stack<Transform>();
        stack.Push(mapRoot.transform);

        while (stack.Count > 0)
        {
            Transform current = stack.Pop();
            if (current.name == targetName)
                return current;

            foreach(Transform child in current)
                stack.Push(child);
        }

        return null;
    }
}
