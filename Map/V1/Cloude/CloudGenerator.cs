using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class CloudGenerator : MonoBehaviour
{
    [Header("���ɲ���")]
    [SerializeField] private GameObject cloudPrefab;
    [SerializeField] private int cloudCount = 20;
    [SerializeField] private Vector3 generationArea = new Vector3(100, 20, 100);
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float minDistanceFromPlayer = 30f;

    [Header("��ͼ����")]
    [SerializeField] private string textureFolderPath = "Assets/Textures/Clouds";
    [SerializeField] private string textureFileExtension = "*.png";
    [SerializeField] private string texturePropertyName = "_BaseMap";

    [Header("�ƶ�仯����")]
    [SerializeField] private Vector3 scaleRange = new Vector3(0.8f, 1.2f, 1f);
    [SerializeField] private bool randomizeRotation = true;

    [Header("CloudController������Χ")]
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
        // �Զ�������ң����δָ����
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("δ�ҵ���Ҷ�������CloudGenerator��ָ�����Transform���������'Player'��ǩ");
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
            Debug.LogError($"�ƶ���ͼ�ļ��в�����: {textureFolderPath}");
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
                Debug.Log($"�ɹ���������: {path}");
            }
            else
            {
                Debug.LogWarning($"�޷���������: {path}");
            }
        }

        if (cloudTextures.Count == 0)
        {
            Debug.LogError("δ�ҵ���Ч���ƶ���ͼ");
        }
        else
        {
            Debug.Log($"�ɹ����� {cloudTextures.Count} ���ƶ���ͼ");
        }

        textureId = Shader.PropertyToID(texturePropertyName);
    }

    private void GenerateClouds()
    {
        if (cloudPrefab == null)
        {
            Debug.LogError("δָ���ƶ�Ԥ����");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogError("���Transformδ���ã��޷������ƶ�");
            return;
        }

        if (cloudTextures.Count == 0)
        {
            Debug.LogError("û�п��õ��ƶ���ͼ���޷������ƶ�");
            return;
        }

        for (int i = 0; i < cloudCount; i++)
        {
            Vector3 position = GenerateValidPosition();
            GameObject cloud = Instantiate(cloudPrefab, position, Quaternion.identity, transform);

            // �������
            float scale = Random.Range(scaleRange.x, scaleRange.y);
            cloud.transform.localScale = new Vector3(scale, scale, scaleRange.z);

            // �����ת
            if (randomizeRotation)
            {
                cloud.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }

            // ���������ͼ
            AssignRandomTexture(cloud);

            // ����CloudController����
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
                Debug.LogWarning("�����ҵ����ʵ��ƶ�����λ�ã�������Ҫ�������������С");
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
            Debug.LogWarning("�ƶ�Ԥ����û��Renderer��Material���");
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

        // �������׷�ٲ���
        controller.playerTransform = playerTransform;
        controller.rotationSpeed = Random.Range(rotationSpeedMin, rotationSpeedMax);
        controller.followSmoothing = Random.Range(followSmoothingMin, followSmoothingMax);
        controller.cloudOrbitRadius = Random.Range(cloudOrbitRadiusMin, cloudOrbitRadiusMax);
        controller.heightAbovePlayer = Random.Range(heightAbovePlayerMin, heightAbovePlayerMax);

        // ����Ʈ������
        controller.moveSpeed = Random.Range(moveSpeedMin, moveSpeedMax);
        controller.moveDirection = new Vector3(
            Random.Range(moveDirectionMin.x, moveDirectionMax.x),
            Random.Range(moveDirectionMin.y, moveDirectionMax.y),
            Random.Range(moveDirectionMin.z, moveDirectionMax.z)
        );

        // ���ó�����Բ���
        controller.faceOffsetEuler = new Vector3(
            Random.Range(faceOffsetEulerMin.x, faceOffsetEulerMax.x),
            Random.Range(faceOffsetEulerMin.y, faceOffsetEulerMax.y),
            Random.Range(faceOffsetEulerMin.z, faceOffsetEulerMax.z)
        );
        controller.randomizeFaceOffset = randomizeFaceOffset;
        controller.maxRandomOffset = Random.Range(maxRandomOffsetMin, maxRandomOffsetMax);

        // ���ö��㶯������
        controller.enableVertexAnimation = enableVertexAnimation;
        controller.vertexAnimSpeed = Random.Range(vertexAnimSpeedMin, vertexAnimSpeedMax);
        controller.vertexAnimAmplitude = Random.Range(vertexAnimAmplitudeMin, vertexAnimAmplitudeMax);
    }
}
