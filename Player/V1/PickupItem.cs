using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("��Ʒ��Ϣ")]
    [SerializeField] private string itemName = "��Ʒ";
    [SerializeField] private int itemID = 0;
    [SerializeField] private int itemCount = 1;

    [Header("��������")]
    [SerializeField] private string interactionPrompt = "��Eʰȡ";
    [SerializeField] private float promptDisplayTime = 2f;

    private float promptTimer = 0f;
    private bool promptVisible = false;

    public void Interact(GameObject interactor)
    {
        if (InventoryManager.Instance != null)
        {
            bool success = InventoryManager.Instance.AddItem(itemID, itemCount);
            if (success)
            {
                Debug.Log($"ʰȡ�� {itemCount} �� {itemName}");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("�޷�ʰȡ��Ʒ����������");
            }
        }
        else
        {
            Debug.Log("δ�ҵ�����������");
        }
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