using System.Globalization;
using System.Text;

namespace Natrix.Utils
{
    internal static class ParkingLayoutKey
    {
        // Keep the full key: Dictionary compares equality after hashing, so a hash
        // collision can never discard a different layout. Ignore IDs and scores.
        internal static string Create(Parking parking)
        {
            var matrix = parking.PlanMatrix;
            var key = new StringBuilder();
            key.Append(parking.CellSize.ToString("R", CultureInfo.InvariantCulture)).Append('|');
            key.Append(parking.CarsPerCell).Append('|');
            key.Append(matrix.RowCount).Append(',').Append(matrix.ColumnCount).Append('|');
            AppendCell(key, parking.EntryCell);
            AppendCell(key, parking.PathStartCell);
            var ramp = parking.Ramp;
            if (ramp == null) key.Append("no-ramp|");
            else
            {
                key.Append(ramp.TypeIndex).Append(',').Append(ramp.OrientationIndex).Append(',')
                    .Append(ramp.EntranceSide).Append(',').Append(ramp.SideCellIndex).Append('|');
                AppendCell(key, parking.RampEndCell);
            }
            for (int row = 0; row < matrix.RowCount; row++)
                for (int col = 0; col < matrix.ColumnCount; col++)
                    key.Append(matrix[row, col].ToString("R", CultureInfo.InvariantCulture)).Append(',');
            return key.ToString();
        }

        private static void AppendCell(StringBuilder key, ParkingUtils.PathInfo.Cell cell)
        {
            if (cell == null) key.Append("none|");
            else key.Append(cell.row).Append(',').Append(cell.col).Append('|');
        }
    }
}
