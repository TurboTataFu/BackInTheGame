using UnityEngine;
using System;
public class DestructionListener : MonoBehaviour
{
    public event Action OnDestroyed; // 销毁事件

    // 当组件所在的GameObject被销毁时调用
    public void OnDestroy()
    {
        OnDestroyed?.Invoke(); // 触发事件，通知订阅者（单元格）
    }
}
