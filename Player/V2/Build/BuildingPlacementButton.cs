using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildingPlacementButton : MonoBehaviour
{
    [Tooltip("选择该按钮对应的建筑")]
    public BuildingData targetBuilding;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        if (BuildingPlacementManager.Instance == null)
        {
            Debug.LogError("场景中未找到BuildingPlacementManager实例！", this);
            return;
        }

        if (targetBuilding == null)
        {
            Debug.LogError("未设置目标建筑，请在Inspector中选择建筑", this);
            return;
        }

        if (!targetBuilding.IsValid())
        {
            Debug.LogError($"建筑数据不完整：{targetBuilding.category}/{targetBuilding.id}", this);
            return;
        }

        BuildingPlacementManager.Instance.StartAdjusting(targetBuilding);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (targetBuilding != null && !targetBuilding.IsValid())
        {
            Debug.LogWarning($"绑定的建筑数据不完整：{targetBuilding.category}/{targetBuilding.id}", this);
        }
    }
#endif
}
