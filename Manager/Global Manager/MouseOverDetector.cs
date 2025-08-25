using UnityEngine;

public class MouseOverDetector : MonoBehaviour
{
    [Tooltip("��Ҫ����/���õĶ���")]
    public GameObject targetObject;

    void OnMouseEnter()
    {
        // ����������ײ��ʱ����Ŀ�����
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }

    void OnMouseExit()
    {
        // ������뿪��ײ��ʱ����Ŀ�����
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }
}