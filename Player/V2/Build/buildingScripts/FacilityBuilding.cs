using UnityEngine;

public enum BuildingState
{
    Pending,   // �ȴ�����
    Building,  // ������
    Completed  // �����
}

public enum BuildingType
{
    Production,      // �����ཨ��
    Storage,         // �洢�ཨ��
    Defense,         // �����ཨ��
    Trap,            // �����ཨ��
    LivingFacility,  // ������ʩ��
    Other            // ��������
}
public class FacilityBuilding : MonoBehaviour
{
    public ScriptBuilding buildingData;
    public BuildingState buildingState;

    private BuildingType buildingType;

    public int buildingLevel = 0;
    
}
