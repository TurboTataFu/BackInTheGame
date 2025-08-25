using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCWander : MonoBehaviour
{
    [Header("漫游设置")]
    [Tooltip("随机目标点的生成范围")]
    public float wanderRadius = 10f;

    [Tooltip("到达目标点后停留的最短时间")]
    public float minWaitTime = 1f;

    [Tooltip("到达目标点后停留的最长时间")]
    public float maxWaitTime = 3f;

    [Tooltip("避免往复运动的最小距离（与上一个点的距离）")]
    public float minDistanceFromPrevious = 5f;

    [Header("防卡住设置")]
    [Tooltip("判定为卡住的时间阈值（秒）")]
    public float stuckThreshold = 1f;

    [Tooltip("判定位置变化的最小阈值")]
    public float positionChangeThreshold = 0.1f;

    private NavMeshAgent agent;
    private Vector3 targetPosition;
    private Vector3 previousPosition;
    private Vector3 lastPosition; // 用于检测卡住的上一位置
    private float waitTimer;
    private float stuckTimer; // 卡住检测计时器
    private bool isWaiting;
    private Animator animator;
    // 记录上一帧的动画状态，避免重复设置参数
    private bool wasWalking;
    private bool wasIdling;

    // 定义角色正面方向：Y=0时朝-X方向
    private Vector3 forwardDirection => -transform.right;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        // 禁用自动旋转，手动控制
        agent.updateRotation = false;

        // 初始化旋转（Y=0时朝-X方向）
        SetRotation(0);

        // 初始化动画状态为Idle
        SetAnimationState(isWalking: false);

        // 初始化位置记录
        lastPosition = transform.position;
        previousPosition = transform.position;

        // 初始化第一个目标点
        GenerateNewTargetPosition();
    }

    void Update()
    {
        // 强制限制旋转角度
        ClampRotation();

        // 检查是否在移动，并更新动画状态
        bool isWalking = agent.velocity.sqrMagnitude > 0.1f;
        UpdateAnimationState(isWalking);

        // 防卡住检测逻辑
        CheckStuckStatus(isWalking);

        // 如果正在等待，更新计时器
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= Random.Range(minWaitTime, maxWaitTime))
            {
                isWaiting = false;
                waitTimer = 0;
                GenerateNewTargetPosition();
            }
            return;
        }

        // 检查是否到达目标点
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isWaiting = true;
            previousPosition = targetPosition;
        }
        else if (agent.hasPath)
        {
            // 根据目标方向调整旋转
            AdjustRotationToTarget();
        }
    }

    /// <summary>
    /// 检查NPC是否卡住
    /// </summary>
    private void CheckStuckStatus(bool isWalking)
    {
        // 只有在移动状态下才检测是否卡住
        if (!isWaiting && isWalking)
        {
            // 计算当前位置与上一帧位置的距离
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            // 如果移动距离小于阈值，增加卡住计时器
            if (distanceMoved < positionChangeThreshold)
            {
                stuckTimer += Time.deltaTime;

                // 如果超过卡住阈值，生成新目标点
                if (stuckTimer >= stuckThreshold)
                {
                    Debug.Log("NPC卡住了，正在切换目标点...");
                    GenerateNewTargetPosition();
                    stuckTimer = 0;
                }
            }
            else
            {
                // 如果有明显移动，重置卡住计时器和上一位置
                stuckTimer = 0;
                lastPosition = transform.position;
            }
        }
        else
        {
            // 不在移动状态时重置计时器
            stuckTimer = 0;
            lastPosition = transform.position;
        }
    }

    private void GenerateNewTargetPosition()
    {
        bool validPositionFound = false;
        int maxAttempts = 10;
        int attempts = 0;

        while (!validPositionFound && attempts < maxAttempts)
        {
            attempts++;

            // 在XZ平面上生成随机点
            Vector2 randomDir2D = Random.insideUnitCircle * wanderRadius;
            Vector3 randomDirection = new Vector3(randomDir2D.x, 0, randomDir2D.y) + transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, 1))
            {
                Vector3 newPosition = hit.position;
                newPosition.y = transform.position.y; // 保持Y轴一致

                if (Vector3.Distance(newPosition, previousPosition) >= minDistanceFromPrevious)
                {
                    targetPosition = newPosition;
                    agent.SetDestination(targetPosition);
                    validPositionFound = true;
                    AdjustRotationToTarget();
                }
            }
        }

        if (!validPositionFound)
        {
            Debug.LogWarning("找不到合适的漫游点，放宽距离限制");
            Vector2 randomDir2D = Random.insideUnitCircle * wanderRadius;
            Vector3 randomDirection = new Vector3(randomDir2D.x, 0, randomDir2D.y) + transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, 1))
            {
                targetPosition = hit.position;
                targetPosition.y = transform.position.y;
                agent.SetDestination(targetPosition);
                AdjustRotationToTarget();
            }
        }

        // 重置卡住检测
        stuckTimer = 0;
        lastPosition = transform.position;
    }

    /// <summary>
    /// 调整旋转使角色面向目标，Y=0时朝-X方向，Y=180时朝+X方向
    /// </summary>
    private void AdjustRotationToTarget()
    {
        // 计算目标方向（忽略Y轴）
        Vector3 targetDir = targetPosition - transform.position;
        targetDir.y = 0;

        if (targetDir.sqrMagnitude > 0.1f) // 距离足够时才调整
        {
            // 计算目标在X轴上的相对方向（因为我们主要关注X方向移动）
            float xDir = targetDir.x;

            // 目标在-X方向附近时，设置Y=0（朝-X移动）
            // 目标在+X方向附近时，设置Y=180（朝+X移动）
            if (xDir < 0)
            {
                SetRotation(0); // Y=0朝-X
            }
            else
            {
                SetRotation(180); // Y=180朝+X
            }
        }
    }

    /// <summary>
    /// 设置旋转角度，确保X和Z轴为0
    /// </summary>
    private void SetRotation(float yAngle)
    {
        transform.rotation = Quaternion.Euler(0, yAngle, 0);
    }

    /// <summary>
    /// 强制限制旋转角度
    /// </summary>
    private void ClampRotation()
    {
        Vector3 euler = transform.rotation.eulerAngles;

        // 锁定X和Z轴旋转
        euler.x = 0;
        euler.z = 0;

        // 确保Y轴只能是0或180度
        if (euler.y > 90 && euler.y < 270)
        {
            euler.y = 180;
        }
        else
        {
            euler.y = 0;
        }

        transform.rotation = Quaternion.Euler(euler);
    }

    /// <summary>
    /// 更新动画状态
    /// </summary>
    private void UpdateAnimationState(bool isWalking)
    {
        // 只有当状态发生变化时才更新参数，优化性能
        if (isWalking != wasWalking)
        {
            SetAnimationState(isWalking);
        }
    }

    /// <summary>
    /// 设置动画参数
    /// </summary>
    private void SetAnimationState(bool isWalking)
    {
        if (animator == null) return;

        wasWalking = isWalking;
        wasIdling = !isWalking;

        // 设置动画参数
        animator.SetBool("IsWalk", isWalking);
        animator.SetBool("IsIdle", !isWalking);
    }

    void OnDrawGizmos()
    {
        // 绘制漫游范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        // 绘制到目标点的路径
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.green;
            Vector3[] corners = agent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }

        // 绘制当前目标点
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 0.3f);

        // 绘制角色当前朝向（Y=0时朝-X，Y=180时朝+X）
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, forwardDirection * 2f);
    }
}
