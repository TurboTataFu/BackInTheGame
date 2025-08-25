using UnityEngine;

public class PrometeoTransmissionController : MonoBehaviour
{
    //因为特殊原因，新增的变量不能放在PrometeoCarController脚本，只能放在PrometeoTransmissionController，PrometeoCarController想要使用新的变量只能引用



    [Header("TRANSMISSION SETTINGS")]
    public float[] gearRatios;
    public AnimationCurve[] powerCurves;
    public int currentGear = 1;
    public int maxGear = 5;
    public float shiftRPM = 3000f;
    // 新增变量：标记玩家是否在车内
    public bool isPlayerInVehicle = false;

    //驱动类型枚举
    public enum DriveType { FrontWheelDrive, RearWheelDrive, AllWheelDrive };
    public DriveType driveType = DriveType.AllWheelDrive;

    // 不同驱动类型的转向灵敏度曲线
    public AnimationCurve frontWheelDriveSteeringCurve;
    public AnimationCurve rearWheelDriveSteeringCurve;
    public AnimationCurve allWheelDriveSteeringCurve;

    // 调试参数
    public float gearRatiosDebugMultiplier = 1.0f;
    public float powerCurvesDebugMultiplier = 1.0f;

    [Header("GLOBAL DEBUG SETTINGS")]
    public float globalSpeedMultiplier = 100.0f;
    public float accelerationDebugMultiplier = 1.0f;
    public float decelerationDebugMultiplier = 0.01f;
    public float maxSpeedDebugMultiplier = 1.0f;
    public float steeringDebugMultiplier = 1.0f;
    public float speedDisplayMultiplier = 1.0f;

    // 换挡模式
    public enum ShiftMode { Manual, Automatic, ManualAutomatic };
    public ShiftMode shiftMode = ShiftMode.Automatic;

    // 是否为手动换挡
    public bool isManualShift = false;

    // ESP相关参数
    public bool hasESP = true; // 车辆是否有ESP功能
    public bool isESPEnabled = true; // ESP功能是否开启
    public float espSteeringLimit = 0.5f; // ESP转向限制系数
    public float espSpeedThreshold = 30f; // ESP生效的速度阈值

    // 引用PrometeoCarController
    private PrometeoCarController carController;
    // 引用 PrometeoTransmissionController
    private PrometeoTransmissionController transmissionController;

    private void Start()
    {

        // 自动获取 PrometeoTransmissionController 组件
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

    // 检查传动系统是否有效
    public bool IsTransmissionValid()
    {
        return gearRatios != null && gearRatios.Length > 0 && powerCurves != null && powerCurves.Length > 0;
    }

    // 根据当前RPM换挡
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

    // 手动升挡
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

    // 手动降挡
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

    // 切换手动/自动模式
    public void ToggleShiftMode()
    {
        if (shiftMode == ShiftMode.ManualAutomatic)
        {
            isManualShift = !isManualShift;
        }
    }

    // 获取当前挡位的功率
    public float GetCurrentGearPower(float rpm)
    {
        if (currentGear > 0 && currentGear <= powerCurves.Length)
        {
            return powerCurves[currentGear - 1].Evaluate(rpm) * powerCurvesDebugMultiplier;
        }
        return 0f;
    }

    // 获取当前挡位的传动比
    public float GetCurrentGearRatio()
    {
        if (currentGear > 0 && currentGear <= gearRatios.Length)
        {
            return gearRatios[currentGear - 1] * gearRatiosDebugMultiplier;
        }
        return 0f;
    }
}