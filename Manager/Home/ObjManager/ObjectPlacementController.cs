using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacementController : MonoBehaviour
{

    [Header("建造模式设置")]
    [SerializeField] private bool isConstructionMode = false;
    [SerializeField] private GameObject constructionHUD;

    // 新增字段
    [Header("即时放置设置")]
    [SerializeField] private GameObject placementPrefab; // 要放置的预制体
    [SerializeField] private float moveSpeed = 5f;        // 移动跟随速度
    private GameObject currentObject;                     // 当前操作物体
    private bool isPlacing;                                // 是否处于放置状态

    #region Region Settings
    [Header("区域设置")]
    [Tooltip("放置区域起点")]
    [SerializeField] private Vector3 placementStart = Vector3.zero;

    [Tooltip("放置区域终点")]
    [SerializeField] private Vector3 placementEnd = Vector3.forward * 5f;

    [Tooltip("放置区域检测半径")]
    [SerializeField] private float placementRadius = 0.5f;
    #endregion

    #region Axis Constraints
    [Header("轴约束")]
    [Tooltip("锁定X轴移动")]
    [SerializeField] private bool lockXAxis = false;

    [Tooltip("锁定Y轴移动")]
    [SerializeField] private bool lockYAxis = true;

    [Tooltip("锁定Z轴移动")]
    [SerializeField] private bool lockZAxis = false;
    #endregion

    #region Placement Settings
    [Header("放置设置")]
    [Tooltip("放置偏移量")]
    [SerializeField] private Vector3 placementOffset = Vector3.up * 0.1f;

    [Tooltip("放置物体时使用的层级")]
    [SerializeField] private LayerMask placementLayer;
    #endregion

    #region Shader & Color Settings
    [Header("着色器设置")]
    [SerializeField] private Shader targetShader;
    [SerializeField] private Color highlightColor = Color.white;
    #endregion

    #region Runtime Variables
    private Dictionary<Material, Color> originalColors = new Dictionary<Material, Color>();
    private bool isDragging;
    private GameObject draggedObject;
    private Vector3 dragStartPosition;
    #endregion

    #region Unity Callbacks
    // 修改Update方法
    private void Update()
    {
        if (isPlacing)
        {
            UpdateObjectPosition();
            HandlePlacementInput();
        }
    }

    private void OnDrawGizmos()
    {
        // 绘制放置区域可视化
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(placementStart, placementEnd);
        Gizmos.DrawWireSphere(placementStart, placementRadius);
        Gizmos.DrawWireSphere(placementEnd, placementRadius);
    }
    #endregion

    // 修改建造模式切换方法
    public void ToggleConstructionMode()
    {
        isConstructionMode = !isConstructionMode;
        constructionHUD.SetActive(isConstructionMode);

        if (isConstructionMode)
        {
            StartPlacement();
        }
        else
        {
            ConfirmPlacement();
        }
    }

    // 新增放置初始化方法
    private void StartPlacement()
    {
        // 实例化新物体
        currentObject = Instantiate(placementPrefab);
        isPlacing = true;
        UpdateObjectPosition(); // 立即更新位置
    }

    #region Input Handling
    // 修改输入处理
    private void HandlePlacementInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ConfirmPlacement();
        }
    }


    // 新增实时位置更新方法
    private void UpdateObjectPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane movementPlane = new Plane(CalculateLockedNormal(), placementStart);

        if (movementPlane.Raycast(ray, out float enter))
        {
            Vector3 targetPosition = ray.GetPoint(enter);
            Vector3 clampedPosition = ClampToLine(targetPosition);
            ApplyAxisConstraints(ref clampedPosition);

            // 平滑移动
            currentObject.transform.position = Vector3.Lerp(
                currentObject.transform.position,
                clampedPosition + placementOffset,
                Time.deltaTime * moveSpeed
            );
        }
    }

    private void TryStartDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayer))
        {
            if (IsInPlacementArea(hit.point))
            {
                StartDrag(hit.collider.gameObject);
            }
        }
    }

    private void StartDrag(GameObject obj)
    {
        isDragging = true;
        draggedObject = obj;
        dragStartPosition = obj.transform.position;
        ModifyOutlineColor(highlightColor);
    }

    private void EndDrag()
    {
        isDragging = false;
        draggedObject = null;
        RestoreOriginalColors();
    }
    #endregion

    #region Placement Logic
    private void UpdateDragPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane placementPlane = new Plane(CalculateLockedNormal(), dragStartPosition);

        if (placementPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPosition = ray.GetPoint(enter);
            Vector3 clampedPosition = ClampToLine(worldPosition);
            ApplyAxisConstraints(ref clampedPosition);
            draggedObject.transform.position = clampedPosition + placementOffset;
        }
    }

    private Vector3 CalculateLockedNormal()
    {
        if (lockXAxis && !lockZAxis) return Vector3.right;
        if (lockZAxis && !lockXAxis) return Vector3.forward;
        return Vector3.up;
    }

    private Vector3 ClampToLine(Vector3 position)
    {
        Vector3 lineDirection = placementEnd - placementStart;
        float lineLength = lineDirection.magnitude;
        Vector3 normalizedDirection = lineDirection.normalized;

        float projection = Vector3.Dot(position - placementStart, normalizedDirection);
        projection = Mathf.Clamp(projection, 0f, lineLength);

        return placementStart + normalizedDirection * projection;
    }

    private void ApplyAxisConstraints(ref Vector3 position)
    {
        if (lockXAxis) position.x = dragStartPosition.x;
        if (lockYAxis) position.y = dragStartPosition.y;
        if (lockZAxis) position.z = dragStartPosition.z;
    }

    private bool IsInPlacementArea(Vector3 position)
    {
        float distanceToLine = DistanceToLineSegment(position);
        return distanceToLine <= placementRadius;
    }

    private float DistanceToLineSegment(Vector3 point)
    {
        Vector3 a = placementStart;
        Vector3 b = placementEnd;
        Vector3 ab = b - a;
        Vector3 ap = point - a;

        float projection = Vector3.Dot(ap, ab);
        float lengthSqr = ab.sqrMagnitude;

        if (projection <= 0) return Vector3.Distance(point, a);
        if (projection >= lengthSqr) return Vector3.Distance(point, b);

        Vector3 closestPoint = a + ab * (projection / lengthSqr);
        return Vector3.Distance(point, closestPoint);
    }
    #endregion

    // 修改确认放置方法
    public void ConfirmPlacement()
    {
        if (!isPlacing) return;

        // 锁定最终位置
        Vector3 finalPosition = currentObject.transform.position;
        currentObject.transform.position = ClampToLine(finalPosition);
        ApplyAxisConstraints(ref finalPosition);

        currentObject = null;
        isPlacing = false;
        Debug.Log("物体已固定");
    }

    #region Visual Feedback
    private void UpdateHighlight()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayer))
        {
            bool inArea = IsInPlacementArea(hit.point);
            if (inArea && !isDragging)
            {
                ModifyOutlineColor(highlightColor);
            }
            else
            {
                RestoreOriginalColors();
            }
        }
        else
        {
            RestoreOriginalColors();
        }
    }

    #region Visual Feedback
    private void ModifyOutlineColor(Color newColor)
    {
        // 修改后的查找方法（第205行附近）
        foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (!IsInPlacementArea(renderer.transform.position)) continue;

            foreach (Material mat in renderer.materials)
            {
                if (mat.shader == targetShader)
                {
                    if (!originalColors.ContainsKey(mat))
                    {
                        originalColors.Add(mat, mat.GetColor("_OutlineColor"));
                    }
                    mat.SetColor("_OutlineColor", newColor);
                }
            }
        }
    }
    #endregion

    private void RestoreOriginalColors()
    {
        foreach (var kvp in originalColors)
        {
            if (kvp.Key != null)
            {
                kvp.Key.SetColor("_OutlineColor", kvp.Value);
            }
        }
        originalColors.Clear();
    }
    #endregion

    #region Public Methods
    public void ConfigurePlacementLine(Vector3 start, Vector3 end)
    {
        placementStart = start;
        placementEnd = end;
    }

    public void SetAxisConstraints(bool x, bool y, bool z)
    {
        lockXAxis = x;
        lockYAxis = y;
        lockZAxis = z;
    }
    #endregion
}