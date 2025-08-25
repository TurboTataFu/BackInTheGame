using UnityEngine;

public class MouseOverDetector : MonoBehaviour
{
    [Tooltip("需要启用/禁用的对象")]
    public GameObject targetObject;

    void OnMouseEnter()
    {
        // 当鼠标进入碰撞体时启用目标对象
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }

    void OnMouseExit()
    {
        // 当鼠标离开碰撞体时禁用目标对象
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }
}