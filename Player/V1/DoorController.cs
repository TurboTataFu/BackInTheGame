using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    // 门移动速度
    public float doorSpeed = 2f;
    // 门打开的角度
    public float openAngle = 90f;

    // 打开门的协程（修改后）
    public IEnumerator OpenDoor(GameObject door)
    {
        DoorState doorState = door.GetComponent<DoorState>();
        if (doorState.isOpening || doorState.isOpen)
        {
            yield break; // 避免重复触发
        }

        doorState.isOpening = true;
        Quaternion startRotation = door.transform.rotation; // 记录初始旋转（世界空间）

        // 计算目标旋转：围绕世界Y轴旋转openAngle度
        Quaternion targetRotation = Quaternion.AngleAxis(openAngle, Vector3.up) * startRotation;

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * doorSpeed;
            // 使用Slerp平滑插值（保持世界空间旋转）
            door.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime);
            yield return null;
        }

        doorState.isOpen = true;
        doorState.isOpening = false;
    }

    // 关闭门的协程（修改后）
    public IEnumerator CloseDoor(GameObject door)
    {
        DoorState doorState = door.GetComponent<DoorState>();
        if (doorState.isClosing || !doorState.isOpen)
        {
            yield break; // 避免重复触发
        }

        doorState.isClosing = true;
        Quaternion startRotation = door.transform.rotation; // 记录当前旋转（世界空间）

        // 计算目标旋转：围绕世界Y轴反向旋转openAngle度
        Quaternion targetRotation = Quaternion.AngleAxis(-openAngle, Vector3.up) * startRotation;

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * doorSpeed;
            // 使用Lerp或Slerp平滑插值
            door.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime);
            yield return null;
        }

        doorState.isOpen = false;
        doorState.isClosing = false;
    }

}

// 新增：门状态管理组件
public class DoorState : MonoBehaviour
{
    public bool isOpen = false;
    public bool isOpening = false;
    public bool isClosing = false;
}