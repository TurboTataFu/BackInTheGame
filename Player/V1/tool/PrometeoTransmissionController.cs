using UnityEngine;

public class PrometeoTransmissionController : MonoBehaviour
{
    //��Ϊ����ԭ�������ı������ܷ���PrometeoCarController�ű���ֻ�ܷ���PrometeoTransmissionController��PrometeoCarController��Ҫʹ���µı���ֻ������



    [Header("TRANSMISSION SETTINGS")]
    public float[] gearRatios;
    public AnimationCurve[] powerCurves;
    public int currentGear = 1;
    public int maxGear = 5;
    public float shiftRPM = 3000f;
    // �����������������Ƿ��ڳ���
    public bool isPlayerInVehicle = false;

    //��������ö��
    public enum DriveType { FrontWheelDrive, RearWheelDrive, AllWheelDrive };
    public DriveType driveType = DriveType.AllWheelDrive;

    // ��ͬ�������͵�ת������������
    public AnimationCurve frontWheelDriveSteeringCurve;
    public AnimationCurve rearWheelDriveSteeringCurve;
    public AnimationCurve allWheelDriveSteeringCurve;

    // ���Բ���
    public float gearRatiosDebugMultiplier = 1.0f;
    public float powerCurvesDebugMultiplier = 1.0f;

    [Header("GLOBAL DEBUG SETTINGS")]
    public float globalSpeedMultiplier = 100.0f;
    public float accelerationDebugMultiplier = 1.0f;
    public float decelerationDebugMultiplier = 0.01f;
    public float maxSpeedDebugMultiplier = 1.0f;
    public float steeringDebugMultiplier = 1.0f;
    public float speedDisplayMultiplier = 1.0f;

    // ����ģʽ
    public enum ShiftMode { Manual, Automatic, ManualAutomatic };
    public ShiftMode shiftMode = ShiftMode.Automatic;

    // �Ƿ�Ϊ�ֶ�����
    public bool isManualShift = false;

    // ESP��ز���
    public bool hasESP = true; // �����Ƿ���ESP����
    public bool isESPEnabled = true; // ESP�����Ƿ���
    public float espSteeringLimit = 0.5f; // ESPת������ϵ��
    public float espSpeedThreshold = 30f; // ESP��Ч���ٶ���ֵ

    // ����PrometeoCarController
    private PrometeoCarController carController;
    // ���� PrometeoTransmissionController
    private PrometeoTransmissionController transmissionController;

    private void Start()
    {

        // �Զ���ȡ PrometeoTransmissionController ���
        transmissionController = gameObject.GetComponent<PrometeoTransmissionController>();
        if (transmissionController == null)
        {
            Debug.LogError("PrometeoTransmissionController component not found on the same GameObject.");
            return;
        }

        carController = GetComponent<PrometeoCarController>();
        if (carController == null)
        {
            Debug.LogError("PrometeoCarController component not found on the same GameObject.");
        }
    }

    // ��鴫��ϵͳ�Ƿ���Ч
    public bool IsTransmissionValid()
    {
        return gearRatios != null && gearRatios.Length > 0 && powerCurves != null && powerCurves.Length > 0;
    }

    // ���ݵ�ǰRPM����
    public void ShiftGears(float currentRPM)
    {
        if (shiftMode == ShiftMode.Automatic || (shiftMode == ShiftMode.ManualAutomatic && !isManualShift))
        {
            if (currentRPM > shiftRPM && currentGear < maxGear && currentGear <= powerCurves.Length && currentGear <= gearRatios.Length)
            {
                currentGear++;
            }
        }
    }

    // �ֶ�����
    public void ManualShiftUp()
    {
        if (shiftMode == ShiftMode.Manual || (shiftMode == ShiftMode.ManualAutomatic && isManualShift))
        {
            if (currentGear < maxGear && currentGear <= powerCurves.Length && currentGear <= gearRatios.Length)
            {
                currentGear++;
            }
        }
    }

    // �ֶ�����
    public void ManualShiftDown()
    {
        if (shiftMode == ShiftMode.Manual || (shiftMode == ShiftMode.ManualAutomatic && isManualShift))
        {
            if (currentGear > 0)
            {
                currentGear--;
            }
        }
    }

    // �л��ֶ�/�Զ�ģʽ
    public void ToggleShiftMode()
    {
        if (shiftMode == ShiftMode.ManualAutomatic)
        {
            isManualShift = !isManualShift;
        }
    }

    // ��ȡ��ǰ��λ�Ĺ���
    public float GetCurrentGearPower(float rpm)
    {
        if (currentGear > 0 && currentGear <= powerCurves.Length)
        {
            return powerCurves[currentGear - 1].Evaluate(rpm) * powerCurvesDebugMultiplier;
        }
        return 0f;
    }

    // ��ȡ��ǰ��λ�Ĵ�����
    public float GetCurrentGearRatio()
    {
        if (currentGear > 0 && currentGear <= gearRatios.Length)
        {
            return gearRatios[currentGear - 1] * gearRatiosDebugMultiplier;
        }
        return 0f;
    }
}