using UnityEngine;

public class CloudController : MonoBehaviour
{
    [Header("飘动参数")]
    [Tooltip("云朵移动速度")]
    public float moveSpeed = 5f;
    [Tooltip("云朵移动方向")]
    public Vector3 moveDirection = Vector3.right;
    [Tooltip("重置位置的距离阈值")]
    public float resetDistance = 100f;
    [Tooltip("初始位置")]
    public Vector3 startPosition;

    [Header("颜色参数")]
    [Tooltip("云朵颜色渐变")]
    public Gradient cloudColor;
    [Tooltip("颜色循环周期(小时)")]
    public float colorCycleDuration = 24f;
    [Tooltip("云朵材质")]
    public Material cloudMaterial;
    [Tooltip("颜色属性名称")]
    public string colorPropertyName = "_BaseColor";

    [Header("玩家追踪参数")]
    [Tooltip("玩家Transform")]
    public Transform playerTransform;
    [Tooltip("玩家标签")]
    public string playerTag = "Player";
    [Tooltip("绕玩家旋转速度")]
    public float rotationSpeed = 10f;
    [Tooltip("平滑跟随系数")]
    public float followSmoothing = 0.1f;
    [Tooltip("绕玩家轨道半径")]
    public float cloudOrbitRadius = 50f;
    [Tooltip("玩家上方高度")]
    public float heightAbovePlayer = 20f;

    [Header("朝向调试参数")]
    [Tooltip("朝向偏移角度")]
    public Vector3 faceOffsetEuler;
    [Tooltip("是否随机化朝向偏移")]
    public bool randomizeFaceOffset;
    [Tooltip("最大随机偏移角度")]
    public float maxRandomOffset = 15f;

    [Header("顶点动画参数")]
    [Tooltip("是否启用顶点动画")]
    public bool enableVertexAnimation = true;
    [Tooltip("顶点动画速度")]
    public float vertexAnimSpeed = 1f;
    [Tooltip("顶点动画幅度")]
    public float vertexAnimAmplitude = 0.5f;
    [Tooltip("顶点动画曲线")]
    public AnimationCurve vertexAnimCurve = AnimationCurve.Linear(0, 0, 1, 1); // 默认线性曲线

    private int colorPropertyId;
    private Light directionalLight;
    private Vector3 targetPosition;
    private float orbitAngle;
    private Vector3 randomizedOffset;
    private MeshFilter meshFilter;
    private Mesh originalMesh;
    private Mesh cloneMesh;
    private Vector3[] originalVertices;
    private Vector3[] animatedVertices;

    private void Start()
    {
        // 自动查找玩家对象
        FindPlayer();

        if (playerTransform == null)
        {
            Debug.LogError("未找到带有'" + playerTag + "'标签的玩家对象，云朵将不会追踪玩家");
            enabled = false;
            return;
        }

        startPosition = transform.position;
        directionalLight = RenderSettings.sun;
        if (cloudMaterial == null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                cloudMaterial = renderer.material;
            }
        }

        colorPropertyId = Shader.PropertyToID(colorPropertyName);

        // 初始化云朵位置
        orbitAngle = UnityEngine.Random.Range(0f, 360f);
        UpdateTargetPosition();
        transform.position = targetPosition;

        // 初始化朝向偏移
        if (randomizeFaceOffset)
        {
            randomizedOffset = new Vector3(
                UnityEngine.Random.Range(-maxRandomOffset, maxRandomOffset),
                UnityEngine.Random.Range(-maxRandomOffset, maxRandomOffset),
                UnityEngine.Random.Range(-maxRandomOffset, maxRandomOffset)
            );
        }

