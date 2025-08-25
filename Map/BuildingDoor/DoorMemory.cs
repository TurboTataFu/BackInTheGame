// 门记忆组件（需挂载到每个门子级）
using UnityEngine;

public class DoorMemory : MonoBehaviour
{
    [HideInInspector]
    public Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.localRotation;
    }
}