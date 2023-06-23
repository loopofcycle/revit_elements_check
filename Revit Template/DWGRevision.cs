using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit_MP_check
{
    internal class DWGRevision
    {
        public Ui.DataObject Result { get; set; }

        public DWGRevision(Autodesk.Revit.DB.Document doc)
        {
            FilteredElementCollector linktypes = new FilteredElementCollector(doc)
            .OfClass(typeof(CADLinkType))
            .WhereElementIsElementType();

            if (linktypes.ToList().Count > 0)
            {
                this.Result = null;
            }
            else
            {
                this.Result = new Ui.DataObject()
                {
                    ID = -1,
                    Name = "Активный файл revit",
                    Category = "Наличие файла dwg общей подложки",
                    Result = "Не найден файл dwg",
                };
            }

        }
    }
}
