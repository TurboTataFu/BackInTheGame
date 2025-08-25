using UnityEngine;
using System.Collections.Generic;

public class TreeInstancingSystem : MonoBehaviour
{
    [Header("��ײ������")]
    public Mesh lowPolyColliderMesh;
    public float colliderActiveDistance = 20f;
    public int maxActiveColliders = 100;
    public float colliderCheckInterval = 2f;
    public float parentMaxDistance = 100f;

    private List<GameObject> colliderPool = new List<GameObject>();
    private Dictionary<TreeInstanceData, GameObject> treeToColliderMap = new Dictionary<TreeInstanceData, GameObject>();

    [Header("LODģ������")]
    public Mesh lod0Mesh;
    public Mesh lod1Mesh;
    public Mesh lod2Mesh;

    [Header("��������")]
    public Material[] treeMaterials;

    [Header("LOD�л�����")]
    [Range(0, 100)] public float lod0Distance = 30f;
    [Range(0, 100)] public float lod1Distance = 60f;

    [Header("λ�ø�����")]
    public Transform treePositionsParent;

    [Header("��ת����")]
    public Vector3 rotationOffset = new Vector3(90, 0, 0);
    public Vector3 billboardRotationOffset = Vector3.zero;

    [Header("��������")]
    public float baseScale = 1.0f;

    private Camera mainCamera;
    private List<Matrix4x4>[] lodMatrices = new List<Matrix4x4>[3];

    // �������
    private List<TreeInstanceData> lazyUpdateTrees = new List<TreeInstanceData>(); // ���Ը�����(3000)
    private List<TreeInstanceData> mediumUpdateTrees = new List<TreeInstanceData>(); // ��ӹ������(1500)
    private List<TreeInstanceData> urgentUpdateTrees = new List<TreeInstanceData>(); // ���ȸ�����(1000)

