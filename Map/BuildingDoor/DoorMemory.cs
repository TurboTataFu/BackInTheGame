// �ż������������ص�ÿ�����Ӽ���
using UnityEngine;

public class DoorMemory : MonoBehaviour
{
    [HideInInspector]
    public Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.localRotation;
    }
}