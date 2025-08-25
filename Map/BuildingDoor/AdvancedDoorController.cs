using UnityEngine;
using System.Collections;

public class AdvancedDoorController : MonoBehaviour
{
    [Header("��������")]
    public float rotationAngle = 90f;       // ������ת�Ƕ�
    public float dualRotationAngle = 45f;  // ˫�ŵ�����ת�Ƕ�
    public float rotationSpeed = 3f;       // ��ת�ٶ�
    public float maxDistance = 3f;         // ��������
    public LayerMask doorLayer;            // �Ų㼶

    private Camera mainCamera;
    private Transform currentDoorParent;   // ��ǰ��⵽���Ÿ���
    private bool isOpen;                   // ��״̬
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

    // ׼�Ǽ���߼�
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

    // ����������
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

    // ��תЭ��
    IEnumerator RotateDoors()
    {
        int childCount = currentDoorParent.childCount;
        Quaternion[] startRotations = new Quaternion[childCount];
        Quaternion[] targetRotations = new Quaternion[childCount];

        // ��ʼ����ת����
        for (int i = 0; i < childCount; i++)
        {
            Transform door = currentDoorParent.GetChild(i);
            startRotations[i] = door.localRotation;

            // ������ת���򣨸����ŵ����λ���Զ��жϣ�
            float direction = Vector3.Dot(door.right, mainCamera.transform.forward) > 0 ? 1 : -1;
            float angle = childCount == 2 ? dualRotationAngle : rotationAngle;

            // Z����ת���㹫ʽ
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

        // ��¼��ʼ��ת�����ڸ�λ��
        if (!isOpen)
        {
            foreach (Transform child in currentDoorParent)
            {
                child.GetComponent<DoorMemory>().originalRotation = child.localRotation;
            }
        }
    }
}

// �ż������������ص�ÿ�����Ӽ���
