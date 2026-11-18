public static class PreviousScan
{
    public static (int Row, int Column)? Find(int[][] grid)
    {
        for (int row = 0; row < grid.Length; row++)
        {
            bool skip = false;
            for (int column = 0; column < grid[row].Length; column++)
            {
                if (grid[row][column] < 0) { skip = true; break; }
                if (grid[row][column] == 7) return (row, column);
            }
            if (skip) continue;
        }
        return null;
    }
}
