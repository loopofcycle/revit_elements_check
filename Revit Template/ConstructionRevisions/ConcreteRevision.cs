using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Revit_product_check.ConstructionRevisions
{
    internal class ConcreteRevision: BasicRevision
    {
        public double floors_area = 0;

        public Ui.DataObject GetResultObject(string title)
        {
            string result_as_str = "проверка пройдена";

            double result = elements_total_volume / floors_area;
            double fluctuation = 0;

            if ( 0 < result && result < 0.22)
            {
                result_as_str = "проверка не пройдена";
                fluctuation = 100 * (0.22 - result) / 0.22;
            }

            if (0.3 < result)
            {
                result_as_str = "проверка не пройдена";
                fluctuation = 100 * (result - 0.3) / 0.3;
            }

            ResultDataObject = new Ui.DataObject()
            {
                ID = -1,
                Name = "проверка бетоноемкости",

                Category =
                    "количество конструктивных элементов " + elements_count + " шт."
                    + "\n общий объем элементов " + elements_total_volume + " куб.м"
                    + "\n площадь плиты перекрытия " + floors_area.ToString() + " кв.м",

                Standart = "0,22 ... 0,3",
                Fluctuation = fluctuation.ToString().Substring(0,2) + "%",
                Group = GetGroup(title),
                Level = level,
                Count = elements_count.ToString(),
                Result = result_as_str,
                Summ = floors_area.ToString() + "кв.м",
                Value = result.ToString().Substring(0, 4),
            };

            return ResultDataObject;
        }

        public double GetFloorsArea(Document doc) 
        {
            double area = 1;

            Document linked_doc = null;
            foreach (Document d in doc.Application.Documents)
            {
                if (d.Title.Contains("_Г")) linked_doc = d;
            }

            FilteredElementCollector floors = new FilteredElementCollector(linked_doc)
            .OfCategory(BuiltInCategory.OST_Floors)
            .WhereElementIsNotElementType();

            foreach (Element f in floors)
            {
                //if (f.LevelId != level.Id) continue;

                //Parameter elev_par = f.LookupParameter("Смещение от уровня");
                //if (elev_par != null & elev_par.AsDouble() != 0) continue;

                Floor floor = (Floor)f;
                Parameter area_par = f.LookupParameter("Площадь");
                area += area_par.AsDouble() / 10.7639104166;
            }

            return area;
        }

    }
}
