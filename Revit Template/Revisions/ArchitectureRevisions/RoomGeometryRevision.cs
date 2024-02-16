using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Revit_product_check;
using Revit_product_check.Revisions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Revit_product_check.Revisions
{
    public class RoomGeometryRevision: ArchRevision
    {

        //private List<Wall> walls = new List<Wall>();
        private List<Curve> exterior_curves = new List<Curve>();
        private List<Curve> perimeter = new List<Curve>();
        private ApartementCollector ap_collector = null;
        private Dictionary<string, CheckRule> CheckingRulesForRooms = new Dictionary<string, CheckRule>();


        public RoomGeometryRevision(Document doc)
        {
            this.ap_collector = new ApartementCollector(doc);
            this.CheckingRulesForRooms = SetRules(apartement_area: 0);
            
            FilteredElementCollector walls_col
               = new FilteredElementCollector(doc)
               .WhereElementIsNotElementType()
               .OfCategory(BuiltInCategory.OST_Walls);

            foreach (Wall wall in walls_col.ToList())
            {
                if (wall.WallType.Function != WallFunction.Exterior)
                    continue;

                foreach (Curve c in GetCurves(doc, wall))
                {
                    this.exterior_curves.Add(c);
                }

                //this.walls.Add(wall);
            }

            ResultDataObject = GetResultObject(doc);
        }

        public new List<Ui.DataObject> GetResultObject(Document doc)
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
            //defining rule for room
            CheckRule rule = null;
            CheckingRulesForRooms = 
                SetRules(apartement_area: ap_collector.GetApartementArea(e.Id));
            foreach (string name in CheckingRulesForRooms.Keys)
            {
                if (e.Name.Contains(name))
                {
                    rule = CheckingRulesForRooms[name];
                }
            }

            //skip if there is no rules
            if (rule == null) { return null; }
            
            //calculating geometry, perimeter, etc.
            Level level = (Level)doc.GetElement(e.LevelId);
            this.perimeter = SetPerimeter(level.Elevation);
            Dictionary<string, double> room_geometry = this.GetRoomGeometry(e);

            //checking for product rules
            string dim = rule.geometry_dict.First().Key;
            double limit = rule.geometry_dict.First().Value;
            string standart_as_str = dim + " > " + limit;
            
            //creating data objects with results
            Ui.DataObject result_DO = null;
            if (room_geometry[dim] < limit)
            {
                //adding info about apartement
                string appart_summary = this.ap_collector.GetApartementSummary(e.Id);

                result_DO = new Ui.DataObject()
                {
                    ID = e.Id.IntegerValue,
                    Check_ID = rule.ID,
                    Check_Text = rule.Text,
                    Category = "проверка размеров помещений",
                    Standart = standart_as_str,
                    Value = "Высота: " + String.Format("{0:0.00}", room_geometry["height"])
                        + "\nГлубина: " + String.Format("{0:0.00}", room_geometry["depth"])
                        + "\nН.стена: " + String.Format("{0:0.00}", room_geometry["length"])
                        + "\nПлощадь: " + String.Format("{0:0.00}", room_geometry["area"]),
                    Name = e.Name,
                    Group = "Помещения",
                    Level = level.Name,
                    Result = "проверка не пройдена",
                    Summ = appart_summary,
                };
            }

            return result_DO;
        }

        private List<Curve> GetCurves(Document doc, Wall wall)
        {
            IList<CurveLoop> _curve_loops = new List<CurveLoop>();

            IList<Reference> _exterior =
                HostObjectUtils.GetSideFaces(wall, ShellLayerType.Exterior);

            if (_exterior != null)
            {
                foreach (Reference _ext in _exterior)
                {
                    Element _ext2 = doc.GetElement(_ext);
                    Face ext_face = _ext2.GetGeometryObjectFromReference(_ext) as Face;

                    if (ext_face == null) continue;

                    foreach (CurveLoop cl in ext_face.GetEdgesAsCurveLoops())
                    {
                        _curve_loops.Add(cl);
                    }
                }
            }

            IList<Reference> _interior =
                HostObjectUtils.GetSideFaces(wall, ShellLayerType.Interior);

            if (_interior != null)
            {
                foreach (Reference _inter in _interior)
                {
                    Element int_e2 = doc.GetElement(_inter);
                    Face interior_face = int_e2.GetGeometryObjectFromReference(_inter) as Face;

                    if (interior_face == null) continue;

                    foreach (CurveLoop cl in interior_face.GetEdgesAsCurveLoops())
                    {
                        _curve_loops.Add(cl);
                    }
                }
            }

            List<Curve> _curves = new List<Curve>();
            foreach (CurveLoop cl in _curve_loops)
            {
                CurveLoopIterator cli = cl.GetCurveLoopIterator();
                _curves.Add(cli.Current);

                while (cli.MoveNext())
                {
                    _curves.Add(cli.Current);
                }
            }

            return _curves;
        }

        private List<Curve> SetPerimeter(double lvl_elevation)
        {
            List<Curve> _result = new List<Curve>();

            foreach (Curve _curve in this.exterior_curves)
            {
                if (_curve.GetEndPoint(0).Z != _curve.GetEndPoint(1).Z) continue;

                if (_curve.GetEndPoint(0).Z - lvl_elevation > 0.2) continue;

                _result.Add(_curve);
            }

            return _result;
        }
    }
}
