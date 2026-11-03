using UnityEngine;
using UnityEngine.UI;
using System;
using MainSystem;

[CreateAssetMenu(fileName = "Configs/ItemConfig", menuName = "Scriptable Objects/ItemConfig")]
public class ItemConfig : ScriptableObject
{
    public float cellSize = 64f;
    public Color cellColor      = new Color(0.1f, 0.1f, 0.1f, 0.6f);
    public Color validColor     = new Color(0.2f, 0.9f, 0.3f, 0.35f);
    public Color invalidColor   = new Color(0.95f, 0.2f, 0.2f, 0.35f);

    [Header("풀링 대상 프리팹")]
    public ItemView itemViewPrefab;
    public Image previewCellPrefab;

    [Header("사전 생성 수 (첫 사용 시 끊김 방지)")]
    [Min(0)] public int prewarmItemViews = 16;
    [Min(0)] public int prewarmPreviewCells = 12;

    [NonSerialized] private bool prewarmed;

    public void Prewarm()
    {
        if (prewarmed) return;
        prewarmed = true;
        PoolManager.Instance.Prewarm(itemViewPrefab.gameObject, prewarmItemViews);
        PoolManager.Instance.Prewarm(previewCellPrefab.gameObject, prewarmPreviewCells);
    }
}
