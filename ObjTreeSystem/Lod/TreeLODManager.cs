using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeLODManager : MonoBehaviour
{
    // ��ģLODԤ���壬���������滻
    public GameObject lowLODPrefab;

    // �����������׮Ԥ����
    public GameObject stumpPrefab;

    // �������ľ���ڵ�
    public Transform treesParent;

    // �洢��ľ��Ϣ���ֵ�
    private Dictionary<GameObject, TreeInfo> treeDict = new Dictionary<GameObject, TreeInfo>();

    // ��ľ��Ϣ�ṹ
    private struct TreeInfo
    {
        public GameObject originalTree;
        public GameObject currentLOD;
        public bool isCutDown;
        public LODGroup lodGroup;
    }

    // ��TreeLODManager�������
    void Start()
    {
        // �Զ���ʼ��ʼ����ľLOD
        StartCoroutine(InitializeAllTrees());
    }

    // ��ʼ��������ľ��LOD - �޸�Ϊ����IEnumerator
    public IEnumerator InitializeAllTrees()
    {
        int childCount = treesParent.childCount;

        // ��ʼ�����������ǰ�������Ż�
        UnityEngine.Profiling.Profiler.BeginSample("TreeLODInitialization");

        for (int i = 0; i < childCount; i++)
        {
            Transform treeTransform = treesParent.GetChild(i);
            GameObject tree = treeTransform.gameObject;

            if (tree.activeSelf && tree.CompareTag("Tree"))
            {
                InitializeTreeLOD(tree);
            }

            // ÿ����100����ľ���ó�һЩʱ������̣߳����⿨��
            if (i % 100 == 0)
            {
                yield return null;
            }
        }

        UnityEngine.Profiling.Profiler.EndSample();
        Debug.Log("��ľLOD��ʼ����ɣ�������: " + treeDict.Count + " ����");
    }

    // ��ʼ��������ľ��LOD
    private void InitializeTreeLOD(GameObject tree)
    {
        LODGroup lodGroup = tree.GetComponent<LODGroup>();

        if (lodGroup == null)
        {
            lodGroup = tree.AddComponent<LODGroup>();
        }

        // ��ȡԭʼ��ģ��Ⱦ��
        Renderer[] originalRenderers = tree.GetComponentsInChildren<Renderer>();

        // ����LOD����
        LOD[] lods = new LOD[2];

        // ���ø�ģLOD (Ĭ��ʹ��ԭʼ��Ⱦ��)
        lods[0] = new LOD(0.1f, originalRenderers);

        // ������ģʵ��
        GameObject lowLODInstance = Instantiate(lowLODPrefab, tree.transform.position,
                                                tree.transform.rotation, tree.transform);
        lowLODInstance.SetActive(false); // ��ʼ�������LODGroup����

        // ���õ�ģLOD
        Renderer[] lowLODRenderers = lowLODInstance.GetComponentsInChildren<Renderer>();
        lods[1] = new LOD(0.02f, lowLODRenderers);

        // Ӧ��LOD����
        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();

        // �洢��ľ��Ϣ
        TreeInfo info = new TreeInfo
        {
            originalTree = tree,
            currentLOD = lowLODInstance,
            isCutDown = false,
            lodGroup = lodGroup
        };

        treeDict[tree] = info;
    }

    // ��������������ľ��LOD��ʾ����
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

    // ������ľ
    public void CutDownTree(GameObject tree)
    {
        if (!treeDict.ContainsKey(tree))
        {
            Debug.LogWarning("���Կ���δע�����ľ: " + tree.name);
            return;
        }

        TreeInfo info = treeDict[tree];

        if (info.isCutDown)
        {
            Debug.Log("��ľ�ѱ�����: " + tree.name);
            return;
        }

        // ����LODGroup���ֶ�������ʾ
        info.lodGroup.enabled = false;

        // ��������LOD
        foreach (Transform child in tree.transform)
        {
            child.gameObject.SetActive(false);
        }

        // ������׮
        GameObject stumpInstance = Instantiate(stumpPrefab, tree.transform.position,
                                              tree.transform.rotation, tree.transform);
        stumpInstance.SetActive(true);

        // ������ľ��Ϣ
        info.isCutDown = true;
        treeDict[tree] = info;

        // ������ľ���������¼�
        OnTreeCutDown?.Invoke(tree);
    }

    // �����滻������ľ�ĵ�ģLOD
    public void ReplaceAllLowLODModels(GameObject newLowLODPrefab)
    {
        lowLODPrefab = newLowLODPrefab;

        foreach (var pair in treeDict)
        {
            GameObject tree = pair.Key;
            TreeInfo info = pair.Value;

            if (!info.isCutDown)
            {
                // ���پɵĵ�ģ
                if (info.currentLOD != null)
                {
                    Destroy(info.currentLOD);
                }

                // �����µĵ�ģʵ��
                GameObject newLowLODInstance = Instantiate(lowLODPrefab, tree.transform.position,
                                                           tree.transform.rotation, tree.transform);
                newLowLODInstance.SetActive(false); // ��LODGroup����

                // ����LOD����
                LODGroup lodGroup = info.lodGroup;
                LOD[] lods = lodGroup.GetLODs();

                if (lods.Length >= 2)
                {
                    Renderer[] newLowLODRenderers = newLowLODInstance.GetComponentsInChildren<Renderer>();
                    lods[1] = new LOD(0.02f, newLowLODRenderers);
                    lodGroup.SetLODs(lods);
                    lodGroup.RecalculateBounds();
                }

                // ������ľ��Ϣ
                info.currentLOD = newLowLODInstance;
                treeDict[tree] = info;
            }
        }

        Debug.Log("������ľ�ĵ�ģLOD���滻");
    }

    // �¼�������ľ������ʱ����
    public System.Action<GameObject> OnTreeCutDown;
}