using UnityEngine;
using System.Collections.Generic;

public class ObjectPlacer : MonoBehaviour
{
    public GameObject prefab;         // ��Ҫʵ������Ԥ�Ƽ�
    public List<Transform> snapAreas; // ָ����5����������
    public bool enableX = true;       // ����ƿ���
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
            // ʹ�����߻�ȡ����ڳ����е�λ��
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // ��ȡ�����������λ��
                Vector3 targetPos = GetNearestSnapPosition(hit.point);

                // Ӧ��������
                if (!enableX) targetPos.x = originalPosition.x;
                if (!enableY) targetPos.y = originalPosition.y;
                if (!enableZ) targetPos.z = originalPosition.z;

                currentObject.transform.position = targetPos;
            }

            // �������ȷ�Ϸ���
            if (Input.GetMouseButtonDown(0))
            {
                SetHighlight(currentObject, false);
                isPlacing = false;
            }
        }
    }

    // ��ť����¼����õķ���
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

        // ����������������Ѱ������ĵ�
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