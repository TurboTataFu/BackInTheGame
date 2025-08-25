using UnityEngine;

public class EnableObjectOnClick : MonoBehaviour
{
    // 在Inspector中拖放需要启用的对象
    public GameObject objectToEnable;
    public GameObject craftObj;
    public string jsonFileName;
    // 当鼠标点击带有碰撞体的对象时调用
    private void OnMouseDown()
    {
        // 检查是否已指定要启用的对象
        if (objectToEnable != null)
        {
            // 启用目标对象
            objectToEnable.SetActive(true);
            DialogueSystem diaSys = craftObj.GetComponent<DialogueSystem>();
            diaSys.Initialize(jsonFileName);

            Debug.Log($"已启用对象: {objectToEnable.name}");
        }
        else
        {
            Debug.LogWarning("请在Inspector中指定要启用的对象!");
        }
    }
}
