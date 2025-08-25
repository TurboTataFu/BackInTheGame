using UnityEngine;

[System.Serializable]
public class AC130Gunship : MonoBehaviour
{
    public float bankSpeed = 15f;
    public float maxBankAngle = 45f;

    [HideInInspector] public float CurrentBankAngle;

    public void ApplyBank(float input)
    {
        CurrentBankAngle += input * bankSpeed * Time.deltaTime;
        CurrentBankAngle = Mathf.Clamp(CurrentBankAngle, -maxBankAngle, maxBankAngle);

        transform.rotation = Quaternion.Euler(
            0,
            transform.eulerAngles.y,
            CurrentBankAngle
        );
    }

    // 武器系统接口
    public void RequestPrecisionStrike(Vector3 target) { /* 精确打击逻辑 */ }
    public void RequestAreaSuppression(Vector3 target) { /* 区域压制逻辑 */ }
    public void RequestThermalScan(Vector3 target) { /* 热成像扫描逻辑 */ }
}