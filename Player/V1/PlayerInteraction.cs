using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("交互设置")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 10f; // 交互距离
    [SerializeField] private LayerMask canUseLayer;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("调试选项")]
    [SerializeField] private bool showDebugRay = true; // 确定是否显示调试射线
    [SerializeField] private Color debugRayColor = Color.green; // 调试射线颜色

    [Header("需要禁用的脚本和对象列表")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;
    [SerializeField] private GameObject[] objectsToDisable;

    private IInteractable currentInteractable;
    private Vehicle currentVehicle;
    private bool isInVehicle = false;
    private GameObject player;
    private CharacterController characterController;

    private GameObject currentDoor;
    private DoorController doorController;

    private PrometeoTransmissionController transmissionController;

    // 记录相机原始信息
    private Camera mainCamera;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Start()
    {
        doorController = FindFirstObjectByType<DoorController>();
        if (doorController == null)
        {
            Debug.LogError("未找到 DoorController 组件");
        }

        // 获取PrometeoTransmissionController组件
        if (currentVehicle != null)
        {
            PrometeoCarController carController = currentVehicle.GetComponent<PrometeoCarController>();
            if (carController != null)
            {
                transmissionController = carController.transmissionController;
            }
        }

        player = gameObject;
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // 获取MainCamera的原始父对象、位置和旋转
        mainCamera = Camera.main;
        originalParent = mainCamera.transform.parent;
        originalPosition = mainCamera.transform.localPosition;
        originalRotation = mainCamera.transform.localRotation;
    }

    private void Update()
    {
        HandleInteractionDetection();
        HandleInteractionInput();
    }

    private void HandleInteractionDetection()
    {
        RaycastHit hit;
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (showDebugRay)
            Debug.DrawRay(ray.origin, ray.direction * interactionDistance, debugRayColor);

        bool hitSomething = Physics.Raycast(ray, out hit, interactionDistance, canUseLayer);
        currentDoor = null; // 先重置当前门

        if (hitSomething)
        {
            GameObject hitObject = hit.collider.gameObject;

            // 第一步：检测门
            if (hitObject.CompareTag("Door"))
            {
                currentDoor = hitObject;
                return; // 命中门时直接返回
            }

            // 第二步：检测其他可交互对象
            Transform rootParent = GetRootParent(hitObject.transform);
            IInteractable interactable = rootParent.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable?.OnEndHover();
                    currentInteractable = interactable;
                    currentInteractable.OnStartHover();
                }

                // 车辆检测
                Vehicle vehicle = rootParent.GetComponent<Vehicle>();
                currentVehicle = vehicle;
            }
        }
        else // 无命中时清空状态
        {
            currentInteractable?.OnEndHover();
            currentInteractable = null;
            currentVehicle = null;
        }
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactionKey))
        {
            if (currentInteractable != null)
            {
                if (currentVehicle != null)
                {
                    if (!isInVehicle)
                    {
                        EnterVehicle(currentVehicle);
                        // 设置transmissionController的一个有效状态
                        // transmissionController.isPlayerInVehicle = true;
                    }
                    else
                    {
                        ExitVehicle(currentVehicle);
                        // 设置transmissionController的一个有效状态
                        // transmissionController.isPlayerInVehicle = false;
                    }
                    return;
                }

                // 普通交互逻辑
                currentInteractable.Interact(player);
            }

            if (currentDoor != null && doorController != null)
            {
                DoorState doorState = currentDoor.GetComponent<DoorState>();
                if (doorState == null)
                {
                    doorState = currentDoor.AddComponent<DoorState>();
                }

                if (doorState.isOpen)
                {
                    doorController.StartCoroutine(doorController.CloseDoor(currentDoor));
                }
                else
                {
                    doorController.StartCoroutine(doorController.OpenDoor(currentDoor));
                }
            }
        }

        if (Input.GetKeyDown(interactionKey))
        {
            Debug.Log($"交互键按下 [门:{(currentDoor ? currentDoor.name : "null")}], [物体:{currentInteractable}]");
            // ...
        }
    }

    private void EnterVehicle(Vehicle vehicle)
    {
        isInVehicle = true;
        // 设置isPlayerInVehicle为true
        if (transmissionController != null)
        {
            transmissionController.isPlayerInVehicle = true;
        }
        // 角色移动和碰撞检测
        characterController.enabled = false;
        player.transform.parent = vehicle.transform;
        player.transform.position = vehicle.GetEntryPosition();

        // 禁用指定脚本和对象
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = false;
            }
        }
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // 启用车辆控制
        vehicle.EnableVehicleControl(true);

        // 暂停库存系统响应
        if (InventoryManager.Instance != null)
        {
            // 实际库存系统暂停逻辑，可能需要关闭某些UI
            Debug.Log("库存系统暂停响应");
        }

        // 找到带有CameraPosition标签的游戏对象
        GameObject cameraPositionObj = GameObject.FindWithTag("CameraPosition");
        if (cameraPositionObj != null)
        {
            // 记录原始位置和旋转
            originalParent = mainCamera.transform.parent;
            originalPosition = mainCamera.transform.localPosition;
            originalRotation = mainCamera.transform.localRotation;

            // 替换相机位置
            mainCamera.transform.parent = cameraPositionObj.transform.parent;
            mainCamera.transform.localPosition = cameraPositionObj.transform.localPosition;
            mainCamera.transform.localRotation = cameraPositionObj.transform.localRotation;

            Debug.Log("相机位置已替换");
        }
        else
        {
            Debug.LogError("未找到带有CameraPosition标签的游戏对象，无法替换相机位置");
        }

        Debug.Log("上车成功");
    }

    private void ExitVehicle(Vehicle vehicle)
    {
        isInVehicle = false;
        // 设置isPlayerInVehicle为false
        if (transmissionController != null)
        {
            transmissionController.isPlayerInVehicle = false;
        }

        // 角色移动和碰撞检测
        player.transform.parent = null;
        player.transform.position = vehicle.GetExitPosition();
        characterController.enabled = true;

        // 启用禁用的脚本和对象
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = true;
            }
        }
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        // 禁用车辆控制
        vehicle.EnableVehicleControl(false);

        // 恢复库存系统响应
        if (InventoryManager.Instance != null)
        {
            // 实际库存系统恢复逻辑，可能需要显示某些UI
            Debug.Log("库存系统恢复响应");
        }

        // 恢复MainCamera的父对象、位置和旋转
        mainCamera.transform.parent = originalParent;
        mainCamera.transform.localPosition = originalPosition;
        mainCamera.transform.localRotation = originalRotation;

        Debug.Log("下车成功");
    }

    private Transform GetRootParent(Transform transform)
    {
        while (transform.parent != null)
        {
            transform = transform.parent;
        }
        return transform;
    }
}