        // 初始化顶点动画
        if (enableVertexAnimation)
        {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                originalMesh = meshFilter.sharedMesh;
                if (originalMesh != null)
                {
                    cloneMesh = Instantiate(originalMesh);
                    meshFilter.mesh = cloneMesh;
                    originalVertices = originalMesh.vertices;
                    animatedVertices = new Vector3[originalVertices.Length];
                    System.Array.Copy(originalVertices, animatedVertices, originalVertices.Length);
                }
                else
                {
                    Debug.LogWarning("云朵没有网格数据，无法启用顶点动画");
                    enableVertexAnimation = false;
                }
            }
            else
            {
                Debug.LogWarning("云朵没有MeshFilter组件，无法启用顶点动画");
                enableVertexAnimation = false;
            }
        }
    }

    private void FindPlayer()
    {
        // 如果没有手动指定玩家，尝试通过标签查找
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindWithTag(playerTag);
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
                Debug.Log("自动找到玩家对象: " + playerObject.name);
            }
        }
    }

    private void Update()
    {
        // 每帧检查玩家引用是否有效
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        MoveCloud();
        UpdateCloudColor();
        FacePlayer();

        if (enableVertexAnimation && meshFilter != null && cloneMesh != null)
        {
            AnimateVertices();
        }
    }

    private void MoveCloud()
    {
        // 更新轨道角度（绕玩家旋转）
        orbitAngle += rotationSpeed * Time.deltaTime;
        UpdateTargetPosition();

        // 平滑移动到目标位置
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSmoothing);

        // 本地移动（云朵自身飘动）
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.Self);
    }

    private void UpdateTargetPosition()
    {
        // 计算绕玩家的轨道位置
        float x = playerTransform.position.x + Mathf.Cos(orbitAngle * Mathf.Deg2Rad) * cloudOrbitRadius;
        float z = playerTransform.position.z + Mathf.Sin(orbitAngle * Mathf.Deg2Rad) * cloudOrbitRadius;
        targetPosition = new Vector3(x, playerTransform.position.y + heightAbovePlayer, z);
    }

    private void FacePlayer()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        if (directionToPlayer != Vector3.zero)
        {
            // 主朝向：保持Y轴垂直
            Quaternion baseRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);

            // 应用局部偏移
            Vector3 offset = randomizeFaceOffset ? randomizedOffset : faceOffsetEuler;
            transform.rotation = baseRotation * Quaternion.Euler(offset);

            // 可选：保持水平（防止倾斜过度）
            Vector3 euler = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(offset.x, euler.y, offset.z);
        }
    }

    private void AnimateVertices()
    {
        for (int i = 0; i < originalVertices.Length; i++)
        {
            // 使用顶点索引和时间生成随机但连贯的动画
            float timeFactor = Time.time * vertexAnimSpeed + i * 0.5f;
            float noise = Mathf.PerlinNoise(timeFactor, timeFactor * 0.7f);

            // 使用曲线调整动画强度
            float curveValue = vertexAnimCurve.Evaluate(noise);

            // 计算顶点偏移
            Vector3 offset = UnityEngine.Random.insideUnitSphere * vertexAnimAmplitude * curveValue;

            // 应用偏移
            animatedVertices[i] = originalVertices[i] + offset;
        }

        // 更新网格顶点
        cloneMesh.vertices = animatedVertices;
        cloneMesh.RecalculateNormals();
        cloneMesh.RecalculateBounds();
    }

    private void UpdateCloudColor()
    {
        if (cloudMaterial == null || !cloudMaterial.HasProperty(colorPropertyId))
            return;

        float timeOfDay = GetTimeOfDay();
        Color targetColor = cloudColor.Evaluate(timeOfDay);

        cloudMaterial.SetColor(colorPropertyId, targetColor);

        // 可选：根据时间调整透明度
        if (cloudMaterial.HasProperty("_Surface"))
        {
            float alpha = Mathf.Lerp(0.3f, 1f, Mathf.Clamp01(timeOfDay - 0.2f) * 5f);
            Color currentColor = cloudMaterial.GetColor(colorPropertyId);
            cloudMaterial.SetColor(colorPropertyId, new Color(currentColor.r, currentColor.g, currentColor.b, alpha));
        }
    }

    private float GetTimeOfDay()
    {
        // 使用系统时间
        System.DateTime currentTime = System.DateTime.Now;
        float normalizedTime = (currentTime.Hour * 3600 + currentTime.Minute * 60 + currentTime.Second) / 86400f;

        // 或者使用场景中的灯光旋转（如果使用了昼夜循环系统）
        if (directionalLight != null)
        {
            normalizedTime = (directionalLight.transform.eulerAngles.x + 180f) / 360f;
        }

        return normalizedTime;
    }
}