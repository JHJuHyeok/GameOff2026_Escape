using UnityEngine;

/// <summary>
/// 외부 요청에 따라 대상 스프라이트의 아웃라인을 표시하거나 숨깁니다.
/// 표시 상태는 다음 요청 또는 오브젝트 비활성화까지 유지됩니다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class SpriteOutlineView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _targetRenderer;
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField, Min(0f)] private float _outlineWidth = 1f; //원본 텍스쳐의 픽셀 기준의 두께, 셰이더에서도 같은 단위로 사용함
    [SerializeField] private bool _showOnEnable; // 활성화 될 때 적용할 표시값임, 상호작용 조건을 판단하지 않아용

    private const string OUTLINE_ENABLED = "_OutlineEnabled";
    private const string OUTLINE_COLOR = "_OutlineColor";
    private const string OUTLINE_WIDTH = "_OutlineWidth";

    private static readonly int _outlineEnabledId = Shader.PropertyToID(OUTLINE_ENABLED);
    private static readonly int _outlineColorId = Shader.PropertyToID(OUTLINE_COLOR);
    private static readonly int _outlineWidthId = Shader.PropertyToID(OUTLINE_WIDTH);

    private MaterialPropertyBlock _propertyBlock;
    public bool IsVisible { get; private set; } // 현재 View에 설정된 아웃라인 표시 상태

    private void Reset()
    {
        _targetRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        SetVisibility(_showOnEnable);
    }

    private void OnDisable()
    {
        Hide();
    }

    /// <summary>
    /// 아웃라인을 표시 함, hide하지 않으면 유지되요
    /// </summary>
    public void Show()
    {
        SetVisibility(true);
    }

    /// <summary>
    /// 아웃라인을 숨깁니다
    /// </summary>
    public void Hide()
    {
        SetVisibility(false);
    }

    /// <summary>
    /// 아웃라인 색상과 원본 텍스처 픽셀 기준 두께를 변경
    /// </summary>
    public void SetStyle(Color color, float width)
    {
        _outlineColor = color;
        _outlineWidth = Mathf.Max(0f, width);

        ApplyProperties();
    }

    private void SetVisibility(bool visible)
    {
        IsVisible = visible;
        ApplyProperties();
    }

    private void ApplyProperties()
    {
        if (_targetRenderer == null)
        {
            _targetRenderer = GetComponent<SpriteRenderer>();
        }
        if (_targetRenderer == null) return;
        if (_propertyBlock == null)
        {
            _propertyBlock = new MaterialPropertyBlock();
        }

        _targetRenderer.GetPropertyBlock(_propertyBlock);

        float enabledValue = 0f;

        if (IsVisible)
        {
            enabledValue = 1f;
        }

        _propertyBlock.SetFloat(_outlineEnabledId, enabledValue);
        _propertyBlock.SetColor(_outlineColorId, _outlineColor);
        _propertyBlock.SetFloat(_outlineWidthId,Mathf.Max(0f, _outlineWidth));
        _targetRenderer.SetPropertyBlock(_propertyBlock); // 공유 머테리얼말고, 대상 Renderer에만 적용합니다
    }

#if UNITY_EDITOR
    [ContextMenu("아웃라인/표시")]
    private void PreviewShow()
    {
        Show();
    }

    [ContextMenu("아웃라인/숨김")]
    private void PreviewHide()
    {
        Hide();
    }
#endif
}