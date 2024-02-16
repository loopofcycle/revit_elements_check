using Autodesk.Revit.DB;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Revit_product_check.Revisions
{
    internal class WallMetallRevision: MetallRevision
    {
        public WallMetallRevision(Document doc, string project)
        {
            this.doc = doc;
            SetCalculationData(doc);
            if (elements_count > 1)
                ResultDataObject = GetResultObject(doc, project);
        }

        public new void SetCalculationData(Document doc)
        {
            var fo = doc.GetUnits().GetFormatOptions(SpecTypeId.Volume);
            
            var walls = new FilteredElementCollector(doc)
               .OfCategory(BuiltInCategory.OST_Walls)
               .WhereElementIsNotElementType()
               .ToList();
            

            concrete_class = GetConcreteClass(walls).First().Key;
            level = GetLevel(walls);
            elements_total_volume += GetElementsVolume(walls, fo);
            elements_total_volume += GetElementsVolume(
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Floors)
                .WhereElementIsNotElementType()
                .ToList(),
                fo
                );

            elements_total_volume += GetElementsVolume(
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsNotElementType()
                .ToList(),
                fo
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
