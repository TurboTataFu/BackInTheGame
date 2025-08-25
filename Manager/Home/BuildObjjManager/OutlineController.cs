using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class OutlineController : MonoBehaviour
{
    [Header("��������")]
    public float outlineMultiplier = 2f;
    public Color highlightColor = Color.white;
    public string outlineWidthProperty = "_OutlineWidth";
    public string outlineColorProperty = "_OutlineColor";

    [Header("�������")]
    public GameObject targetObject;

    // �洢ԭʼֵ�����ݽṹ
    private class MaterialData
    {
        public Material material;
        public float originalWidth;
        public Color originalColor;
    }

    private List<MaterialData> materialsData = new List<MaterialData>();
    private bool isHighlighted = false;

    private void Start()
    {
        // ��ʼ��ʱ�ռ����в�������
        CollectMaterials(transform);
    }

    // �ռ����в������ݣ����������壩
    private void CollectMaterials(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                foreach (Material mat in renderer.materials)
                {
                    // ������֧�������Ĳ���
                    if (!mat.HasProperty(outlineWidthProperty) ||
                        !mat.HasProperty(outlineColorProperty))
                    {
                        Debug.LogWarning($"���� {mat.name} ȱ���������ԣ�������");
                        continue;
                    }

                    materialsData.Add(new MaterialData
                    {
                        material = mat,
                        originalWidth = mat.GetFloat(outlineWidthProperty),
                        originalColor = mat.GetColor(outlineColorProperty)
                    });
                }
            }
            CollectMaterials(child); // �ݹ�������
        }
    }

    private void OnMouseEnter()
    {
        if (!isHighlighted)
        {
            ModifyOutline(outlineMultiplier, highlightColor);
            isHighlighted = true;
        }
    }

    private void OnMouseExit()
    {
        if (isHighlighted)
        {
            ResetOutline();
            isHighlighted = false;
        }
    }

    private void OnMouseDown()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);
        }
    }

    // �޸���������
    private void ModifyOutline(float multiplier, Color color)
    {
        foreach (MaterialData data in materialsData)
        {
            if (data.material != null)
            {
                data.material.SetFloat(outlineWidthProperty,
                    data.originalWidth * multiplier);
                data.material.SetColor(outlineColorProperty, color);
            }
        }
    }

    // �ָ�ԭʼ����
    private void ResetOutline()
    {
        foreach (MaterialData data in materialsData)
        {
            if (data.material != null)
            {
                data.material.SetFloat(outlineWidthProperty, data.originalWidth);
                data.material.SetColor(outlineColorProperty, data.originalColor);
            }
        }
    }

    private void OnDestroy()
    {
        // ȷ������ʵ��������
        foreach (MaterialData data in materialsData)
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(data.material);
            }
            else
            {
                Destroy(data.material);
            }
        }
    }
}