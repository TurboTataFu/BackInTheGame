using UnityEngine;

// 放置区域的配置组件（支持差异化约束）
[RequireComponent(typeof(Collider))]
public class PlacementAreaSettings : MonoBehaviour
{
    [Header("X轴约束")]
    public AxisConstraint xConstraint = new AxisConstraint();
    
    [Header("Y轴约束")]
    public AxisConstraint yConstraint = new AxisConstraint();
    
    [Header("Z轴约束")]
    public AxisConstraint zConstraint = new AxisConstraint();

    [Tooltip("该区域的标识名称")]
    public string areaId;

    private Collider areaCollider;
    
    private void Awake()
    {
        areaCollider = GetComponent<Collider>();
        // 确保碰撞体属于放置区域图层
        gameObject.layer = LayerMask.NameToLayer("PlacementArea");
        if (areaCollider.isTrigger)
        {
            Debug.LogWarning($"放置区域 {name} 启用了Trigger，可能影响射线检测", this);
        }
    }

    /// <summary>
    /// 将位置应用到该区域的所有轴约束
    /// </summary>
    public Vector3 ApplyAllConstraints(Vector3 originalPosition)
    {
        return new Vector3(
            xConstraint.ApplyConstraint(originalPosition.x),
            yConstraint.ApplyConstraint(originalPosition.y),
            zConstraint.ApplyConstraint(originalPosition.z)
        );
    }

    /// <summary>
    /// 检查位置是否在所有启用的约束范围内
    /// </summary>
    public bool IsPositionValid(Vector3 position)
    {
        return xConstraint.IsValueValid(position.x) &&
               yConstraint.IsValueValid(position.y) &&
               zConstraint.IsValueValid(position.z);
    }

    // 绘制约束范围Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0.5f, 1, 0.2f);
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.DrawCube(collider.bounds.center, collider.bounds.size);
        }
        
        // 绘制X轴约束范围
        if (xConstraint.enabled)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            DrawAxisRange(Vector3.right, xConstraint.minValue, xConstraint.maxValue);
        }
        
        // 绘制Y轴约束范围
        if (yConstraint.enabled)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            DrawAxisRange(Vector3.up, yConstraint.minValue, yConstraint.maxValue);
        }
        
        // 绘制Z轴约束范围
        if (zConstraint.enabled)
        {
            Gizmos.color = new Color(0, 0, 1, 0.3f);
            DrawAxisRange(Vector3.forward, zConstraint.minValue, zConstraint.maxValue);
        }
    }
    
    // 绘制单轴约束范围
    private void DrawAxisRange(Vector3 axis, float min, float max)
    {
        Vector3 center = transform.position;
        Vector3 scale = GetComponent<Collider>()?.bounds.size ?? Vector3.one;
        
        // 垂直于当前轴的平面尺寸
        Vector3 planeScale = scale;
        if (axis == Vector3.right) planeScale.x = 0.1f;
        else if (axis == Vector3.up) planeScale.y = 0.1f;
        else if (axis == Vector3.forward) planeScale.z = 0.1f;
        
        // 绘制最小值平面
        Vector3 minPos = center + axis * min;
        Gizmos.DrawCube(minPos, planeScale);
        
        // 绘制最大值平面
        Vector3 maxPos = center + axis * max;
        Gizmos.DrawCube(maxPos, planeScale);
        
        // 绘制范围连接线
        Gizmos.DrawLine(minPos - planeScale/2, maxPos - planeScale/2);
        Gizmos.DrawLine(minPos + planeScale/2, maxPos + planeScale/2);
    }
}
