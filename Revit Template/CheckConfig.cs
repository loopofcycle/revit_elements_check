using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_MP_check
{
    /// <summary>
    /// Class with configuration for parameters checking
    /// </summary>
    public class CheckConfig
    {
        public Dictionary<BuiltInCategory, Dictionary<string, string>> ParameterForEachCategory =
            new Dictionary<BuiltInCategory, Dictionary<string, string>>();

        /// <summary>
        /// XML commentary: config for checking parameters
        /// </summary>
        public CheckConfig()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "ADSK_Номер секции","not_empty"},
                { "ADSK_Номер здания","not_empty"},
                //{ "ADSK_Наименование", "not_empty" },
            };
            this.ParameterForEachCategory.Add(BuiltInCategory.OST_Doors, parameters);
            this.ParameterForEachCategory.Add(BuiltInCategory.OST_Walls, parameters);
            this.ParameterForEachCategory.Add(BuiltInCategory.OST_Columns, parameters);
            this.ParameterForEachCategory.Add(BuiltInCategory.OST_Windows, parameters);
            this.ParameterForEachCategory.Add(BuiltInCategory.OST_Floors, parameters);
        }
    }
}
