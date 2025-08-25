using UnityEngine;

public class LightController : MonoBehaviour
{
    [Header("�ƹ�����")]
    public float minIntensity = 0f;
    public float maxIntensity = 5f;
    public float adjustSpeed = 2f;
    public float detectionDistance = 5f;

    private Light[] childLights;
    private float[] originalIntensities;
    private bool lightEnabled = true;
    private bool isLookingAtParent = false;

    void Awake()
    {
        // ��ȡ�����������еĵƹ����
        childLights = GetComponentsInChildren<Light>();
        originalIntensities = new float[childLights.Length];

        // �����ʼǿ��
        for (int i = 0; i < childLights.Length; i++)
        {
            originalIntensities[i] = childLights[i].intensity;
        }
    }

    void Update()
    {
        CheckLookingAtParent();
        HandleLightControl();
    }

    void CheckLookingAtParent()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        isLookingAtParent = Physics.Raycast(ray, out hit, detectionDistance)
                            && hit.collider.gameObject == gameObject;
    }

    void HandleLightControl()
    {
        if (!isLookingAtParent) return;

        // F�����ƿ���
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleLights();
        }

        // ���ڿ���״̬�������ǿ��
        if (lightEnabled)
        {
            AdjustIntensityWithMouseWheel();
        }
    }

    void ToggleLights()
    {
        lightEnabled = !lightEnabled;

        foreach (Light light in childLights)
        {
            light.enabled = lightEnabled;

            // �ر�ʱ����ǿ��Ϊ0������ʱ�ָ�ԭʼǿ��
            light.intensity = lightEnabled ? originalIntensities[System.Array.IndexOf(childLights, light)] : 0f;
        }
    }

    void AdjustIntensityWithMouseWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            foreach (Light light in childLights)
            {
                float newIntensity = light.intensity + scroll * adjustSpeed;
                newIntensity = Mathf.Clamp(newIntensity, minIntensity, maxIntensity);
                light.intensity = newIntensity;
            }
        }
    }

    // ��ѡ���ڱ༭���п��ӻ���ⷶΧ
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
    }
}