using UnityEngine;
using System.Collections;

public class AdvancedDoorController : MonoBehaviour
{
    [Header("基础设置")]
    public float rotationAngle = 90f;       // 单门旋转角度
    public float dualRotationAngle = 45f;  // 双门单侧旋转角度
    public float rotationSpeed = 3f;       // 旋转速度
    public float maxDistance = 3f;         // 交互距离
    public LayerMask doorLayer;            // 门层级

    private Camera mainCamera;
    private Transform currentDoorParent;   // 当前检测到的门父级
    private bool isOpen;                   // 门状态
    private Coroutine rotationCoroutine;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        CheckDoorInSight();
        HandleDoorInteraction();
    }

    // 准星检测逻辑
    void CheckDoorInSight()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, doorLayer))
        {
            Transform hitParent = hit.transform.parent;
            if (hitParent != null && hitParent.CompareTag("Door"))
            {
                currentDoorParent = hitParent;
                return;
            }
        }
        currentDoorParent = null;
    }

    // 处理交互输入
    void HandleDoorInteraction()
    {
        if (Input.GetKeyDown(KeyCode.F) && currentDoorParent != null)
        {
            isOpen = !isOpen;

            if (rotationCoroutine != null)
                StopCoroutine(rotationCoroutine);

            rotationCoroutine = StartCoroutine(RotateDoors());
        }
    }

    // 旋转协程
    IEnumerator RotateDoors()
    {
        int childCount = currentDoorParent.childCount;
        Quaternion[] startRotations = new Quaternion[childCount];
        Quaternion[] targetRotations = new Quaternion[childCount];

        // 初始化旋转数据
        for (int i = 0; i < childCount; i++)
        {
            Transform door = currentDoorParent.GetChild(i);
            startRotations[i] = door.localRotation;

            // 计算旋转方向（根据门的相对位置自动判断）
            float direction = Vector3.Dot(door.right, mainCamera.transform.forward) > 0 ? 1 : -1;
            float angle = childCount == 2 ? dualRotationAngle : rotationAngle;

            // Z轴旋转计算公式
            targetRotations[i] = isOpen ?
                startRotations[i] * Quaternion.Euler(0, 0, angle * direction) :
                currentDoorParent.GetChild(i).GetComponent<DoorMemory>().originalRotation;
        }

        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime * rotationSpeed;

            for (int i = 0; i < childCount; i++)
            {
                currentDoorParent.GetChild(i).localRotation = Quaternion.Slerp(
                    startRotations[i],
                    targetRotations[i],
                    Mathf.SmoothStep(0, 1, progress)
                );
            }

            yield return null;
        }

        // 记录初始旋转（用于复位）
        if (!isOpen)
        {
            foreach (Transform child in currentDoorParent)
            {
                child.GetComponent<DoorMemory>().originalRotation = child.localRotation;
            }
        }
    }
}

// 门记忆组件（需挂载到每个门子级）
