using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CombatCommanderController : MonoBehaviour
{
    // 新增UI相关变量
    [Header("AD限制解除UI")]
    public GameObject overrideUI;          // 包含按钮和文本的父物体
    public float blinkSpeed = 2f;          // 闪烁速度
    public Button overrideButton;          // 解除按钮
    public Text[] warningTexts;            // 警告文本数组
    private CanvasGroup _uiCanvasGroup;     // UI透明度控制
    private bool _isOverThreshold;         // 是否超过阈值
    private bool _overrideActive;          // 是否已解除限制


    // 系统绑定
    public AC130Gunship gunship;
    public Volume nightVisionVolume;
    public Transform targetMarker;

    // 指挥官控制参数
    public float panSpeed = 5f;
    public float rotationThreshold = 30f;
    public Vector2 zoomLimits = new Vector2(20, 100);
    public float zoomSensitivity = 10f;
    public VolumeProfile[] visionProfiles;

    // 运行时状态
    private Vector3 _currentTarget;
    private bool _isLocked;
    private float _currentZoom;
    private int _currentProfileIndex;

    [Header("AD控制")]
    public float rotationResetTime = 5f;   // 自动复位时间

    // 区域限制参数
    [Header("移动范围限制")]
    public Vector3 minBounds = new Vector3(-50, 20, -50);
    public Vector3 maxBounds = new Vector3(50, 100, 50);
    public bool showDebugBounds = true;

    [Header("父物体限制设置")]
    public bool useParentSpace = true;
    public Vector3 parentLocalBoundsMin = new Vector3(-50, 20, -50);
    public Vector3 parentLocalBoundsMax = new Vector3(50, 100, 50);

    [Header("相对模式")]
    public Transform boundsCenter;
    public Vector3 relativeBounds = new Vector3(50, 50, 50);

    [Header("动态边界设置")]
    public bool dynamicBounds = true;
    public Vector3 dynamicBoundsSize = new Vector3(100, 50, 100);

    [Header("跟随目标设置")]
    public Transform followTarget;
    public bool useFollowTarget = false;

    public enum BoundsMode { Absolute, Relative }
    [Header("移动范围设置")]
    public BoundsMode boundsMode = BoundsMode.Absolute;

    private float _resetTimer;             // 复位计时器

    void Start()
    {

        // 初始化UI
        if (overrideUI != null)
        {
            overrideUI.SetActive(false);
            _uiCanvasGroup = overrideUI.GetComponent<CanvasGroup>() ?? overrideUI.AddComponent<CanvasGroup>();
        }

        _currentZoom = GetComponent<Camera>().fieldOfView;
        Cursor.visible = true;
        targetMarker.gameObject.SetActive(false);

        if (visionProfiles.Length > 0 && nightVisionVolume != null)
            nightVisionVolume.profile = visionProfiles[0];
    }

    void LateUpdate()
    {
        UpdateBounds();
    }

    void UpdateBounds()
    {
        if (useParentSpace && transform.parent)
        {
            Vector3 parentPos = transform.parent.position;
            minBounds = parentPos + parentLocalBoundsMin;
            maxBounds = parentPos + parentLocalBoundsMax;
        }
        else if (dynamicBounds)
        {
            Vector3 centerPos = useFollowTarget && followTarget != null ?
                followTarget.position : transform.position;

            minBounds = centerPos - dynamicBoundsSize;
            maxBounds = centerPos + dynamicBoundsSize;
        }
        else if (boundsMode == BoundsMode.Relative && boundsCenter != null)
        {
            Vector3 centerPos = useFollowTarget && followTarget != null ?
                followTarget.position : boundsCenter.position;

            minBounds = centerPos - relativeBounds;
            maxBounds = centerPos + relativeBounds;
        }
    }

    void Update()
    {
        HandleThresholdUI();
        HandleCombatCommands();
        UpdateCameraControl();
    }

    void HandleThresholdUI()
    {
        if (_overrideActive || gunship == null) return;

        // 检测是否超过阈值
        bool isOver = Mathf.Abs(gunship.CurrentBankAngle) >= rotationThreshold;

        if (isOver && !_isOverThreshold)
        {
            // 首次超过阈值
            overrideUI.SetActive(true);
            _isOverThreshold = true;
            _resetTimer = rotationResetTime;
        }
        else if (!isOver && _isOverThreshold)
        {
            // 回到安全范围
            overrideUI.SetActive(false);
            _isOverThreshold = false;
        }

        // 更新UI闪烁
        if (_isOverThreshold)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            _uiCanvasGroup.alpha = alpha;

            // 倒计时复位
            if ((_resetTimer -= Time.deltaTime) <= 0)
            {
                overrideUI.SetActive(false);
                _isOverThreshold = false;
                gunship.CurrentBankAngle = Mathf.MoveTowards(gunship.CurrentBankAngle, 0, 10f);
            }
        }
    }

    // 按钮点击事件
    public void OnOverrideButtonClick()
    {
        _overrideActive = true;
        overrideUI.SetActive(false);

        // 重置飞行姿态
        StartCoroutine(ResetBankAngle());
    }

    System.Collections.IEnumerator ResetBankAngle()
    {
        float startAngle = gunship.CurrentBankAngle;
        float duration = 0.5f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            gunship.CurrentBankAngle = Mathf.Lerp(startAngle, 0, t / duration);
            yield return null;
        }
        gunship.CurrentBankAngle = 0;
    }

    void HandleCombatCommands()
    {
        // 右键拖动摄像机（仅在XZ平面移动）
        if (Input.GetMouseButton(1))
        {
            // 获取摄像机实际右方向（水平面投影）
            Vector3 cameraRight = GetComponent<Camera>().transform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            // 计算基础移动量
            float mouseX = Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
            Vector3 moveDelta = cameraRight * mouseX;

            // 应用父物体空间转换
            if (useParentSpace && transform.parent)
            {
                // 将移动向量转换到父物体局部空间
                Vector3 localMove = transform.parent.InverseTransformDirection(moveDelta);
                localMove.y = 0; // 保持垂直方向不变

                // 应用局部空间限制
                Vector3 localPos = transform.parent.InverseTransformPoint(transform.position) + localMove;
                localPos.x = Mathf.Clamp(localPos.x, parentLocalBoundsMin.x, parentLocalBoundsMax.x);
                localPos.z = Mathf.Clamp(localPos.z, parentLocalBoundsMin.z, parentLocalBoundsMax.z);

                // 转换回世界坐标
                Vector3 newPos = transform.parent.TransformPoint(localPos);
                newPos.y = transform.position.y; // 保持高度不变
                transform.position = newPos;
            }
            else
            {
                // 世界空间移动处理
                Vector3 newPos = transform.position + moveDelta;

                // 应用全局限制
                newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
                newPos.z = Mathf.Clamp(newPos.z, minBounds.z, maxBounds.z);
                newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);

                transform.position = newPos;
            }
        }

        // 左键锁定目标
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                _currentTarget = hit.point;
                _isLocked = true;
                targetMarker.position = _currentTarget + Vector3.up * 0.5f;
                targetMarker.gameObject.SetActive(true);
            }
        }

        // 火力指令
        for (int i = 1; i <= 3; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                ExecuteFireCommand(i, _currentTarget);
            }
        }
    }

    void UpdateCameraControl()
    {
        // 修改后的AD控制
        float bankInput = Input.GetAxis("Horizontal");

        if (_overrideActive || Mathf.Abs(gunship.CurrentBankAngle) < rotationThreshold)
        {
            gunship.ApplyBank(bankInput);
        }

        // 摄像机缩放
        _currentZoom -= Input.mouseScrollDelta.y * zoomSensitivity;
        _currentZoom = Mathf.Clamp(_currentZoom, zoomLimits.x, zoomLimits.y);
        GetComponent<Camera>().fieldOfView = Mathf.Lerp(
            GetComponent<Camera>().fieldOfView,
            _currentZoom,
            Time.deltaTime * 5f
        );

        // 切换视觉模式
        if (Input.GetKeyDown(KeyCode.Q)) SwitchVisionProfile(-1);
        if (Input.GetKeyDown(KeyCode.E)) SwitchVisionProfile(1);
    }

    void SwitchVisionProfile(int direction)
    {
        _currentProfileIndex = (_currentProfileIndex + direction + visionProfiles.Length) % visionProfiles.Length;
        if (nightVisionVolume != null)
            nightVisionVolume.profile = visionProfiles[_currentProfileIndex];
    }

    void ExecuteFireCommand(int commandType, Vector3 target)
    {
        if (!_isLocked) return;

        switch (commandType)
        {
            case 1:
                gunship.RequestPrecisionStrike(target);
                break;
            case 2:
                gunship.RequestAreaSuppression(target);
                break;
            case 3:
                gunship.RequestThermalScan(target);
                break;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 85, 300, 25), $"限制状态: {(_overrideActive ? "已解除" : "正常")}");
        GUI.Label(new Rect(10, 10, 300, 25), $"锁定状态: {(_isLocked ? "已锁定" : "未锁定")}");
        GUI.Label(new Rect(10, 35, 300, 25), $"当前模式: {visionProfiles[_currentProfileIndex].name}");
        GUI.Label(new Rect(10, 60, 300, 25), $"飞行姿态: {gunship.CurrentBankAngle:F1}°");
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugBounds) return;

        // 父物体局部边界
        if (useParentSpace && transform.parent)
        {
            Gizmos.color = Color.magenta;
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.parent.localToWorldMatrix;
            Gizmos.DrawWireCube(
                (parentLocalBoundsMin + parentLocalBoundsMax) / 2,
                parentLocalBoundsMax - parentLocalBoundsMin
            );
            Gizmos.matrix = originalMatrix;
        }

        // 动态边界
        Gizmos.color = dynamicBounds ? Color.green :
                      (boundsMode == BoundsMode.Relative ? Color.cyan : Color.yellow);

        if (dynamicBounds)
        {
            Gizmos.DrawWireCube(transform.position, dynamicBoundsSize * 2);
        }
        else if (boundsMode == BoundsMode.Relative && boundsCenter != null)
        {
            Gizmos.DrawWireCube(boundsCenter.position, relativeBounds * 2);
        }
        else
        {
            Vector3 center = (minBounds + maxBounds) / 2;
            Vector3 size = maxBounds - minBounds;
            Gizmos.DrawWireCube(center, size);
        }
    }
}