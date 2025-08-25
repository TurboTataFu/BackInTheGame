using UnityEngine;

// 修复 GridMath.cs
public static class GridMath
{
    public static Vector2Int WorldToGrid(Vector2 pos, InventoryGrid grid)
    {
        RectTransform rt = grid.GetComponent<RectTransform>();
        Vector2 localPos = rt.InverseTransformPoint(pos);

        // 添加具体计算逻辑
        int x = Mathf.FloorToInt((localPos.x + rt.rect.width / 2) / grid.CellSize);
        int y = Mathf.FloorToInt((-localPos.y + rt.rect.height / 2) / grid.CellSize);

        return new Vector2Int(
            Mathf.Clamp(x, 0, grid.GridSize.x - 1),
            Mathf.Clamp(y, 0, grid.GridSize.y - 1)
        );
    }
}