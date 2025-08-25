using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    // ���ƶ��ٶ�
    public float doorSpeed = 2f;
    // �Ŵ򿪵ĽǶ�
    public float openAngle = 90f;

    // ���ŵ�Э�̣��޸ĺ�
    public IEnumerator OpenDoor(GameObject door)
    {
        DoorState doorState = door.GetComponent<DoorState>();
        if (doorState.isOpening || doorState.isOpen)
        {
            yield break; // �����ظ�����
        }

        doorState.isOpening = true;
        Quaternion startRotation = door.transform.rotation; // ��¼��ʼ��ת������ռ䣩

        // ����Ŀ����ת��Χ������Y����תopenAngle��
        Quaternion targetRotation = Quaternion.AngleAxis(openAngle, Vector3.up) * startRotation;

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * doorSpeed;
            // ʹ��Slerpƽ����ֵ����������ռ���ת��
            door.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime);
            yield return null;
        }

        doorState.isOpen = true;
        doorState.isOpening = false;
    }

    // �ر��ŵ�Э�̣��޸ĺ�
    public IEnumerator CloseDoor(GameObject door)
    {
        DoorState doorState = door.GetComponent<DoorState>();
        if (doorState.isClosing || !doorState.isOpen)
        {
            yield break; // �����ظ�����
        }

        doorState.isClosing = true;
        Quaternion startRotation = door.transform.rotation; // ��¼��ǰ��ת������ռ䣩

        // ����Ŀ����ת��Χ������Y�ᷴ����תopenAngle��
        Quaternion targetRotation = Quaternion.AngleAxis(-openAngle, Vector3.up) * startRotation;

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * doorSpeed;
            // ʹ��Lerp��Slerpƽ����ֵ
            door.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime);
            yield return null;
        }

        doorState.isOpen = false;
        doorState.isClosing = false;
    }

}

// ��������״̬�������
public class DoorState : MonoBehaviour
{
    public bool isOpen = false;
    public bool isOpening = false;
    public bool isClosing = false;
}