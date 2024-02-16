using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_product_check.Revisions
{
    internal class FloorsConcreteRevision: ConcreteRevision
    {

        FormatOptions fo_area;
        FormatOptions fo_volume;

        public FloorsConcreteRevision(Document doc, string project)
        {

            this.fo_area = doc.GetUnits().GetFormatOptions(SpecTypeId.Area);
            this.fo_volume = doc.GetUnits().GetFormatOptions(SpecTypeId.Volume);


            SetCalculationData(doc);
            if (elements_count > 0)
                ResultDataObject = GetResultObject(doc, project);
        }

        public void SetCalculationData(Document doc)
        {
            var fo = doc.GetUnits().GetFormatOptions(SpecTypeId.Volume);

            var elements = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Floors)
                .WhereElementIsNotElementType()
                .ToList();

            level = GetLevel(elements);
            elements_total_volume = GetElementsVolume(elements, fo);
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
                Floor floor = (Floor)f;
                Parameter area_par = f.LookupParameter("Площадь");

                double area_value = UnitUtils.ConvertToInternalUnits(
                        area_par.AsDouble(),
                        this.fo_area.GetUnitTypeId()
                        );
                
                area_value = UnitUtils.ConvertToInternalUnits(
                        area_par.AsDouble(),
                        UnitTypeId.SquareMeters
                        );

                area += area_value;
            }

            return area;
        }
    }
}
