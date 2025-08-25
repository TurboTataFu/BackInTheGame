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

    // ����ѡ��
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
        GUILayout.Label("����������ѡ����", EditorStyles.boldLabel);

        // �ο�����
        referenceObject = (GameObject)EditorGUILayout.ObjectField(
            "�ο�����", referenceObject, typeof(GameObject), true);

        // ��������ֵ
        similarityThreshold = EditorGUILayout.Slider(
            "��������ֵ", similarityThreshold, 0f, 1f);

        // �Ƿ�����Ǽ������
        includeInactive = EditorGUILayout.Toggle("�����Ǽ������", includeInactive);

        // ��ť
        if (GUILayout.Button("���������������"))
        {
            FindSimilarObjects();
        }

        if (selectedObjects.Count > 0 && GUILayout.Button("ѡ���������ƶ���"))
        {
            SelectObjects();
        }

        // �ָ���
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("��������", EditorStyles.boldLabel);

        // ����ѡ��
        removeMesh = EditorGUILayout.Toggle("ɾ���������", removeMesh);
        removeCollider = EditorGUILayout.Toggle("ɾ����ײ�����", removeCollider);

        EditorGUILayout.BeginHorizontal();
        setActive = EditorGUILayout.Toggle("���ü���״̬", setActive);
        toggleActiveState = EditorGUILayout.Toggle("�л�����״̬", toggleActiveState);
        EditorGUILayout.EndHorizontal();

        tagToAdd = EditorGUILayout.TagField("��ӱ�ǩ", tagToAdd);

        EditorGUILayout.LabelField("������ѡ��", EditorStyles.miniBoldLabel);
        renamePrefix = EditorGUILayout.TextField("ǰ׺", renamePrefix);
        renameSuffix = EditorGUILayout.TextField("��׺", renameSuffix);
        useNumbering = EditorGUILayout.Toggle("ʹ�ñ��", useNumbering);
        if (useNumbering)
            startNumber = EditorGUILayout.IntField("��ʼ���", startNumber);

        // ִ�в�����ť
        if (selectedObjects.Count > 0 && GUILayout.Button("ִ����������"))
        {
            PerformBatchOperations();
        }

        // �����ʾ
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"�ҵ� {selectedObjects.Count} �����ƶ���",
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
            EditorUtility.DisplayDialog("����", "��ѡ��ο�����", "ȷ��");
            return;
        }

        selectedObjects.Clear();

        // ��ȡ�ο����������
        Mesh referenceMesh = GetMeshFromObject(referenceObject);
        if (referenceMesh == null)
        {
            EditorUtility.DisplayDialog("����", "�ο�����û����Ч���������", "ȷ��");
            return;
        }

        // ��ȡ���������ж���
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // ���㲢������������
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

        Debug.Log($"�ҵ� {selectedObjects.Count} ���� {referenceObject.name} ���Ƶ��������");
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
        // �Ƚ϶�������
        if (mesh1.vertexCount != mesh2.vertexCount)
            return false;

        // �Ƚ�����������
        if (mesh1.triangles.Length != mesh2.triangles.Length)
            return false;

        // �򵥱Ƚϣ�����������������������ͬ����Ϊ����
        // �����ӵıȽϿ��Լ��㶥��λ�ò��졢UV�����
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

        Undo.RecordObjects(selectedObjects.ToArray(), "�������������������");

        int number = startNumber;

        foreach (GameObject obj in selectedObjects)
        {
            // ɾ���������
            if (removeMesh)
            {
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter != null)
                    DestroyImmediate(meshFilter);

                SkinnedMeshRenderer skinnedMesh = obj.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMesh != null)
                    DestroyImmediate(skinnedMesh);
            }

            // ɾ����ײ�����
            if (removeCollider)
            {
                Collider[] colliders = obj.GetComponents<Collider>();
                foreach (Collider collider in colliders)
                {
                    DestroyImmediate(collider);
                }
            }

            // ���ü���״̬
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

            // ��ӱ�ǩ
            if (tagToAdd != "Untagged")
            {
                obj.tag = tagToAdd;
            }

            // ������
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
        Debug.Log($"�Ѷ� {selectedObjects.Count} ������ִ����������");
    }

    // �ڱ༭��ģʽ�»�ȡ���ж���ĸ�������
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