    class TreeInstanceData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Matrix4x4 worldMatrix;
        public int currentLOD;
        public float distanceToPlayer;
        public GameObject assignedCollider; // ��¼�������������ײ��
        public int instanceId; // Ψһ��ʶ������׼ȷƥ��
    }

    private List<TreeInstanceData> allTreeInstances = new List<TreeInstanceData>();
    private float lastLazyUpdateTime;
    private float lastMediumUpdateTime;
    private float lastUrgentUpdateTime;
    private int nextInstanceId = 0;

    // ���¼������
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
            Debug.LogError("δָ����λ�ø����壡");
            return;
        }

        CollectTreeInstances();
        CreateColliderPool();

        // ��ʼ����
        UpdateTreeGroups();

        // ������ʱ���
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
            data.instanceId = nextInstanceId++; // ����ΨһID

            allTreeInstances.Add(data);
        }

        Debug.Log($"�ɹ��ռ� {allTreeInstances.Count} ����ʵ��");
    }

    void CreateColliderPool()
    {
        colliderPool.Clear();

        for (int i = 0; i < maxActiveColliders; i++)
        {
            GameObject colliderObj = new GameObject($"TreeCollider_{i}");
            colliderObj.transform.SetParent(transform);
            colliderObj.SetActive(false);

            // �����ײ���ʶ��������ڷ������
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

        // ����ͬ������¸�������LOD
        UpdateLazyGroup();
        UpdateMediumGroup();
        UpdateUrgentGroup();

        // ׼����Ⱦ����
        PrepareRenderMatrices();

        // ��Ⱦʵ��������
        RenderInstancedMeshes();
    }

    // ���¶��Ը����� (40��һ��)
    void UpdateLazyGroup()
    {
        if (Time.time - lastLazyUpdateTime >= LAZY_UPDATE_INTERVAL)
        {
            lastLazyUpdateTime = Time.time;
            UpdateTreeDistancesAndLOD(lazyUpdateTrees);
            UpdateTreeGroups();
        }
    }

    // ������ӹ������ (10��һ��)
    void UpdateMediumGroup()
    {
        if (Time.time - lastMediumUpdateTime >= MEDIUM_UPDATE_INTERVAL)
        {
            lastMediumUpdateTime = Time.time;
            UpdateTreeDistancesAndLOD(mediumUpdateTrees);
        }
    }

    // ���½��ȸ����� (5��һ��)
    void UpdateUrgentGroup()
    {
        if (Time.time - lastUrgentUpdateTime >= URGENT_UPDATE_INTERVAL)
        {
            lastUrgentUpdateTime = Time.time;
            UpdateTreeDistancesAndLOD(urgentUpdateTrees);
        }
    }

    // ����ָ��������ľ����LOD
    void UpdateTreeDistancesAndLOD(List<TreeInstanceData> trees)
    {
        Vector3 camPos = mainCamera.transform.position;
        foreach (var tree in trees)
        {
            tree.distanceToPlayer = Vector3.Distance(camPos, tree.position);
            tree.currentLOD = GetLODLevel(tree.distanceToPlayer);
        }
    }

    // ���ݾ����ȡLOD����
    int GetLODLevel(float distance)
    {
        if (distance < lod0Distance) return 0;
        if (distance < lod1Distance) return 1;
        return 2;
    }

    // ���·�����
    void UpdateTreeGroups()
    {
        if (mainCamera == null) return;

        Vector3 camPos = mainCamera.transform.position;
        foreach (var tree in allTreeInstances)
        {
            tree.distanceToPlayer = Vector3.Distance(camPos, tree.position);
        }

        // ���������� (����Զ)
        allTreeInstances.Sort((a, b) => a.distanceToPlayer.CompareTo(b.distanceToPlayer));

        // ������з���
        lazyUpdateTrees.Clear();
        mediumUpdateTrees.Clear();
        urgentUpdateTrees.Clear();

        int totalTrees = allTreeInstances.Count;

        // ���������1000�õ�������
        int urgentCount = Mathf.Min(1000, totalTrees);
        for (int i = 0; i < urgentCount; i++)
        {
            urgentUpdateTrees.Add(allTreeInstances[i]);
            allTreeInstances[i].currentLOD = GetLODLevel(allTreeInstances[i].distanceToPlayer);
        }

        // �����������1500�õ���ӹ��
        int mediumStart = urgentCount;
        int mediumCount = Mathf.Min(1500, totalTrees - mediumStart);
        for (int i = 0; i < mediumCount; i++)
        {
            mediumUpdateTrees.Add(allTreeInstances[mediumStart + i]);
            allTreeInstances[mediumStart + i].currentLOD = GetLODLevel(allTreeInstances[mediumStart + i].distanceToPlayer);
        }

        // ����ʣ��ĵ������� (���3000)
        int lazyStart = mediumStart + mediumCount;
        int lazyCount = Mathf.Min(3000, totalTrees - lazyStart);
        for (int i = 0; i < lazyCount; i++)
        {
            lazyUpdateTrees.Add(allTreeInstances[lazyStart + i]);
            allTreeInstances[lazyStart + i].currentLOD = GetLODLevel(allTreeInstances[lazyStart + i].distanceToPlayer);
        }
    }

    // ׼����Ⱦ����
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

    // ������ӵ���Ⱦ����
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

    // ��ʱ������ײ��״̬
    void UpdateColliders()
    {
        if (mainCamera == null || treePositionsParent == null) return;

        Vector3 camPos = mainCamera.transform.position;

        // ��������Զ���ر�������ײ��
        if (Vector3.Distance(camPos, treePositionsParent.position) > parentMaxDistance)
        {
            DisableAllColliders();
            return;
        }

        // 1. �Ƚ��ó�����Χ����ײ�䣬���������
        List<GameObject> freeColliders = new List<GameObject>();

        foreach (var tree in allTreeInstances)
        {
            if (tree.assignedCollider != null)
            {
                // ������Ƿ����ڼ��Χ��
                if (tree.distanceToPlayer >= colliderActiveDistance)
                {
                    tree.assignedCollider.SetActive(false);
                    freeColliders.Add(tree.assignedCollider);
                    tree.assignedCollider = null; // �������
                }
            }
        }

        // 2. �ռ����п��õ���ײ�䣨�������ͷŵĺ�ԭ���Ϳ��еģ�
        foreach (var collider in colliderPool)
        {
            if (!collider.activeInHierarchy && !freeColliders.Contains(collider))
            {
                freeColliders.Add(collider);
            }
        }

        // 3. �ռ���Ҫ��ײ��������ڷ�Χ������δ������ײ�䣩
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

        // ������������������Ȼ����ײ��
        treesNeedingColliders.Sort((a, b) => a.distanceToPlayer.CompareTo(b.distanceToPlayer));

        // 4. ������ײ��
        int assignCount = Mathf.Min(treesNeedingColliders.Count, freeColliders.Count);
        for (int i = 0; i < assignCount; i++)
        {
            TreeInstanceData tree = treesNeedingColliders[i];
            GameObject collider = freeColliders[i];

            // ������ײ��λ����̬
            collider.transform.position = tree.position;
            collider.transform.rotation = tree.rotation;
            collider.transform.localScale = tree.scale;

            // ����������
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
                tree.assignedCollider = null; // �������
            }
        }
    }

    // ���ָ�����Ƿ��м������ײ��
    bool HasActiveCollider(TreeInstanceData tree)
    {
        return tree.assignedCollider != null && tree.assignedCollider.activeInHierarchy;
    }

    // ����ָ����ײ���Ӧ����
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

        // ��������ײ��������ú�ɫ��ʶ��
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

        // ������������λ�ã�����ɫ��ʶ��
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

// ���ڱ�ʶ��ײ��ĸ������
public class ColliderIdentifier : MonoBehaviour
{
    public int colliderId;
}