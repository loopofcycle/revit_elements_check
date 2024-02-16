using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace Revit_product_check.Revisions
{
    internal class FloorMetallRevision: MetallRevision
    {
        public FloorMetallRevision(Document doc, string project)
        {
            this.doc = doc;

            var elements = new FilteredElementCollector(doc)
               .OfCategory(BuiltInCategory.OST_Floors)
               .WhereElementIsNotElementType()
               .ToList();

            SetCalculationData(doc, elements);
            if (elements_count > 0)
                ResultDataObject = GetResultObject(doc, project);
        }

        public new void SetCalculationData(Document doc, List<Element> elements)
        {

            concrete_class = GetConcreteClass(elements).First().Key;
            level = GetLevel(elements);

            var fo = doc.GetUnits().GetFormatOptions(SpecTypeId.Volume);

            elements_total_volume += GetElementsVolume(elements, fo);

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
