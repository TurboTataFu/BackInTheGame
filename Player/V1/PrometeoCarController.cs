using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrometeoCarController : MonoBehaviour
{

    [Header("GLOBAL SPEED SETTINGS")]
    private float AppliedMaxSpeed => maxSpeed * transmissionController.globalSpeedMultiplier * transmissionController.maxSpeedDebugMultiplier;
    private float AppliedMaxReverseSpeed => maxReverseSpeed * transmissionController.globalSpeedMultiplier * transmissionController.maxSpeedDebugMultiplier;
    private float AppliedAccelerationMultiplier => accelerationMultiplier * transmissionController.globalSpeedMultiplier * transmissionController.accelerationDebugMultiplier;
    private float AppliedDecelerationMultiplier => decelerationMultiplier * transmissionController.globalSpeedMultiplier * transmissionController.decelerationDebugMultiplier;

    private float averageSpeed;
    private float averageRPM;
    private int currentGear;

    private List<float> speedSamples = new List<float>();
    private List<float> rpmSamples = new List<float>();

    private const int sampleCount = 10; // 采样数量


    [Space(20)]
    [Space(10)]
    public int maxSpeed = 90;
    public int maxReverseSpeed = 45;
    public int accelerationMultiplier = 2;
    [Space(10)]
    public int maxSteeringAngle = 27;
    public float steeringSpeed = 0.5f;
    [Space(10)]
    public int brakeForce = 350;
    public int decelerationMultiplier = 2;
    public int handbrakeDriftMultiplier = 5;
    [Space(10)]
    public Vector3 bodyMassCenter;

    // WHEELS
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    [Space(10)]
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    [Space(10)]
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    [Space(10)]
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;

    // PARTICLE SYSTEMS
    [Space(20)]
    [Space(10)]
    public bool useEffects = false;
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;
    [Space(10)]
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;

    // SPEED TEXT (UI)
    [Space(20)]
    [Space(10)]
    public bool useUI = false;
    public Text carSpeedText;

    // SOUNDS
    [Space(20)]
    [Space(10)]
    public bool useSounds = false;
    public AudioSource carEngineSound;
    public AudioSource tireScreechSound;
    float initialCarEngineSoundPitch;

    // CONTROLS
    [Space(20)]
    [Space(10)]
    public bool useTouchControls = false;
    public GameObject throttleButton;
    PrometeoTouchInput throttlePTI;
    public GameObject reverseButton;
    PrometeoTouchInput reversePTI;
    public GameObject turnRightButton;
    PrometeoTouchInput turnRightPTI;
    public GameObject turnLeftButton;
    PrometeoTouchInput turnLeftPTI;
    public GameObject handbrakeButton;
    PrometeoTouchInput handbrakePTI;

    // CAR DATA
    [HideInInspector]
    public float carSpeed;
    [HideInInspector]
    public bool isDrifting;
    [HideInInspector]
    public bool isTractionLocked;

    // PRIVATE VARIABLES
    Rigidbody carRigidbody;
    float steeringAxis;
    float throttleAxis;
    float driftingAxis;
    float localVelocityZ;
    float localVelocityX;
    bool deceleratingCar;
    bool touchControlsSetup = false;
    WheelFrictionCurve FLwheelFriction;
    float FLWextremumSlip;
    WheelFrictionCurve FRwheelFriction;
    float FRWextremumSlip;
    WheelFrictionCurve RLwheelFriction;
    float RLWextremumSlip;
    WheelFrictionCurve RRwheelFriction;
    float RRWextremumSlip;

    // 引用传动系统控制器
    public PrometeoTransmissionController transmissionController;

    // Start is called before the first frame update
    void Start()
    {

        carRigidbody = gameObject.GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;

        FLwheelFriction = new WheelFrictionCurve();
        FLwheelFriction.extremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLWextremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLwheelFriction.extremumValue = frontLeftCollider.sidewaysFriction.extremumValue;
        FLwheelFriction.asymptoteSlip = frontLeftCollider.sidewaysFriction.asymptoteSlip;
        FLwheelFriction.asymptoteValue = frontLeftCollider.sidewaysFriction.asymptoteValue;
        FLwheelFriction.stiffness = frontLeftCollider.sidewaysFriction.stiffness;
        FRwheelFriction = new WheelFrictionCurve();
        FRwheelFriction.extremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRWextremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRwheelFriction.extremumValue = frontRightCollider.sidewaysFriction.extremumValue;
        FRwheelFriction.asymptoteSlip = frontRightCollider.sidewaysFriction.asymptoteSlip;
        FRwheelFriction.asymptoteValue = frontRightCollider.sidewaysFriction.asymptoteValue;
        FRwheelFriction.stiffness = frontRightCollider.sidewaysFriction.stiffness;
        RLwheelFriction = new WheelFrictionCurve();
        RLwheelFriction.extremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLWextremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLwheelFriction.extremumValue = rearLeftCollider.sidewaysFriction.extremumValue;
        RLwheelFriction.asymptoteSlip = rearLeftCollider.sidewaysFriction.asymptoteSlip;
        RLwheelFriction.asymptoteValue = rearLeftCollider.sidewaysFriction.asymptoteValue;
        RLwheelFriction.stiffness = rearLeftCollider.sidewaysFriction.stiffness;
        RRwheelFriction = new WheelFrictionCurve();
        RRwheelFriction.extremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRWextremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRwheelFriction.extremumValue = rearRightCollider.sidewaysFriction.extremumValue;
        RRwheelFriction.asymptoteSlip = rearRightCollider.sidewaysFriction.asymptoteSlip;
        RRwheelFriction.asymptoteValue = rearRightCollider.sidewaysFriction.asymptoteValue;
        RRwheelFriction.stiffness = rearRightCollider.sidewaysFriction.stiffness;

        if (carEngineSound != null)
        {
            initialCarEngineSoundPitch = carEngineSound.pitch;
        }

        if (useUI)
        {
            InvokeRepeating("CarSpeedUI", 0f, 0.1f);
        }
        else if (!useUI)
        {
            if (carSpeedText != null)
            {
                carSpeedText.text = "0";
            }
        }


        // 自动获取PrometeoTransmissionController组件
        transmissionController = gameObject.GetComponent<PrometeoTransmissionController>();
        if (transmissionController == null)
        {
            Debug.LogError("PrometeoTransmissionController component not found on the same GameObject.");
            return;
        }

        // 检查传动系统是否有效
        if (!transmissionController.IsTransmissionValid())
        {
            Debug.LogError("powerCurves or gearRatios array is empty. Please initialize them in the Inspector.");
            return;
        }

        if (useSounds)
        {
            InvokeRepeating("CarSounds", 0f, 0.1f);
        }
        else if (!useSounds)
        {
            if (carEngineSound != null)
            {
                carEngineSound.Stop();
            }
            if (tireScreechSound != null)
            {
                tireScreechSound.Stop();
            }
        }

        if (!useEffects)
        {
            if (RLWParticleSystem != null)
            {
                RLWParticleSystem.Stop();
            }
            if (RRWParticleSystem != null)
            {
                RRWParticleSystem.Stop();
            }
            if (RLWTireSkid != null)
            {
                RLWTireSkid.emitting = false;
            }
            if (RRWTireSkid != null)
            {
                RRWTireSkid.emitting = false;
            }
        }

        if (useTouchControls)
        {
            if (throttleButton != null && reverseButton != null &&
                turnRightButton != null && turnLeftButton != null
                && handbrakeButton != null)
            {
                throttlePTI = throttleButton.GetComponent<PrometeoTouchInput>();
                reversePTI = reverseButton.GetComponent<PrometeoTouchInput>();
                turnLeftPTI = turnLeftButton.GetComponent<PrometeoTouchInput>();
                turnRightPTI = turnRightButton.GetComponent<PrometeoTouchInput>();
                handbrakePTI = handbrakeButton.GetComponent<PrometeoTouchInput>();
                touchControlsSetup = true;
            }
            else
            {
                String ex = "Touch controls are not completely set up. You must drag and drop your scene buttons in the" +
                            " PrometeoCarController component.";
                Debug.LogWarning(ex);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        float appliedMaxSpeed = AppliedMaxSpeed;
        float appliedMaxReverseSpeed = AppliedMaxReverseSpeed;
        float appliedAcceleration = AppliedAccelerationMultiplier;
        float appliedDeceleration = AppliedDecelerationMultiplier;

        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        localVelocityX = transform.InverseTransformDirection(carRigidbody.linearVelocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

        // 检查玩家是否在车内
        if (!transmissionController.isPlayerInVehicle)
        {
            // 如果玩家不在车内，关闭车辆控制
            ThrottleOff();
            CancelInvoke("DecelerateCar");
            deceleratingCar = false;
            return;
        }

        // 检查轮子是否着地
        bool isGrounded = frontLeftCollider.isGrounded || frontRightCollider.isGrounded || rearLeftCollider.isGrounded || rearRightCollider.isGrounded;

        // 换挡逻辑
        float currentRPM = frontLeftCollider.rpm;
        transmissionController.ShiftGears(currentRPM);

        // 手动换挡操作
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            transmissionController.ManualShiftUp();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            transmissionController.ManualShiftDown();
        }

        // 切换手动/自动模式
        if (Input.GetKeyDown(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.V))
        {
            transmissionController.ToggleShiftMode();
        }

        // 只有轮子着地时才处理输入
        if (isGrounded)
        {
            // 触摸控制逻辑
            if (useTouchControls && touchControlsSetup)
            {
                if (throttlePTI.buttonPressed)
                {
                    CancelInvoke("DecelerateCar");
                    deceleratingCar = false;
                    GoForward();
                }
                if (reversePTI.buttonPressed)
                {
                    CancelInvoke("DecelerateCar");
                    deceleratingCar = false;
                    GoReverse();
                }

                if (turnLeftPTI.buttonPressed)
                {
                    TurnLeft();
                }
                if (turnRightPTI.buttonPressed)
                {
                    TurnRight();
                }

                if (handbrakePTI.buttonPressed)
                {
                    CancelInvoke("DecelerateCar");
                    deceleratingCar = false;
                    Handbrake();
                }
            }

            // 键盘控制逻辑
            if (Input.GetKey(KeyCode.W))
            {
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                if (transmissionController.currentGear == 0)
                {
                    if (transmissionController.shiftMode == PrometeoTransmissionController.ShiftMode.Manual ||
                        (transmissionController.shiftMode == PrometeoTransmissionController.ShiftMode.ManualAutomatic && transmissionController.isManualShift))
                    {
                        GoReverse();
                    }
                    else
                    {
                        GoForward();
                    }
                }
                else
                {
                    GoForward();
                }
            }
            if (Input.GetKey(KeyCode.S))
            {
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                if (transmissionController.currentGear == 0)
                {
                    if (transmissionController.shiftMode == PrometeoTransmissionController.ShiftMode.Manual ||
                        (transmissionController.shiftMode == PrometeoTransmissionController.ShiftMode.ManualAutomatic && transmissionController.isManualShift))
                    {
                        Brakes();
                    }
                    else
                    {
                        GoReverse();
                    }
                }
                else
                {
                    if (transmissionController.shiftMode == PrometeoTransmissionController.ShiftMode.Manual ||
                        (transmissionController.shiftMode == PrometeoTransmissionController.ShiftMode.ManualAutomatic && transmissionController.isManualShift))
                    {
                        Brakes();
                    }
                    else
                    {
                        GoReverse();
                    }
                }
            }
            if (Input.GetKey(KeyCode.A))
            {
                TurnLeft();
            }
            if (Input.GetKey(KeyCode.D))
            {
                TurnRight();
            }
            if (Input.GetKey(KeyCode.Space))
            {
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                Handbrake();
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                RecoverTraction();
            }

            if ((!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W)))
            {
                ThrottleOff();
            }
            if ((!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W)) && !Input.GetKey(KeyCode.Space) && !deceleratingCar)
            {
                InvokeRepeating("DecelerateCar", 0f, 0.1f);
                deceleratingCar = true;
            }
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && steeringAxis != 0f)
            {
                ResetSteeringAngle();
            }
        }
        else
        {
            // 在空中时，关闭所有输入控制
            ThrottleOff();
            CancelInvoke("DecelerateCar");
            deceleratingCar = false;
        }

        // 记录车速和转速
        speedSamples.Add(carSpeed);
        rpmSamples.Add(frontLeftCollider.rpm);

        // 保持采样数量不超过指定值
        if (speedSamples.Count > sampleCount)
        {
            speedSamples.RemoveAt(0);
        }
        if (rpmSamples.Count > sampleCount)
        {
            rpmSamples.RemoveAt(0);
        }

        // 计算平均值
        averageSpeed = CalculateAverage(speedSamples);
        averageRPM = CalculateAverage(rpmSamples);
        currentGear = transmissionController.currentGear;

        // 显示数据
        Debug.Log($"车速: {averageSpeed:F2} km/h, 转速: {averageRPM:F2} RPM, 挡位: {currentGear}");


        AnimateWheelMeshes();

    }
    private float CalculateAverage(List<float> samples)
    {
        if (samples.Count == 0) return 0f;

        float sum = 0f;
        foreach (float sample in samples)
        {
            sum += sample;
        }
        return sum / samples.Count;
    }

    // This method apply positive torque to the wheels in order to go forward.
    public void GoForward()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }

        throttleAxis = throttleAxis + (Time.deltaTime * 3f);
        if (throttleAxis > 1f)
        {
            throttleAxis = 1f;
        }

        if (transmissionController.IsTransmissionValid())
        {
            // 获取当前挡位的功率和传动比
            float power = transmissionController.GetCurrentGearPower(frontLeftCollider.rpm);
            float gearRatio = transmissionController.GetCurrentGearRatio();
            float appliedAcceleration = power * gearRatio * throttleAxis;

            if (Mathf.RoundToInt(carSpeed) < AppliedMaxSpeed)
            {
                frontLeftCollider.brakeTorque = 0;
                frontLeftCollider.motorTorque = appliedAcceleration;
                frontRightCollider.brakeTorque = 0;
                frontRightCollider.motorTorque = appliedAcceleration;
                rearLeftCollider.brakeTorque = 0;
                rearLeftCollider.motorTorque = appliedAcceleration;
                rearRightCollider.brakeTorque = 0;
                rearRightCollider.motorTorque = appliedAcceleration;
            }
        }
        else
        {
            Debug.LogError($"Invalid transmission settings. powerCurves length: {transmissionController.powerCurves.Length}, gearRatios length: {transmissionController.gearRatios.Length}");
        }
    }

    // This method apply negative torque to the wheels in order to go backwards.
    public void GoReverse()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }

        throttleAxis = throttleAxis - (Time.deltaTime * 3f);
        if (throttleAxis < -1f)
        {
            throttleAxis = -1f;
        }

        if (transmissionController.IsTransmissionValid())
        {
            if (localVelocityZ > 1f)
            {
                Brakes();
            }
            else
            {
                // 获取当前挡位的功率和传动比
                float power = transmissionController.powerCurves[0].Evaluate(frontLeftCollider.rpm);
                float gearRatio = transmissionController.gearRatios[0];
                float appliedAcceleration = power * gearRatio * throttleAxis;

                if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < AppliedMaxReverseSpeed)
                {
                    frontLeftCollider.brakeTorque = 0;
                    frontLeftCollider.motorTorque = appliedAcceleration;
                    frontRightCollider.brakeTorque = 0;
                    frontRightCollider.motorTorque = appliedAcceleration;
                    rearLeftCollider.brakeTorque = 0;
                    rearLeftCollider.motorTorque = appliedAcceleration;
                    rearRightCollider.brakeTorque = 0;
                    rearRightCollider.motorTorque = appliedAcceleration;
                }
                else
                {
                    frontLeftCollider.motorTorque = 0;
                    frontRightCollider.motorTorque = 0;
                    rearLeftCollider.motorTorque = 0;
                    rearRightCollider.motorTorque = 0;
                }
            }
        }
        else
        {
            Debug.LogError($"Invalid transmission settings. powerCurves length: {transmissionController.powerCurves.Length}, gearRatios length: {transmissionController.gearRatios.Length}");
        }
    }

    // The following method turns the front car wheels to the left. The speed of this movement will depend on the steeringSpeed variable.
    public void TurnLeft()
    {

        float steeringLimit = 1f;
        if (transmissionController.hasESP && transmissionController.isESPEnabled && carSpeed > transmissionController.espSpeedThreshold)
        {
            steeringLimit = transmissionController.espSteeringLimit;
        }

        steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed * transmissionController.steeringDebugMultiplier * steeringLimit);
        if (steeringAxis < -1f)
        {
            steeringAxis = -1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    //The following method turns the front car wheels to the right. The speed of this movement will depend on the steeringSpeed variable.
    public void TurnRight()
    {
        float steeringLimit = 1f;
        if (transmissionController.hasESP && transmissionController.isESPEnabled && carSpeed > transmissionController.espSpeedThreshold)
        {
            steeringLimit = transmissionController.espSteeringLimit;
        }

        steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed * transmissionController.steeringDebugMultiplier * steeringLimit);
        if (steeringAxis > 1f)
        {
            steeringAxis = 1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    //The following method takes the front car wheels to their default position (rotation = 0). The speed of this movement will depend
    // on the steeringSpeed variable.
    public void ResetSteeringAngle()
    {
        if (steeringAxis < 0f)
        {
            steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        }
        else if (steeringAxis > 0f)
        {
            steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
        }
        if (Mathf.Abs(frontLeftCollider.steerAngle) < 1f)
        {
            steeringAxis = 0f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    // This method matches both the position and rotation of the WheelColliders with the WheelMeshes.
    void AnimateWheelMeshes()
    {
        try
        {
            Quaternion FLWRotation;
            Vector3 FLWPosition;
            frontLeftCollider.GetWorldPose(out FLWPosition, out FLWRotation);
            frontLeftMesh.transform.position = FLWPosition;
            frontLeftMesh.transform.rotation = FLWRotation;

            Quaternion FRWRotation;
            Vector3 FRWPosition;
            frontRightCollider.GetWorldPose(out FRWPosition, out FRWRotation);
            frontRightMesh.transform.position = FRWPosition;
            frontRightMesh.transform.rotation = FRWRotation;

            Quaternion RLWRotation;
            Vector3 RLWPosition;
            rearLeftCollider.GetWorldPose(out RLWPosition, out RLWRotation);
            rearLeftMesh.transform.position = RLWPosition;
            rearLeftMesh.transform.rotation = RLWRotation;

            Quaternion RRWRotation;
            Vector3 RRWPosition;
            rearRightCollider.GetWorldPose(out RRWPosition, out RRWRotation);
            rearRightMesh.transform.position = RRWPosition;
            rearRightMesh.transform.rotation = RRWRotation;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    // This method converts the car speed data from float to string, and then set the text of the UI carSpeedText with this value.
    public void CarSpeedUI()
    {
        if (useUI)
        {
            if (carSpeedText != null)
            {
                carSpeedText.text = Mathf.RoundToInt(carSpeed * transmissionController.speedDisplayMultiplier).ToString();
            }
        }
    }

    public void ThrottleOff()
    {
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
    }

    // The following method decelerates the speed of the car according to the decelerationMultiplier variable, where
    // 1 is the slowest and 10 is the fastest deceleration. This method is called by the function InvokeRepeating,
    // usually every 0.1f when the user is not pressing W (throttle), S (reverse) or Space bar (handbrake).
    public void DecelerateCar()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }

        if (throttleAxis != 0f)
        {
            if (throttleAxis > 0f)
            {
                throttleAxis = throttleAxis - (Time.deltaTime * 10f);
            }
            else if (throttleAxis < 0f)
            {
                throttleAxis = throttleAxis + (Time.deltaTime * 10f);
            }

            if (Mathf.Abs(throttleAxis) < 0.15f)
            {
                throttleAxis = 0f;
            }
        }

        float appliedDeceleration = AppliedDecelerationMultiplier * transmissionController.decelerationDebugMultiplier;
        carRigidbody.linearVelocity = carRigidbody.linearVelocity * (1f / (1f + (0.001f * appliedDeceleration)));

        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;

        if (carRigidbody.linearVelocity.magnitude < 0.25f)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            CancelInvoke("DecelerateCar");
        }
    }

    // 该方法根据用户提供的动态刹车力实现动态刹车
    public void Brakes()
    {
        // 速度动态刹车力
        float dynamicBrakeForce = brakeForce * (carRigidbody.linearVelocity.magnitude / AppliedMaxSpeed);
        frontLeftCollider.brakeTorque = dynamicBrakeForce;
        frontRightCollider.brakeTorque = dynamicBrakeForce;
        rearLeftCollider.brakeTorque = dynamicBrakeForce;
        rearRightCollider.brakeTorque = dynamicBrakeForce;
    }

    // This function is used to make the car lose traction. By using this, the car will start drifting. The amount of traction lost
    // will depend on the handbrakeDriftMultiplier variable. If this value is small, then the car will not drift too much, but if
    // it is high, then you could make the car to feel like going on ice.
    public void Handbrake()
    {
        CancelInvoke("RecoverTraction");
        driftingAxis = driftingAxis + (Time.deltaTime);
        float secureStartingPoint = driftingAxis * FLWextremumSlip * handbrakeDriftMultiplier;

        if (secureStartingPoint < FLWextremumSlip)
        {
            driftingAxis = FLWextremumSlip / (FLWextremumSlip * handbrakeDriftMultiplier);
        }
        if (driftingAxis > 1f)
        {
            driftingAxis = 1f;
        }

        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
        }
        else
        {
            isDrifting = false;
        }

        if (driftingAxis < 1f)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRwheelFriction;
        }

        isTractionLocked = true;
        DriftCarPS();
    }

    // This function is used to emit both the particle systems of the tires' smoke and the trail renderers of the tire skids
    // depending on the value of the bool variables 'isDrifting' and 'isTractionLocked'.
    public void DriftCarPS()
    {
        if (useEffects)
        {
            try
            {
                if (isDrifting)
                {
                    RLWParticleSystem.Play();
                    RRWParticleSystem.Play();
                }
                else if (!isDrifting)
                {
                    RLWParticleSystem.Stop();
                    RRWParticleSystem.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }

            try
            {
                if ((isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f)
                {
                    RLWTireSkid.emitting = true;
                    RRWTireSkid.emitting = true;
                }
                else
                {
                    RLWTireSkid.emitting = false;
                    RRWTireSkid.emitting = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
    }

    // This function is used to recover the traction of the car when the user has stopped using the car's handbrake.
    public void RecoverTraction()
    {
        isTractionLocked = false;
        driftingAxis = driftingAxis - (Time.deltaTime / 1.5f);
        if (driftingAxis < 0f)
        {
            driftingAxis = 0f;
        }

        if (FLwheelFriction.extremumSlip > FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRwheelFriction;
        }
    }

    public void CarSounds()
    {
        if (useSounds)
        {
            try
            {
                if (carEngineSound != null)
                {
                    float engineSoundPitch = initialCarEngineSoundPitch + (Mathf.Abs(carRigidbody.linearVelocity.magnitude) / 25f);
                    carEngineSound.pitch = engineSoundPitch;
                }

                if ((isDrifting) || (isTractionLocked && Mathf.Abs(carSpeed) > 12f))
                {
                    if (!tireScreechSound.isPlaying)
                    {
                        tireScreechSound.Play();
                    }
                }
                else if ((!isDrifting) && (!isTractionLocked || Mathf.Abs(carSpeed) < 12f))
                {
                    tireScreechSound.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        else
        {
            if (carEngineSound != null && carEngineSound.isPlaying)
            {
                carEngineSound.Stop();
            }
            if (tireScreechSound != null && tireScreechSound.isPlaying)
            {
                tireScreechSound.Stop();
            }
        }
    }


    void OnDisable()
    {
        CancelInvoke("CarSpeedUI");
        CancelInvoke("CarSounds");
        CancelInvoke("DecelerateCar");
        CancelInvoke("RecoverTraction");

        if (useEffects)
        {
            if (RLWParticleSystem != null) RLWParticleSystem.Stop();
            if (RRWParticleSystem != null) RRWParticleSystem.Stop();
            if (RLWTireSkid != null) RLWTireSkid.emitting = false;
            if (RRWTireSkid != null) RRWTireSkid.emitting = false;
        }

        if (useSounds)
        {
            if (carEngineSound != null) carEngineSound.Stop();
            if (tireScreechSound != null) tireScreechSound.Stop();
        }
    }
}