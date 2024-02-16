using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace Revit_product_check
{
    class ElementsInfo
    {
        public Dictionary<ElementId, double> id_with_values = new Dictionary<ElementId, double>();

        public double elements = 0;
        public double summary = 0;

        public ElementsInfo(ElementId id, double value)
        {
            id_with_values.Add(id, value);
            summary = value;
        }
    }
}
