using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CombatCommanderController : MonoBehaviour
{
    // ����UI��ر���
    [Header("AD���ƽ��UI")]
    public GameObject overrideUI;          // ������ť���ı��ĸ�����
    public float blinkSpeed = 2f;          // ��˸�ٶ�
    public Button overrideButton;          // �����ť
    public Text[] warningTexts;            // �����ı�����
    private CanvasGroup _uiCanvasGroup;     // UI͸���ȿ���
    private bool _isOverThreshold;         // �Ƿ񳬹���ֵ
    private bool _overrideActive;          // �Ƿ��ѽ������


    // ϵͳ��
    public AC130Gunship gunship;
    public Volume nightVisionVolume;
    public Transform targetMarker;

    // ָ�ӹٿ��Ʋ���
    public float panSpeed = 5f;
    public float rotationThreshold = 30f;
    public Vector2 zoomLimits = new Vector2(20, 100);
    public float zoomSensitivity = 10f;
    public VolumeProfile[] visionProfiles;

    // ����ʱ״̬
    private Vector3 _currentTarget;
    private bool _isLocked;
    private float _currentZoom;
    private int _currentProfileIndex;

    [Header("AD����")]
    public float rotationResetTime = 5f;   // �Զ���λʱ��

    // �������Ʋ���
    [Header("�ƶ���Χ����")]
    public Vector3 minBounds = new Vector3(-50, 20, -50);
    public Vector3 maxBounds = new Vector3(50, 100, 50);
    public bool showDebugBounds = true;

    [Header("��������������")]
    public bool useParentSpace = true;
    public Vector3 parentLocalBoundsMin = new Vector3(-50, 20, -50);
    public Vector3 parentLocalBoundsMax = new Vector3(50, 100, 50);

    [Header("���ģʽ")]
    public Transform boundsCenter;
    public Vector3 relativeBounds = new Vector3(50, 50, 50);

    [Header("��̬�߽�����")]
    public bool dynamicBounds = true;
    public Vector3 dynamicBoundsSize = new Vector3(100, 50, 100);

    [Header("����Ŀ������")]
    public Transform followTarget;
    public bool useFollowTarget = false;

    public enum BoundsMode { Absolute, Relative }
    [Header("�ƶ���Χ����")]
    public BoundsMode boundsMode = BoundsMode.Absolute;

    private float _resetTimer;             // ��λ��ʱ��

    void Start()
    {

        // ��ʼ��UI
        if (overrideUI != null)
        {
            overrideUI.SetActive(false);
            _uiCanvasGroup = overrideUI.GetComponent<CanvasGroup>() ?? overrideUI.AddComponent<CanvasGroup>();
        }

        _currentZoom = GetComponent<Camera>().fieldOfView;
        Cursor.visible = true;
        targetMarker.gameObject.SetActive(false);

        if (visionProfiles.Length > 0 && nightVisionVolume != null)
            nightVisionVolume.profile = visionProfiles[0];
    }

    void LateUpdate()
    {
        UpdateBounds();
    }

    void UpdateBounds()
    {
        if (useParentSpace && transform.parent)
        {
            Vector3 parentPos = transform.parent.position;
            minBounds = parentPos + parentLocalBoundsMin;
            maxBounds = parentPos + parentLocalBoundsMax;
        }
        else if (dynamicBounds)
        {
            Vector3 centerPos = useFollowTarget && followTarget != null ?
                followTarget.position : transform.position;

            minBounds = centerPos - dynamicBoundsSize;
            maxBounds = centerPos + dynamicBoundsSize;
        }
        else if (boundsMode == BoundsMode.Relative && boundsCenter != null)
        {
            Vector3 centerPos = useFollowTarget && followTarget != null ?
                followTarget.position : boundsCenter.position;

            minBounds = centerPos - relativeBounds;
            maxBounds = centerPos + relativeBounds;
        }
    }

    void Update()
    {
        HandleThresholdUI();
        HandleCombatCommands();
        UpdateCameraControl();
    }

    void HandleThresholdUI()
    {
        if (_overrideActive || gunship == null) return;

        // ����Ƿ񳬹���ֵ
        bool isOver = Mathf.Abs(gunship.CurrentBankAngle) >= rotationThreshold;

        if (isOver && !_isOverThreshold)
        {
            // �״γ�����ֵ
            overrideUI.SetActive(true);
            _isOverThreshold = true;
            _resetTimer = rotationResetTime;
        }
        else if (!isOver && _isOverThreshold)
        {
            // �ص���ȫ��Χ
            overrideUI.SetActive(false);
            _isOverThreshold = false;
        }

        // ����UI��˸
        if (_isOverThreshold)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            _uiCanvasGroup.alpha = alpha;

            // ����ʱ��λ
            if ((_resetTimer -= Time.deltaTime) <= 0)
            {
                overrideUI.SetActive(false);
                _isOverThreshold = false;
                gunship.CurrentBankAngle = Mathf.MoveTowards(gunship.CurrentBankAngle, 0, 10f);
            }
        }
    }

    // ��ť����¼�
    public void OnOverrideButtonClick()
    {
        _overrideActive = true;
        overrideUI.SetActive(false);

        // ���÷�����̬
        StartCoroutine(ResetBankAngle());
    }

    System.Collections.IEnumerator ResetBankAngle()
    {
        float startAngle = gunship.CurrentBankAngle;
        float duration = 0.5f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            gunship.CurrentBankAngle = Mathf.Lerp(startAngle, 0, t / duration);
            yield return null;
        }
        gunship.CurrentBankAngle = 0;
    }

    void HandleCombatCommands()
    {
        // �Ҽ��϶������������XZƽ���ƶ���
        if (Input.GetMouseButton(1))
        {
            // ��ȡ�����ʵ���ҷ���ˮƽ��ͶӰ��
            Vector3 cameraRight = GetComponent<Camera>().transform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            // ��������ƶ���
            float mouseX = Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
            Vector3 moveDelta = cameraRight * mouseX;

            // Ӧ�ø�����ռ�ת��
            if (useParentSpace && transform.parent)
            {
                // ���ƶ�����ת����������ֲ��ռ�
                Vector3 localMove = transform.parent.InverseTransformDirection(moveDelta);
                localMove.y = 0; // ���ִ�ֱ���򲻱�

                // Ӧ�þֲ��ռ�����
                Vector3 localPos = transform.parent.InverseTransformPoint(transform.position) + localMove;
                localPos.x = Mathf.Clamp(localPos.x, parentLocalBoundsMin.x, parentLocalBoundsMax.x);
                localPos.z = Mathf.Clamp(localPos.z, parentLocalBoundsMin.z, parentLocalBoundsMax.z);

                // ת������������
                Vector3 newPos = transform.parent.TransformPoint(localPos);
                newPos.y = transform.position.y; // ���ָ߶Ȳ���
                transform.position = newPos;
            }
            else
            {
                // ����ռ��ƶ�����
                Vector3 newPos = transform.position + moveDelta;

                // Ӧ��ȫ������
                newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
                newPos.z = Mathf.Clamp(newPos.z, minBounds.z, maxBounds.z);
                newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);

                transform.position = newPos;
            }
        }

        // �������Ŀ��
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                _currentTarget = hit.point;
                _isLocked = true;
                targetMarker.position = _currentTarget + Vector3.up * 0.5f;
                targetMarker.gameObject.SetActive(true);
            }
        }

        // ����ָ��
        for (int i = 1; i <= 3; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                ExecuteFireCommand(i, _currentTarget);
            }
        }
    }

    void UpdateCameraControl()
    {
        // �޸ĺ��AD����
        float bankInput = Input.GetAxis("Horizontal");

        if (_overrideActive || Mathf.Abs(gunship.CurrentBankAngle) < rotationThreshold)
        {
            gunship.ApplyBank(bankInput);
        }

        // ���������
        _currentZoom -= Input.mouseScrollDelta.y * zoomSensitivity;
        _currentZoom = Mathf.Clamp(_currentZoom, zoomLimits.x, zoomLimits.y);
        GetComponent<Camera>().fieldOfView = Mathf.Lerp(
            GetComponent<Camera>().fieldOfView,
            _currentZoom,
            Time.deltaTime * 5f
        );

        // �л��Ӿ�ģʽ
        if (Input.GetKeyDown(KeyCode.Q)) SwitchVisionProfile(-1);
        if (Input.GetKeyDown(KeyCode.E)) SwitchVisionProfile(1);
    }

    void SwitchVisionProfile(int direction)
    {
        _currentProfileIndex = (_currentProfileIndex + direction + visionProfiles.Length) % visionProfiles.Length;
        if (nightVisionVolume != null)
            nightVisionVolume.profile = visionProfiles[_currentProfileIndex];
    }

    void ExecuteFireCommand(int commandType, Vector3 target)
    {
        if (!_isLocked) return;

        switch (commandType)
        {
            case 1:
                gunship.RequestPrecisionStrike(target);
                break;
            case 2:
                gunship.RequestAreaSuppression(target);
                break;
            case 3:
                gunship.RequestThermalScan(target);
                break;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 85, 300, 25), $"����״̬: {(_overrideActive ? "�ѽ��" : "����")}");
        GUI.Label(new Rect(10, 10, 300, 25), $"����״̬: {(_isLocked ? "������" : "δ����")}");
        GUI.Label(new Rect(10, 35, 300, 25), $"��ǰģʽ: {visionProfiles[_currentProfileIndex].name}");
        GUI.Label(new Rect(10, 60, 300, 25), $"������̬: {gunship.CurrentBankAngle:F1}��");
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugBounds) return;

        // ������ֲ��߽�
        if (useParentSpace && transform.parent)
        {
            Gizmos.color = Color.magenta;
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.parent.localToWorldMatrix;
            Gizmos.DrawWireCube(
                (parentLocalBoundsMin + parentLocalBoundsMax) / 2,
                parentLocalBoundsMax - parentLocalBoundsMin
            );
            Gizmos.matrix = originalMatrix;
        }

        // ��̬�߽�
        Gizmos.color = dynamicBounds ? Color.green :
                      (boundsMode == BoundsMode.Relative ? Color.cyan : Color.yellow);

        if (dynamicBounds)
        {
            Gizmos.DrawWireCube(transform.position, dynamicBoundsSize * 2);
        }
        else if (boundsMode == BoundsMode.Relative && boundsCenter != null)
        {
            Gizmos.DrawWireCube(boundsCenter.position, relativeBounds * 2);
        }
        else
        {
            Vector3 center = (minBounds + maxBounds) / 2;
            Vector3 size = maxBounds - minBounds;
            Gizmos.DrawWireCube(center, size);
        }
    }
}