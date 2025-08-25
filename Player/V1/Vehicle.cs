using UnityEngine;

public class Vehicle : MonoBehaviour, IInteractable
{
    [Header("交互点设置")]
    [SerializeField] private Transform entryPoint;
    [SerializeField] private Transform exitPoint;

    [Header("交互提示")]
    [SerializeField] private string interactionPrompt = "按E上车";
    [SerializeField] private float promptDisplayTime = 2f;

    private float promptTimer = 0f;
    private bool promptVisible = false;

    public void Interact(GameObject interactor)
    {
        // 交互逻辑在 PlayerInteraction 脚本中实现
    }

    public void OnStartHover()
    {
        promptVisible = true;
        promptTimer = promptDisplayTime;
        Debug.Log($"显示提示: {interactionPrompt}");
    }

    public void OnEndHover()
    {
        promptVisible = false;
        Debug.Log("隐藏提示");
    }

    public void EnableVehicleControl(bool enable)
    {
        // 自动检测所在对象上是否存在 PrometeoCarController 脚本
        PrometeoCarController carController = GetComponent<PrometeoCarController>();
        if (carController != null)
        {
            carController.enabled = enable;
        }
        else
        {
            Debug.LogWarning("未在车辆对象上找到 PrometeoCarController 脚本。");
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