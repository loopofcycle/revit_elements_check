using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_product_check.Revisions
{
    internal class WallConcreteRevision: ConcreteRevision
    {
        public WallConcreteRevision(Autodesk.Revit.DB.Document doc, string project)
        {
            SetCalculationData(doc);
            if (elements_count > 0)
                ResultDataObject = GetResultObject(doc, project);
        }

        public void SetCalculationData(Document doc)
        {
            var elements = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToList();

            level = GetLevel(elements);

            var fo = doc.GetUnits().GetFormatOptions(SpecTypeId.Volume);

            elements_total_volume = GetElementsVolume(elements, fo);

            elements_total_volume += GetElementsVolume(
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Floors)
                .WhereElementIsNotElementType()
                .ToList(),
                fo);

            elements_total_volume += GetElementsVolume(
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsNotElementType()
                .ToList(),
                fo);


            floors_area = GetFloorsArea(doc);
        }
    }
}
