using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Revit_product_check.ConstructionRevisions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Revit_product_check.Revisions
{
    internal class MetallRevision: BasicRevision
    {
        public double rebar_total_mass = 0;
        public int rebar_count = 0;
        public int rebar_NOT_counted = 0;

        public Ui.DataObject GetResultObject(string title)
        {
            var _end_result = (int)(rebar_total_mass / elements_total_volume);

            int limit = GetLimitValue(GetGroup(title), level);
            double fluctuation = 100 * (_end_result - limit) / limit;

            string result_as_string = "проверка пройдена";
            if (_end_result > limit)
                result_as_string = "проверка не пройдена";
                fluctuation = 100 * (limit - _end_result) / limit;


            ResultDataObject = new Ui.DataObject()
            {
                ID = -1,
                Name = "проверка металлоемкости",

                Category =
                    "количество конструктивных элементов " + elements_count + " шт."
                    + "\n общий объем элементов " + elements_total_volume + " куб.м"
                    + "\n не учтен. элементы арматуры " + rebar_NOT_counted + " шт.",
                
                Standart = limit.ToString() + " кг./куб.м",
                Group = GetGroup(title),
                Level = level,
                Count = rebar_count.ToString(),
                Fluctuation = fluctuation.ToString().Substring(0,2) + "%",
                Result = result_as_string,
                Summ = rebar_total_mass + " кг.",
                Value = _end_result.ToString(),
            };

            return ResultDataObject;
        }

        public double GetRebarsMass(List<Element> elements)
        {
            double total_mass = 0;

            foreach (Element element in elements)
            {
                Rebar rebar = element as Rebar;
                if (rebar != null)
                {
                    total_mass += Rebar_Revision(rebar)["total_weight"];
                    rebar_count += 1;
                    continue;
                }

                RebarInSystem ris = element as RebarInSystem;
                if (ris != null)
                {
                    total_mass += RebarInSystem_Revision(ris)["total_weight"];
                    rebar_count += 1;
                    continue;
                }
                
                if(rebar == null && ris == null)
                {
                    Parameter mass_par = element.LookupParameter(":Масса");
                    if (mass_par != null)
                    {
                        total_mass += mass_par.AsDouble();
                        rebar_count += 1;
                    }
                }
            }

            return total_mass;

        }

        public double GetModelsMass(List<Element> elements)
        {
            double total_mass = 0;

            Document doc = elements.First().Document;

            foreach (Element element in elements)
            {
                Parameter mass_par = element.LookupParameter(":Масса");

                Element _type_element = doc.GetElement(element.GetTypeId());
                Parameter _type_mass_par = _type_element.LookupParameter(":Масса");
                
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

        public int GetLimitValue(string group, string level)
        {
            int result = 0;

            var strana_standart = new Dictionary<string, Dictionary<string, int>>()
            {
                { "ФП", new Dictionary<string, int>() { 
                    { "default", 110 } } },
                
                { "ВК", new Dictionary<string, int>() { 
                        { "-1", 185 },
                        { "1", 170 },
                        { "2", 160 },
                        { "default", 125 } } },
                
                { "ПП", new Dictionary<string, int>() {
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

        public Dictionary<string, double> Rebar_Revision(Rebar r)
        {
            Dictionary<string, double> _elem_data = new Dictionary<string, double>();

            Parameter diam = r.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER);
            _elem_data["diameter"] = diam.AsDouble();
            _elem_data["total_length"] = r.TotalLength * 304.8 / 1000;
            _elem_data["quantity"] = r.Quantity;
            _elem_data["number_of_bars"] = r.NumberOfBarPositions;
            _elem_data["weight_of_meter"] = GetWeightOfMeter(r.Name);
            _elem_data["total_weight"] = _elem_data["weight_of_meter"] * _elem_data["total_length"];

            return _elem_data;
        }

        public Dictionary<string, double> RebarInSystem_Revision(RebarInSystem r)
        {
            Dictionary<string, double> _elem_data = new Dictionary<string, double>();

            Parameter diam = r.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER);
            _elem_data["diameter"] = diam.AsDouble();
            _elem_data["total_length"] = r.TotalLength * 304.8 / 1000;
            _elem_data["quantity"] = r.Quantity;
            _elem_data["number_of_bars"] = r.NumberOfBarPositions;
            _elem_data["weight_of_meter"] = GetWeightOfMeter(r.Name);
            _elem_data["total_weight"] = _elem_data["weight_of_meter"] * _elem_data["total_length"];

            return _elem_data;
        }

        public double GetWeightOfMeter(string name)
        {
            Dictionary<string, double> gost_table =
                new Dictionary<string, double>()
                {
                    { "Ø4", 0.099 },
                    { "Ø5", 0.154 },
                    { "Ø6", 0.222 },
                    { "Ø8", 0.395 },
                    { "Ø10", 0.617 },
                    { "Ø12", 0.888 },
                    { "Ø14", 1.208 },
                    { "Ø16", 1.578 },
                    { "Ø18", 1.998 },
                    { "Ø20", 2.466 },
                };

            double result = 0;
            foreach (var bar in gost_table)
            {
                if (name.Contains(bar.Key)) result = bar.Value;
            }

            if (result == 0) return 0;
            else return result;
        }

        public static Solid GetTargetSolids(Element element)
        {
            List<Solid> solids = new List<Solid>();

            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement geomElem = element.get_Geometry(options);
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)
                {
                    Solid solid = (Solid)geomObj;
                    if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                    {
                        solids.Add(solid);
                    }
                    // Single-level recursive check of instances. If viable solids are more than
                    // one level deep, this example ignores them.
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstance geomInst = (GeometryInstance)geomObj;
                    GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                    foreach (GeometryObject instGeomObj in instGeomElem)
                    {
                        if (instGeomObj is Solid)
                        {
                            Solid solid = (Solid)instGeomObj;
                            if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                            {
                                solids.Add(solid);
                            }
                        }
                    }
                }
            }
            return solids.FirstOrDefault();
        }

    }
}
