using Autodesk.Revit.DB;
using System.Linq;

namespace Revit_product_check.Revisions
{
    internal class FoundationMetallRevision : MetallRevision
    {
        public FoundationMetallRevision(Document doc, string project)
        {
            this.doc = doc;
            SetCalculationData(doc);
            if (elements_count > 1)
                ResultDataObject = GetResultObject(doc, project);
        }

        public new void SetCalculationData(Document doc)
        {
            var fo = doc.GetUnits().GetFormatOptions(SpecTypeId.Volume);

            var foundation_elem = new FilteredElementCollector(doc)
               .OfCategory(BuiltInCategory.OST_StructuralFoundation)
               .WhereElementIsNotElementType()
               .ToList();
            
            
            elements_total_volume += GetElementsVolume(foundation_elem, fo);

            var floor_elem = new FilteredElementCollector(doc)
               .OfCategory(BuiltInCategory.OST_Floors)
               .WhereElementIsNotElementType()
               .ToList();
            elements_total_volume += GetElementsVolume(floor_elem, fo);

            level = GetLevel(foundation_elem);

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
