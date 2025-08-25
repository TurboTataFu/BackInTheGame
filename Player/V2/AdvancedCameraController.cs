using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AdvancedCameraController : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("摄像机移动速度")]
    public float moveSpeed = 10f;
    [Tooltip("摄像机移动范围限制 - 最小值")]
    public Vector3 minMoveBounds = new Vector3(-50, 5, -50);
    [Tooltip("摄像机移动范围限制 - 最大值")]
    public Vector3 maxMoveBounds = new Vector3(50, 50, 50);
    [Tooltip("是否反转水平（左右）移动方向")]
    public bool invertXMovement = false;
    [Tooltip("是否反转垂直（上下）移动方向")]
    public bool invertYMovement = false;

    [Header("旋转设置")]
    [Tooltip("基础旋转角度")]
    public Vector3 baseRotation = new Vector3(30, 0, 0);
    [Tooltip("旋转灵敏度")]
    public float rotationSensitivity = 0.5f;
    [Tooltip("旋转阻尼系数")]
    public float rotationDamping = 12f;
    [Tooltip("最大X轴旋转限制")]
    public float maxXRotation = 60f;
    [Tooltip("最小X轴旋转限制")]
    public float minXRotation = 10f;
    [Tooltip("最大Y轴旋转限制")]
    public float maxYRotation = 45f;
    [Tooltip("最小Y轴旋转限制")]
    public float minYRotation = -45f;
    [Tooltip("判断为长按的时间阈值（秒）")]
    public float longPressThreshold = 0.3f;
    [Tooltip("是否反转垂直（上下）旋转方向")]
    public bool invertYRotation = false;
    [Tooltip("是否反转水平（左右）旋转方向")]
    public bool invertXRotation = false;

    [Header("FOV设置")]
    [Tooltip("默认FOV值")]
    public float defaultFOV = 60f;
    [Tooltip("FOV调整速度")]
    public float fovChangeSpeed = 2f;
    [Tooltip("最小FOV限制")]
    public float minFOV = 30f;
    [Tooltip("最大FOV限制")]
    public float maxFOV = 90f;

    private Camera mainCamera;
    private Quaternion targetRotation;
    private bool isRotating = false;
    private bool isLongPress = false;
    private float middleClickPressTime;

    void Start()
    {
        // 获取摄像机组件
        mainCamera = GetComponent<Camera>();

        // 初始化摄像机状态
        transform.rotation = Quaternion.Euler(baseRotation);
        targetRotation = transform.rotation;
        mainCamera.fieldOfView = defaultFOV;
    }

    void Update()
    {
        // 处理移动控制 - 鼠标右键拖动（上下左右）
        if (Input.GetMouseButton(1))
        {
            HandleMovement();
        }

        // 处理鼠标中键输入（长按旋转，短按重置FOV）
        HandleMiddleClickInput();

        // 如果处于旋转状态，应用旋转
        if (isRotating)
        {
            HandleRotation();
        }
        // 如果旋转结束，平滑回正
        else if (!Input.GetMouseButton(2) && targetRotation != Quaternion.Euler(baseRotation))
        {
            targetRotation = Quaternion.Lerp(targetRotation, Quaternion.Euler(baseRotation), rotationDamping * Time.deltaTime);
            transform.rotation = targetRotation;
        }

        // 处理FOV控制 - 鼠标滚轮
        if (Input.mouseScrollDelta.y != 0)
        {
            HandleFOVChange(Input.mouseScrollDelta.y);
        }
    }

    /// <summary>
    /// 处理鼠标中键的长按和短按逻辑
    /// </summary>
    private void HandleMiddleClickInput()
    {
        // 中键按下
        if (Input.GetMouseButtonDown(2))
        {
            middleClickPressTime = Time.time;
            isLongPress = false;
            isRotating = false;
        }
        // 中键按住
        else if (Input.GetMouseButton(2))
        {
            // 检查是否达到长按阈值
            if (!isLongPress && Time.time - middleClickPressTime > longPressThreshold)
            {
                isLongPress = true;
                isRotating = true;
            }
        }
        // 中键松开
        else if (Input.GetMouseButtonUp(2))
        {
            // 如果是短按（未达到长按阈值），则重置FOV
            if (!isLongPress)
            {
                ResetFOV();
            }

            isLongPress = false;
            isRotating = false;
        }
    }

    /// <summary>
    /// 处理摄像机移动（上下左右），支持方向反转
    /// </summary>
    private void HandleMovement()
    {
        // 获取鼠标输入并应用移动反转设置
        float horizontal = Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
        float vertical = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;

        // 应用移动方向反转
        if (invertXMovement) horizontal = -horizontal;
        if (invertYMovement) vertical = -vertical;

        // 计算新位置（左右为X轴，上下为Y轴）
        Vector3 newPosition = transform.position +
                              (Vector3.right * horizontal) -
                              (Vector3.up * vertical);

        // 应用边界限制
        newPosition.x = Mathf.Clamp(newPosition.x, minMoveBounds.x, maxMoveBounds.x);
        newPosition.y = Mathf.Clamp(newPosition.y, minMoveBounds.y, maxMoveBounds.y);
        newPosition.z = Mathf.Clamp(newPosition.z, minMoveBounds.z, maxMoveBounds.z);

        transform.position = newPosition;
    }

    /// <summary>
    /// 处理摄像机旋转（中键长按拖动），支持方向反转
    /// </summary>
    private void HandleRotation()
    {
        // 获取鼠标输入并应用旋转反转设置
        float xRot = Input.GetAxis("Mouse Y") * rotationSensitivity;
        float yRot = Input.GetAxis("Mouse X") * rotationSensitivity;

        // 应用旋转方向反转
        if (invertYRotation) xRot = -xRot;
        if (invertXRotation) yRot = -yRot;

        // 基于当前目标旋转计算新旋转
        Quaternion xRotation = Quaternion.AngleAxis(-xRot, Vector3.right);
        Quaternion yRotation = Quaternion.AngleAxis(yRot, Vector3.up);

        targetRotation = targetRotation * xRotation * yRotation;

        // 将旋转限制在指定范围内
        Vector3 euler = targetRotation.eulerAngles;
        euler.x = ClampAngle(euler.x, minXRotation, maxXRotation);
        euler.y = ClampAngle(euler.y, minYRotation, maxYRotation);
        euler.z = 0; // 锁定Z轴旋转
        targetRotation = Quaternion.Euler(euler);

        // 应用阻尼效果
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationDamping * Time.deltaTime);
    }

    /// <summary>
    /// 角度限制辅助函数（处理0-360度循环问题）
    /// </summary>
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < 90 || angle > 270)
        {
            if (angle > 180) angle -= 360;
            if (angle < -180) angle += 360;
        }
        return Mathf.Clamp(angle, min, max);
    }

    /// <summary>
    /// 处理FOV变化（鼠标滚轮）
    /// </summary>
    private void HandleFOVChange(float scrollValue)
    {
        float newFOV = mainCamera.fieldOfView - scrollValue * fovChangeSpeed;
        mainCamera.fieldOfView = Mathf.Clamp(newFOV, minFOV, maxFOV);
    }

    /// <summary>
    /// 重置FOV到默认值（中键短按）
    /// </summary>
    private void ResetFOV()
    {
        mainCamera.fieldOfView = defaultFOV;
    }

    // 绘制Gizmos以显示移动范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = (minMoveBounds + maxMoveBounds) / 2;
        Vector3 size = maxMoveBounds - minMoveBounds;
        Gizmos.DrawWireCube(center, size);
    }
}
