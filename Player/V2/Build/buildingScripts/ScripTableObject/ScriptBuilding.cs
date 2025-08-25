using System.Collections.Generic;
using UnityEngine;//ʹ�������ռ�

[CreateAssetMenu(fileName = "New Building", menuName = "Building/New Building")]//ScriptableObject�Ĵ���·��
public class ScriptBuilding : ScriptableObject//ScriptableObject���ݿ��Դ���̶���ֵ�������ʺ϶�̬����
{
    public int maxLevel = 3;//0��Ϊʩ��״̬����Ϊ���޵ȼ�
    public int buildingId = 1;//buildingId Ϊ������
    public string buildingName = "NewBuilding";//stringΪ�ַ�����Csharp��Դ���String��Ҫ�����Ż�
    public List<int>buildingHealth = new List<int>();//<int>��ʾ����int���б�
    public List<int> CreatTime = new List<int>();//����ʱ�䣬�Է��Ӽƣ���Ϊ0������,intΪ����ֵ������listΪ�б�
    [TextArea]//����дС����
    public List<string>buildingStory = new List<string>();
    public List<Mesh>buildingMesh = new List<Mesh>();//MeshΪ3dģ��������ı�����
    public List<Material>buildingMateriral = new List<Material>();//Material��ΪMeshRender������õĲ��ʱ���

    public GameObject BuildingPrefab;//GameObject����Ϸ���󣬻��ߴ����Ԥ�Ƽ�
}
