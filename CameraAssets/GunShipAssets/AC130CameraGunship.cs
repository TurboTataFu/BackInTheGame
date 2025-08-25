using UnityEngine;

public class AC130CameraGunship : MonoBehaviour
{
    public enum FlightPhase
    {
        Approach,
        Transition,
        Orbiting
    }

    [Header("入场参数")]
    public float approachDistance = 500f;
    public float approachSpeed = 80f;
    public float transitionDuration = 3f;

    [Header("轨道参数")]
    public float orbitRadius = 300f;
    public float orbitSpeed = 15f;
    public float altitude = 150f;

    [Header("飞行姿态")]
    public float bankAngle = 25f;
    public Transform target;

    private float _currentAngle;
    private Vector3 _lastPosition;
    private FlightPhase _currentPhase;
    private Vector3 _entryPoint;
    private float _transitionProgress;

    void Start()
    {
        _lastPosition = transform.position;
        InitializeApproach();
    }

    void InitializeApproach()
    {
        if (target == null) return;

        Vector3 orbitStartPos = CalculateOrbitPosition(0);
        Vector3 approachDirection = (orbitStartPos - target.position).normalized;

        transform.position = orbitStartPos + approachDirection * approachDistance;
        transform.LookAt(orbitStartPos);

        _entryPoint = orbitStartPos;
        _currentPhase = FlightPhase.Approach;
    }

    void Update()
    {
        if (target == null) return;

        switch (_currentPhase)
        {
            case FlightPhase.Approach:
                HandleApproachPhase();
                break;
            case FlightPhase.Transition:
                HandleTransitionPhase();
                break;
            case FlightPhase.Orbiting:
                UpdateOrbitMotion();
                break;
        }

        UpdateRotation();
    }

    void HandleApproachPhase()
    {
        Vector3 newPos = Vector3.MoveTowards(transform.position, _entryPoint, approachSpeed * Time.deltaTime);
        newPos.y = target.position.y + altitude;
        transform.position = newPos;

        if (Vector3.Distance(transform.position, _entryPoint) < orbitRadius * 0.3f)
        {
            _currentPhase = FlightPhase.Transition;
            _transitionProgress = 0f;

            Vector3 toCenter = (target.position - _entryPoint).normalized;
            _currentAngle = Mathf.Atan2(toCenter.z, toCenter.x) * Mathf.Rad2Deg;
        }
    }

    void HandleTransitionPhase()
    {
        _transitionProgress += Time.deltaTime / transitionDuration;

        Vector3 orbitPos = CalculateOrbitPosition(_currentAngle);
        transform.position = Vector3.Lerp(transform.position, orbitPos, _transitionProgress);

        _currentAngle += orbitSpeed * Time.deltaTime * Mathf.Clamp01(_transitionProgress * 2);

        if (_transitionProgress >= 1f) _currentPhase = FlightPhase.Orbiting;
    }

    void UpdateOrbitMotion()
    {
        _currentAngle += orbitSpeed * Time.deltaTime;
        transform.position = CalculateOrbitPosition(_currentAngle);
    }

    Vector3 CalculateOrbitPosition(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(
            target.position.x + orbitRadius * Mathf.Cos(rad),
            target.position.y + altitude,
            target.position.z + orbitRadius * Mathf.Sin(rad)
        );
    }

    void UpdateRotation()
    {
        Vector3 moveDirection = (transform.position - _lastPosition).normalized;
        _lastPosition = transform.position;

        if (moveDirection != Vector3.zero)
        {
            float turnDirection = Vector3.Dot(moveDirection, transform.right);
            float dynamicBank = 0f;

            switch (_currentPhase)
            {
                case FlightPhase.Approach:
                    dynamicBank = bankAngle * Mathf.Sign(turnDirection) * 0.5f;
                    break;
                case FlightPhase.Transition:
                    dynamicBank = bankAngle * Mathf.Sign(turnDirection) * Mathf.Clamp01(_transitionProgress * 2);
                    break;
                default:
                    dynamicBank = bankAngle * Mathf.Sign(turnDirection);
                    break;
            }

            Quaternion targetRot = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0, 0, dynamicBank);
            float rotationSpeed = _currentPhase == FlightPhase.Approach ? 5f : 10f;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, 10f);

        Gizmos.color = Color.cyan;
        const int segments = 30;
        Vector3 prevPos = CalculateOrbitPosition(0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = 360f * i / segments;
            Vector3 newPos = CalculateOrbitPosition(angle);
            Gizmos.DrawLine(prevPos, newPos);
            prevPos = newPos;
        }

        if (Application.isPlaying && _currentPhase == FlightPhase.Approach)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _entryPoint);
            Gizmos.DrawWireSphere(_entryPoint, 5f);
        }
    }
}