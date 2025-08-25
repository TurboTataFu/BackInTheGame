using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Header("�ƶ�����")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    [Header("��ת����")]
    public float mouseSensitivity = 2f;
    public float rotationSmoothing = 0.1f;

    [Header("������")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    // ��Ҫ���Ķ����б�
    public GameObject[] objectsToCheck;

    // ���������������
    public Camera playerCamera;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Quaternion characterTargetRot;
    private Quaternion cameraTargetRot;

    // ��ǰ�������״̬
    private bool currentCursorLocked = true;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        // ��ʼ��ʱĬ���������
        SetCursorLockState(true);

        characterTargetRot = transform.localRotation;
        // ��������ʼ���������Ŀ����ת
        cameraTargetRot = playerCamera.transform.localRotation;
    }

    private void Update()
    {
        // �������Ƿ�����
        bool anyObjectEnabled = false;
        foreach (GameObject obj in objectsToCheck)
        {
            if (obj != null && obj.activeSelf)
            {
                anyObjectEnabled = true;
                break;
            }
        }

        // ���ݶ���״̬�����������״̬
        bool newCursorLocked = !anyObjectEnabled;
        if (newCursorLocked != currentCursorLocked)
        {
            SetCursorLockState(newCursorLocked);
            currentCursorLocked = newCursorLocked;
        }

        // ������
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // WASD�ƶ�
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // ��Ծ
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
        }

        // Ӧ������
        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // ��ɫ��ת (ˮƽ��ת��ɫ)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;

        characterTargetRot *= Quaternion.Euler(0f, mouseX, 0f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, characterTargetRot, rotationSmoothing);

        // �������������������������������ת
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        cameraTargetRot *= Quaternion.Euler(-mouseY, 0f, 0f);
        // �����������������ת�Ƕ�
        cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);
        playerCamera.transform.localRotation = Quaternion.Slerp(playerCamera.transform.localRotation, cameraTargetRot, rotationSmoothing);
    }

    // �����������������������ת�Ƕ�
    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, -90f, 90f);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    // ��װ�����������״̬�ķ���
    private void SetCursorLockState(bool lockCursor)
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}