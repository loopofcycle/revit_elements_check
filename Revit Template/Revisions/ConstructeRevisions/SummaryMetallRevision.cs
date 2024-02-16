using Autodesk.Revit.DB;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace Revit_product_check.Revisions
{
    internal class SummaryMetallRevision: MetallRevision
    {
        public List<Ui.DataObject> Results { get; set; }

        public SummaryMetallRevision(Document doc, string project)
        {
            this.doc = doc;
            SetCalculationData(doc, project);
        }

        public void SetCalculationData(Document doc, string project)
        {
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

            var floors_volume = GetFloorsVolumeInfoByLevel(
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Floors)
                .WhereElementIsNotElementType()
                .ToList());

            var levels = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .ToList();

            var mass_info = GetElementsMassInfo(doc);

            var proj_info = GetBuildingInfo(doc.Title);
            
            this.Results = new List<Ui.DataObject>();
            foreach (Level l in levels)
            {
                int columns_total = 0;
                int walls_total = 0;
                double total_volume = 0;

                if (walls_info.ContainsKey(l.Id))
                {
                    total_volume += walls_info[l.Id].summary;
                    walls_total = walls_info[l.Id].id_with_values.Count;
                }
                if (columns_info.ContainsKey(l.Id))
                {
                    columns_total = columns_info[l.Id].id_with_values.Count;
                    total_volume += columns_info[l.Id].summary;
                }
                if (floors_volume.ContainsKey(l.Id))
                {
                    total_volume += floors_volume[l.Id].summary;
                }
                
                
                double total_mass = 0;
                if (mass_info.ContainsKey(l.Id))
                {
                    total_mass += mass_info[l.Id].summary;
                }

                if (total_mass == 0 || total_volume == 0)
                {
                    Debug.Print("not found mass or volume: " + l.Id.ToString() + " " + l.Name);
                    continue;
                }

                // compiling result
                string result_as_str = "проверка пройдена";
                double _end_result = (total_mass / total_volume);

                var _title = new TitleAnalyzer(doc.Title);

                int limit = GetLimitValue(_title.group, level);
                double fluctuation = -100 * (limit - _end_result) / limit;
                if (_end_result > limit)
                {
                    result_as_str = "проверка не пройдена";
                    fluctuation = 100 * (_end_result - limit) / limit;
                }
                // calc fluctuation
                FluctuationEvaluator fe = new FluctuationEvaluator(fluctuation);
                result_as_str += fe.evaluation;
                string fluc_str = String.Format("{0:0.00}", fluctuation);
                
                // additional parameters
                string lvl_name = l.Name;
                
                string group = _title.group;
                this.Results.Add(new Ui.DataObject
                {
                    ID = l.Id.IntegerValue,
                    Name = "проверка металлоемкости",

                    Check_ID = 94,
                    Check_Text = "Металлоемкость - кг арматуры на м3 объема бетонных армируемых конструкций",

                    Title = proj_info["title"],
                    Project = project,
                    Category = "масса арматуры: " + String.Format("{0:0.0}", total_mass)
                            + "\n общий объем: " + String.Format("{0:0.0}", total_volume),
                    Section = proj_info["section"],
                    Levels_quantity = proj_info["levels_quantity"],
                    Doc_complect = proj_info["doc_complect"],
                    Standart = limit.ToString(),
                    Value = String.Format("{0:0.00}", _end_result),
                    Fluctuation = fluc_str,
                    Group = group,
                    Level = lvl_name,
                    Result = result_as_str,
                    Count = "количество стен: " + walls_total.ToString()
                            + "\nколичество колонн: " + columns_total.ToString(),
                    Summ = "количество неучтоенной арматуры: " + this.rebar_NOT_counted.ToString(),
                });
            }
        }

        public Dictionary<ElementId, ElementsInfo> GetElementsMassInfo(Document doc)
        {
            Dictionary<ElementId, ElementId> __group_to_level = new Dictionary<ElementId, ElementId>();
            var groups = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_IOSModelGroups)
                .WhereElementIsNotElementType()
                .ToList();
            foreach(Element g in groups)
            {
                if(!__group_to_level.ContainsKey(g.Id))
                    __group_to_level[g.Id] = g.LevelId;
            }
            
            Dictionary<ElementId, List<Element>> rebar_to_level = 
                new Dictionary<ElementId, List<Element>>();
            var rebars = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rebar)
                .WhereElementIsNotElementType()
                .ToList();
            foreach(Element r in rebars)
            {
                ElementId level_id = null;
                
                ElementId group_id = r.GroupId;
                if (group_id != null && group_id.IntegerValue > 0)
                    level_id = __group_to_level[group_id];

                FamilyInstance fi = r as FamilyInstance;
                if(level_id == null && fi != null)
                    level_id = GetFromFamilyinsatanceLevelId(fi);
                
                if(level_id == null)
                    level_id = GetFromRebarLevelId(r);

                if(level_id == null)
                {
                    Debug.Print("cannot find level, not counted: " + r.Id.ToString() + " " + r.Name);
                    continue;
                }

                if(!rebar_to_level.ContainsKey(level_id))
                    rebar_to_level[level_id] = new List<Element>() { r };
                else
                    rebar_to_level[level_id].Add(r);
            }
            
            Dictionary<ElementId, List<Element>> model_to_level =
                new Dictionary<ElementId, List<Element>>();
            var models = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType()
                .ToList();
            foreach (Element m in models)
            {
                var group_id = m.GroupId;

                if (group_id == null || group_id.IntegerValue < 1)
                    continue;

                var lvl_id = __group_to_level[group_id];

                if (!model_to_level.ContainsKey(lvl_id))
                    model_to_level[lvl_id] = new List<Element>() { m };
                else
                    model_to_level[lvl_id].Add(m);
            }


            Dictionary<ElementId, ElementsInfo> mass_info = new Dictionary<ElementId, ElementsInfo>();

            var levels = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .ToList();
            foreach (Element _lvl in levels)
            {
                double _total_mass = 0;

                if (rebar_to_level.ContainsKey(_lvl.Id))
                {
                    var rebar_mass = GetRebarsMass(rebar_to_level[_lvl.Id]);
                    _total_mass += rebar_mass;
                }

                if (model_to_level.ContainsKey(_lvl.Id))
                {
                    var models_mass = GetModelsMass(model_to_level[_lvl.Id]);
                    _total_mass += models_mass;
                }

                if (!mass_info.ContainsKey(_lvl.Id))
                {
                    var info = new ElementsInfo(_lvl.Id, _total_mass);
                    mass_info.Add(_lvl.Id, info);
                }
                else
                {
                    var info = mass_info[_lvl.Id];
                    info.summary += _total_mass;
                }
            }

            return mass_info;
        }

        public ElementId GetFromRebarLevelId(Element element)
        {
            Autodesk.Revit.DB.Structure.Rebar elem_r = element as Autodesk.Revit.DB.Structure.Rebar;
            if (elem_r == null)
                return null;
            ElementId host_id = elem_r.GetHostId();
            Element host = elem_r.Document.GetElement(host_id);

            return host.LevelId;
        }

        public ElementId GetFromFamilyinsatanceLevelId(FamilyInstance fi)
        {
            ElementId result = null;

            Element elem_lvl = fi.Host;
            if (elem_lvl != null)
            {
                Level lvl = elem_lvl as Level;
                if (lvl != null)
                    result = lvl.Id;
            }

            return result;
        }

        public ElementId GetHostLevelId(Element element)
        {
            Autodesk.Revit.DB.Structure.Rebar elem_r = element as Autodesk.Revit.DB.Structure.Rebar;
            if (elem_r == null)
                return null;
            ElementId host_id = elem_r.GetHostId();
            Element host = elem_r.Document.GetElement(host_id);

            return host.LevelId;
        }
    }
}
