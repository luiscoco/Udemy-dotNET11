int[][] grid = [[-1, 7], [0, 7], [7, 0]];
Check.That(WarehouseScan.Find(grid) == (1, 1), "blocked warehouse is skipped and first eligible target wins");
Check.That(WarehouseScan.Find([[0, 0], [0]]) is null, "missing stock returns no position");
Check.That(WarehouseScan.Find([]) is null, "empty scan has no result");
Check.That(WarehouseScan.Find(grid) == PreviousScan.Find(grid), "labelled scan preserves the earlier result");
Console.WriteLine($"Selected warehouse and bin {WarehouseScan.Find(grid)}");
