using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_product_check.DB_connection
{
    class DB_Uploader
    {
        public DB_Uploader(DataTable check_dt)
        {
            DB_ConnectionBuilder builder = new DB_ConnectionBuilder();

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                string sql_cmd = "SELECT * FROM dbo.abs_Product_check_results";
                SqlDataAdapter adapter = new SqlDataAdapter(sql_cmd, connection);
                DataSet existing_ds = new DataSet();
                adapter.Fill(existing_ds);
                DataTable existing_dt = existing_ds.Tables[0];

                //create object of SqlBulkCopy which help to insert  
                existing_dt.Merge(check_dt, true);
                DataTable result_table = existing_dt;

                SqlCommandBuilder cmdBldr = new SqlCommandBuilder(adapter);
                adapter.Update(result_table);
                result_table.AcceptChanges();
                adapter.Update(existing_ds);
                existing_ds.AcceptChanges();
                existing_ds.Clear();
                adapter.Fill(existing_ds);

                connection.Close();
            }
        }

    }

}
