using Autodesk.Revit.DB;
using System.Linq;

namespace Revit_product_check.Revisions
{
    internal class FloorMetallRevision: MetallRevision
    {
        public FloorMetallRevision(Autodesk.Revit.DB.Document doc)
        {
            SetCalculationData(doc);
            if (elements_count > 0)
                ResultDataObject = GetResultObject(doc.Title);
        }

        public new void SetCalculationData(Autodesk.Revit.DB.Document doc)
        {
            var elements = new FilteredElementCollector(doc)
               .OfCategory(BuiltInCategory.OST_Floors)
               .WhereElementIsNotElementType()
               .ToList();

            level = GetLevel(elements);
            
            elements_total_volume += GetElementsVolume(elements);

            rebar_total_mass += GetRebarsMass(
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rebar)
                .WhereElementIsNotElementType()
                .ToList()
                );

            rebar_total_mass += GetModelsMass(
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType()
                .ToList()
                );
        }
    }
}
