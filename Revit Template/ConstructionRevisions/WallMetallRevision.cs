using Autodesk.Revit.DB;
using System.Linq;

namespace Revit_product_check.Revisions
{
    internal class WallMetallRevision: MetallRevision
    {
        public WallMetallRevision(Autodesk.Revit.DB.Document doc)
        {
            SetCalculationData(doc);
            if (elements_count > 1)
                ResultDataObject = GetResultObject(doc.Title);
        }

        public new void SetCalculationData(Autodesk.Revit.DB.Document doc)
        {
            var elements = new FilteredElementCollector(doc)
               .OfCategory(BuiltInCategory.OST_Walls)
               .WhereElementIsNotElementType()
               .ToList();

            level = GetLevel(elements);
            
            elements_total_volume += GetElementsVolume(elements);

            elements_total_volume += GetElementsVolume(
            new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Floors)
            .WhereElementIsNotElementType()
            .ToList()
            );

            elements_total_volume += GetElementsVolume(
            new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_StructuralColumns)
            .WhereElementIsNotElementType()
            .ToList()
            );

            rebar_total_mass += GetRebarsMass(
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rebar)
                .WhereElementIsNotElementType()
                .ToList()
                );
        }
    }
}
