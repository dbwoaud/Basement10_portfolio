using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class UIBinder
{
    public static Transform FindTransform(Transform root, string name) // DFS로 트랜스폼을 찾는 함수
    {
        if (root == null || string.IsNullOrEmpty(name))
            return null;

        Stack<Transform> stack = new Stack<Transform>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            Transform current = stack.Pop();

            if (current != root && current.name == name)
                return current;

            for (int i = current.childCount - 1; i >= 0; i--)
                stack.Push(current.GetChild(i));
        }

        return null;
    }

    public static GameObject FindObject(Transform root, string name) // DFS로 오브젝트를 찾는 함수
    {
        Transform found = FindTransform(root, name);
        return found != null ? found.gameObject : null;
    }

    public static T Find<T>(Transform root, string name) where T : Component // DFS로 컴포넌트를 찾는 함수
    {
        Transform found = FindTransform(root, name);
        return found != null ? found.GetComponent<T>() : null;
    }

    public static T FindInRow<T>(Transform root, string rowName, string childName = null) where T : Component // DFS로 특정 오브젝트의 자식 컴포넌트를 찾는 함수
    {
        Transform row = FindTransform(root, rowName);
        if (row == null)
            return null;
        
        if (string.IsNullOrEmpty(childName))
            return row.GetComponentInChildren<T>(true);

        Transform child = FindTransform(row, childName);
        return child != null ? child.GetComponent<T>() : null;
    }

    public static void BindButtons(Transform root, IReadOnlyDictionary<string, UnityAction> handlers) // 오브젝트 이름으로 버튼을 할당하는 함수
    {
        if (root == null || handlers == null)
            return;

        Button[] buttons = root.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (!handlers.TryGetValue(button.gameObject.name, out UnityAction handler))
                continue;

            button.onClick.RemoveListener(handler);
            button.onClick.AddListener(handler);
        }
    }
}