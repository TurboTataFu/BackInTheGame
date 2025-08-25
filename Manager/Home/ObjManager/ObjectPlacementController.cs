using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacementController : MonoBehaviour
{

    [Header("����ģʽ����")]
    [SerializeField] private bool isConstructionMode = false;
    [SerializeField] private GameObject constructionHUD;

    // �����ֶ�
    [Header("��ʱ��������")]
    [SerializeField] private GameObject placementPrefab; // Ҫ���õ�Ԥ����
    [SerializeField] private float moveSpeed = 5f;        // �ƶ������ٶ�
    private GameObject currentObject;                     // ��ǰ��������
    private bool isPlacing;                                // �Ƿ��ڷ���״̬

    #region Region Settings
    [Header("��������")]
    [Tooltip("�����������")]
    [SerializeField] private Vector3 placementStart = Vector3.zero;

    [Tooltip("���������յ�")]
    [SerializeField] private Vector3 placementEnd = Vector3.forward * 5f;

    [Tooltip("����������뾶")]
    [SerializeField] private float placementRadius = 0.5f;
    #endregion

    #region Axis Constraints
    [Header("��Լ��")]
    [Tooltip("����X���ƶ�")]
    [SerializeField] private bool lockXAxis = false;

    [Tooltip("����Y���ƶ�")]
    [SerializeField] private bool lockYAxis = true;

    [Tooltip("����Z���ƶ�")]
    [SerializeField] private bool lockZAxis = false;
    #endregion

    #region Placement Settings
    [Header("��������")]
    [Tooltip("����ƫ����")]
    [SerializeField] private Vector3 placementOffset = Vector3.up * 0.1f;

    [Tooltip("��������ʱʹ�õĲ㼶")]
    [SerializeField] private LayerMask placementLayer;
    #endregion

    #region Shader & Color Settings
    [Header("��ɫ������")]
    [SerializeField] private Shader targetShader;
    [SerializeField] private Color highlightColor = Color.white;
    #endregion

    #region Runtime Variables
    private Dictionary<Material, Color> originalColors = new Dictionary<Material, Color>();
    private bool isDragging;
    private GameObject draggedObject;
    private Vector3 dragStartPosition;
    #endregion

    #region Unity Callbacks
    // �޸�Update����
    private void Update()
    {
        if (isPlacing)
        {
            UpdateObjectPosition();
            HandlePlacementInput();
        }
    }

    private void OnDrawGizmos()
    {
        // ���Ʒ���������ӻ�
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(placementStart, placementEnd);
        Gizmos.DrawWireSphere(placementStart, placementRadius);
        Gizmos.DrawWireSphere(placementEnd, placementRadius);
    }
    #endregion

    // �޸Ľ���ģʽ�л�����
    public void ToggleConstructionMode()
    {
        isConstructionMode = !isConstructionMode;
        constructionHUD.SetActive(isConstructionMode);

        if (isConstructionMode)
        {
            StartPlacement();
        }
        else
        {
            ConfirmPlacement();
        }
    }

    // �������ó�ʼ������
    private void StartPlacement()
    {
        // ʵ����������
        currentObject = Instantiate(placementPrefab);
        isPlacing = true;
        UpdateObjectPosition(); // ��������λ��
    }

    #region Input Handling
    // �޸����봦��
    private void HandlePlacementInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ConfirmPlacement();
        }
    }


    // ����ʵʱλ�ø��·���
    private void UpdateObjectPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane movementPlane = new Plane(CalculateLockedNormal(), placementStart);

        if (movementPlane.Raycast(ray, out float enter))
        {
            Vector3 targetPosition = ray.GetPoint(enter);
            Vector3 clampedPosition = ClampToLine(targetPosition);
            ApplyAxisConstraints(ref clampedPosition);

            // ƽ���ƶ�
            currentObject.transform.position = Vector3.Lerp(
                currentObject.transform.position,
                clampedPosition + placementOffset,
                Time.deltaTime * moveSpeed
            );
        }
    }

    private void TryStartDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayer))
        {
            if (IsInPlacementArea(hit.point))
            {
                StartDrag(hit.collider.gameObject);
            }
        }
    }

    private void StartDrag(GameObject obj)
    {
        isDragging = true;
        draggedObject = obj;
        dragStartPosition = obj.transform.position;
        ModifyOutlineColor(highlightColor);
    }

    private void EndDrag()
    {
        isDragging = false;
        draggedObject = null;
        RestoreOriginalColors();
    }
    #endregion

    #region Placement Logic
    private void UpdateDragPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane placementPlane = new Plane(CalculateLockedNormal(), dragStartPosition);

        if (placementPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPosition = ray.GetPoint(enter);
            Vector3 clampedPosition = ClampToLine(worldPosition);
            ApplyAxisConstraints(ref clampedPosition);
            draggedObject.transform.position = clampedPosition + placementOffset;
        }
    }

    private Vector3 CalculateLockedNormal()
    {
        if (lockXAxis && !lockZAxis) return Vector3.right;
        if (lockZAxis && !lockXAxis) return Vector3.forward;
        return Vector3.up;
    }

    private Vector3 ClampToLine(Vector3 position)
    {
        Vector3 lineDirection = placementEnd - placementStart;
        float lineLength = lineDirection.magnitude;
        Vector3 normalizedDirection = lineDirection.normalized;

        float projection = Vector3.Dot(position - placementStart, normalizedDirection);
        projection = Mathf.Clamp(projection, 0f, lineLength);

        return placementStart + normalizedDirection * projection;
    }

    private void ApplyAxisConstraints(ref Vector3 position)
    {
        if (lockXAxis) position.x = dragStartPosition.x;
        if (lockYAxis) position.y = dragStartPosition.y;
        if (lockZAxis) position.z = dragStartPosition.z;
    }

    private bool IsInPlacementArea(Vector3 position)
    {
        float distanceToLine = DistanceToLineSegment(position);
        return distanceToLine <= placementRadius;
    }

    private float DistanceToLineSegment(Vector3 point)
    {
        Vector3 a = placementStart;
        Vector3 b = placementEnd;
        Vector3 ab = b - a;
        Vector3 ap = point - a;

        float projection = Vector3.Dot(ap, ab);
        float lengthSqr = ab.sqrMagnitude;

        if (projection <= 0) return Vector3.Distance(point, a);
        if (projection >= lengthSqr) return Vector3.Distance(point, b);

        Vector3 closestPoint = a + ab * (projection / lengthSqr);
        return Vector3.Distance(point, closestPoint);
    }
    #endregion

    // �޸�ȷ�Ϸ��÷���
    public void ConfirmPlacement()
    {
        if (!isPlacing) return;

        // ��������λ��
        Vector3 finalPosition = currentObject.transform.position;
        currentObject.transform.position = ClampToLine(finalPosition);
        ApplyAxisConstraints(ref finalPosition);

        currentObject = null;
        isPlacing = false;
        Debug.Log("�����ѹ̶�");
    }

    #region Visual Feedback
    private void UpdateHighlight()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayer))
        {
            bool inArea = IsInPlacementArea(hit.point);
            if (inArea && !isDragging)
            {
                ModifyOutlineColor(highlightColor);
            }
            else
            {
                RestoreOriginalColors();
            }
        }
        else
        {
            RestoreOriginalColors();
        }
    }

    #region Visual Feedback
    private void ModifyOutlineColor(Color newColor)
    {
        // �޸ĺ�Ĳ��ҷ�������205�и�����
        foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (!IsInPlacementArea(renderer.transform.position)) continue;

            foreach (Material mat in renderer.materials)
            {
                if (mat.shader == targetShader)
                {
                    if (!originalColors.ContainsKey(mat))
                    {
                        originalColors.Add(mat, mat.GetColor("_OutlineColor"));
                    }
                    mat.SetColor("_OutlineColor", newColor);
                }
            }
        }
    }
    #endregion

    private void RestoreOriginalColors()
    {
        foreach (var kvp in originalColors)
        {
            if (kvp.Key != null)
            {
                kvp.Key.SetColor("_OutlineColor", kvp.Value);
            }
        }
        originalColors.Clear();
    }
    #endregion

    #region Public Methods
    public void ConfigurePlacementLine(Vector3 start, Vector3 end)
    {
        placementStart = start;
        placementEnd = end;
    }

    public void SetAxisConstraints(bool x, bool y, bool z)
    {
        lockXAxis = x;
        lockYAxis = y;
        lockZAxis = z;
    }
    #endregion
}