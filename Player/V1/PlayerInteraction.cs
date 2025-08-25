using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 10f; // ��������
    [SerializeField] private LayerMask canUseLayer;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("����ѡ��")]
    [SerializeField] private bool showDebugRay = true; // ȷ���Ƿ���ʾ��������
    [SerializeField] private Color debugRayColor = Color.green; // ����������ɫ

    [Header("��Ҫ���õĽű��Ͷ����б�")]
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

    // ��¼���ԭʼ��Ϣ
    private Camera mainCamera;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Start()
    {
        doorController = FindFirstObjectByType<DoorController>();
        if (doorController == null)
        {
            Debug.LogError("δ�ҵ� DoorController ���");
        }

        // ��ȡPrometeoTransmissionController���
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

        // ��ȡMainCamera��ԭʼ������λ�ú���ת
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
        currentDoor = null; // �����õ�ǰ��

        if (hitSomething)
        {
            GameObject hitObject = hit.collider.gameObject;

            // ��һ���������
            if (hitObject.CompareTag("Door"))
            {
                currentDoor = hitObject;
                return; // ������ʱֱ�ӷ���
            }

            // �ڶ�������������ɽ�������
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

                // �������
                Vehicle vehicle = rootParent.GetComponent<Vehicle>();
                currentVehicle = vehicle;
            }
        }
        else // ������ʱ���״̬
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
                        // ����transmissionController��һ����Ч״̬
                        // transmissionController.isPlayerInVehicle = true;
                    }
                    else
                    {
                        ExitVehicle(currentVehicle);
                        // ����transmissionController��һ����Ч״̬
                        // transmissionController.isPlayerInVehicle = false;
                    }
                    return;
                }

                // ��ͨ�����߼�
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
            Debug.Log($"���������� [��:{(currentDoor ? currentDoor.name : "null")}], [����:{currentInteractable}]");
            // ...
        }
    }

    private void EnterVehicle(Vehicle vehicle)
    {
        isInVehicle = true;
        // ����isPlayerInVehicleΪtrue
        if (transmissionController != null)
        {
            transmissionController.isPlayerInVehicle = true;
        }
        // ��ɫ�ƶ�����ײ���
        characterController.enabled = false;
        player.transform.parent = vehicle.transform;
        player.transform.position = vehicle.GetEntryPosition();

        // ����ָ���ű��Ͷ���
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

        // ���ó�������
        vehicle.EnableVehicleControl(true);

        // ��ͣ���ϵͳ��Ӧ
        if (InventoryManager.Instance != null)
        {
            // ʵ�ʿ��ϵͳ��ͣ�߼���������Ҫ�ر�ĳЩUI
            Debug.Log("���ϵͳ��ͣ��Ӧ");
        }

        // �ҵ�����CameraPosition��ǩ����Ϸ����
        GameObject cameraPositionObj = GameObject.FindWithTag("CameraPosition");
        if (cameraPositionObj != null)
        {
            // ��¼ԭʼλ�ú���ת
            originalParent = mainCamera.transform.parent;
            originalPosition = mainCamera.transform.localPosition;
            originalRotation = mainCamera.transform.localRotation;

            // �滻���λ��
            mainCamera.transform.parent = cameraPositionObj.transform.parent;
            mainCamera.transform.localPosition = cameraPositionObj.transform.localPosition;
            mainCamera.transform.localRotation = cameraPositionObj.transform.localRotation;

            Debug.Log("���λ�����滻");
        }
        else
        {
            Debug.LogError("δ�ҵ�����CameraPosition��ǩ����Ϸ�����޷��滻���λ��");
        }

        Debug.Log("�ϳ��ɹ�");
    }

    private void ExitVehicle(Vehicle vehicle)
    {
        isInVehicle = false;
        // ����isPlayerInVehicleΪfalse
        if (transmissionController != null)
        {
            transmissionController.isPlayerInVehicle = false;
        }

        // ��ɫ�ƶ�����ײ���
        player.transform.parent = null;
        player.transform.position = vehicle.GetExitPosition();
        characterController.enabled = true;

        // ���ý��õĽű��Ͷ���
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

        // ���ó�������
        vehicle.EnableVehicleControl(false);

        // �ָ����ϵͳ��Ӧ
        if (InventoryManager.Instance != null)
        {
            // ʵ�ʿ��ϵͳ�ָ��߼���������Ҫ��ʾĳЩUI
            Debug.Log("���ϵͳ�ָ���Ӧ");
        }

        // �ָ�MainCamera�ĸ�����λ�ú���ת
        mainCamera.transform.parent = originalParent;
        mainCamera.transform.localPosition = originalPosition;
        mainCamera.transform.localRotation = originalRotation;

        Debug.Log("�³��ɹ�");
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