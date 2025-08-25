using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Renderer), typeof(Collider))]
public class AdvancedOutlineInteraction : MonoBehaviour
{
    [Tooltip("����ʹ�õ�����Shader")]
    public Shader outlineShader;

    [Tooltip("�������ʱ��������ɫ")]
    public Color hoverColor = Color.yellow;

    [Tooltip("�����ʱ��������ɫ")]
    public Color selectColor = Color.green;

    [Tooltip("����ʱ��������ȱ���")]
    public float hoverWidthMultiplier = 1.5f;

    [Tooltip("��ɫ�Ϳ�ȹ��ɵĳ���ʱ�䣨�룩")]
    public float transitionDuration = 0.2f;

    [Tooltip("���ʱҪ����Ķ���")]
    public GameObject targetObject;

    [Tooltip("�������ڵ�������λ��")]
    public GameObject mySelfObject;

    // ��⵽����������
    private string _outlineColorProperty;
    private string _outlineWidthProperty;

    // ԭʼ��������
    private Color _originalOutlineColor;
    private float _originalOutlineWidth;

    // Ŀ������ֵ�����ڹ��ɣ�
    private Color _targetColor;
    private float _targetWidth;

    // ��ǰ����ֵ�����ڹ��ɣ�
    private Color _currentColor;
    private float _currentWidth;

    // ��Ⱦ���Ͳ�������
    private Renderer _renderer;
    private Material _materialInstance;

    // ״̬��ʶ
    private bool _isSelected = false;
    private bool _isHovering = false;
    private Coroutine _transitionCoroutine;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();

        // ��������ʵ���Ա���Ӱ����������
        _materialInstance = new Material(_renderer.material);
        _renderer.material = _materialInstance;

        // ���û��ָ��shader��ʹ�õ�ǰ���ʵ�shader
        if (outlineShader == null)
        {
            outlineShader = _materialInstance.shader;
        }

        // �Զ������������
        DetectOutlineProperties();

        // ����ԭʼ����
        SaveOriginalProperties();

