using UnityEngine;

public class Vehicle : MonoBehaviour, IInteractable
{
    [Header("����������")]
    [SerializeField] private Transform entryPoint;
    [SerializeField] private Transform exitPoint;

    [Header("������ʾ")]
    [SerializeField] private string interactionPrompt = "��E�ϳ�";
    [SerializeField] private float promptDisplayTime = 2f;

    private float promptTimer = 0f;
    private bool promptVisible = false;

    public void Interact(GameObject interactor)
    {
        // �����߼��� PlayerInteraction �ű���ʵ��
    }

    public void OnStartHover()
    {
        promptVisible = true;
        promptTimer = promptDisplayTime;
        Debug.Log($"��ʾ��ʾ: {interactionPrompt}");
    }

    public void OnEndHover()
    {
        promptVisible = false;
        Debug.Log("������ʾ");
    }

    public void EnableVehicleControl(bool enable)
    {
        // �Զ�������ڶ������Ƿ���� PrometeoCarController �ű�
        PrometeoCarController carController = GetComponent<PrometeoCarController>();
        if (carController != null)
        {
            carController.enabled = enable;
        }
        else
        {
            Debug.LogWarning("δ�ڳ����������ҵ� PrometeoCarController �ű���");
        }
    }

    public Vector3 GetEntryPosition()
    {
        return entryPoint != null ? entryPoint.position : transform.position + transform.forward * 2f;
    }

    public Vector3 GetExitPosition()
    {
        return exitPoint != null ? exitPoint.position : transform.position + transform.forward * 2f;
    }

    private void Update()
    {
        if (promptVisible)
        {
            promptTimer -= Time.deltaTime;
            if (promptTimer <= 0)
            {
                promptVisible = false;
            }
        }
    }
}