using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_product_check.ConstructionRevisions
{
    internal class BasicRevision
    {
        public double elements_total_volume = 0;
        public int elements_count = 0;
        public string group = "";
        public string level = "";

        public Ui.DataObject ResultDataObject = null;

        public static string GetGroup(string title)
        {
            string result = null;

            if (title.Contains("В-1") ||
                title.Contains("В-2") ||
                title.Contains("В1") ||
                title.Contains("В2")
                )
                result = "ВК";

            if (title.Contains("Г-1") ||
                title.Contains("Г-2") ||
                title.Contains("Г1") ||
                title.Contains("Г2")
                )
                result = "ПП";

            if (title.Contains("ФП"))
                result = "ФП";

            return result;
        }

        public string GetLevel(List<Element> elements)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>()
            {
                { "level",new List<string>() },
            };

            foreach (Element element in elements)
            {
                Parameter level_par = element.LookupParameter(":Уровень Размещения");
                Parameter group_par = element.LookupParameter(":Группа элементов");

                string level = level_par.AsValueString();
                string group = group_par.AsString();

                if (!result["level"].Contains(level) && level != null)
                    result["level"].Add(level);
            }

            return result["level"].First();
        }

        public double GetElementsVolume(List<Element> elements)
        {
            double total_volume = 0;
            this.elements_count += elements.Count();

            foreach (Element element in elements)
            {
                Parameter volume_par = element.LookupParameter("Объем");
                double volume = volume_par.AsDouble() / 35.074;
                total_volume += volume;
            }

            return total_volume;

        }

    }
}
