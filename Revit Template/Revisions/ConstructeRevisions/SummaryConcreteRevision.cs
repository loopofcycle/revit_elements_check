using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parameter = Autodesk.Revit.DB.Parameter;

namespace Revit_product_check.Revisions
{
    internal partial class SummaryConcreteRevision: ConcreteRevision
    {
        public List<Ui.DataObject> Results { get; set; }

        public SummaryConcreteRevision(Document doc, string project)
        {
            SetCalculationData(doc, project);
        }

        public void SetCalculationData(Document doc, string project)
        {
            this.Results = new List<Ui.DataObject>();
            var walls_info = GetVolumeInfoByLevel(
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToList());

            var columns_info = GetVolumeInfoByLevel(
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsNotElementType()
                .ToList());

            var floors_area = GetFloorsAreaInfoByLevel(doc);

            var floors_info = GetFloorsVolumeInfoByLevel(
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Floors)
                .WhereElementIsNotElementType()
                .ToList());

            var levels = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .ToList();

            var proj_info = GetBuildingInfo(doc.Title);

            foreach (Level l in levels)
            {
                if (!floors_info.ContainsKey(l.Id)) continue;

                string result_as_str = "проверка пройдена";

                int columns_total = 0;
                int walls_total = 0;
                double total_volume = 0;
                double total_area = 0;
                double result;

                if (walls_info.ContainsKey(l.Id))
                {
                    total_volume += walls_info[l.Id].summary;
                    walls_total = walls_info[l.Id].id_with_values.Count;
                }
                if(columns_info.ContainsKey(l.Id))
                {
                    columns_total = columns_info[l.Id].id_with_values.Count;
                    total_volume += columns_info[l.Id].summary;
                }
                if(floors_info.ContainsKey(l.Id))
                {
                    total_volume += floors_info[l.Id].summary;
                }
                if (floors_area.ContainsKey(l.Id))
                {
                    total_area += floors_area[l.Id].summary;
                }

                if (total_volume == 0 || total_area == 0)
                {
                    Debug.Print("not found area or volume" + l.Id.ToString() + " " + l.Name);
                    continue;
                }

                result = total_volume / total_area;
                double fluctuation = 0;

                if (0 < result && result < 0.22)
                {
                    result_as_str = "проверка не пройдена";
                    double temp_result = (0.22 - result) / 0.22;
                    fluctuation = Convert.ToInt32(-100 * temp_result);
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
                string lvl_name = l.Name;
                var _title = new TitleAnalyzer(doc.Title);
                string group = _title.group;

                this.Results.Add(new Ui.DataObject
                {
                    ID = l.Id.IntegerValue,
                    Name = "проверка бетоноемкости",

                    Check_ID = 94,
                    Check_Text = "Металлоемкость - кг арматуры на м3 объема бетонных армируемых конструкций",

                    Title = proj_info["title"],
                    Project = project,
                    Category = "общий объем: " + String.Format("{0:0.0}", total_volume)
                        + "\nплощадь перекрытий: " + String.Format("{0:0.0}", total_area),
                    Section = proj_info["section"],
                    Levels_quantity = proj_info["levels_quantity"],
                    Doc_complect = proj_info["doc_complect"],
                    Standart = "0,22 ... 0,3",
                    Fluctuation = fluc_str,
                    Group = group,
                    Level = lvl_name,
                    Result = result_as_str,
                    Count = "количество стен: " + walls_total.ToString()
                            + "\nколичество колонн: " + columns_total.ToString(),
                    Summ = "количество перекрытий: " + floors_info[l.Id].id_with_values.Count.ToString(),
                    Value = String.Format("{0:0.00}", result),
                });

            }
        }

        public Dictionary<ElementId, ElementsInfo> GetFloorsAreaInfoByLevel(Document doc)
        {
            Dictionary<ElementId, ElementsInfo> floors_dict = new Dictionary<ElementId, ElementsInfo>();

            double area;

            FilteredElementCollector floors = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Floors)
            .WhereElementIsNotElementType();


            foreach (Element f in floors)
            {
                if (f.LevelId == null) continue;

                //Parameter offset_par = f.LookupParameter("Смещение от уровня");
                //Parameter offset_par = f.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);
                //if (offset_par != null && offset_par.AsDouble() > 0) continue;

                Parameter area_par = f.LookupParameter("Площадь");
                area = UnitUtils.ConvertFromInternalUnits(area_par.AsDouble(), UnitTypeId.SquareMeters);
                //area = area_par.AsDouble() / 10.7639;
                if (!floors_dict.ContainsKey(f.LevelId))
                {
                    floors_dict[f.LevelId] = new ElementsInfo(f.Id, area);
                }
                else
                {
                    floors_dict[f.LevelId].summary += area;
                    floors_dict[f.LevelId].id_with_values.Add(f.Id, area);
                }
            }

            return floors_dict;
        }
    }
}
