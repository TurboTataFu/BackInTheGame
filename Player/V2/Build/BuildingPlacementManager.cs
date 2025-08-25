using UnityEngine;
using System.Collections.Generic;

public class BuildingPlacementManager : MonoBehaviour
{
    public static BuildingPlacementManager Instance { get; private set; }

    [Header("配置")]
    public List<BuildingData> allBuildings = new List<BuildingData>();

    [Header("预览设置")]
    public Material previewMaterial;
    public Material invalidMaterial;

    [Header("调试与辅助线")]
    public bool debugLogs = true;
    public bool showMouseToPointLine = true;
    public Color lineColor = Color.cyan;
    public Color pointColor = Color.yellow;
    public float pointSize = 0.2f;

    // 内部状态管理
    private Dictionary<string, Dictionary<string, BuildingData>> categorizedBuildings = new Dictionary<string, Dictionary<string, BuildingData>>();

    // 放置阶段核心变量
    private BuildingData currentBuildingData;
    private GameObject previewObject;
    private bool isAdjustingPosition = false;
    private bool isPositionValid = true;
    private Vector3 currentMouseWorldPos;
    private Vector3 currentNearestPoint;
    private PlacementAreaSettings currentAreaSettings;  // 当前检测到的放置区域设置
    private GameObject targetExistingObject;// 存储待调整的原始对象
    // 公开属性
    public bool IsAdjusting => isAdjustingPosition;
    public BuildingData CurrentBuilding => currentBuildingData;
    public bool IsPositionValid => isPositionValid;
    public PlacementAreaSettings CurrentAreaSettings => currentAreaSettings;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeBuildings();
            ValidateSetup();
        }
        else
        {
            LogWarning("场景中已存在BuildingPlacementManager实例，当前实例将被销毁");
            Destroy(gameObject);
        }
    }

    private void InitializeBuildings()
    {
        categorizedBuildings.Clear();

        foreach (var building in allBuildings)
        {
            if (!building.IsValid())
            {
                LogError($"建筑数据不完整 - {building.DisplayName}");
                continue;
            }

            if (!categorizedBuildings.ContainsKey(building.category))
            {
                categorizedBuildings[building.category] = new Dictionary<string, BuildingData>();
                Log($"添加建筑分类: {building.category}");
            }

            if (categorizedBuildings[building.category].ContainsKey(building.id))
            {
                LogWarning($"建筑ID重复 - {building.DisplayName}，将覆盖已有数据");
            }

            categorizedBuildings[building.category][building.id] = building;
            Log($"注册建筑 - {building.DisplayName}");
        }
    }

    private void ValidateSetup()
    {
// 查找所有放置区域
        var placementAreas = FindObjectsByType<PlacementAreaSettings>(FindObjectsSortMode.None);
        if (placementAreas.Length == 0)
        {
            LogWarning("场景中未找到任何PlacementAreaSettings组件！请至少添加一个放置区域");
        }

        if (previewMaterial == null)
        {
            LogError("未设置预览材质！请指定previewMaterial");
        }
        if (invalidMaterial == null)
        {
            LogError("未设置无效材质！请指定invalidMaterial");
        }

        if (allBuildings == null || allBuildings.Count == 0)
        {
            LogWarning("未添加任何建筑数据，无法进行放置操作");
        }
    }

    /// <summary>
    /// 开始位置调整阶段
    /// </summary>
    public bool StartAdjusting(BuildingData data)
    {
        if (data == null)
        {
            LogError("StartAdjusting失败：建筑数据为空");
            return false;
        }
        return StartAdjusting(data.category, data.id);
    }

    public bool StartAdjusting(string category, string id)
    {
        EndAdjustment();

        if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(id))
        {
            LogError("建筑分类或ID不能为空");
            return false;
        }

        if (categorizedBuildings.TryGetValue(category, out var categoryDict) &&
            categoryDict.TryGetValue(id, out var data))
        {
            currentBuildingData = data;

            if (currentBuildingData.prefab == null)
            {
                LogError($"建筑预制件为空 - {currentBuildingData.DisplayName}");
                currentBuildingData = null;
                return false;
            }

            CreatePreviewObject();
            isAdjustingPosition = true;
            Log($"开始调整建筑位置 - {currentBuildingData.DisplayName}");
            return true;
        }
        else
        {
            LogError($"未找到建筑 - {category}/{id}");
            return false;
        }
    }

    private void CreatePreviewObject()
    {
        if (currentBuildingData?.prefab == null)
        {
            LogError("无法创建预览物体：建筑数据或预制件为空");
            return;
        }

        if (previewObject != null)
            Destroy(previewObject);

        previewObject = Instantiate(currentBuildingData.prefab);
        previewObject.name = $"Adjust_Preview_{currentBuildingData.id}";
        previewObject.tag = "EditorOnly";

        foreach (var collider in previewObject.GetComponentsInChildren<Collider>())
            collider.enabled = false;

        foreach (var rigidbody in previewObject.GetComponentsInChildren<Rigidbody>())
        {
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }

        UpdatePreviewMaterials();
    }

    private void UpdatePreviewMaterials()
    {
        if (previewObject == null) return;

        var renderers = previewObject.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        foreach (var renderer in renderers)
        {
            Material[] newMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
                newMaterials[i] = isPositionValid ? previewMaterial : invalidMaterial;

            renderer.materials = newMaterials;
        }
    }

    private void Update()
    {
        if (isAdjustingPosition && previewObject != null)
        {
            UpdateAdjustingPosition();

            if (Input.GetMouseButtonDown(0) && isPositionValid)
            {
                ConfirmPlacement();
            }

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                EndAdjustment();
            }
        }
    }

    private void UpdateAdjustingPosition()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        currentMouseWorldPos = mouseRay.origin;

        // 计算带区域约束的位置
        Vector3 targetPos = CalculateConstrainedPosition(mouseRay, out isPositionValid);
        currentNearestPoint = targetPos;

        previewObject.transform.position = targetPos;
        UpdatePreviewMaterials();
    }

    /// <summary>
    /// 计算带有区域轴约束的位置
    /// </summary>
    private Vector3 CalculateConstrainedPosition(Ray mouseRay, out bool isValid)
    {
        int placementLayer = LayerMask.NameToLayer("PlacementArea");
        if (placementLayer == -1)
        {
            LogError("未找到'PlacementArea'图层！请创建该图层");
            isValid = false;
            currentAreaSettings = null;
            return Vector3.zero;
        }

        int layerMask = 1 << placementLayer;
        RaycastHit hit;
        currentAreaSettings = null;
        Vector3 basePosition = Vector3.zero;

        // 检测是否直接命中放置区域
        if (Physics.Raycast(mouseRay, out hit, Mathf.Infinity, layerMask))
        {
            basePosition = hit.point;
            currentAreaSettings = hit.collider.GetComponent<PlacementAreaSettings>();
        }
        else
        {
            // 未直接命中时，找最近的放置区域
            basePosition = FindClosestPointToAnyPlacementArea(mouseRay.origin, out currentAreaSettings);
        }

        // 应用当前区域的轴约束
        if (currentAreaSettings != null)
        {
            Vector3 constrainedPos = currentAreaSettings.ApplyAllConstraints(basePosition);
            isValid = currentAreaSettings.IsPositionValid(constrainedPos);
            return constrainedPos;
        }

        // 没有找到任何放置区域
        isValid = false;
        return basePosition;
    }

    /// <summary>
    /// 找到离目标点最近的放置区域和点
    /// </summary>
    private Vector3 FindClosestPointToAnyPlacementArea(Vector3 fromPosition, out PlacementAreaSettings closestArea)
    {
        closestArea = null;
        float minDistance = float.MaxValue;
        Vector3 nearestPoint = fromPosition;

        var allAreas = FindObjectsByType<PlacementAreaSettings>(FindObjectsSortMode.None);
        foreach (var area in allAreas)
        {
            Collider collider = area.GetComponent<Collider>();
            if (collider == null) continue;

            Vector3 closest = collider.ClosestPoint(fromPosition);
            float distance = Vector3.Distance(fromPosition, closest);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = closest;
                closestArea = area;
            }
        }

        return nearestPoint;
    }

    /// <summary>
    /// 开始调整场景中已有对象的位置
    /// </summary>
    public void StartAdjustingExistingObject(GameObject existingObject)
    {
        EndAdjustment();  // 结束当前放置
        targetExistingObject = existingObject;

        // 创建预览对象（复制现有对象的位置和旋转）
        previewObject = Instantiate(existingObject);
        previewObject.name = $"Adjust_Existing_{existingObject.name}";
        previewObject.tag = "EditorOnly";

        // 禁用碰撞体和刚体（同新建筑预览逻辑）
        foreach (var collider in previewObject.GetComponentsInChildren<Collider>())
            collider.enabled = false;
        foreach (var rigidbody in previewObject.GetComponentsInChildren<Rigidbody>())
        {
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }

        UpdatePreviewMaterials();
        isAdjustingPosition = true;
    }

    private void ConfirmPlacement()
    {
        if (targetExistingObject != null)
        {
            // 调整已有对象的位置和旋转
            targetExistingObject.transform.position = previewObject.transform.position;
            targetExistingObject.transform.rotation = previewObject.transform.rotation;
            Log($"已调整对象位置：{targetExistingObject.name}");

            targetExistingObject = null;  // 清空引用
        }
        else
        {
            if (currentBuildingData?.prefab == null || previewObject == null)
            {
                LogError("放置失败：数据不完整");
                EndAdjustment();
                return;
            }

            GameObject placedBuilding = Instantiate(
                currentBuildingData.prefab,
                previewObject.transform.position,
                previewObject.transform.rotation
            );
            placedBuilding.name = $"{currentBuildingData.id}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";

            foreach (var collider in placedBuilding.GetComponentsInChildren<Collider>())
                collider.enabled = true;

            foreach (var rigidbody in placedBuilding.GetComponentsInChildren<Rigidbody>())
            {
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
            }

            Log($"成功放置建筑: {placedBuilding.name} 在区域: {currentAreaSettings?.areaId ?? "未知区域"}");
        }
        EndAdjustment();
    }

    public void EndAdjustment()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        currentBuildingData = null;
        currentAreaSettings = null;
        isAdjustingPosition = false;
        isPositionValid = true;
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log($"[建筑放置管理器] {message}");
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[建筑放置管理器] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[建筑放置管理器] {message}");
    }

    private void OnDrawGizmos()
    {
        if (showMouseToPointLine && isAdjustingPosition)
        {
            Gizmos.color = lineColor;
            Gizmos.DrawLine(currentMouseWorldPos, currentNearestPoint);

            Gizmos.color = pointColor;
            Gizmos.DrawSphere(currentNearestPoint, pointSize);
        }
    }
}
