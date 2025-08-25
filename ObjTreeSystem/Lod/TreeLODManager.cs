using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeLODManager : MonoBehaviour
{
    // 低模LOD预制体，用于批量替换
    public GameObject lowLODPrefab;

    // 被砍伐后的树桩预制体
    public GameObject stumpPrefab;

    // 管理的树木根节点
    public Transform treesParent;

    // 存储树木信息的字典
    private Dictionary<GameObject, TreeInfo> treeDict = new Dictionary<GameObject, TreeInfo>();

    // 树木信息结构
    private struct TreeInfo
    {
        public GameObject originalTree;
        public GameObject currentLOD;
        public bool isCutDown;
        public LODGroup lodGroup;
    }

    // 在TreeLODManager类中添加
    void Start()
    {
        // 自动开始初始化树木LOD
        StartCoroutine(InitializeAllTrees());
    }

    // 初始化所有树木的LOD - 修改为返回IEnumerator
    public IEnumerator InitializeAllTrees()
    {
        int childCount = treesParent.childCount;

        // 开始处理大量对象前的性能优化
        UnityEngine.Profiling.Profiler.BeginSample("TreeLODInitialization");

        for (int i = 0; i < childCount; i++)
        {
            Transform treeTransform = treesParent.GetChild(i);
            GameObject tree = treeTransform.gameObject;

            if (tree.activeSelf && tree.CompareTag("Tree"))
            {
                InitializeTreeLOD(tree);
            }

            // 每处理100个树木，让出一些时间给主线程，避免卡顿
            if (i % 100 == 0)
            {
                yield return null;
            }
        }

        UnityEngine.Profiling.Profiler.EndSample();
        Debug.Log("树木LOD初始化完成，共处理: " + treeDict.Count + " 棵树");
    }

    // 初始化单个树木的LOD
    private void InitializeTreeLOD(GameObject tree)
    {
        LODGroup lodGroup = tree.GetComponent<LODGroup>();

        if (lodGroup == null)
        {
            lodGroup = tree.AddComponent<LODGroup>();
        }

        // 获取原始高模渲染器
        Renderer[] originalRenderers = tree.GetComponentsInChildren<Renderer>();

        // 创建LOD数组
        LOD[] lods = new LOD[2];

        // 设置高模LOD (默认使用原始渲染器)
        lods[0] = new LOD(0.1f, originalRenderers);

        // 创建低模实例
        GameObject lowLODInstance = Instantiate(lowLODPrefab, tree.transform.position,
                                                tree.transform.rotation, tree.transform);
        lowLODInstance.SetActive(false); // 初始不激活，由LODGroup控制

        // 设置低模LOD
        Renderer[] lowLODRenderers = lowLODInstance.GetComponentsInChildren<Renderer>();
        lods[1] = new LOD(0.02f, lowLODRenderers);

        // 应用LOD设置
        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();

        // 存储树木信息
        TreeInfo info = new TreeInfo
        {
            originalTree = tree,
            currentLOD = lowLODInstance,
            isCutDown = false,
            lodGroup = lodGroup
        };

        treeDict[tree] = info;
    }

    // 批量设置所有树木的LOD显示距离
    public void SetAllTreeLODDistances(float highLODDistance, float lowLODDistance)
    {
        foreach (var pair in treeDict)
        {
            LODGroup lodGroup = pair.Value.lodGroup;
            LOD[] lods = lodGroup.GetLODs();

            if (lods.Length >= 2)
            {
                lods[0].screenRelativeTransitionHeight = highLODDistance;
                lods[1].screenRelativeTransitionHeight = lowLODDistance;
                lodGroup.SetLODs(lods);
                lodGroup.RecalculateBounds();
            }
        }
    }

    // 砍伐树木
    public void CutDownTree(GameObject tree)
    {
        if (!treeDict.ContainsKey(tree))
        {
            Debug.LogWarning("尝试砍伐未注册的树木: " + tree.name);
            return;
        }

        TreeInfo info = treeDict[tree];

        if (info.isCutDown)
        {
            Debug.Log("树木已被砍伐: " + tree.name);
            return;
        }

        // 禁用LODGroup，手动控制显示
        info.lodGroup.enabled = false;

        // 隐藏所有LOD
        foreach (Transform child in tree.transform)
        {
            child.gameObject.SetActive(false);
        }

        // 创建树桩
        GameObject stumpInstance = Instantiate(stumpPrefab, tree.transform.position,
                                              tree.transform.rotation, tree.transform);
        stumpInstance.SetActive(true);

        // 更新树木信息
        info.isCutDown = true;
        treeDict[tree] = info;

        // 触发树木被砍伐的事件
        OnTreeCutDown?.Invoke(tree);
    }

    // 批量替换所有树木的低模LOD
    public void ReplaceAllLowLODModels(GameObject newLowLODPrefab)
    {
        lowLODPrefab = newLowLODPrefab;

        foreach (var pair in treeDict)
        {
            GameObject tree = pair.Key;
            TreeInfo info = pair.Value;

            if (!info.isCutDown)
            {
                // 销毁旧的低模
                if (info.currentLOD != null)
                {
                    Destroy(info.currentLOD);
                }

                // 创建新的低模实例
                GameObject newLowLODInstance = Instantiate(lowLODPrefab, tree.transform.position,
                                                           tree.transform.rotation, tree.transform);
                newLowLODInstance.SetActive(false); // 由LODGroup控制

                // 更新LOD设置
                LODGroup lodGroup = info.lodGroup;
                LOD[] lods = lodGroup.GetLODs();

                if (lods.Length >= 2)
                {
                    Renderer[] newLowLODRenderers = newLowLODInstance.GetComponentsInChildren<Renderer>();
                    lods[1] = new LOD(0.02f, newLowLODRenderers);
                    lodGroup.SetLODs(lods);
                    lodGroup.RecalculateBounds();
                }

                // 更新树木信息
                info.currentLOD = newLowLODInstance;
                treeDict[tree] = info;
            }
        }

        Debug.Log("所有树木的低模LOD已替换");
    }

    // 事件：当树木被砍伐时触发
    public System.Action<GameObject> OnTreeCutDown;
}