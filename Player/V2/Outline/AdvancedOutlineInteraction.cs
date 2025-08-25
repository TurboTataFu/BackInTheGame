using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Renderer), typeof(Collider))]
public class AdvancedOutlineInteraction : MonoBehaviour
{
    [Tooltip("拖入使用的轮廓Shader")]
    public Shader outlineShader;

    [Tooltip("鼠标悬浮时的轮廓颜色")]
    public Color hoverColor = Color.yellow;

    [Tooltip("鼠标点击时的轮廓颜色")]
    public Color selectColor = Color.green;

    [Tooltip("悬浮时的轮廓宽度倍数")]
    public float hoverWidthMultiplier = 1.5f;

    [Tooltip("颜色和宽度过渡的持续时间（秒）")]
    public float transitionDuration = 0.2f;

    [Tooltip("点击时要激活的对象")]
    public GameObject targetObject;

    [Tooltip("自身，用于调整对象位置")]
    public GameObject mySelfObject;

    // 检测到的属性名称
    private string _outlineColorProperty;
    private string _outlineWidthProperty;

    // 原始材质属性
    private Color _originalOutlineColor;
    private float _originalOutlineWidth;

    // 目标属性值（用于过渡）
    private Color _targetColor;
    private float _targetWidth;

    // 当前属性值（用于过渡）
    private Color _currentColor;
    private float _currentWidth;

    // 渲染器和材质引用
    private Renderer _renderer;
    private Material _materialInstance;

    // 状态标识
    private bool _isSelected = false;
    private bool _isHovering = false;
    private Coroutine _transitionCoroutine;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();

        // 创建材质实例以避免影响其他对象
        _materialInstance = new Material(_renderer.material);
        _renderer.material = _materialInstance;

        // 如果没有指定shader，使用当前材质的shader
        if (outlineShader == null)
        {
            outlineShader = _materialInstance.shader;
        }

        // 自动检测轮廓属性
        DetectOutlineProperties();

        // 保存原始属性
        SaveOriginalProperties();

