using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_EIR_check
{
    public class CheckConfig
    {
        public Dictionary<string, BuiltInCategory> categories = new Dictionary<string, BuiltInCategory>()
            {
                {"Doors", BuiltInCategory.OST_Doors },
                {"StructuralFraming", BuiltInCategory.OST_StructuralFraming },
                {"Columns", BuiltInCategory.OST_Columns },
                {"StructuralRebar", BuiltInCategory.OST_Rebar},
                {"StructuralColumns", BuiltInCategory.OST_StructuralColumns },
                {"StructuralFoundations", BuiltInCategory.OST_StructuralFoundation },
                {"GenericModels", BuiltInCategory.OST_GenericModel},
                {"Windows", BuiltInCategory.OST_Windows },
                {"CurtainPanels", BuiltInCategory.OST_CurtainWallPanels },
                {"Floors", BuiltInCategory.OST_Floors },
                {"Ceilings", BuiltInCategory.OST_Ceilings },
                {"Walls", BuiltInCategory.OST_Walls },
                {"MechanicalEquipment", BuiltInCategory.OST_MechanicalEquipment },
                {"Pipes", BuiltInCategory.OST_PipeSegments },
                {"PipeFittings", BuiltInCategory.OST_PipeFitting },
                {"PipeInsulations", BuiltInCategory.OST_PipeInsulations },
                {"PipeAccessories", BuiltInCategory.OST_PipeAccessory },
                {"ElectricalFixtures", BuiltInCategory.OST_ElectricalFixtures },
            };

        public Dictionary<BuiltInCategory, Dictionary<string, string>> ParameterForEachCategory =
            new Dictionary<BuiltInCategory, Dictionary<string, string>>()
            {
                BuiltInCategory.OST_Doors, 
                        { "ADSK_Номер секции","not_empty" },
                        { "ADSK_Наименование", "not_empty" },
                
                 BuiltInCategory.OST_ElectricalFixtures, 
                        { "ADSK_Количество", ">1" },
                        { "ADSK_Номер секции","not_empty" },
                        { "ADSK_Наименование", "not_empty" },
                                
            };
    

        public CheckConfig()
        {
            this.ParameterForEachCategory.Add(
                BuiltInCategory.OST_Doors, {
                    { "ADSK_Номер секции","not_empty" }
                    { "ADSK_Наименование", "not_empty" },
                    }
                )
        }
    }
}
