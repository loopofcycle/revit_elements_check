using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Revit_product_check.Revisions
{
    internal class ConcreteRevision: BasicRevision
    {
        public double floors_area = 1;

        public Ui.DataObject GetResultObject(Document doc, string project)
        {
            string result_as_str = "проверка пройдена";
            double result = elements_total_volume / floors_area;
            double fluctuation = 0;

            if ( 0 < result && result < 0.22)
            {
                result_as_str = "проверка не пройдена";
                double temp_result = (0.22 - result) / 0.22;
                fluctuation = Convert.ToInt32( - 100 * temp_result);
            }

            if (0.3 < result)
            {
                result_as_str = "проверка не пройдена";
                double temp_result = (result - 0.3) / 0.3;
                fluctuation = Convert.ToInt32(100 * temp_result);
            }

            FluctuationEvaluator fe = new FluctuationEvaluator(fluctuation);
            result_as_str += fe.evaluation;

            string fluc_str = String.Format("{0:0.00}", fluctuation);

            string area_str = "площадь не вычислена";
            if(floors_area.ToString().Length > 4)
                area_str = floors_area.ToString().Substring(0, 5) + "кв.м";

            if (floors_area == 1)
            {
                fluc_str = "-";
                result_as_str = "проверка не пройдена, не определена площадь плиты";
                result = 0;
            }

            var proj_info = GetBuildingInfo(doc.Title);
            var _title = new TitleAnalyzer(doc.Title);

            ResultDataObject = new Ui.DataObject()
            {
                ID = -1,
                Name = "проверка бетоноемкости",

                Check_ID = 94,
                Check_Text = "Металлоемкость - кг арматуры на м3 объема бетонных армируемых конструкций",

                Title = proj_info["title"],
                Project = project,
                Section = proj_info["section"],
                Levels_quantity = proj_info["levels_quantity"],
                Doc_complect = proj_info["doc_complect"],

                Category =
                    "количество конструктивных элементов " + elements_count + " шт."
                    + "\n общий объем элементов " + elements_total_volume + " куб.м"
                    + "\n площадь плиты перекрытия " + floors_area.ToString() + " кв.м",

                Standart = "0,22 ... 0,3",
                Fluctuation = fluc_str,
                Group = _title.group,
                Level = level,
                Count = elements_count.ToString(),
                Result = result_as_str,
                Summ = area_str,
                Value = String.Format("{0:0.00}", result),
            };

            return ResultDataObject;
        }

        public double GetFloorsArea(Document doc) 
        {
            double area = 1;

            Document linked_doc = null;
            foreach (Document d in doc.Application.Documents)
            {
                if (d.Title.Contains("_Г")) linked_doc = d;
            }

            var format_opt = linked_doc.GetUnits().GetFormatOptions(SpecTypeId.Area);

            FilteredElementCollector floors = null;
            if (linked_doc != null)
            {
                floors = new FilteredElementCollector(linked_doc)
                .OfCategory(BuiltInCategory.OST_Floors)
                .WhereElementIsNotElementType();

                foreach (Element f in floors)
                {
                    Floor floor = (Floor)f;
                    Parameter area_par = f.LookupParameter("Площадь");
                    double old_area_value = UnitUtils.ConvertToInternalUnits(
                        area_par.AsDouble(),
                        format_opt.GetUnitTypeId());

                    double area_value = UnitUtils.ConvertToInternalUnits(
                        area_par.AsDouble(),
                        UnitTypeId.SquareMeters);

                    area += area_value;
                }

            }

            return area;
        }

    }
}
