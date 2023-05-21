using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data
{
    public static class DataTableExtensions
    {

        public static void SaveToCSV(this DataTable dt, FileStream stream)
        {
            string[] columnsArray = new string[dt.Columns.Count];

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                columnsArray[i] = dt.Columns[i].ColumnName;
            }

            stream.WriteCSVRow(columnsArray);

            foreach (DataRow item in dt.Rows)
            {
                stream.WriteCSVRow(item.ItemArray);
            }
            stream.Flush();

        }

        public static void LoadFromCSV(this DataTable dt, Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true))
            {
                //reading all the lines(rows) from the file.
                var block = reader.ReadToEnd();
                string[] rows = block.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                DataTable dtData = new DataTable();
                string[] rowValues = null;
                DataRow dr = dtData.NewRow();

                //Creating columns
                if (rows.Length > 0)
                {
                    foreach (string columnName in rows[0].Split(','))
                        dtData.Columns.Add(columnName);
                }

                //Creating row for each line.(except the first line, which contain column names)
                for (int row = 1; row < rows.Length; row++)
                {
                    rowValues = rows[row].Split(',');
                    dr = dtData.NewRow();
                    dr.ItemArray = rowValues;
                    dtData.Rows.Add(dr);
                }
            }
        }
    }
}
