using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Revit_product_check
{
    internal class DB_ConnectionBuilder
    {
        public string ConnectionString;
        public DB_ConnectionBuilder()
        {
            var con_str = "";
            con_str += "Data Source = SB-REVIT-DB;";
            //con_str += "Data Source = ST-4Q-VM004;";
            con_str += "Initial Catalog = revit;";
            con_str += "Integrated Security = SSPI";
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(con_str);
            this.ConnectionString = builder.ConnectionString;
        }
    }
}
