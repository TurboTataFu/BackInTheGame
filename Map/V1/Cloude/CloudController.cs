using UnityEngine;

public class CloudController : MonoBehaviour
{
    [Header("Ʈ������")]
    [Tooltip("�ƶ��ƶ��ٶ�")]
    public float moveSpeed = 5f;
    [Tooltip("�ƶ��ƶ�����")]
    public Vector3 moveDirection = Vector3.right;
    [Tooltip("����λ�õľ�����ֵ")]
    public float resetDistance = 100f;
    [Tooltip("��ʼλ��")]
    public Vector3 startPosition;

    [Header("��ɫ����")]
    [Tooltip("�ƶ���ɫ����")]
    public Gradient cloudColor;
    [Tooltip("��ɫѭ������(Сʱ)")]
    public float colorCycleDuration = 24f;
    [Tooltip("�ƶ����")]
    public Material cloudMaterial;
    [Tooltip("��ɫ��������")]
    public string colorPropertyName = "_BaseColor";

    [Header("���׷�ٲ���")]
    [Tooltip("���Transform")]
    public Transform playerTransform;
    [Tooltip("��ұ�ǩ")]
    public string playerTag = "Player";
    [Tooltip("�������ת�ٶ�")]
    public float rotationSpeed = 10f;
    [Tooltip("ƽ������ϵ��")]
    public float followSmoothing = 0.1f;
    [Tooltip("����ҹ���뾶")]
    public float cloudOrbitRadius = 50f;
    [Tooltip("����Ϸ��߶�")]
    public float heightAbovePlayer = 20f;

    [Header("������Բ���")]
    [Tooltip("����ƫ�ƽǶ�")]
    public Vector3 faceOffsetEuler;
    [Tooltip("�Ƿ����������ƫ��")]
    public bool randomizeFaceOffset;
    [Tooltip("������ƫ�ƽǶ�")]
    public float maxRandomOffset = 15f;

    [Header("���㶯������")]
    [Tooltip("�Ƿ����ö��㶯��")]
    public bool enableVertexAnimation = true;
    [Tooltip("���㶯���ٶ�")]
    public float vertexAnimSpeed = 1f;
    [Tooltip("���㶯������")]
    public float vertexAnimAmplitude = 0.5f;
    [Tooltip("���㶯������")]
    public AnimationCurve vertexAnimCurve = AnimationCurve.Linear(0, 0, 1, 1); // Ĭ����������

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
        // �Զ�������Ҷ���
        FindPlayer();

        if (playerTransform == null)
        {
            Debug.LogError("δ�ҵ�����'" + playerTag + "'��ǩ����Ҷ����ƶ佫����׷�����");
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

        // ��ʼ���ƶ�λ��
        orbitAngle = UnityEngine.Random.Range(0f, 360f);
        UpdateTargetPosition();
        transform.position = targetPosition;

        // ��ʼ������ƫ��
        if (randomizeFaceOffset)
        {
            randomizedOffset = new Vector3(
                UnityEngine.Random.Range(-maxRandomOffset, maxRandomOffset),
                UnityEngine.Random.Range(-maxRandomOffset, maxRandomOffset),
                UnityEngine.Random.Range(-maxRandomOffset, maxRandomOffset)
            );
        }

        // ��ʼ�����㶯��
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
                    Debug.LogWarning("�ƶ�û���������ݣ��޷����ö��㶯��");
                    enableVertexAnimation = false;
                }
            }
            else
            {
                Debug.LogWarning("�ƶ�û��MeshFilter������޷����ö��㶯��");
                enableVertexAnimation = false;
            }
        }
    }

    private void FindPlayer()
    {
        // ���û���ֶ�ָ����ң�����ͨ����ǩ����
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindWithTag(playerTag);
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
                Debug.Log("�Զ��ҵ���Ҷ���: " + playerObject.name);
            }
        }
    }

    private void Update()
    {
        // ÿ֡�����������Ƿ���Ч
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
        // ���¹���Ƕȣ��������ת��
        orbitAngle += rotationSpeed * Time.deltaTime;
        UpdateTargetPosition();

        // ƽ���ƶ���Ŀ��λ��
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSmoothing);

        // �����ƶ����ƶ�����Ʈ����
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.Self);
    }

    private void UpdateTargetPosition()
    {
        // ��������ҵĹ��λ��
        float x = playerTransform.position.x + Mathf.Cos(orbitAngle * Mathf.Deg2Rad) * cloudOrbitRadius;
        float z = playerTransform.position.z + Mathf.Sin(orbitAngle * Mathf.Deg2Rad) * cloudOrbitRadius;
        targetPosition = new Vector3(x, playerTransform.position.y + heightAbovePlayer, z);
    }

    private void FacePlayer()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        if (directionToPlayer != Vector3.zero)
        {
            // �����򣺱���Y�ᴹֱ
            Quaternion baseRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);

            // Ӧ�þֲ�ƫ��
            Vector3 offset = randomizeFaceOffset ? randomizedOffset : faceOffsetEuler;
            transform.rotation = baseRotation * Quaternion.Euler(offset);

            // ��ѡ������ˮƽ����ֹ��б���ȣ�
            Vector3 euler = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(offset.x, euler.y, offset.z);
        }
    }

    private void AnimateVertices()
    {
        for (int i = 0; i < originalVertices.Length; i++)
        {
            // ʹ�ö���������ʱ���������������Ķ���
            float timeFactor = Time.time * vertexAnimSpeed + i * 0.5f;
            float noise = Mathf.PerlinNoise(timeFactor, timeFactor * 0.7f);

            // ʹ�����ߵ�������ǿ��
            float curveValue = vertexAnimCurve.Evaluate(noise);

            // ���㶥��ƫ��
            Vector3 offset = UnityEngine.Random.insideUnitSphere * vertexAnimAmplitude * curveValue;

            // Ӧ��ƫ��
            animatedVertices[i] = originalVertices[i] + offset;
        }

        // �������񶥵�
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

        // ��ѡ������ʱ�����͸����
        if (cloudMaterial.HasProperty("_Surface"))
        {
            float alpha = Mathf.Lerp(0.3f, 1f, Mathf.Clamp01(timeOfDay - 0.2f) * 5f);
            Color currentColor = cloudMaterial.GetColor(colorPropertyId);
            cloudMaterial.SetColor(colorPropertyId, new Color(currentColor.r, currentColor.g, currentColor.b, alpha));
        }
    }

    private float GetTimeOfDay()
    {
        // ʹ��ϵͳʱ��
        System.DateTime currentTime = System.DateTime.Now;
        float normalizedTime = (currentTime.Hour * 3600 + currentTime.Minute * 60 + currentTime.Second) / 86400f;

        // ����ʹ�ó����еĵƹ���ת�����ʹ������ҹѭ��ϵͳ��
        if (directionalLight != null)
        {
            normalizedTime = (directionalLight.transform.eulerAngles.x + 180f) / 360f;
        }

        return normalizedTime;
    }
}