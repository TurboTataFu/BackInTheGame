using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AdvancedCameraController : MonoBehaviour
{
    [Header("�ƶ�����")]
    [Tooltip("������ƶ��ٶ�")]
    public float moveSpeed = 10f;
    [Tooltip("������ƶ���Χ���� - ��Сֵ")]
    public Vector3 minMoveBounds = new Vector3(-50, 5, -50);
    [Tooltip("������ƶ���Χ���� - ���ֵ")]
    public Vector3 maxMoveBounds = new Vector3(50, 50, 50);
    [Tooltip("�Ƿ�תˮƽ�����ң��ƶ�����")]
    public bool invertXMovement = false;
    [Tooltip("�Ƿ�ת��ֱ�����£��ƶ�����")]
    public bool invertYMovement = false;

    [Header("��ת����")]
    [Tooltip("������ת�Ƕ�")]
    public Vector3 baseRotation = new Vector3(30, 0, 0);
    [Tooltip("��ת������")]
    public float rotationSensitivity = 0.5f;
    [Tooltip("��ת����ϵ��")]
    public float rotationDamping = 12f;
    [Tooltip("���X����ת����")]
    public float maxXRotation = 60f;
    [Tooltip("��СX����ת����")]
    public float minXRotation = 10f;
    [Tooltip("���Y����ת����")]
    public float maxYRotation = 45f;
    [Tooltip("��СY����ת����")]
    public float minYRotation = -45f;
    [Tooltip("�ж�Ϊ������ʱ����ֵ���룩")]
    public float longPressThreshold = 0.3f;
    [Tooltip("�Ƿ�ת��ֱ�����£���ת����")]
    public bool invertYRotation = false;
    [Tooltip("�Ƿ�תˮƽ�����ң���ת����")]
    public bool invertXRotation = false;

    [Header("FOV����")]
    [Tooltip("Ĭ��FOVֵ")]
    public float defaultFOV = 60f;
    [Tooltip("FOV�����ٶ�")]
    public float fovChangeSpeed = 2f;
    [Tooltip("��СFOV����")]
    public float minFOV = 30f;
    [Tooltip("���FOV����")]
    public float maxFOV = 90f;

    private Camera mainCamera;
    private Quaternion targetRotation;
    private bool isRotating = false;
    private bool isLongPress = false;
    private float middleClickPressTime;

    void Start()
    {
        // ��ȡ��������
        mainCamera = GetComponent<Camera>();

        // ��ʼ�������״̬
        transform.rotation = Quaternion.Euler(baseRotation);
        targetRotation = transform.rotation;
        mainCamera.fieldOfView = defaultFOV;
    }

    void Update()
    {
        // �����ƶ����� - ����Ҽ��϶����������ң�
        if (Input.GetMouseButton(1))
        {
            HandleMovement();
        }

        // ��������м����루������ת���̰�����FOV��
        HandleMiddleClickInput();

        // ���������ת״̬��Ӧ����ת
        if (isRotating)
        {
            HandleRotation();
        }
        // �����ת������ƽ������
        else if (!Input.GetMouseButton(2) && targetRotation != Quaternion.Euler(baseRotation))
        {
            targetRotation = Quaternion.Lerp(targetRotation, Quaternion.Euler(baseRotation), rotationDamping * Time.deltaTime);
            transform.rotation = targetRotation;
        }

        // ����FOV���� - ������
        if (Input.mouseScrollDelta.y != 0)
        {
            HandleFOVChange(Input.mouseScrollDelta.y);
        }
    }

    /// <summary>
    /// ��������м��ĳ����Ͷ̰��߼�
    /// </summary>
    private void HandleMiddleClickInput()
    {
        // �м�����
        if (Input.GetMouseButtonDown(2))
        {
            middleClickPressTime = Time.time;
            isLongPress = false;
            isRotating = false;
        }
        // �м���ס
        else if (Input.GetMouseButton(2))
        {
            // ����Ƿ�ﵽ������ֵ
            if (!isLongPress && Time.time - middleClickPressTime > longPressThreshold)
            {
                isLongPress = true;
                isRotating = true;
            }
        }
        // �м��ɿ�
        else if (Input.GetMouseButtonUp(2))
        {
            // ����Ƕ̰���δ�ﵽ������ֵ����������FOV
            if (!isLongPress)
            {
                ResetFOV();
            }

            isLongPress = false;
            isRotating = false;
        }
    }

    /// <summary>
    /// ����������ƶ����������ң���֧�ַ���ת
    /// </summary>
    private void HandleMovement()
    {
        // ��ȡ������벢Ӧ���ƶ���ת����
        float horizontal = Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
        float vertical = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;

        // Ӧ���ƶ�����ת
        if (invertXMovement) horizontal = -horizontal;
        if (invertYMovement) vertical = -vertical;

        // ������λ�ã�����ΪX�ᣬ����ΪY�ᣩ
        Vector3 newPosition = transform.position +
                              (Vector3.right * horizontal) -
                              (Vector3.up * vertical);

        // Ӧ�ñ߽�����
        newPosition.x = Mathf.Clamp(newPosition.x, minMoveBounds.x, maxMoveBounds.x);
        newPosition.y = Mathf.Clamp(newPosition.y, minMoveBounds.y, maxMoveBounds.y);
        newPosition.z = Mathf.Clamp(newPosition.z, minMoveBounds.z, maxMoveBounds.z);

        transform.position = newPosition;
    }

    /// <summary>
    /// �����������ת���м������϶�����֧�ַ���ת
    /// </summary>
    private void HandleRotation()
    {
        // ��ȡ������벢Ӧ����ת��ת����
        float xRot = Input.GetAxis("Mouse Y") * rotationSensitivity;
        float yRot = Input.GetAxis("Mouse X") * rotationSensitivity;

        // Ӧ����ת����ת
        if (invertYRotation) xRot = -xRot;
        if (invertXRotation) yRot = -yRot;

        // ���ڵ�ǰĿ����ת��������ת
        Quaternion xRotation = Quaternion.AngleAxis(-xRot, Vector3.right);
        Quaternion yRotation = Quaternion.AngleAxis(yRot, Vector3.up);

        targetRotation = targetRotation * xRotation * yRotation;

        // ����ת������ָ����Χ��
        Vector3 euler = targetRotation.eulerAngles;
        euler.x = ClampAngle(euler.x, minXRotation, maxXRotation);
        euler.y = ClampAngle(euler.y, minYRotation, maxYRotation);
        euler.z = 0; // ����Z����ת
        targetRotation = Quaternion.Euler(euler);

        // Ӧ������Ч��
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationDamping * Time.deltaTime);
    }

    /// <summary>
    /// �Ƕ����Ƹ�������������0-360��ѭ�����⣩
    /// </summary>
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < 90 || angle > 270)
        {
            if (angle > 180) angle -= 360;
            if (angle < -180) angle += 360;
        }
        return Mathf.Clamp(angle, min, max);
    }

    /// <summary>
    /// ����FOV�仯�������֣�
    /// </summary>
    private void HandleFOVChange(float scrollValue)
    {
        float newFOV = mainCamera.fieldOfView - scrollValue * fovChangeSpeed;
        mainCamera.fieldOfView = Mathf.Clamp(newFOV, minFOV, maxFOV);
    }

    /// <summary>
    /// ����FOV��Ĭ��ֵ���м��̰���
    /// </summary>
    private void ResetFOV()
    {
        mainCamera.fieldOfView = defaultFOV;
    }

    // ����Gizmos����ʾ�ƶ���Χ
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = (minMoveBounds + maxMoveBounds) / 2;
        Vector3 size = maxMoveBounds - minMoveBounds;
        Gizmos.DrawWireCube(center, size);
    }
}
