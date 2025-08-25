using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class CloudGenerator : MonoBehaviour
{
    [Header("生成参数")]
    [SerializeField] private GameObject cloudPrefab;
    [SerializeField] private int cloudCount = 20;
    [SerializeField] private Vector3 generationArea = new Vector3(100, 20, 100);
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float minDistanceFromPlayer = 30f;

    [Header("贴图参数")]
    [SerializeField] private string textureFolderPath = "Assets/Textures/Clouds";
    [SerializeField] private string textureFileExtension = "*.png";
    [SerializeField] private string texturePropertyName = "_BaseMap";

    [Header("云朵变化参数")]
    [SerializeField] private Vector3 scaleRange = new Vector3(0.8f, 1.2f, 1f);
    [SerializeField] private bool randomizeRotation = true;

    [Header("CloudController参数范围")]
    [SerializeField] private float moveSpeedMin = 2f;
    [SerializeField] private float moveSpeedMax = 5f;
    [SerializeField] private Vector3 moveDirectionMin = new Vector3(-0.1f, 0, 0.1f);
    [SerializeField] private Vector3 moveDirectionMax = new Vector3(0.1f, 0, 0.3f);
    [SerializeField] private float rotationSpeedMin = 5f;
    [SerializeField] private float rotationSpeedMax = 15f;
    [SerializeField] private float followSmoothingMin = 0.05f;
    [SerializeField] private float followSmoothingMax = 0.15f;
    [SerializeField] private float cloudOrbitRadiusMin = 40f;
    [SerializeField] private float cloudOrbitRadiusMax = 60f;
    [SerializeField] private float heightAbovePlayerMin = 15f;
    [SerializeField] private float heightAbovePlayerMax = 25f;
    [SerializeField] private Vector3 faceOffsetEulerMin = new Vector3(-5, -10, 0);
    [SerializeField] private Vector3 faceOffsetEulerMax = new Vector3(5, 10, 0);
    [SerializeField] private bool randomizeFaceOffset = true;
    [SerializeField] private float maxRandomOffsetMin = 5f;
    [SerializeField] private float maxRandomOffsetMax = 15f;
    [SerializeField] private bool enableVertexAnimation = true;
    [SerializeField] private float vertexAnimSpeedMin = 0.5f;
    [SerializeField] private float vertexAnimSpeedMax = 1.5f;
    [SerializeField] private float vertexAnimAmplitudeMin = 0.3f;
    [SerializeField] private float vertexAnimAmplitudeMax = 0.7f;

    private List<Texture2D> cloudTextures = new List<Texture2D>();
    private int textureId;

    private void Start()
    {
        // 自动查找玩家（如果未指定）
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("未找到玩家对象，请在CloudGenerator中指定玩家Transform或给玩家添加'Player'标签");
                return;
            }
        }

        LoadCloudTextures();
        GenerateClouds();
    }

    private void LoadCloudTextures()
    {
        if (!Directory.Exists(textureFolderPath))
        {
            Debug.LogError($"云朵贴图文件夹不存在: {textureFolderPath}");
            return;
        }

        string[] texturePaths = Directory.GetFiles(textureFolderPath, textureFileExtension);

        foreach (string path in texturePaths)
        {
#if UNITY_EDITOR
            string assetPath = path.Replace(Application.dataPath, "Assets");
            Texture2D texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
#else
            string relativePath = Path.GetRelativePath(Application.dataPath, path);
            string resourcePath = Path.ChangeExtension(relativePath, null);
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
#endif

            if (texture != null)
            {
                cloudTextures.Add(texture);
                Debug.Log($"成功加载纹理: {path}");
            }
            else
            {
                Debug.LogWarning($"无法加载纹理: {path}");
            }
        }

        if (cloudTextures.Count == 0)
        {
            Debug.LogError("未找到有效的云朵贴图");
        }
        else
        {
            Debug.Log($"成功加载 {cloudTextures.Count} 个云朵贴图");
        }

        textureId = Shader.PropertyToID(texturePropertyName);
    }

    private void GenerateClouds()
    {
        if (cloudPrefab == null)
        {
            Debug.LogError("未指定云朵预制体");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogError("玩家Transform未设置，无法生成云朵");
            return;
        }

        if (cloudTextures.Count == 0)
        {
            Debug.LogError("没有可用的云朵贴图，无法生成云朵");
            return;
        }

        for (int i = 0; i < cloudCount; i++)
        {
            Vector3 position = GenerateValidPosition();
            GameObject cloud = Instantiate(cloudPrefab, position, Quaternion.identity, transform);

            // 随机缩放
            float scale = Random.Range(scaleRange.x, scaleRange.y);
            cloud.transform.localScale = new Vector3(scale, scale, scaleRange.z);

            // 随机旋转
            if (randomizeRotation)
            {
                cloud.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }

            // 随机分配贴图
            AssignRandomTexture(cloud);

            // 配置CloudController参数
            ConfigureCloudController(cloud);
        }
    }

    private Vector3 GenerateValidPosition()
    {
        Vector3 position;
        int attempts = 0;

        do
        {
            position = new Vector3(
                playerTransform.position.x + Random.Range(-generationArea.x / 2, generationArea.x / 2),
                playerTransform.position.y + Random.Range(0, generationArea.y),
                playerTransform.position.z + Random.Range(-generationArea.z / 2, generationArea.z / 2)
            );

            attempts++;
            if (attempts > 100)
            {
                Debug.LogWarning("难以找到合适的云朵生成位置，可能需要调整生成区域大小");
                break;
            }
        }
        while (Vector3.Distance(position, playerTransform.position) < minDistanceFromPlayer);

        return position;
    }

    private void AssignRandomTexture(GameObject cloud)
    {
        Renderer renderer = cloud.GetComponent<Renderer>();
        if (renderer == null || renderer.material == null)
        {
            Debug.LogWarning("云朵预制体没有Renderer或Material组件");
            return;
        }

        Texture2D randomTexture = cloudTextures[Random.Range(0, cloudTextures.Count)];
        renderer.material.SetTexture(textureId, randomTexture);
    }

    private void ConfigureCloudController(GameObject cloud)
    {
        CloudController controller = cloud.GetComponent<CloudController>();
        if (controller == null)
        {
            controller = cloud.AddComponent<CloudController>();
        }

        // 配置玩家追踪参数
        controller.playerTransform = playerTransform;
        controller.rotationSpeed = Random.Range(rotationSpeedMin, rotationSpeedMax);
        controller.followSmoothing = Random.Range(followSmoothingMin, followSmoothingMax);
        controller.cloudOrbitRadius = Random.Range(cloudOrbitRadiusMin, cloudOrbitRadiusMax);
        controller.heightAbovePlayer = Random.Range(heightAbovePlayerMin, heightAbovePlayerMax);

        // 配置飘动参数
        controller.moveSpeed = Random.Range(moveSpeedMin, moveSpeedMax);
        controller.moveDirection = new Vector3(
            Random.Range(moveDirectionMin.x, moveDirectionMax.x),
            Random.Range(moveDirectionMin.y, moveDirectionMax.y),
            Random.Range(moveDirectionMin.z, moveDirectionMax.z)
        );

        // 配置朝向调试参数
        controller.faceOffsetEuler = new Vector3(
            Random.Range(faceOffsetEulerMin.x, faceOffsetEulerMax.x),
            Random.Range(faceOffsetEulerMin.y, faceOffsetEulerMax.y),
            Random.Range(faceOffsetEulerMin.z, faceOffsetEulerMax.z)
        );
        controller.randomizeFaceOffset = randomizeFaceOffset;
        controller.maxRandomOffset = Random.Range(maxRandomOffsetMin, maxRandomOffsetMax);

        // 配置顶点动画参数
        controller.enableVertexAnimation = enableVertexAnimation;
        controller.vertexAnimSpeed = Random.Range(vertexAnimSpeedMin, vertexAnimSpeedMax);
        controller.vertexAnimAmplitude = Random.Range(vertexAnimAmplitudeMin, vertexAnimAmplitudeMax);
    }
}
