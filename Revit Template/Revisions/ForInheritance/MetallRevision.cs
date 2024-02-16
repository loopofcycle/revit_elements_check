using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Revit_product_check.Revisions
{
    internal class MetallRevision: BasicRevision
    {
        public double rebar_total_mass = 0;
        public int rebar_count = 0;
        public int rebar_NOT_counted = 0;
        public string concrete_class = "";
        public Document doc = null;

        public Ui.DataObject GetResultObject(Document doc, string project)
        {
            this.doc = doc;

            var _end_result = (int)(rebar_total_mass / elements_total_volume);

            var title_analyzer = new TitleAnalyzer(doc.Title);
            int limit = GetLimitValue(title_analyzer.group, level);
            
            double fluctuation = -100 * (limit - _end_result) / limit;

            string result_as_string = "проверка пройдена";
            if (_end_result > limit)
            {
                result_as_string = "проверка не пройдена";
                fluctuation = 100 * (_end_result - limit) / limit;
            }

            FluctuationEvaluator fe = new FluctuationEvaluator(fluctuation);
            result_as_string += fe.evaluation;

            string fluc_str = String.Format("{0:0.00}", fluctuation);

            var proj_info = GetBuildingInfo(doc.Title);

            ResultDataObject = new Ui.DataObject()
            {
                ID = -1,
                Name = "проверка металлоемкости",

                Check_ID = 94,
                Check_Text= "Металлоемкость - кг арматуры на м3 объема бетонных армируемых конструкций",

                Title = proj_info["title"],
                Project = project,
                Section = proj_info["section"],
                Levels_quantity = proj_info["levels_quantity"],
                Doc_complect = proj_info["doc_complect"],

                Category =
                    "количество конструктивных элементов " + elements_count + " шт."
                    + "\n общий объем элементов " + elements_total_volume + " куб.м"
                    + "\n не учтен. элементы арматуры " + rebar_NOT_counted + " шт."
                    +"\n класс бетона " + concrete_class,

                Standart = limit.ToString() + " кг./куб.м",
                Group = title_analyzer.group,
                Level = level,
                Count = rebar_count.ToString(),
                Fluctuation = fluc_str,
                Result = result_as_string,
                Summ = ((int)rebar_total_mass).ToString() + " кг.",
                Value = _end_result.ToString(),
            };

            return ResultDataObject;
        }

        public double GetRebarsMass(List<Element> elements)
        {
            double total_mass = 0;

            foreach (Element element in elements)
            {

                var rebar_rev = Rebar_Revision(element);
                if (rebar_rev != null)
                {
                    if (rebar_rev["total_weight"] > 0)
                    {
                        total_mass += rebar_rev["total_weight"];
                        rebar_count += 1;
                        Debug.Print(element.Id.ToString() + ", " + rebar_rev["total_weight"].ToString());
                        continue;
                    }
                    Debug.Print(element.Id.ToString() + "failed to calculate weight");
                }
                
                var ris_rev = RebarInSystem_Revision(element);
                if (ris_rev != null)
                {
                    if(ris_rev["total_weight"] > 0)
                    {
                        total_mass += ris_rev["total_weight"];
                        rebar_count += 1;
                        Debug.Print(element.Id.ToString() + ", " + ris_rev["total_weight"].ToString());
                        continue;
                    }
                }

                var primary_rev = RebarPrimary_Revision(element);
                if (primary_rev != null)
                {
                    if(primary_rev["total_weight"] > 0)
                    {
                        total_mass += primary_rev["total_weight"];
                        rebar_count += 1;
                        Debug.Print(element.Id.ToString() + ", " + primary_rev["total_weight"].ToString());
                        continue;
                    }
                }

                FamilyInstance as_fi = element as FamilyInstance;
                if (as_fi != null)
                {
                    double components_mass = 0;
                    var sub_components_ids = as_fi.GetSubComponentIds().ToList();
                    if (sub_components_ids.Count > 1)
                    {
                        foreach (var id in sub_components_ids)
                        {
                            var c = this.doc.GetElement(id);
                            List<Element> components = new List<Element>() {c};
                            components_mass += this.GetRebarsMass(components);
                        }
                        total_mass += components_mass;
                        Debug.Print("with sub components: " + element.Id.ToString() + ", " + components_mass);
                    }

                }

                Debug.Print("not counted: " + element.Id.ToString() + " "+ element.Name);
                rebar_NOT_counted += 1;
            }

            return total_mass;

        }

        public double GetModelsMass(List<Element> elements)
        {
            double total_mass = 0;

            Document doc = elements.First().Document;

            foreach (Element element in elements)
            {
                Autodesk.Revit.DB.Parameter mass_par = element.LookupParameter(":Масса");

                Element _type_element = doc.GetElement(element.GetTypeId());
                Autodesk.Revit.DB.Parameter _type_mass_par = _type_element.LookupParameter(":Масса");
                
                if(mass_par != null)
                {
                    total_mass += mass_par.AsDouble();
                    rebar_count += 1;
                }

                if (mass_par == null && _type_mass_par != null)
                {
                    total_mass += _type_mass_par.AsDouble();
                    rebar_count += 1;
                }

                if (mass_par == null && _type_mass_par == null)
                {
                    Debug.Print("not counted" + element.Id.ToString() + " " + element.Name);
                    rebar_NOT_counted += 1;
                }
            }

            return total_mass;
        }

        public SortedDictionary<string, List<ElementId>> GetConcreteClass(List<Element> elements)
        {
            SortedDictionary <string, List<ElementId>> results = 
                new SortedDictionary<string, List<ElementId>>();

            Document doc = elements.First().Document;

            foreach (Element element in elements)
            {
                //Element _type_element = doc.GetElement(element.GetTypeId());
                //Parameter _type_mass_par = _type_element.LookupParameter("Описание");
                //string concrete = _type_mass_par.AsString();
                string[] type_str = element.LookupParameter("Тип").AsValueString().Split('_');
                string concrete = type_str[2];

                if (concrete == null) continue;

                if (!results.ContainsKey(concrete))
                {
                    results.Add(concrete, new List<ElementId>());
                }
            
                results[concrete].Add(element.Id);

            }

            return results;
        }

        public int GetLimitValue(string group, string level)
        {
            int result = 0;

            var strana_standart = new Dictionary<string, Dictionary<string, int>>()
            {
                { "ФП", new Dictionary<string, int>() { 
                    { "default", 110 } } },
                
                { "В*", new Dictionary<string, int>() { 
                        { "-1", 185 },
                        { "1", 170 },
                        { "2", 160 },
                        { "default", 125 } } },

                { "В*-В*", new Dictionary<string, int>() {
                        { "-1", 185 },
                        { "1", 170 },
                        { "2", 160 },
                        { "default", 125 } } },

                { "Г*", new Dictionary<string, int>() {
                    { "-1", 140 },
                    { "default", 115 } } },
                
                { "Г*-Г*", new Dictionary<string, int>() {
                    { "-1", 140 },
                    { "default", 115 } } }
            };

            if (strana_standart.ContainsKey(group))
            {
                Dictionary<string, int> limits_dict = strana_standart[group];
                result = limits_dict["default"];

                if (limits_dict.ContainsKey(level))
                {
                    result = limits_dict[level];
                }
            }

            return result;
        }

        public Dictionary<string, double> RebarPrimary_Revision(Element element)
        {
            Dictionary<string, double> _elem_data = new Dictionary<string, double>();
            double id = 0;
            double.TryParse(element.Id.ToString(), out id);
            _elem_data["id"] = id;


            Autodesk.Revit.DB.Parameter l_par = element.LookupParameter("Рзм.Длина");
            Autodesk.Revit.DB.Parameter fam_l_par = element.LookupParameter("Длина стержня");
            double total_length = 0;
            if (l_par != null)
            {
                _elem_data["total_length"] = l_par.AsDouble() * 304.8 / 1000;
                total_length = UnitUtils.ConvertFromInternalUnits(l_par.AsDouble(), UnitTypeId.Meters);
            }
            else if (l_par == null && fam_l_par != null)
            {
                _elem_data["total_length"] = fam_l_par.AsDouble() * 304.8 / 1000;
                total_length = UnitUtils.ConvertFromInternalUnits(fam_l_par.AsDouble(), UnitTypeId.Meters);
            }
            _elem_data["total_length"] = total_length;


            Autodesk.Revit.DB.Parameter q_par = element.LookupParameter("О_Количество");
            if (q_par != null)
            {
                _elem_data["quantity"] = q_par.AsDouble();
                _elem_data["number_of_bars"] = q_par.AsDouble();
            }
            else
            {
                _elem_data["quantity"] = 1;
                _elem_data["number_of_bars"] = 1;
            }


            Element _type = element.Document.GetElement(element.GetTypeId());
            Autodesk.Revit.DB.Parameter _type_d_par = _type.LookupParameter("Рзм.Диаметр");
            Autodesk.Revit.DB.Parameter d_par = element.LookupParameter("Рзм.Диаметр");
            if (d_par != null)
            {
                double parsed_diam = 0;
                double.TryParse(d_par.AsValueString(), out parsed_diam);
                _elem_data["diameter"] = parsed_diam;
                //_elem_data["weight_of_meter"] = this.GetWeightOfMeter("Ø" + d_par.AsValueString());
                _elem_data["weight_of_meter"] = this.GetWeightOfMeter("Ø" + parsed_diam.ToString());
            }
            else if (d_par == null && _type_d_par != null)
            {
                double parsed_diam = 0;
                double.TryParse(_type_d_par.AsValueString(), out parsed_diam);
                _elem_data["diameter"] = parsed_diam;
                //_elem_data["weight_of_meter"] = this.GetWeightOfMeter("Ø" + _type_d_par.AsValueString());
                _elem_data["weight_of_meter"] = this.GetWeightOfMeter("Ø" + parsed_diam.ToString());
            }
            else if (d_par == null && _type_d_par == null)
            {
                double weight = this.GetWeightOfMeter(element.Name);
                if (weight > 0)
                    _elem_data["weight_of_meter"] = weight;
            }

            if (!_elem_data.ContainsKey("weight_of_meter") ||
                !_elem_data.ContainsKey("quantity") ||
                !_elem_data.ContainsKey("total_length"))
                return null;

            _elem_data["total_weight"] = 
                _elem_data["weight_of_meter"] 
                * _elem_data["total_length"] 
                * _elem_data["quantity"];

            return _elem_data;
        }

        public Dictionary<string, double> Rebar_Revision(Element element)
        {

            Autodesk.Revit.DB.Structure.Rebar r = element as Autodesk.Revit.DB.Structure.Rebar;
            if (r == null)
                return null;

            Dictionary<string, double> _elem_data = new Dictionary<string, double>();

            Autodesk.Revit.DB.Parameter diam = r.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER);
            _elem_data["diameter"] = diam.AsDouble();
            //_elem_data["total_length"] = r.TotalLength * 304.8 / 1000;
            _elem_data["total_length"] = UnitUtils.ConvertFromInternalUnits(r.TotalLength, UnitTypeId.Meters);
            _elem_data["quantity"] = r.Quantity;
            _elem_data["number_of_bars"] = r.NumberOfBarPositions;
            _elem_data["weight_of_meter"] = GetWeightOfMeter(r.Name);
            _elem_data["total_weight"] = _elem_data["weight_of_meter"] 
                * _elem_data["total_length"]
                * _elem_data["quantity"];

            return _elem_data;
        }

        public Dictionary<string, double> RebarInSystem_Revision(Element element)
        {
            RebarInSystem r = element as RebarInSystem;
            if (r == null)
                return null;

            Dictionary<string, double> _elem_data = new Dictionary<string, double>();

            //Autodesk.Revit.DB.Parameter diam = r.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER);
            //_elem_data["diameter"] = diam.AsDouble();

            Element _type = element.Document.GetElement(element.GetTypeId());
            Autodesk.Revit.DB.Parameter _type_d_par = _type.LookupParameter("Рзм.Диаметр");
            Autodesk.Revit.DB.Parameter d_par = element.LookupParameter("Рзм.Диаметр");
            if (d_par != null)
            {
                double parsed_diam = 0;
                double.TryParse(d_par.AsValueString(), out parsed_diam);
                _elem_data["diameter"] = parsed_diam;
                //_elem_data["weight_of_meter"] = this.GetWeightOfMeter("Ø" + d_par.AsValueString());
                _elem_data["weight_of_meter"] = this.GetWeightOfMeter("Ø" + parsed_diam);
            }
            else if (d_par == null && _type_d_par != null)
            {
                double parsed_diam = 0;
                double.TryParse(_type_d_par.AsValueString(), out parsed_diam);
                _elem_data["diameter"] = parsed_diam;
                //_elem_data["weight_of_meter"] = this.GetWeightOfMeter("Ø" + _type_d_par.AsValueString());
                _elem_data["weight_of_meter"] = this.GetWeightOfMeter("Ø" + parsed_diam);
            }
            else if (d_par == null && _type_d_par == null)
            {
                double weight = this.GetWeightOfMeter(element.Name);
                if (weight > 0)
                    _elem_data["weight_of_meter"] = weight;
            }

            if (!_elem_data.ContainsKey("weight_of_meter")) return null;

            //_elem_data["total_length"] = r.TotalLength * 304.8 / 1000;
            _elem_data["total_length"] = UnitUtils.ConvertFromInternalUnits(r.TotalLength, UnitTypeId.Meters);
            _elem_data["quantity"] = r.Quantity;
            _elem_data["number_of_bars"] = r.NumberOfBarPositions;
            _elem_data["total_weight"] = _elem_data["weight_of_meter"] 
                * _elem_data["total_length"]
                * _elem_data["quantity"];

            return _elem_data;
        }
        
        public double GetWeightOfMeter(string name)
        {
            Dictionary<string, double> gost_table =
                new Dictionary<string, double>()
                {
                    { "Ø4", 0.099 },
                    { "ø4", 0.099 },
                    { "д.4", 0.099 },

                    { "Ø5", 0.154 },
                    { "ø5", 0.154 },
                    { "д.5", 0.154 },

                    { "Ø6", 0.222 },
                    { "ø6", 0.222 },
                    { "д.6", 0.222 },

                    { "Ø8", 0.395 },
                    { "ø8", 0.395 },
                    { "д.8", 0.395 },

                    { "Ø10", 0.617 },
                    { "ø10", 0.617 },
                    { "д.10", 0.617 },

                    { "Ø12", 0.888 },
                    { "ø12", 0.888 },
                    { "д.12", 0.888 },

                    { "Ø14", 1.208 },
                    { "ø14", 1.208 },
                    { "д.14", 1.208 },

                    { "Ø16", 1.578 },
                    { "ø16", 1.578 },
                    { "д.16", 1.578 },

                    { "Ø18", 1.998 },
                    { "ø18", 1.998 },
                    { "д.18", 1.998 },

                    { "Ø20", 2.466 },
                    { "ø20", 2.466 },
                    { "д.20", 2.466 },

                    { "Ø25", 3.853 },
                    { "ø25", 3.853 },
                    { "д.25", 3.853 },

                };

            double result = 0;
            foreach (var bar in gost_table)
            {
                if (name.Contains(bar.Key)) result = bar.Value;
            }

            if (result == 0)
            {
                return 0;
            }
            else return result;
        }

    }
}
