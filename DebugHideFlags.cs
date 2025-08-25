using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class MeshSimilaritySelector : EditorWindow
{
    private GameObject referenceObject;
    private float similarityThreshold = 0.9f;
    private bool includeInactive = false;
    private List<GameObject> selectedObjects = new List<GameObject>();

    // 操作选项
    private bool removeMesh = false;
    private bool removeCollider = false;
    private bool setActive = false;
    private bool toggleActiveState = false;
    private string tagToAdd = "Untagged";
    private string renamePrefix = "";
    private string renameSuffix = "";
    private bool useNumbering = false;
    private int startNumber = 1;

    [MenuItem("Tools/Mesh Similarity Selector")]
    public static void ShowWindow()
    {
        GetWindow<MeshSimilaritySelector>("Mesh Similarity Selector");
    }

    private void OnGUI()
    {
        GUILayout.Label("网格相似性选择器", EditorStyles.boldLabel);

        // 参考对象
        referenceObject = (GameObject)EditorGUILayout.ObjectField(
            "参考对象", referenceObject, typeof(GameObject), true);

        // 相似性阈值
        similarityThreshold = EditorGUILayout.Slider(
            "相似性阈值", similarityThreshold, 0f, 1f);

        // 是否包含非激活对象
        includeInactive = EditorGUILayout.Toggle("包含非激活对象", includeInactive);

        // 按钮
        if (GUILayout.Button("查找相似网格对象"))
        {
            FindSimilarObjects();
        }

        if (selectedObjects.Count > 0 && GUILayout.Button("选择所有相似对象"))
        {
            SelectObjects();
        }

        // 分隔线
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("批量操作", EditorStyles.boldLabel);

        // 操作选项
        removeMesh = EditorGUILayout.Toggle("删除网格组件", removeMesh);
        removeCollider = EditorGUILayout.Toggle("删除碰撞体组件", removeCollider);

        EditorGUILayout.BeginHorizontal();
        setActive = EditorGUILayout.Toggle("设置激活状态", setActive);
        toggleActiveState = EditorGUILayout.Toggle("切换激活状态", toggleActiveState);
        EditorGUILayout.EndHorizontal();

        tagToAdd = EditorGUILayout.TagField("添加标签", tagToAdd);

        EditorGUILayout.LabelField("重命名选项", EditorStyles.miniBoldLabel);
        renamePrefix = EditorGUILayout.TextField("前缀", renamePrefix);
        renameSuffix = EditorGUILayout.TextField("后缀", renameSuffix);
        useNumbering = EditorGUILayout.Toggle("使用编号", useNumbering);
        if (useNumbering)
            startNumber = EditorGUILayout.IntField("起始编号", startNumber);

        // 执行操作按钮
        if (selectedObjects.Count > 0 && GUILayout.Button("执行批量操作"))
        {
            PerformBatchOperations();
        }

        // 结果显示
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"找到 {selectedObjects.Count} 个相似对象",
            EditorStyles.boldLabel);

        if (selectedObjects.Count > 0)
        {
            EditorGUILayout.BeginScrollView(
                new Vector2(0, 0),
                GUILayout.MaxHeight(200));

            foreach (var obj in selectedObjects)
            {
                EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void FindSimilarObjects()
    {
        if (referenceObject == null)
        {
            EditorUtility.DisplayDialog("错误", "请选择参考对象", "确定");
            return;
        }

        selectedObjects.Clear();

        // 获取参考对象的网格
        Mesh referenceMesh = GetMeshFromObject(referenceObject);
        if (referenceMesh == null)
        {
            EditorUtility.DisplayDialog("错误", "参考对象没有有效的网格组件", "确定");
            return;
        }

        // 获取场景中所有对象
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // 计算并查找相似网格
        foreach (GameObject obj in allObjects)
        {
            if (!includeInactive && !obj.activeInHierarchy)
                continue;

            Mesh currentMesh = GetMeshFromObject(obj);
            if (currentMesh == null)
                continue;

            if (AreMeshesSimilar(referenceMesh, currentMesh, similarityThreshold))
            {
                selectedObjects.Add(obj);
            }
        }

        Debug.Log($"找到 {selectedObjects.Count} 个与 {referenceObject.name} 相似的网格对象");
    }

    private Mesh GetMeshFromObject(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null)
            return meshFilter.sharedMesh;

        SkinnedMeshRenderer skinnedMesh = obj.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMesh != null)
            return skinnedMesh.sharedMesh;

        return null;
    }

    private bool AreMeshesSimilar(Mesh mesh1, Mesh mesh2, float threshold)
    {
        // 比较顶点数量
        if (mesh1.vertexCount != mesh2.vertexCount)
            return false;

        // 比较三角形数量
        if (mesh1.triangles.Length != mesh2.triangles.Length)
            return false;

        // 简单比较：顶点数量和三角形数量相同，认为相似
        // 更复杂的比较可以计算顶点位置差异、UV坐标等
        return true;
    }

    private void SelectObjects()
    {
        Selection.objects = selectedObjects.ToArray();
    }

    private void PerformBatchOperations()
    {
        if (selectedObjects.Count == 0)
            return;

        Undo.RecordObjects(selectedObjects.ToArray(), "批量操作相似网格对象");

        int number = startNumber;

        foreach (GameObject obj in selectedObjects)
        {
            // 删除网格组件
            if (removeMesh)
            {
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter != null)
                    DestroyImmediate(meshFilter);

                SkinnedMeshRenderer skinnedMesh = obj.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMesh != null)
                    DestroyImmediate(skinnedMesh);
            }

            // 删除碰撞体组件
            if (removeCollider)
            {
                Collider[] colliders = obj.GetComponents<Collider>();
                foreach (Collider collider in colliders)
                {
                    DestroyImmediate(collider);
                }
            }

            // 设置激活状态
            if (toggleActiveState)
            {
                obj.SetActive(!obj.activeSelf);
            }
            else if (setActive)
            {
                obj.SetActive(true);
            }
            else if (setActive == false)
            {
                obj.SetActive(false);
            }

            // 添加标签
            if (tagToAdd != "Untagged")
            {
                obj.tag = tagToAdd;
            }

            // 重命名
            if (!string.IsNullOrEmpty(renamePrefix) || !string.IsNullOrEmpty(renameSuffix))
            {
                string newName = obj.name;

                if (!string.IsNullOrEmpty(renamePrefix))
                    newName = renamePrefix + newName;

                if (!string.IsNullOrEmpty(renameSuffix))
                    newName = newName + renameSuffix;

                if (useNumbering)
                    newName += number++;

                obj.name = newName;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"已对 {selectedObjects.Count} 个对象执行批量操作");
    }

    // 在编辑器模式下获取所有对象的辅助方法
    private new GameObject[] FindObjectsOfType<T>() where T : Object
    {
        if (includeInactive)
        {
            return Resources.FindObjectsOfTypeAll<T>()
                .Where(obj => !EditorUtility.IsPersistent(obj) &&
                             obj.hideFlags == HideFlags.None)
                .Cast<GameObject>()
                .ToArray();
        }
        else
        {
            return FindObjectsOfType<GameObject>();
        }
    }
}