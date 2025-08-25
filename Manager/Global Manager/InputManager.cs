using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // 鼠标输入
    public bool LeftMouseDown => Input.GetMouseButtonDown(0);
    public bool LeftMouseHeld => Input.GetMouseButton(0);
    public bool LeftMouseUp => Input.GetMouseButtonUp(0);

    public bool RightMouseDown => Input.GetMouseButtonDown(1);
    public bool RightMouseHeld => Input.GetMouseButton(1);
    public bool RightMouseUp => Input.GetMouseButtonUp(1);

    public float MouseScroll => Input.GetAxis("Mouse ScrollWheel");

    // 键盘输入
    public bool AltHeld => Input.GetKey(KeyCode.LeftAlt);
    public bool CtrlHeld => Input.GetKey(KeyCode.LeftControl);
    public bool CtrlPressed => Input.GetKeyDown(KeyCode.LeftControl);

    void Awake()
    {
        // 单例模式初始化
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Update()
    {
        // 自动处理滚轮值（可选）
        // 可以在其他脚本中直接访问 MouseScroll 属性
    }

    /// <summary>
    /// 获取归一化后的滚轮输入（-1, 0, 1）
    /// </summary>
    public int GetNormalizedScroll()
    {
        if (MouseScroll > 0) return 1;
        if (MouseScroll < 0) return -1;
        return 0;
    }
}