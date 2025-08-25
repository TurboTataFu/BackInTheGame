using UnityEngine;

public enum BuildingState
{
    Pending,   // 等待建造
    Building,  // 建造中
    Completed  // 已完成
}

public enum BuildingType
{
    Production,      // 生产类建筑
    Storage,         // 存储类建筑
    Defense,         // 防御类建筑
    Trap,            // 陷阱类建筑
    LivingFacility,  // 生活设施类
    Other            // 其他类型
}
public class FacilityBuilding : MonoBehaviour
{
    public ScriptBuilding buildingData;
    public BuildingState buildingState;

    private BuildingType buildingType;

    public int buildingLevel = 0;
    
}
