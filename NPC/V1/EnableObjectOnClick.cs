using UnityEngine;

public class EnableObjectOnClick : MonoBehaviour
{
    // ��Inspector���Ϸ���Ҫ���õĶ���
    public GameObject objectToEnable;
    public GameObject craftObj;
    public string jsonFileName;
    // �������������ײ��Ķ���ʱ����
    private void OnMouseDown()
    {
        // ����Ƿ���ָ��Ҫ���õĶ���
        if (objectToEnable != null)
        {
            // ����Ŀ�����
            objectToEnable.SetActive(true);
            DialogueSystem diaSys = craftObj.GetComponent<DialogueSystem>();
            diaSys.Initialize(jsonFileName);

            Debug.Log($"�����ö���: {objectToEnable.name}");
        }
        else
        {
            Debug.LogWarning("����Inspector��ָ��Ҫ���õĶ���!");
        }
    }
}
