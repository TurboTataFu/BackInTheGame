using UnityEngine;

public class LightController : MonoBehaviour
{
    [Header("灯光设置")]
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
        // 获取所有子物体中的灯光组件
        childLights = GetComponentsInChildren<Light>();
        originalIntensities = new float[childLights.Length];

        // 保存初始强度
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

        // F键控制开关
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleLights();
        }

        // 仅在开启状态允许调整强度
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

            // 关闭时重置强度为0，开启时恢复原始强度
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

    // 可选：在编辑器中可视化检测范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
    }
}