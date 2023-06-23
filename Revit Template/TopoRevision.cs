using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit_MP_check
{
    internal class TopoRevision
    {
        public Ui.DataObject Result { get; set; }

        public TopoRevision(Autodesk.Revit.DB.Document doc)
        {
            FilteredElementCollector topo = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Topography)
            .WhereElementIsNotElementType();

            if (topo.ToList().Count > 0)
            {
                this.Result = null;
            }
            else
            {
                this.Result = new Ui.DataObject()
                {
                    ID = -1,
                    Name = "Активный файл revit",
                    Category = "Наличие топоверхности",
                    Result = "Не найдена топоповерхность",
                };
            }
        }
    }
}
