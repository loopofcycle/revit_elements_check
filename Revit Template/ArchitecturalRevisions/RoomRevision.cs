using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Revit_product_check
{
    public class RoomRevision: ArchitecturalRevisions.ArchRevision
    {
        public RoomRevision(Document doc)
        {
            ResultDataObject = GetResultObject(doc);
        }

        public List<Ui.DataObject> GetResultObject(Document doc)
        {
            FilteredElementCollector rooms = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType();

            List<Ui.DataObject> _result = new List<Ui.DataObject>();
            
            foreach (var room in rooms)
            {
                Ui.DataObject obj = CheckRoom(room);
                if (obj != null) _result.Add(obj);
            }

            return _result;
        }

        private Ui.DataObject CheckRoom(Element e)
        {
            
            
            
            if (!e.Name.Contains("ПУИ")) return null;

            Parameter p = e.LookupParameter("Площадь");

            if ((p.AsDouble() / 10.7639104166) > 7) return null;

            Element level = (Level)e.Document.GetElement(e.LevelId);

            return (new Ui.DataObject()
            {
                ID = e.Id.IntegerValue,
                Category = "проверка кладовых",
                Standart = "7 кв.м",
                Value = (p.AsDouble() / 10.7639104166).ToString().Substring(0,3),
                Name = e.Name,
                Group = "Помещения",
                Level = level.Name,
                Count = "",
                Result = "Площадь меньше допустимой",
                Summ = "",
            });
        }
    }
}
