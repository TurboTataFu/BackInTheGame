using UnityEngine;
using System.Collections.Generic;

public class TreeInstancingSystem : MonoBehaviour
{
    [Header("碰撞体设置")]
    public Mesh lowPolyColliderMesh;
    public float colliderActiveDistance = 20f;
    public int maxActiveColliders = 100;
    public float colliderCheckInterval = 2f;
    public float parentMaxDistance = 100f;

    private List<GameObject> colliderPool = new List<GameObject>();
    private Dictionary<TreeInstanceData, GameObject> treeToColliderMap = new Dictionary<TreeInstanceData, GameObject>();

    [Header("LOD模型设置")]
    public Mesh lod0Mesh;
    public Mesh lod1Mesh;
    public Mesh lod2Mesh;

    [Header("材质设置")]
    public Material[] treeMaterials;

    [Header("LOD切换距离")]
    [Range(0, 100)] public float lod0Distance = 30f;
    [Range(0, 100)] public float lod1Distance = 60f;

    [Header("位置父物体")]
    public Transform treePositionsParent;

    [Header("旋转补偿")]
    public Vector3 rotationOffset = new Vector3(90, 0, 0);
    public Vector3 billboardRotationOffset = Vector3.zero;

    [Header("缩放设置")]
    public float baseScale = 1.0f;

    private Camera mainCamera;
    private List<Matrix4x4>[] lodMatrices = new List<Matrix4x4>[3];

    // 分组管理
    private List<TreeInstanceData> lazyUpdateTrees = new List<TreeInstanceData>(); // 惰性更新组(3000)
    private List<TreeInstanceData> mediumUpdateTrees = new List<TreeInstanceData>(); // 中庸更新组(1500)
    private List<TreeInstanceData> urgentUpdateTrees = new List<TreeInstanceData>(); // 紧迫更新组(1000)

