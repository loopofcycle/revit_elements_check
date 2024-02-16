using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Revit_product_check;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_product_check.Revisions
{
    public class ArchRevision
    {
        public List<Ui.DataObject> ResultDataObject = new List<Ui.DataObject>();
        private List<Curve> perimeter = new List<Curve>();

        public Ui.DataObject GetResultObject(Document doc)
        {
            return new Ui.DataObject()
            {
                ID = -1,
                Name = "",
                Category = "",
                Standart = "",
                Group = "",
                Level = "",
                Count = "",
                Result = "",
                Summ = "",
                Value = "",
            };
        }

        public Dictionary<string, double> GetRoomGeometry(Element e)
        {
            Parameter p_area = e.LookupParameter("Площадь");
            Parameter p_height = e.LookupParameter("Полная высота");

            Dictionary<string, double> room_geometry =
                new Dictionary<string, double>()
                {
                    {"height", p_height.AsDouble() * 304.8 },
                    {"area", p_area.AsDouble() / 10.7639 },
                    {"depth", 0 },
                    {"length", 0 },
                };


            //checking room geometry
            Room r = (Room)e;
            SpatialElementBoundaryOptions op = new SpatialElementBoundaryOptions();
            op.StoreFreeBoundaryFaces = true;
            op.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreBoundary;
            IList<IList<BoundarySegment>> BGs = r.GetBoundarySegments(op) as IList<IList<BoundarySegment>>;
            if (BGs.Count == 0) return room_geometry;

            //collecting top points of room
            Dictionary<string, SortedDictionary<double, Curve>> geometry =
                new Dictionary<string, SortedDictionary<double, Curve>>()
                {
                    {"height", new SortedDictionary<double, Curve>() { { 0.0, null } } },
                    {"depth", new SortedDictionary<double, Curve>() { { 0.0, null } } },
                    {"length", new SortedDictionary<double, Curve>() { { 0.0, null } } },
                };

            double _elevation = r.Level.Elevation;
            foreach (IList<BoundarySegment> BG in BGs)
            {
                foreach (BoundarySegment seg in BG)
                {
                    Curve c = seg.GetCurve();

                    if (c == null) continue;

                    bool horizontal = (c.GetEndPoint(1).Z == c.GetEndPoint(0).Z);
                    if (!horizontal)
                        geometry["height"][c.Length] = c;

                    if (c.GetEndPoint(0).Z - _elevation > 1)
                        continue;
                    
                    bool on_perimeter = CurveOnPerimeter(c);
                    if (on_perimeter && horizontal)
                        geometry["length"][c.Length] = c;

                    if (!on_perimeter && horizontal)
                        geometry["depth"][c.Length] = c;
                }
            }

            double summary_length = 0;
            foreach(var l in geometry["length"].Keys)
                summary_length += l;
            room_geometry["length"] = summary_length * 304.8;

            room_geometry["depth"] = geometry["depth"].Last().Key * 304.8;

            return room_geometry;

        }

        private bool CurveOnPerimeter(Curve c)
        {
            bool result = false;
            double distance = 1;

            XYZ start = c.GetEndPoint(0);
            XYZ end = c.GetEndPoint(1);

            foreach (Curve _perimeter_c in perimeter)
            {
                if (_perimeter_c.Distance(start) < distance &&
                    _perimeter_c.Distance(end) < distance)
                    result = true;
            }

            return result;
        }

        public Dictionary<string, CheckRule> SetRules(double apartement_area)
        {
            Dictionary<string, CheckRule> room_rules = new Dictionary<string, CheckRule>();

            //if (apartement_area < 25)
            //{
            //    CheckRule rule57 = new CheckRule(57, "Минимальные площади с/у\r\n -квартира S<25 м.кв., до 3,7 м.кв.\r\n -квартира S>25 м.кв., более 3,7 м.кв.",
            //        new KeyValuePair<string, double>("area", 3.7));
            //    room_rules.Add("Совмещенный санузел", rule57);
            //}

            CheckRule rule39 = new CheckRule(39, "Высота коммерческих помещений минимум 3000 от пола до потолка (до зашивки запотолочного пространства, с учетом всех возможных технологий использования)",
                new KeyValuePair<string, double>("height", 3000));
            room_rules.Add("Нежилое помещение", rule39);

            CheckRule rule33 = new CheckRule(33, "Глубина тамбура не менее 2800 мм",
                new KeyValuePair<string, double>("depth", 2800));
            room_rules.Add("Тамбур", rule33);

            //CheckRule rule12 = new CheckRule(12, "Электрощитовая не жилой части площадью не более 10 м.кв., не располагается под, смежно или над жилыми комнатами",
            //    new KeyValuePair<string, double>("area", 10));
            //room_rules.Add("Электрощитовая", rule12);

            //CheckRule rule10 = new CheckRule(10, "Помещение СС площадью не более 16 м.кв, не располагается под, смежно или над жилыми комнатами",
            //    new KeyValuePair<string, double>("area", 16));
            //room_rules.Add("Помещение связи", rule10);

            //CheckRule rule61 = new CheckRule(61, "Минимальная площадь спальни 10 м.кв.",
            //    new KeyValuePair<string, double>("area", 10));
            //room_rules.Add("Жилая комната", rule61);

            return room_rules;
        }
    }
}
