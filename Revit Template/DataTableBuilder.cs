using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Revit_product_check
{
    internal class DataTableBuilder
    {
        public DataTable table;
        public DataTableBuilder(List<Ui.DataObject> results, string title)
        {
            results.Remove(results.First());

            this.table = new DataTable("Results");
            this.table.Columns.Add("ID", typeof(int));
            this.table.Columns.Add("Category", typeof(string));
            this.table.Columns.Add("Value", typeof(string));
            this.table.Columns.Add("Name", typeof(string));
            this.table.Columns.Add("Result", typeof(string));
            this.table.Columns.Add("Group", typeof(string));
            this.table.Columns.Add("Level", typeof(string));
            this.table.Columns.Add("Count", typeof(string));
            this.table.Columns.Add("Summ", typeof(string));
            this.table.Columns.Add("Fluctuation", typeof(string));
            this.table.Columns.Add("Title", typeof(string));

            foreach (Ui.DataObject obj in results)
            {
                DataRow row = this.table.NewRow();
                row["ID"] = obj.ID;
                row["Category"] = obj.Category;
                row["Value"] = obj.Value;
                row["Name"] = obj.Name;
                row["Result"] = obj.Result;
                row["Group"] = obj.Group;
                row["Level"] = obj.Level;
                row["Count"] = obj.Count;
                row["Summ"] = obj.Summ;
                row["Fluctuation"] = obj.Fluctuation;
                row["Title"] = title;
                this.table.Rows.Add(row);
            }
        }
    }
}
