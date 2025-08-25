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

    // ����ϵͳ�ӿ�
    public void RequestPrecisionStrike(Vector3 target) { /* ��ȷ����߼� */ }
    public void RequestAreaSuppression(Vector3 target) { /* ����ѹ���߼� */ }
    public void RequestThermalScan(Vector3 target) { /* �ȳ���ɨ���߼� */ }
}