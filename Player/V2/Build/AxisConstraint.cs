using UnityEngine;

// 单个轴的约束设置
[System.Serializable]
public class AxisConstraint
{
    [Tooltip("是否启用该轴约束")]
    public bool enabled = false;
    
    [Tooltip("轴的最小值限制")]
    public float minValue;
    
    [Tooltip("轴的最大值限制")]
    public float maxValue;
    
    [Tooltip("是否吸附到指定间隔值")]
    public bool snapToGrid = false;
    
    [Tooltip("吸附间隔值")]
    public float gridStep = 1f;

    /// <summary>
    /// 应用约束到指定值
    /// </summary>
    public float ApplyConstraint(float value)
    {
        if (!enabled) return value;
        
        // 限制在区间内
        float constrained = Mathf.Clamp(value, minValue, maxValue);
        
        // 应用网格吸附
        if (snapToGrid && gridStep > 0)
        {
            constrained = Mathf.Round(constrained / gridStep) * gridStep;
        }
        
        return constrained;
    }

    /// <summary>
    /// 检查值是否在约束范围内
    /// </summary>
    public bool IsValueValid(float value)
    {
        if (!enabled) return true;
        return value >= minValue && value <= maxValue;
    }
}
