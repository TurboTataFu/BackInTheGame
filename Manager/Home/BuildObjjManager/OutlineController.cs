using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class OutlineController : MonoBehaviour
{
    [Header("轮廓设置")]
    public float outlineMultiplier = 2f;
    public Color highlightColor = Color.white;
    public string outlineWidthProperty = "_OutlineWidth";
    public string outlineColorProperty = "_OutlineColor";

    [Header("点击设置")]
    public GameObject targetObject;

    // 存储原始值的数据结构
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
        // 初始化时收集所有材质数据
        CollectMaterials(transform);
    }

    // 收集所有材质数据（包括子物体）
    private void CollectMaterials(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                foreach (Material mat in renderer.materials)
                {
                    // 跳过不支持轮廓的材质
                    if (!mat.HasProperty(outlineWidthProperty) ||
                        !mat.HasProperty(outlineColorProperty))
                    {
                        Debug.LogWarning($"材质 {mat.name} 缺少轮廓属性，已跳过");
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
            CollectMaterials(child); // 递归子物体
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

    // 修改轮廓参数
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

    // 恢复原始参数
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
        // 确保材质实例被销毁
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