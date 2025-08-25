using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 快捷键管理系统，用于绑定快捷键控制对象及其子级的激活状态
/// </summary>
public class ShortcutManager : MonoBehaviour
{
    [Header("快捷键配置列表")]
    [Tooltip("在这里配置所有快捷键与目标对象的映射关系")]
    public List<ShortcutConfig> shortcutConfigs = new List<ShortcutConfig>();

    // 存储快捷键与对应操作的字典，用于快速查找
    private Dictionary<KeyCode, ShortcutAction> _shortcutActions = new Dictionary<KeyCode, ShortcutAction>();

    // 单例实例
    public static ShortcutManager Instance { get; private set; }

    private void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeShortcuts();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 检测所有注册的快捷键
        CheckShortcuts();
    }

    /// <summary>
    /// 初始化快捷键配置
    /// </summary>
    private void InitializeShortcuts()
    {
        _shortcutActions.Clear();

        foreach (var config in shortcutConfigs)
        {
            if (config.targetObject == null)
            {
                Debug.LogWarning($"快捷键配置错误：目标对象未指定 - 快捷键: {config.keyCode}");
                continue;
            }

            if (_shortcutActions.ContainsKey(config.keyCode))
            {
                Debug.LogWarning($"快捷键冲突：{config.keyCode} 已被多次绑定");
                continue;
            }

            // 记录目标对象及其所有子级的初始状态
            var initialStates = new Dictionary<GameObject, bool>();
            RecordHierarchyActiveStates(config.targetObject, initialStates);

            // 创建快捷键动作并添加到字典
            _shortcutActions.Add(config.keyCode, new ShortcutAction
            {
                targetObject = config.targetObject,
                modifierKey = config.modifierKey,
                actionType = config.actionType,
                initialStates = initialStates, // 存储整个层级的初始状态
                initialParentState = config.targetObject.activeSelf
            });
        }
    }

    /// <summary>
    /// 递归记录对象及其子级的激活状态
    /// </summary>
    private void RecordHierarchyActiveStates(GameObject root, Dictionary<GameObject, bool> states)
    {
        states[root] = root.activeSelf;

        foreach (Transform child in root.transform)
        {
            RecordHierarchyActiveStates(child.gameObject, states);
        }
    }

    /// <summary>
    /// 检测快捷键输入
    /// </summary>
    private void CheckShortcuts()
    {
        foreach (var shortcut in _shortcutActions)
        {
            KeyCode key = shortcut.Key;
            ShortcutAction action = shortcut.Value;

            // 检查是否按下了快捷键（考虑修饰键）
            bool isModifierPressed = true;

            // 检查修饰键是否按下
            if (action.modifierKey == ModifierKey.Ctrl && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                isModifierPressed = false;
            if (action.modifierKey == ModifierKey.Alt && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
                isModifierPressed = false;
            if (action.modifierKey == ModifierKey.Shift && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                isModifierPressed = false;

            // 如果需要修饰键但未按下，则跳过
            if (action.modifierKey != ModifierKey.None && !isModifierPressed)
                continue;

            // 处理按下事件
            if (Input.GetKeyDown(key))
            {
                HandleShortcutPress(action);
            }

            // 处理按住/释放事件（仅适用于HoldToggle模式）
            if (action.actionType == ActionType.HoldToggle)
            {
                if (Input.GetKey(key))
                {
                    SetHierarchyActive(action.targetObject, true);
                }
                else
                {
                    SetHierarchyActive(action.targetObject, false);
                }
            }
        }
    }

    /// <summary>
    /// 处理快捷键按下事件
    /// </summary>
    private void HandleShortcutPress(ShortcutAction action)
    {
        switch (action.actionType)
        {
            case ActionType.Toggle:
                // 切换整个层级的激活状态
                bool newState = !action.targetObject.activeSelf;
                SetHierarchyActive(action.targetObject, newState);
                break;
            case ActionType.Activate:
                // 激活整个层级
                SetHierarchyActive(action.targetObject, true);
                break;
            case ActionType.Deactivate:
                // 禁用整个层级
                SetHierarchyActive(action.targetObject, false);
                break;
            case ActionType.ToggleWithReset:
                // 切换状态，再次按下恢复初始状态（包括子级）
                if (action.targetObject.activeSelf == action.initialParentState)
                {
                    SetHierarchyActive(action.targetObject, !action.initialParentState);
                }
                else
                {
                    RestoreInitialHierarchyStates(action);
                }
                break;
        }

        // 触发事件，便于扩展
        OnShortcutTriggered?.Invoke(action);
    }

    /// <summary>
    /// 设置对象及其所有子级的激活状态
    /// </summary>
    private void SetHierarchyActive(GameObject root, bool active)
    {
        root.SetActive(active);

        // 递归设置所有子级
        foreach (Transform child in root.transform)
        {
            child.gameObject.SetActive(active);
        }
    }

    /// <summary>
    /// 恢复对象及其子级的初始激活状态
    /// </summary>
    private void RestoreInitialHierarchyStates(ShortcutAction action)
    {
        foreach (var item in action.initialStates)
        {
            item.Key.SetActive(item.Value);
        }
    }

    /// <summary>
    /// 动态添加快捷键
    /// </summary>
    /// <param name="keyCode">快捷键</param>
    /// <param name="target">目标对象</param>
    /// <param name="actionType">动作类型</param>
    /// <param name="modifier">修饰键</param>
    public void AddShortcut(KeyCode keyCode, GameObject target, ActionType actionType = ActionType.Toggle, ModifierKey modifier = ModifierKey.None)
    {
        if (target == null)
        {
            Debug.LogError("添加快捷键失败：目标对象不能为空");
            return;
        }

        if (_shortcutActions.ContainsKey(keyCode))
        {
            Debug.LogWarning($"添加快捷键失败：{keyCode} 已被绑定");
            return;
        }

        // 记录初始状态
        var initialStates = new Dictionary<GameObject, bool>();
        RecordHierarchyActiveStates(target, initialStates);

        _shortcutActions.Add(keyCode, new ShortcutAction
        {
            targetObject = target,
            modifierKey = modifier,
            actionType = actionType,
            initialStates = initialStates,
            initialParentState = target.activeSelf
        });
    }

    /// <summary>
    /// 移除快捷键
    /// </summary>
    /// <param name="keyCode">要移除的快捷键</param>
    public void RemoveShortcut(KeyCode keyCode)
    {
        if (_shortcutActions.ContainsKey(keyCode))
        {
            _shortcutActions.Remove(keyCode);
        }
    }

    /// <summary>
    /// 快捷键触发事件（用于扩展功能）
    /// </summary>
    public event Action<ShortcutAction> OnShortcutTriggered;
}

/// <summary>
/// 快捷键动作数据类
/// </summary>
public class ShortcutAction
{
    public GameObject targetObject;
    public ModifierKey modifierKey;
    public ActionType actionType;
    public Dictionary<GameObject, bool> initialStates; // 记录整个层级的初始激活状态
    public bool initialParentState; // 父对象的初始状态
}

/// <summary>
/// 快捷键配置（在Inspector中可见）
/// </summary>
[Serializable]
public class ShortcutConfig
{
    [Tooltip("快捷键")]
    public KeyCode keyCode;

    [Tooltip("修饰键（Ctrl/Alt/Shift）")]
    public ModifierKey modifierKey = ModifierKey.None;

    [Tooltip("快捷键动作类型")]
    public ActionType actionType = ActionType.Toggle;

    [Tooltip("目标对象（将同时控制其所有子级）")]
    public GameObject targetObject;
}

/// <summary>
/// 修饰键枚举
/// </summary>
public enum ModifierKey
{
    None,
    Ctrl,
    Alt,
    Shift
}

/// <summary>
/// 动作类型枚举
/// </summary>
public enum ActionType
{
    [Tooltip("切换对象及其子级的激活状态")]
    Toggle,
    [Tooltip("激活对象及其所有子级")]
    Activate,
    [Tooltip("禁用对象及其所有子级")]
    Deactivate,
    [Tooltip("切换激活状态，再次按下恢复初始状态（包括子级）")]
    ToggleWithReset,
    [Tooltip("按住时激活对象及其子级，释放时禁用")]
    HoldToggle
}
