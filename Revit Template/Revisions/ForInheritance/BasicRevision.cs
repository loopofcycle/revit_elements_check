using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DocumentFormat.OpenXml;

namespace Revit_product_check.Revisions
{
    internal class BasicRevision
    {
        public double elements_total_volume = 0;
        public int elements_count = 0;
        public string group = "";
        public string level = "";

        public Ui.DataObject ResultDataObject = null;

        public string GetLevel(List<Element> elements)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>()
            {
                { "level",new List<string>() },
            };

            foreach (Element element in elements)
            {
                Parameter level_par = element.LookupParameter(":Уровень Размещения");
                if (level_par == null) continue;
                string level = level_par.AsValueString();

                if (!result["level"].Contains(level) && level != null)
                    result["level"].Add(level);
            }

            if (result["level"].Count > 0 )
                return result["level"].First();
            else
                return "Уровень не определен";
        }

        public double GetElementsVolume(List<Element> elements, FormatOptions fo)
        {
            double total_volume = 0;
            this.elements_count += elements.Count();

            foreach (Element element in elements)
            {
                Parameter volume_par = element.LookupParameter("Объем");

                double rawvalue = volume_par.AsDouble();

                double old_volume = UnitUtils.
                    ConvertFromInternalUnits(rawvalue, fo.GetUnitTypeId());

                double volume = UnitUtils.
                    ConvertFromInternalUnits(rawvalue, UnitTypeId.CubicMeters);

                total_volume += volume;
            }

            return total_volume;

        }

        public static Dictionary<string, string> GetBuildingInfo(string title)
        {
            Dictionary<string, string> results = new Dictionary<string, string>()
            {
                { "title", title },
                { "project", "-"},
                { "section", "-" },
                { "levels_quantity", "-" },
                { "doc_complect", "-" },
            };

            foreach (string s in title.Split('_'))
            {
                if (s.Contains("КЖ")) results["doc_complect"] = s;
                if (s.Contains("S")) results["section"] = s;
            }

            return results;
        }

        public Dictionary<ElementId, ElementsInfo> GetVolumeInfoByLevel(List<Element> elements)
        {
            Dictionary<ElementId, ElementsInfo> volume_info = new Dictionary<ElementId, ElementsInfo>();
            this.elements_count += elements.Count();

            foreach (Element element in elements)
            {
                Parameter volume_par = element.LookupParameter("Объем");
                double old_volume = volume_par.AsDouble() / 35.074;
                double volume = UnitUtils.ConvertFromInternalUnits(volume_par.AsDouble(), UnitTypeId.CubicMeters);

                if (!volume_info.ContainsKey(element.LevelId))
                {
                    volume_info[element.LevelId] = new ElementsInfo(element.Id, volume);
                }
                else
                {
                    volume_info[element.LevelId].summary += volume;
                    volume_info[element.LevelId].id_with_values.Add(element.Id, volume);
                }
            }

            return volume_info;
        }

        public Dictionary<ElementId, ElementsInfo> GetFloorsVolumeInfoByLevel(List<Element> elements)
        {
            Dictionary<ElementId, ElementsInfo> volume_info = new Dictionary<ElementId, ElementsInfo>();
            this.elements_count += elements.Count();

            foreach (Element element in elements)
            {
                Parameter volume_par = element.LookupParameter("Объем");
                double old_volume = volume_par.AsDouble() / 35.074;
                double volume = UnitUtils.ConvertFromInternalUnits(volume_par.AsDouble(), UnitTypeId.CubicMeters);

                if (!volume_info.ContainsKey(element.LevelId))
                {
                    volume_info[element.LevelId] = new ElementsInfo(element.Id, volume);
                }
                else
                {
                    volume_info[element.LevelId].summary += volume;
                    volume_info[element.LevelId].id_with_values.Add(element.Id, volume);
                }
            }

            return volume_info;
        }
    }
}
