using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace SudokuSolver
{
    internal class Program
    {
        private static Cell[,] solvedGrid;

        static void Main(string[] args)
        {
            var inputList = new List<string>
            {
                "120070560",
                "507932080",
                "000001000",
                "010240050",
                "308000402",
                "070085010",
                "000700000",
                "080423701",
                "034010028",
            };

            //var inputList = new List<string>
            //{
            //    "800000000",
            //    "003600000",
            //    "070090200",
            //    "050007000",
            //    "000045700",
            //    "000100030",
            //    "001000068",
            //    "008500010",
            //    "090000400",
            //};

            var grid = InitializeGrid(inputList);

            grid.Write();

            ComputeAndTest(grid);
            
            if(solvedGrid != null)
            {
                Console.WriteLine("Solution find");
                solvedGrid.Write();
            }
                

            Console.WriteLine("Hello, World!");
        }

        public static void ComputeAndTest(Cell[,] grid)
        {
            grid.Write();

            //1. Copy the grid
            var copy = grid.Copy();            

            //2. Pick a cell
            var currentCell = copy.PickEmptyCell();

            if(currentCell == null)
            {
                solvedGrid = copy;
                return;
            }
            else
            {
                //3. Get allowed values for the given cell
                var allowedValues = GetAllowedValues(copy, currentCell);

                //No solution
                if (!allowedValues.Any())
                    return;

                foreach (var givenValue in allowedValues)
                {                    
                    copy[currentCell.RowId, currentCell.ColumnId].Value = givenValue;
                    ComputeAndTest(copy);
                }
            }
        }

        public static Cell[,] InitializeGrid(IEnumerable<string> input) 
        {
            var grid = new Cell[9, 9];

            var rowIdx = -1;

            foreach(var row in input) 
            {
                rowIdx++;
                var tmp = row.ToArray().Select(t => int.Parse(t.ToString())).ToArray();

                for(var idxCol = 0; idxCol < tmp.Length; idxCol++)
                {
                    grid[rowIdx, idxCol] = new Cell(tmp[idxCol], rowIdx, idxCol);
                }
            }
            
            return grid;
        }

        public static List<int> GetAllowedValues(Cell[,] grid, Cell cell)
        {
            var rowValues = grid.GetRow(cell.RowId).Select((t, i) => new { Value = t, ColumnIndex = i }).Where(t => t.ColumnIndex != cell.ColumnId).Where(t => t.Value.Value.HasValue).Select(t => t.Value.Value.Value).ToList();
            var colValues = grid.GetColumn(cell.ColumnId).Select((t, i) => new { Value = t, RowIndex = i }).Where(t => t.RowIndex != cell.RowId).Where(t => t.Value.Value.HasValue).Select(t => t.Value.Value.Value).ToList();

            var subGridValues = grid.GetSubGrid(cell).Where(t => t.Value.HasValue).Select(t => t.Value.Value).ToList();

            var tmp = rowValues.Concat(colValues).Concat(subGridValues).Distinct().ToList();

            var possibleValues = Enumerable.Range(1, 9).Except(tmp).ToList();

            return possibleValues;
        }       
    }

    public class Cell
    {
        public bool IsInputData { get; set; }
        public int? Value { get; set; }
        public int RowId { get; }
        public int ColumnId { get; }
        public List<int> PossibleValues { get; }
        public List<int> InitialForbiddenValues { get; }
        public List<int> ForbiddenValues { get; }

        public Cell(int val, int rowIdx, int colIdx)
        {
            RowId = rowIdx;
            ColumnId = colIdx;

            IsInputData = val != 0;
            Value = val != 0 ? val : default(Nullable<int>);
            PossibleValues = new List<int>();
            InitialForbiddenValues = new List<int>();
            ForbiddenValues = new List<int>();                
        }

        public Cell(Cell input)
        {
            this.IsInputData = input.IsInputData;
            this.Value = input.Value;
            this.RowId = input.RowId;
            this.ColumnId = input.ColumnId;

            this.PossibleValues = new List<int>();
            this.InitialForbiddenValues = new List<int>();
            this.ForbiddenValues = new List<int>();        
        }
    }

    public static class TwoDimensionArrayExtension
    {
        public static T[] GetRow<T>(this T[,] grid, int rowId)
            => Enumerable.Range(0, 9).Select(i => grid[rowId, i]).ToArray();

        public static T[] GetColumn<T>(this T[,] grid, int columnId)
            => Enumerable.Range(0,9).Select(i => grid[i, columnId]).ToArray();


        public static Cell[] GetSubGrid(this Cell[,] grid, Cell cell)
        {
            var (i, j) = (GetIndex(cell.RowId), GetIndex(cell.ColumnId));
            var subGridSubList = Enumerable.Range(i, 3).Select(rowId => Enumerable.Range(j, 3).Select(colId => grid[rowId, colId])).SelectMany(t => t).Where(c => c.RowId != cell.RowId && c.ColumnId != cell.ColumnId).ToArray();
            return subGridSubList;
        }

        public static Cell[,] Copy(this Cell[,] grid)
        {
            var copy = new Cell[9, 9];

            for(var i = 0; i < 9; i++) 
            { 
                for(var j = 0; j < 9; j++)
                {
                    copy[i,j] = new Cell(grid[i, j]);
                }
            }
         
            return copy;
        }

        public static bool IsSolved(this Cell[,] grid)
            => !grid.Cast<Cell>().Any(t => !t.Value.HasValue);

        public static Cell PickEmptyCell(this Cell[,] grid)
            => grid.Cast<Cell>().Where(t => !t.Value.HasValue).FirstOrDefault();

        public static void Write(this Cell[,] grid)
        {
            for(var i = 0;i < 9;i++)
            {
                var row = Enumerable.Range(0, 9).Select(j => grid[i, j].Value.HasValue ? grid[i, j].Value.Value.ToString() : string.Empty).Select(t => $"{t,-5} | ").Aggregate((t, acc) => t + acc);
                Console.WriteLine($"| {row}");
            }

            Console.WriteLine(new string('*', 100));
            Console.WriteLine();
        }

        private static int GetIndex(int idx)
            => idx switch
            {
                var n when n >= 0 && n <= 2 => 0,
                var n when n >= 3 && n <= 5 => 3,
                var n when n >= 6 && n <= 9 => 6,
                _ => throw new Exception(),
            };
    }
}