    class TreeInstanceData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Matrix4x4 worldMatrix;
        public int currentLOD;
        public float distanceToPlayer;
        public GameObject assignedCollider; // 记录分配给该树的碰撞箱
        public int instanceId; // 唯一标识，用于准确匹配
    }

    private List<TreeInstanceData> allTreeInstances = new List<TreeInstanceData>();
    private float lastLazyUpdateTime;
    private float lastMediumUpdateTime;
    private float lastUrgentUpdateTime;
    private int nextInstanceId = 0;

    // 更新间隔常量
    private const float LAZY_UPDATE_INTERVAL = 40f;
    private const float MEDIUM_UPDATE_INTERVAL = 10f;
    private const float URGENT_UPDATE_INTERVAL = 5f;

    void Start()
    {
        mainCamera = Camera.main;

        for (int i = 0; i < 3; i++)
        {
            lodMatrices[i] = new List<Matrix4x4>();
        }

        if (treePositionsParent == null)
        {
            Debug.LogError("未指定树位置父物体！");
            return;
        }

        CollectTreeInstances();
        CreateColliderPool();

        // 初始分组
        UpdateTreeGroups();

        // 启动定时检测
        InvokeRepeating(nameof(UpdateColliders), 0f, colliderCheckInterval);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(UpdateColliders));
    }

    void CollectTreeInstances()
    {
        allTreeInstances.Clear();
        nextInstanceId = 0;

        foreach (Transform child in treePositionsParent)
        {
            if (child.parent != treePositionsParent) continue;

            Matrix4x4 parentMatrix = treePositionsParent.localToWorldMatrix;
            Matrix4x4 childLocalMatrix = Matrix4x4.TRS(
                child.localPosition,
                child.localRotation * Quaternion.Euler(rotationOffset),
                Vector3.one * baseScale * Random.Range(0.8f, 1.2f)
            );

            Matrix4x4 worldMatrix = parentMatrix * childLocalMatrix;

            TreeInstanceData data = new TreeInstanceData();
            data.worldMatrix = worldMatrix;
            data.position = worldMatrix.GetColumn(3);
            data.rotation = Quaternion.LookRotation(worldMatrix.GetColumn(2), worldMatrix.GetColumn(1));
            data.scale = new Vector3(
                worldMatrix.GetColumn(0).magnitude,
                worldMatrix.GetColumn(1).magnitude,
                worldMatrix.GetColumn(2).magnitude
            );
            data.currentLOD = 0;
            data.distanceToPlayer = 0;
            data.assignedCollider = null;
            data.instanceId = nextInstanceId++; // 分配唯一ID

            allTreeInstances.Add(data);
        }

        Debug.Log($"成功收集 {allTreeInstances.Count} 个树实例");
    }

    void CreateColliderPool()
    {
        colliderPool.Clear();

        for (int i = 0; i < maxActiveColliders; i++)
        {
            GameObject colliderObj = new GameObject($"TreeCollider_{i}");
            colliderObj.transform.SetParent(transform);
            colliderObj.SetActive(false);

            // 添加碰撞箱标识组件，用于反向查找
            ColliderIdentifier identifier = colliderObj.AddComponent<ColliderIdentifier>();
            identifier.colliderId = i;

            MeshCollider collider = colliderObj.AddComponent<MeshCollider>();
            collider.sharedMesh = lowPolyColliderMesh;
            collider.convex = true;

            colliderPool.Add(colliderObj);
        }
    }

    void Update()
    {
        if (mainCamera == null) return;

        // 按不同间隔更新各组距离和LOD
        UpdateLazyGroup();
        UpdateMediumGroup();
        UpdateUrgentGroup();

        // 准备渲染矩阵
        PrepareRenderMatrices();

        // 渲染实例化网格
        RenderInstancedMeshes();
    }

    // 更新惰性更新组 (40秒一次)
    void UpdateLazyGroup()
    {
        if (Time.time - lastLazyUpdateTime >= LAZY_UPDATE_INTERVAL)
        {
            lastLazyUpdateTime = Time.time;
            UpdateTreeDistancesAndLOD(lazyUpdateTrees);
            UpdateTreeGroups();
        }
    }

    // 更新中庸更新组 (10秒一次)
    void UpdateMediumGroup()
    {
        if (Time.time - lastMediumUpdateTime >= MEDIUM_UPDATE_INTERVAL)
        {
            lastMediumUpdateTime = Time.time;
            UpdateTreeDistancesAndLOD(mediumUpdateTrees);
        }
    }

    // 更新紧迫更新组 (5秒一次)
    void UpdateUrgentGroup()
    {
        if (Time.time - lastUrgentUpdateTime >= URGENT_UPDATE_INTERVAL)
        {
            lastUrgentUpdateTime = Time.time;
            UpdateTreeDistancesAndLOD(urgentUpdateTrees);
        }
    }

    // 更新指定组的树的距离和LOD
    void UpdateTreeDistancesAndLOD(List<TreeInstanceData> trees)
    {
        Vector3 camPos = mainCamera.transform.position;
        foreach (var tree in trees)
        {
            tree.distanceToPlayer = Vector3.Distance(camPos, tree.position);
            tree.currentLOD = GetLODLevel(tree.distanceToPlayer);
        }
    }

    // 根据距离获取LOD级别
    int GetLODLevel(float distance)
    {
        if (distance < lod0Distance) return 0;
        if (distance < lod1Distance) return 1;
        return 2;
    }

    // 重新分组树
    void UpdateTreeGroups()
    {
        if (mainCamera == null) return;

        Vector3 camPos = mainCamera.transform.position;
        foreach (var tree in allTreeInstances)
        {
            tree.distanceToPlayer = Vector3.Distance(camPos, tree.position);
        }

        // 按距离排序 (近到远)
        allTreeInstances.Sort((a, b) => a.distanceToPlayer.CompareTo(b.distanceToPlayer));

        // 清空现有分组
        lazyUpdateTrees.Clear();
        mediumUpdateTrees.Clear();
        urgentUpdateTrees.Clear();

        int totalTrees = allTreeInstances.Count;

        // 分配最近的1000棵到紧迫组
        int urgentCount = Mathf.Min(1000, totalTrees);
        for (int i = 0; i < urgentCount; i++)
        {
            urgentUpdateTrees.Add(allTreeInstances[i]);
            allTreeInstances[i].currentLOD = GetLODLevel(allTreeInstances[i].distanceToPlayer);
        }

        // 分配接下来的1500棵到中庸组
        int mediumStart = urgentCount;
        int mediumCount = Mathf.Min(1500, totalTrees - mediumStart);
        for (int i = 0; i < mediumCount; i++)
        {
            mediumUpdateTrees.Add(allTreeInstances[mediumStart + i]);
            allTreeInstances[mediumStart + i].currentLOD = GetLODLevel(allTreeInstances[mediumStart + i].distanceToPlayer);
        }

        // 分配剩余的到惰性组 (最多3000)
        int lazyStart = mediumStart + mediumCount;
        int lazyCount = Mathf.Min(3000, totalTrees - lazyStart);
        for (int i = 0; i < lazyCount; i++)
        {
            lazyUpdateTrees.Add(allTreeInstances[lazyStart + i]);
            allTreeInstances[lazyStart + i].currentLOD = GetLODLevel(allTreeInstances[lazyStart + i].distanceToPlayer);
        }
    }

    // 准备渲染矩阵
    void PrepareRenderMatrices()
    {
        for (int i = 0; i < 3; i++)
        {
            lodMatrices[i].Clear();
        }

        AddTreesToRenderMatrices(lazyUpdateTrees);
        AddTreesToRenderMatrices(mediumUpdateTrees);
        AddTreesToRenderMatrices(urgentUpdateTrees);
    }

    // 将树添加到渲染矩阵
    void AddTreesToRenderMatrices(List<TreeInstanceData> trees)
    {
        foreach (var tree in trees)
        {
            if (tree.currentLOD == 2)
            {
                Vector3 dir = mainCamera.transform.position - tree.position;
                Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(billboardRotationOffset);
                lodMatrices[2].Add(Matrix4x4.TRS(tree.position, rot, tree.scale));
            }
            else
            {
                lodMatrices[tree.currentLOD].Add(tree.worldMatrix);
            }
        }
    }

    void RenderInstancedMeshes()
    {
        for (int lod = 0; lod < 3; lod++)
        {
            Mesh mesh = lod == 0 ? lod0Mesh : (lod == 1 ? lod1Mesh : lod2Mesh);
            List<Matrix4x4> matrices = lodMatrices[lod];

            if (mesh == null || matrices.Count == 0 || treeMaterials == null) continue;

            int subMeshCount = Mathf.Min(mesh.subMeshCount, treeMaterials.Length);
            for (int subMesh = 0; subMesh < subMeshCount; subMesh++)
            {
                Graphics.DrawMeshInstanced(
                    mesh,
                    subMesh,
                    treeMaterials[subMesh],
                    matrices,
                    null,
                    UnityEngine.Rendering.ShadowCastingMode.On,
                    true,
                    0,
                    null,
                    UnityEngine.Rendering.LightProbeUsage.Off,
                    null
                );
            }
        }
    }

    // 定时更新碰撞箱状态
    void UpdateColliders()
    {
        if (mainCamera == null || treePositionsParent == null) return;

        Vector3 camPos = mainCamera.transform.position;

        // 如果距离过远，关闭所有碰撞箱
        if (Vector3.Distance(camPos, treePositionsParent.position) > parentMaxDistance)
        {
            DisableAllColliders();
            return;
        }

        // 1. 先禁用超出范围的碰撞箱，并解除关联
        List<GameObject> freeColliders = new List<GameObject>();

        foreach (var tree in allTreeInstances)
        {
            if (tree.assignedCollider != null)
            {
                // 检查树是否仍在激活范围内
                if (tree.distanceToPlayer >= colliderActiveDistance)
                {
                    tree.assignedCollider.SetActive(false);
                    freeColliders.Add(tree.assignedCollider);
                    tree.assignedCollider = null; // 解除关联
                }
            }
        }

        // 2. 收集所有可用的碰撞箱（包括新释放的和原本就空闲的）
        foreach (var collider in colliderPool)
        {
            if (!collider.activeInHierarchy && !freeColliders.Contains(collider))
            {
                freeColliders.Add(collider);
            }
        }

        // 3. 收集需要碰撞箱的树（在范围内且尚未分配碰撞箱）
        List<TreeInstanceData> treesNeedingColliders = new List<TreeInstanceData>();

        foreach (var tree in urgentUpdateTrees)
        {
            if (tree.distanceToPlayer < colliderActiveDistance && tree.assignedCollider == null)
            {
                treesNeedingColliders.Add(tree);
            }
        }

        foreach (var tree in mediumUpdateTrees)
        {
            if (tree.distanceToPlayer < colliderActiveDistance && tree.assignedCollider == null)
            {
                treesNeedingColliders.Add(tree);
            }
        }

        // 按距离排序，最近的优先获得碰撞箱
        treesNeedingColliders.Sort((a, b) => a.distanceToPlayer.CompareTo(b.distanceToPlayer));

        // 4. 分配碰撞箱
        int assignCount = Mathf.Min(treesNeedingColliders.Count, freeColliders.Count);
        for (int i = 0; i < assignCount; i++)
        {
            TreeInstanceData tree = treesNeedingColliders[i];
            GameObject collider = freeColliders[i];

            // 设置碰撞箱位置姿态
            collider.transform.position = tree.position;
            collider.transform.rotation = tree.rotation;
            collider.transform.localScale = tree.scale;

            // 关联并激活
            tree.assignedCollider = collider;
            collider.SetActive(true);
        }
    }

    void DisableAllColliders()
    {
        foreach (var tree in allTreeInstances)
        {
            if (tree.assignedCollider != null)
            {
                tree.assignedCollider.SetActive(false);
                tree.assignedCollider = null; // 解除关联
            }
        }
    }

    // 检查指定树是否有激活的碰撞箱
    bool HasActiveCollider(TreeInstanceData tree)
    {
        return tree.assignedCollider != null && tree.assignedCollider.activeInHierarchy;
    }

    // 查找指定碰撞箱对应的树
    TreeInstanceData FindTreeForCollider(GameObject collider)
    {
        foreach (var tree in allTreeInstances)
        {
            if (tree.assignedCollider == collider)
            {
                return tree;
            }
        }
        return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lod0Distance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lod1Distance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, colliderActiveDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(treePositionsParent ? treePositionsParent.position : transform.position, parentMaxDistance);

        // 绘制有碰撞箱的树（用红色标识）
        Gizmos.color = Color.red;
        if (allTreeInstances != null)
        {
            foreach (var tree in allTreeInstances)
            {
                if (HasActiveCollider(tree))
                {
                    Gizmos.DrawWireCube(tree.position, tree.scale * 0.6f);
                }
            }
        }

        // 绘制所有树的位置（用蓝色标识）
        Gizmos.color = Color.blue;
        if (allTreeInstances != null)
        {
            foreach (var tree in allTreeInstances)
            {
                if (!HasActiveCollider(tree))
                {
                    Gizmos.DrawWireCube(tree.position, tree.scale * 0.5f);
                }
            }
        }
    }
}

// 用于标识碰撞箱的辅助组件
public class ColliderIdentifier : MonoBehaviour
{
    public int colliderId;
}