using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    [Header("旋转参数")]
    public float mouseSensitivity = 2f;
    public float rotationSmoothing = 0.1f;

    [Header("地面检测")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    // 需要检测的对象列表
    public GameObject[] objectsToCheck;

    // 新增：摄像机引用
    public Camera playerCamera;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Quaternion characterTargetRot;
    private Quaternion cameraTargetRot;

    // 当前鼠标锁定状态
    private bool currentCursorLocked = true;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        // 初始化时默认锁定鼠标
        SetCursorLockState(true);

        characterTargetRot = transform.localRotation;
        // 新增：初始化摄像机的目标旋转
        cameraTargetRot = playerCamera.transform.localRotation;
    }

    private void Update()
    {
        // 检测对象是否启用
        bool anyObjectEnabled = false;
        foreach (GameObject obj in objectsToCheck)
        {
            if (obj != null && obj.activeSelf)
            {
                anyObjectEnabled = true;
                break;
            }
        }

        // 根据对象状态设置鼠标锁定状态
        bool newCursorLocked = !anyObjectEnabled;
        if (newCursorLocked != currentCursorLocked)
        {
            SetCursorLockState(newCursorLocked);
            currentCursorLocked = newCursorLocked;
        }

        // 地面检测
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // WASD移动
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 跳跃
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
        }

        // 应用重力
        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 角色旋转 (水平旋转角色)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;

        characterTargetRot *= Quaternion.Euler(0f, mouseX, 0f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, characterTargetRot, rotationSmoothing);

        // 新增：鼠标纵向输入控制摄像机上下旋转
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        cameraTargetRot *= Quaternion.Euler(-mouseY, 0f, 0f);
        // 限制摄像机的上下旋转角度
        cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);
        playerCamera.transform.localRotation = Quaternion.Slerp(playerCamera.transform.localRotation, cameraTargetRot, rotationSmoothing);
    }

    // 新增：限制摄像机的上下旋转角度
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

    // 封装设置鼠标锁定状态的方法
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