        // 初始化当前属性
        _currentColor = _originalOutlineColor;
        _currentWidth = _originalOutlineWidth;
    }

    private void Update()
    {
        // 持续更新材质属性以实现平滑过渡
        if (!string.IsNullOrEmpty(_outlineColorProperty))
            _materialInstance.SetColor(_outlineColorProperty, _currentColor);

        if (!string.IsNullOrEmpty(_outlineWidthProperty))
            _materialInstance.SetFloat(_outlineWidthProperty, _currentWidth);
    }

    /// <summary>
    /// 自动检测shader中的轮廓相关属性
    /// </summary>
    private void DetectOutlineProperties()
    {
        if (outlineShader == null) return;

        // 获取所有shader属性
        var properties = new string[outlineShader.GetPropertyCount()];
        for (int i = 0; i < outlineShader.GetPropertyCount(); i++)
        {
            properties[i] = outlineShader.GetPropertyName(i);
        }

        // 尝试找到颜色属性
        var colorCandidates = properties.Where(p =>
            p.Contains("Outline") && (p.Contains("Color") || p.Contains("Tint"))
        ).ToList();

        // 尝试找到宽度/强度属性
        var widthCandidates = properties.Where(p =>
            p.Contains("Outline") && (p.Contains("Width") || p.Contains("Size") || p.Contains("Strength"))
        ).ToList();

        // 设置找到的属性
        if (colorCandidates.Count > 0)
            _outlineColorProperty = colorCandidates[0];

        if (widthCandidates.Count > 0)
            _outlineWidthProperty = widthCandidates[0];

        // 检查是否找到必要的属性
        if (string.IsNullOrEmpty(_outlineColorProperty) || string.IsNullOrEmpty(_outlineWidthProperty))
        {
            Debug.LogWarning("未能检测到轮廓属性，请确保Shader包含轮廓相关属性", this);
        }
    }

    /// <summary>
    /// 保存原始材质属性
    /// </summary>
    private void SaveOriginalProperties()
    {
        if (!string.IsNullOrEmpty(_outlineColorProperty))
            _originalOutlineColor = _materialInstance.GetColor(_outlineColorProperty);

        if (!string.IsNullOrEmpty(_outlineWidthProperty))
            _originalOutlineWidth = _materialInstance.GetFloat(_outlineWidthProperty);
    }

    /// <summary>
    /// 鼠标悬停时的处理
    /// </summary>
    private void OnMouseEnter()
    {
        if (_isSelected) return;

        _isHovering = true;
        StartTransition(hoverColor, _originalOutlineWidth * hoverWidthMultiplier);
    }

    /// <summary>
    /// 鼠标离开时的处理
    /// </summary>
    private void OnMouseExit()
    {
        if (_isSelected) return;

        _isHovering = false;
        StartTransition(_originalOutlineColor, _originalOutlineWidth);
    }

    /// <summary>
    /// 鼠标点击时的处理
    /// </summary>
    private void OnMouseDown()
    {
        _isSelected = !_isSelected;

        if (_isSelected)
        {
            // 应用选中效果
            StartTransition(selectColor, _originalOutlineWidth * hoverWidthMultiplier * 1.2f);

            // 激活目标对象
            if (targetObject != null)
            {
                targetObject.SetActive(true);
            }
        }
        else
        {
            // 恢复原始状态或悬停状态
            if (_isHovering)
            {
                StartTransition(hoverColor, _originalOutlineWidth * hoverWidthMultiplier);
            }
            else
            {
                StartTransition(_originalOutlineColor, _originalOutlineWidth);
            }

            // 激活目标对象
            if (targetObject != null)
            {
                targetObject.SetActive(false);
            }
            // 可以在这里添加取消激活目标对象的逻辑
            // if (targetObject != null) targetObject.SetActive(false);
        }
    }

    /// <summary>
    /// 开始属性过渡动画
    /// </summary>
    private void StartTransition(Color targetColor, float targetWidth)
    {
        // 如果有正在进行的过渡，停止它
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
        }

        // 开始新的过渡
        _targetColor = targetColor;
        _targetWidth = targetWidth;
        _transitionCoroutine = StartCoroutine(AnimateTransition());
    }

    /// <summary>
    /// 执行属性过渡动画的协程
    /// </summary>
    private IEnumerator AnimateTransition()
    {
        float elapsedTime = 0;

        while (elapsedTime < transitionDuration)
        {
            // 计算插值因子（0到1之间）
            float t = elapsedTime / transitionDuration;
            // 使用平滑曲线使过渡更自然
            t = Mathf.SmoothStep(0, 1, t);

            // 插值计算当前属性值
            _currentColor = Color.Lerp(_currentColor, _targetColor, t);
            _currentWidth = Mathf.Lerp(_currentWidth, _targetWidth, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保最终值准确
        _currentColor = _targetColor;
        _currentWidth = _targetWidth;

        _transitionCoroutine = null;
    }

    /// <summary>
    /// 恢复材质原始属性的公共接口
    /// </summary>
    public void RestoreOriginalProperties()
    {
        _isSelected = false;
        _isHovering = false;
        StartTransition(_originalOutlineColor, _originalOutlineWidth);
    }

    private void OnDestroy()
    {
        // 清理材质实例
        if (_materialInstance != null)
        {
            Destroy(_materialInstance);
        }
    }

    // 绑定到UI按钮的点击事件
    public void OnAdjustButtonClick()
    {
        if (targetObject != null)
        {
            // 调用放置管理器，开始调整该对象位置
            BuildingPlacementManager.Instance.StartAdjustingExistingObject(mySelfObject);
        }
        else
        {
            Debug.LogError("不知道要重新调整谁的位置！QWQ");
        }
    }
}
