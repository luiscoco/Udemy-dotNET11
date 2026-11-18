public static class WarehouseScan
{
    public static (int Row, int Column)? Find(int[][] grid)
    {
        ArgumentNullException.ThrowIfNull(grid);
        (int Row, int Column)? found = null;
        warehouses: for (int row = 0; row < grid.Length; row++)
        {
            for (int column = 0; column < grid[row].Length; column++)
            {
                if (grid[row][column] < 0) continue warehouses;
                if (grid[row][column] == 7)
                {
                    found = (row, column);
                    break warehouses;
                }
            }
        }
        return found;
    }
}
