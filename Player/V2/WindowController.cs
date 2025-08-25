using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(RectTransform))]
public class WindowController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("窗口组件引用")]
    [Tooltip("窗口标题栏（拖动区域，需包含Graphic组件）")]
    public RectTransform titleBar;

    [Tooltip("窗口右下角缩放区域（需包含Graphic组件）")]
    public RectTransform resizeHandle;

    [Tooltip("最大化/还原按钮")]
    public Button maximizeButton;

    [Tooltip("最小化按钮")]
    public Button minimizeButton;

    [Tooltip("关闭按钮")]
    public Button closeButton;

    [Header("窗口设置")]
    public float minWidth = 200f;
    public float minHeight = 150f;
    public bool draggable = true;
    public bool resizable = true;
    public bool canMaximize = true;

    // 核心变量
    private RectTransform _windowRect;
    private RectTransform _parentRect; // 父容器（必须是RectTransform）
    private bool _isDragging;
    private bool _isResizing;
    private bool _isMaximized;
    private bool _isMinimized;

    // 拖动相关计算参数
    private Vector2 _localMouseOffset; // 鼠标在窗口内的本地偏移（关键参数）
    private Vector2 _originalAnchoredPos;
    private Vector2 _originalSizeDelta;
    private Rect _parentRectBounds; // 父容器的边界（用于限制范围）

    // 事件
    public event Action OnWindowClose;
    public event Action OnWindowMaximize;
    public event Action OnWindowRestore;
    public event Action OnWindowMinimize;
    public event Action OnWindowRestoreFromMinimize;

    private void Awake()
    {
        _windowRect = GetComponent<RectTransform>();

        // 确保获取正确的父容器（优先Canvas的RectTransform）
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            _parentRect = canvas.GetComponent<RectTransform>();
        }
        // 如果没有Canvas，使用直接父级
        if (_parentRect == null)
        {
            _parentRect = _windowRect.parent as RectTransform;
        }

        // 初始化父容器边界
        UpdateParentBounds();

        // 记录初始状态
        _originalAnchoredPos = _windowRect.anchoredPosition;
        _originalSizeDelta = _windowRect.sizeDelta;

        // 绑定按钮事件
        if (maximizeButton != null)
            maximizeButton.onClick.AddListener(ToggleMaximize);
        if (minimizeButton != null)
            minimizeButton.onClick.AddListener(ToggleMinimize);
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseWindow);

        // 检查必要组件
        CheckRequiredComponents();
    }

    // 更新父容器边界（解决分辨率变化问题）
    private void UpdateParentBounds()
    {
        if (_parentRect != null)
        {
            // 计算父容器在本地坐标系中的边界
            _parentRectBounds = new Rect(
                -_parentRect.rect.width / 2,  // 左边界（本地坐标）
                -_parentRect.rect.height / 2, // 下边界（本地坐标）
                _parentRect.rect.width,       // 宽度
                _parentRect.rect.height       // 高度
            );
        }
    }

    // 检查必要组件
    private void CheckRequiredComponents()
    {
        if (titleBar != null && titleBar.GetComponent<Graphic>() == null)
        {
            Debug.LogWarning("标题栏必须添加Graphic组件（如Image）才能接收拖动事件", titleBar);
        }
        if (resizeHandle != null && resizeHandle.GetComponent<Graphic>() == null)
        {
            Debug.LogWarning("缩放区域必须添加Graphic组件（如Image）才能接收缩放事件", resizeHandle);
        }
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            Debug.LogError("场景中缺少EventSystem！请创建EventSystem（右键->UI->EventSystem）");
        }
        if (_parentRect == null)
        {
            Debug.LogError("窗口没有有效的父级RectTransform！请确保窗口放在Canvas下");
        }
    }

    private void Update()
    {
        // 屏幕尺寸变化时更新边界
        if (Input.mousePosition.x != 0 || Input.mousePosition.y != 0)
        {
            UpdateParentBounds();
        }
    }

    // 处理拖动开始（核心：计算鼠标在窗口内的偏移）
    public void OnPointerDown(PointerEventData eventData)
    {
        // 拖动逻辑
        if (draggable && titleBar != null && !_isMaximized && !_isMinimized)
        {
            // 检查鼠标是否在标题栏内
            if (RectTransformUtility.RectangleContainsScreenPoint(
                titleBar,
                eventData.position,
                eventData.pressEventCamera))
            {
                // 关键计算：将鼠标屏幕位置转换为窗口本地坐标，得到偏移量
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _windowRect,  // 目标坐标系：窗口自身
                    eventData.position,
                    eventData.pressEventCamera,
                    out _localMouseOffset
                );

                _isDragging = true;
                transform.SetAsLastSibling(); // 窗口置顶
                eventData.Use();
            }
        }
        // 缩放逻辑
        else if (resizable && resizeHandle != null && !_isMaximized && !_isMinimized)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                resizeHandle,
                eventData.position,
                eventData.pressEventCamera))
            {
                _isResizing = true;
                eventData.Use();
            }
        }
    }

    // 处理拖动中（核心：根据偏移量计算新位置）
    public void OnDrag(PointerEventData eventData)
    {
        if (_isDragging && draggable && !_isMaximized && !_isMinimized)
        {
            // 将鼠标屏幕位置转换为父容器的本地坐标
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRect,  // 目标坐标系：父容器
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 parentLocalPos))
            {
                // 计算窗口新位置：父容器中的鼠标位置 - 鼠标在窗口内的偏移量
                Vector2 newAnchoredPos = parentLocalPos - _localMouseOffset;

                // 限制窗口在父容器内
                newAnchoredPos = ClampPositionToParentBounds(newAnchoredPos);

                // 应用新位置
                _windowRect.anchoredPosition = newAnchoredPos;
                eventData.Use();
            }
        }
        else if (_isResizing && resizable && !_isMaximized && !_isMinimized)
        {
            // 缩放逻辑（保持不变，但依赖正确的父容器边界）
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 mouseLocalPos))
            {
                Vector2 newSize = new Vector2(
                    mouseLocalPos.x - _windowRect.anchoredPosition.x,
                    mouseLocalPos.y - _windowRect.anchoredPosition.y
                );

                newSize.x = Mathf.Clamp(newSize.x, minWidth, _parentRectBounds.width - 20);
                newSize.y = Mathf.Clamp(newSize.y, minHeight, _parentRectBounds.height - 20);

                _windowRect.sizeDelta = newSize;
                eventData.Use();
            }
        }
    }

    // 限制窗口在父容器内
    private Vector2 ClampPositionToParentBounds(Vector2 desiredPos)
    {
        // 窗口半尺寸
        Vector2 halfWindowSize = _windowRect.sizeDelta / 2;

        // 计算父容器内的可移动范围
        float minX = _parentRectBounds.xMin + halfWindowSize.x + 10; // 左边界+边距
        float maxX = _parentRectBounds.xMax - halfWindowSize.x - 10; // 右边界-边距
        float minY = _parentRectBounds.yMin + halfWindowSize.y + 10; // 下边界+边距
        float maxY = _parentRectBounds.yMax - halfWindowSize.y - 10; // 上边界-边距

        // 限制位置
        return new Vector2(
            Mathf.Clamp(desiredPos.x, minX, maxX),
            Mathf.Clamp(desiredPos.y, minY, maxY)
        );
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
        _isResizing = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isDragging = false;
        _isResizing = false;
    }

    // 最大化/还原
    public void ToggleMaximize()
    {
        if (!canMaximize) return;

        if (_isMaximized)
        {
            // 还原
            _windowRect.anchoredPosition = _originalAnchoredPos;
            _windowRect.sizeDelta = _originalSizeDelta;
            _isMaximized = false;
            OnWindowRestore?.Invoke();
        }
        else
        {
            // 记录当前状态
            _originalAnchoredPos = _windowRect.anchoredPosition;
            _originalSizeDelta = _windowRect.sizeDelta;

            // 最大化（填充父容器，留边距）
            _windowRect.anchoredPosition = new Vector2(10, -10);
            _windowRect.sizeDelta = new Vector2(
                _parentRectBounds.width - 20,
                _parentRectBounds.height - 20
            );
            _isMaximized = true;
            OnWindowMaximize?.Invoke();
        }
    }

    // 最小化/还原
    public void ToggleMinimize()
    {
        if (_isMinimized)
        {
            gameObject.SetActive(true);
            _isMinimized = false;
            OnWindowRestoreFromMinimize?.Invoke();
        }
        else
        {
            gameObject.SetActive(false);
            _isMinimized = true;
            OnWindowMinimize?.Invoke();
        }
    }

    public void CloseWindow()
    {
        OnWindowClose?.Invoke();
        Destroy(gameObject);
    }

    // 公开方法
    public void SetDraggable(bool value) => draggable = value;
    public void SetResizable(bool value) => resizable = value;
    public bool IsMaximized() => _isMaximized;
    public bool IsMinimized() => _isMinimized;
}