        // ��ʼ����ǰ����
        _currentColor = _originalOutlineColor;
        _currentWidth = _originalOutlineWidth;
    }

    private void Update()
    {
        // �������²���������ʵ��ƽ������
        if (!string.IsNullOrEmpty(_outlineColorProperty))
            _materialInstance.SetColor(_outlineColorProperty, _currentColor);

        if (!string.IsNullOrEmpty(_outlineWidthProperty))
            _materialInstance.SetFloat(_outlineWidthProperty, _currentWidth);
    }

    /// <summary>
    /// �Զ����shader�е������������
    /// </summary>
    private void DetectOutlineProperties()
    {
        if (outlineShader == null) return;

        // ��ȡ����shader����
        var properties = new string[outlineShader.GetPropertyCount()];
        for (int i = 0; i < outlineShader.GetPropertyCount(); i++)
        {
            properties[i] = outlineShader.GetPropertyName(i);
        }

        // �����ҵ���ɫ����
        var colorCandidates = properties.Where(p =>
            p.Contains("Outline") && (p.Contains("Color") || p.Contains("Tint"))
        ).ToList();

        // �����ҵ����/ǿ������
        var widthCandidates = properties.Where(p =>
            p.Contains("Outline") && (p.Contains("Width") || p.Contains("Size") || p.Contains("Strength"))
        ).ToList();

        // �����ҵ�������
        if (colorCandidates.Count > 0)
            _outlineColorProperty = colorCandidates[0];

        if (widthCandidates.Count > 0)
            _outlineWidthProperty = widthCandidates[0];

        // ����Ƿ��ҵ���Ҫ������
        if (string.IsNullOrEmpty(_outlineColorProperty) || string.IsNullOrEmpty(_outlineWidthProperty))
        {
            Debug.LogWarning("δ�ܼ�⵽�������ԣ���ȷ��Shader���������������", this);
        }
    }

    /// <summary>
    /// ����ԭʼ��������
    /// </summary>
    private void SaveOriginalProperties()
    {
        if (!string.IsNullOrEmpty(_outlineColorProperty))
            _originalOutlineColor = _materialInstance.GetColor(_outlineColorProperty);

        if (!string.IsNullOrEmpty(_outlineWidthProperty))
            _originalOutlineWidth = _materialInstance.GetFloat(_outlineWidthProperty);
    }

    /// <summary>
    /// �����ͣʱ�Ĵ���
    /// </summary>
    private void OnMouseEnter()
    {
        if (_isSelected) return;

        _isHovering = true;
        StartTransition(hoverColor, _originalOutlineWidth * hoverWidthMultiplier);
    }

    /// <summary>
    /// ����뿪ʱ�Ĵ���
    /// </summary>
    private void OnMouseExit()
    {
        if (_isSelected) return;

        _isHovering = false;
        StartTransition(_originalOutlineColor, _originalOutlineWidth);
    }

    /// <summary>
    /// �����ʱ�Ĵ���
    /// </summary>
    private void OnMouseDown()
    {
        _isSelected = !_isSelected;

        if (_isSelected)
        {
            // Ӧ��ѡ��Ч��
            StartTransition(selectColor, _originalOutlineWidth * hoverWidthMultiplier * 1.2f);

            // ����Ŀ�����
            if (targetObject != null)
            {
                targetObject.SetActive(true);
            }
        }
        else
        {
            // �ָ�ԭʼ״̬����ͣ״̬
            if (_isHovering)
            {
                StartTransition(hoverColor, _originalOutlineWidth * hoverWidthMultiplier);
            }
            else
            {
                StartTransition(_originalOutlineColor, _originalOutlineWidth);
            }

            // ����Ŀ�����
            if (targetObject != null)
            {
                targetObject.SetActive(false);
            }
            // �������������ȡ������Ŀ�������߼�
            // if (targetObject != null) targetObject.SetActive(false);
        }
    }

    /// <summary>
    /// ��ʼ���Թ��ɶ���
    /// </summary>
    private void StartTransition(Color targetColor, float targetWidth)
    {
        // ��������ڽ��еĹ��ɣ�ֹͣ��
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
        }

        // ��ʼ�µĹ���
        _targetColor = targetColor;
        _targetWidth = targetWidth;
        _transitionCoroutine = StartCoroutine(AnimateTransition());
    }

    /// <summary>
    /// ִ�����Թ��ɶ�����Э��
    /// </summary>
    private IEnumerator AnimateTransition()
    {
        float elapsedTime = 0;

        while (elapsedTime < transitionDuration)
        {
            // �����ֵ���ӣ�0��1֮�䣩
            float t = elapsedTime / transitionDuration;
            // ʹ��ƽ������ʹ���ɸ���Ȼ
            t = Mathf.SmoothStep(0, 1, t);

            // ��ֵ���㵱ǰ����ֵ
            _currentColor = Color.Lerp(_currentColor, _targetColor, t);
            _currentWidth = Mathf.Lerp(_currentWidth, _targetWidth, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ȷ������ֵ׼ȷ
        _currentColor = _targetColor;
        _currentWidth = _targetWidth;

        _transitionCoroutine = null;
    }

    /// <summary>
    /// �ָ�����ԭʼ���ԵĹ����ӿ�
    /// </summary>
    public void RestoreOriginalProperties()
    {
        _isSelected = false;
        _isHovering = false;
        StartTransition(_originalOutlineColor, _originalOutlineWidth);
    }

    private void OnDestroy()
    {
        // �������ʵ��
        if (_materialInstance != null)
        {
            Destroy(_materialInstance);
        }
    }

    // �󶨵�UI��ť�ĵ���¼�
    public void OnAdjustButtonClick()
    {
        if (targetObject != null)
        {
            // ���÷��ù���������ʼ�����ö���λ��
            BuildingPlacementManager.Instance.StartAdjustingExistingObject(mySelfObject);
        }
        else
        {
            Debug.LogError("��֪��Ҫ���µ���˭��λ�ã�QWQ");
        }
    }
}
