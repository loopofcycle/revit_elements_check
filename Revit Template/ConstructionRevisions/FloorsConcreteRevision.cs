using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_product_check.ConstructionRevisions
{
    internal class FloorsConcreteRevision: ConcreteRevision
    {
        public FloorsConcreteRevision(Autodesk.Revit.DB.Document doc)
        {
            SetCalculationData(doc);
            if (elements_count > 0)
                ResultDataObject = GetResultObject(doc.Title);
        }

        public void SetCalculationData(Document doc)
        {
            var elements = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Floors)
                .WhereElementIsNotElementType()
                .ToList();

            level = GetLevel(elements);
            elements_total_volume = GetElementsVolume(elements);
            floors_area = GetFloorsArea(doc);
        }

        public new double GetFloorsArea(Document doc)
        {
            double area = 1;

            FilteredElementCollector floors = new FilteredElementCollector(doc)
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
