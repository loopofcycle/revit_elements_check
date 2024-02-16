using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using ClosedXML.Excel;
using Revit_product_check;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Revit_product_check.Revisions
{
    public class RoomPlacementRevision: ArchRevision
    {
        private Dictionary<string, CheckRule> CheckingRulesForRooms = new Dictionary<string, CheckRule>();
        public RoomPlacementRevision(Document doc)
        {
            this.CheckingRulesForRooms = SetRules(apartement_area: 100);
            ResultDataObject = GetResultObject(doc);
        }
        public new Dictionary<string, CheckRule> SetRules(double apartement_area)
        {
            Dictionary<string, CheckRule> room_rules = new Dictionary<string, CheckRule>();

            var rule10 = new CheckRule(10,
                "Помещение СС площадью не более 16 м.кв, не располагается под, смежно или над жилыми комнатами",
                new KeyValuePair<string, double>("", 0));

            var rule12 = new CheckRule(12,
                "Электрощитовая не жилой части площадью не более 10 м.кв., не располагается под, смежно или над жилыми комнатами",
                new KeyValuePair<string, double>("", 0));

            var rule14 = new CheckRule(14,
                "Помещение венткамеры общеобменной вентиляции не располагается под, смежно или над жилыми комнатами",
                new KeyValuePair<string, double>("", 0));

            var rule17 = new CheckRule(17,
                "Помещения электрощитовых и СС не располагаются под санузлами и ванными комнатами, в том числе помещений ПОН",
                new KeyValuePair<string, double>("", 0));

            room_rules = new Dictionary<string, CheckRule>()
            {
                { "Электрощитовая", rule12 },
                { "Помещение СС", rule10 },
                { "Помещение связи", rule17 },
                { "Венткамера", rule14 }
            };

            return room_rules;
        }
        public List<Ui.DataObject> GetResultObject(Document doc)
        {
            FilteredElementCollector rooms = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType();

            List<Ui.DataObject> _result = new List<Ui.DataObject>();
            
            foreach (var room in rooms)
            {
                Ui.DataObject obj = CheckRoom(doc, room);
                if (obj != null) _result.Add(obj);
            }

            return _result;
        }
        private Ui.DataObject CheckRoom(Document doc, Element e)
        {
            CheckRule rule = this.NeedsChecking(e.Name);
            if (rule == null) { return null; }

            List<XYZ> check_points = this.FindCheckPoints(e);

            //collecting rooms above
            Dictionary<ElementId, Location> rooms_above = new Dictionary<ElementId, Location>();
            
            FilteredElementCollector rooms = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType();

            double _lift_point = 10;
            var _above_dict = this.FindLevelAbove(doc, e.LevelId);

            bool is_anything_above = false;
            foreach(XYZ p in check_points)
            {
                //XYZ scanning_point = new XYZ(p.X, p.Y, p.Z + scanning_distance);
                XYZ scanning_point = new XYZ(p.X, p.Y, _above_dict.Value + _lift_point);
                var scanning_points = this.MultiplyPoint(scanning_point, 1);

                foreach(Room room in rooms)
                {
                    foreach(XYZ _p in scanning_points)
                    {
                        if (room.IsPointInRoom(_p))
                    {
                        is_anything_above = true;
                        rooms_above[room.Id] = room.Location;
                    }
                    }
                }
            }
            if (!is_anything_above) return null;
            
            //searching for living rooms above
            bool _under_living_area = false;
            List<string> living_rooms = new List<string>() { "Жилая комната" };
            string text_result = "выше находятся помещения:";
            foreach(ElementId room_id in rooms_above.Keys.ToList())
            {
                Room _room = (Room)doc.GetElement(room_id);
                text_result += "\n" + _room.Name;
                text_result += "\t" + _room.Id.ToString();

                foreach (var name in living_rooms)
                {
                    if (_room.Name.Contains(name))
                        _under_living_area = true;
                }

            }

            Ui.DataObject result_DO = null;
            if (_under_living_area)
            {
                Element level = (Level)doc.GetElement(e.LevelId);
                result_DO = new Ui.DataObject()
                {
                    ID = e.Id.IntegerValue,
                    Check_ID = rule.ID,
                    Check_Text = rule.Text,
                    Category = "проверка расположения технических помещений",
                    Standart = "",
                    Value = text_result,
                    Name = e.Name,
                    Group = "Помещения",
                    Level = level.Name,
                    Count = "",
                    Result = "проверка не пройдена",
                    Summ = "",
                };
            }

            return result_DO;
        }
        private KeyValuePair<ElementId, double> FindLevelAbove(Document doc, ElementId current_level_id)
        {
            FilteredElementCollector levels = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Levels)
            .WhereElementIsNotElementType();
            SortedDictionary<double, ElementId> levels_elevation
                = new SortedDictionary<double, ElementId>();
            foreach (Element l in levels)
            {
                Level lvl = (Level)l;
                levels_elevation.Add(lvl.Elevation, l.Id);
            }

            int index = levels_elevation.Values.ToList().IndexOf(current_level_id);
            ElementId result_id = levels_elevation.Values.ToList()[index + 1];
            double elevation = levels_elevation.Keys.ToList()[index + 1];

            return new KeyValuePair<ElementId, double> ( result_id, elevation );
        }
        private List<XYZ> MultiplyPoint(XYZ point, double offset)
        {
            List<XYZ> _points = new List<XYZ>()
            {
                new XYZ (point.X + offset, point.Y + offset, point.Z),
                new XYZ (point.X + offset, point.Y - offset, point.Z),
                new XYZ (point.X - offset, point.Y + offset, point.Z),
                new XYZ (point.X - offset, point.Y - offset, point.Z),
            };
            return _points;
        }

        private CheckRule NeedsChecking(string room_name)
        {
            //checikng rooms name
            bool _matched = false;
            CheckRule rule = null;
            foreach (string name in this.CheckingRulesForRooms.Keys)
            {
                if (room_name.Contains(name))
                {
                    _matched = true;
                    rule = this.CheckingRulesForRooms[name];
                }
            }
            if (!_matched) { return null; }

            return rule;
        }

        private List<XYZ> FindCheckPoints(Element e)
        {
            //checking room geometry
            Room r = (Room)e;
            SpatialElementBoundaryOptions op = new SpatialElementBoundaryOptions();
            op.StoreFreeBoundaryFaces = true;
            op.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreBoundary;
            IList<IList<BoundarySegment>> BGs = r.GetBoundarySegments(op) as IList<IList<BoundarySegment>>;
            if (BGs.Count == 0) return null;

            //collecting top points of room
            Dictionary<double, Dictionary<XYZ, bool>> top_points_ =
                new Dictionary<double, Dictionary<XYZ, bool>>();

            foreach (IList<BoundarySegment> BG in BGs)
            {
                foreach (BoundarySegment seg in BG)
                {
                    Curve c = seg.GetCurve();

                    if (c == null) continue;

                    XYZ higher_point = c.GetEndPoint(0);
                    if (c.GetEndPoint(1).Z > c.GetEndPoint(0).Z)
                        higher_point = c.GetEndPoint(1);

                    if (top_points_.ContainsKey(higher_point.Z))
                        top_points_[higher_point.Z][higher_point] = true;
                    else
                        top_points_[higher_point.Z] =
                            new Dictionary<XYZ, bool>() { { higher_point, true } };
                }
            }

            List<XYZ> check_points = top_points_.First().Value.Keys.ToList();

            return check_points;
        }
    }
}
