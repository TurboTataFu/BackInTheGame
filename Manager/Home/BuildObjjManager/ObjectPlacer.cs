using UnityEngine;
using System.Collections.Generic;

public class ObjectPlacer : MonoBehaviour
{
    public GameObject prefab;         // 需要实例化的预制件
    public List<Transform> snapAreas; // 指定的5个吸附区域
    public bool enableX = true;       // 轴控制开关
    public bool enableY = true;
    public bool enableZ = true;

    private GameObject currentObject;
    private bool isPlacing = false;
    private Vector3 originalPosition;
    private Material originalMaterial;
    public Material highlightMaterial;

    void Update()
    {
        if (isPlacing)
        {
            // 使用射线获取鼠标在场景中的位置
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 获取最近的吸附点位置
                Vector3 targetPos = GetNearestSnapPosition(hit.point);

                // 应用轴锁定
                if (!enableX) targetPos.x = originalPosition.x;
                if (!enableY) targetPos.y = originalPosition.y;
                if (!enableZ) targetPos.z = originalPosition.z;

                currentObject.transform.position = targetPos;
            }

            // 左键单击确认放置
            if (Input.GetMouseButtonDown(0))
            {
                SetHighlight(currentObject, false);
                isPlacing = false;
            }
        }
    }

    // 按钮点击事件调用的方法
    public void StartPlacing()
    {
        if (currentObject != null) Destroy(currentObject);

        currentObject = Instantiate(prefab);
        originalPosition = currentObject.transform.position;
        SetHighlight(currentObject, true);
        isPlacing = true;
    }

    Vector3 GetNearestSnapPosition(Vector3 inputPos)
    {
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        // 遍历所有吸附区域寻找最近的点
        foreach (Transform area in snapAreas)
        {
            float distance = Vector3.Distance(inputPos, area.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = area;
            }
        }
        return closest?.position ?? inputPos;
    }

    void SetHighlight(GameObject target, bool enable)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer)
        {
            if (enable)
            {
                originalMaterial = renderer.material;
                renderer.material = highlightMaterial;
            }
            else
            {
                renderer.material = originalMaterial;
            }
        }
    }
}