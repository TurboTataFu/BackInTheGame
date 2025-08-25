using System.Collections.Generic;
using UnityEngine;//使用命名空间

[CreateAssetMenu(fileName = "New Building", menuName = "Building/New Building")]//ScriptableObject的创建路径
public class ScriptBuilding : ScriptableObject//ScriptableObject数据可以储存固定数值，但不适合动态变量
{
    public int maxLevel = 3;//0级为施工状态，此为极限等级
    public int buildingId = 1;//buildingId 为变量名
    public string buildingName = "NewBuilding";//string为字符串，Csharp面对大量String需要进行优化
    public List<int>buildingHealth = new List<int>();//<int>表示储存int的列表
    public List<int> CreatTime = new List<int>();//建造时间，以分钟计，或为0立马建造,int为整数值变量，list为列表
    [TextArea]//方便写小作文
    public List<string>buildingStory = new List<string>();
    public List<Mesh>buildingMesh = new List<Mesh>();//Mesh为3d模型网格体的变量名
    public List<Material>buildingMateriral = new List<Material>();//Material是为MeshRender组件所用的材质变量

    public GameObject BuildingPrefab;//GameObject，游戏对象，或者储存的预制件
}
