using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // �������
    public bool LeftMouseDown => Input.GetMouseButtonDown(0);
    public bool LeftMouseHeld => Input.GetMouseButton(0);
    public bool LeftMouseUp => Input.GetMouseButtonUp(0);

    public bool RightMouseDown => Input.GetMouseButtonDown(1);
    public bool RightMouseHeld => Input.GetMouseButton(1);
    public bool RightMouseUp => Input.GetMouseButtonUp(1);

    public float MouseScroll => Input.GetAxis("Mouse ScrollWheel");

    // ��������
    public bool AltHeld => Input.GetKey(KeyCode.LeftAlt);
    public bool CtrlHeld => Input.GetKey(KeyCode.LeftControl);
    public bool CtrlPressed => Input.GetKeyDown(KeyCode.LeftControl);

    void Awake()
    {
        // ����ģʽ��ʼ��
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Update()
    {
        // �Զ��������ֵ����ѡ��
        // �����������ű���ֱ�ӷ��� MouseScroll ����
    }

    /// <summary>
    /// ��ȡ��һ����Ĺ������루-1, 0, 1��
    /// </summary>
    public int GetNormalizedScroll()
    {
        if (MouseScroll > 0) return 1;
        if (MouseScroll < 0) return -1;
        return 0;
    }
}