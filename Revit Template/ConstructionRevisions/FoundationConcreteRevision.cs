using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_product_check.ConstructionRevisions
{
    internal class FoundationConcreteRevision: ConcreteRevision
    {
        public FoundationConcreteRevision(Autodesk.Revit.DB.Document doc)
        {
            SetCalculationData(doc);
            if (elements_count > 0)
                ResultDataObject = GetResultObject(doc.Title);
        }

        public void SetCalculationData(Document doc)
        {
            var elements = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_StructuralFoundation)
                .WhereElementIsNotElementType()
                .ToList();

            level = GetLevel(elements);
            elements_total_volume = GetElementsVolume(elements);
            floors_area = GetFloorsArea(doc);
        }
    }
}
