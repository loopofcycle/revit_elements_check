using Autodesk.Revit.DB;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Revit_product_check;

namespace Revit_product_check.Revisions
{
    public class MasterPlanCutKoefRevision: ArchRevision
    {
        Autodesk.Revit.DB.Document doc = null;
        
        public MasterPlanCutKoefRevision(Autodesk.Revit.DB.Document doc)
        {
            this.doc = doc;

            FilteredElementCollector levels = new FilteredElementCollector(this.doc)
            .OfCategory(BuiltInCategory.OST_Levels)
            .WhereElementIsNotElementType();

            foreach(Level l in levels)
            {
                Ui.DataObject dataObject = GetResultObject(l);
                if(dataObject != null ) ResultDataObject.Add(dataObject);
            }

        }

        public Ui.DataObject GetResultObject(Level level)
        {
            double flats_area = GetFlatsArea(level);
            double area_inside = CalcFloorsArea(level);
            //double area_ewa = GetExternalWallsArea(level);
            double area_epa = GetExternalPerimeterArea(level);

            if (flats_area == 0 || area_inside == 0) return null;
            
            double calc_result = flats_area / area_epa;
            
            Ui.DataObject result = new Ui.DataObject()
            {
                ID = level.Id.IntegerValue,
                Category = "коэффициент подрезки",
                Standart = "",
                Value = "К0 = " + calc_result.ToString().Substring(0,4),
                Name = "",
                Group = "Помещения, наружные стены",
                Level = level.Name,
                Count =
                    "S кв = " + ((int)flats_area).ToString() + " кв.м"
                    //+ "\nS ГНС =  " + ((int)area_inside).ToString() + " кв.м"
                    + "\nS ГНС =  " + ((int)area_epa).ToString() + " кв.м",
                Result = "",
                Summ = "",
            };

            return result;
        }

        public double GetFlatsArea(Level level)
        {
            double area = 0;

            FilteredElementCollector rooms = new FilteredElementCollector(this.doc)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType();

            foreach(Room r in rooms)
            {
                if (r.LevelId != level.Id) continue;
                

                bool living_area = false;
                List<string> living_rooms = new List<string>()
                {
                    "Жилая",
                    "Гостиная",
                    "Гардеробная",
                    "Кухня-ниша",
                    "Лоджия",
                    "санузел",
                    "Сан. узел",
                    "Коридор",
                    "Кухня",
                    "Кухня-ниша",
                    "Прихожая",
                    "Мастер-спалня"
                };
                foreach (string room_name in living_rooms)
                    if(r.Name.Contains(room_name)) living_area = true;
                if(!living_area) continue;
                

                Parameter p = r.LookupParameter("Площадь");
                double room_area = p.AsDouble() / 10.7639104166;


                bool is_balcony = false;
                List<string> balcony_rooms = new List<string>()
                {
                    "Лоджия",
                };
                foreach (string room_name in balcony_rooms)
                    if (r.Name.Contains(room_name)) is_balcony = true;
                if (is_balcony) room_area = room_area /2;


                area += room_area;
            }

            return area;

        }
        
        public double CalcFloorsArea(Level level)
        {
            double surface = 0;

            FilteredElementCollector floors = new FilteredElementCollector(this.doc)
            .OfCategory(BuiltInCategory.OST_Floors)
            .WhereElementIsNotElementType();

            foreach (Element f in floors)
            {
                if (f.LevelId != level.Id) continue;

                //Parameter elev_par = f.LookupParameter("Смещение от уровня");
                //if (elev_par != null & elev_par.AsDouble() != 0) continue;

                Floor floor = (Floor)f;
                Parameter area_par = f.LookupParameter("Площадь");
                surface += area_par.AsDouble() / 10.7639104166;
            }

            return surface;
        }

        public double GetExternalWallsArea(Level level)
        {
            FilteredElementCollector walls = new FilteredElementCollector(this.doc)
            .OfCategory(BuiltInCategory.OST_Walls)
            .WhereElementIsNotElementType();

            List<Element> external_walls = new List<Element>();

            foreach (Element w in walls)
            {

                if (w.LevelId != level.Id) continue;

                WallType wt = (WallType)this.doc.GetElement(w.GetTypeId());
                if (wt.Function != WallFunction.Exterior) continue;

                external_walls.Add(w);
            }


            XYZ[] points = new XYZ[5];
            points[0] = new XYZ(0.0, 0.0, 0.0);
            points[1] = new XYZ(10.0, 0.0, 0.0);
            points[2] = new XYZ(10.0, 10.0, 0.0);
            points[3] = new XYZ(0.0, 10.0, 0.0);
            points[4] = new XYZ(0.0, 0.0, 0.0);

            CurveArray curve = new CurveArray();

            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i],
                  points[i + 1]);

                curve.Append(line);
            }

            //foreach (Wall w in external_walls)
            //{
            //    LocationCurve lc = w.Location as LocationCurve;
            //    curve.Append(lc.Curve);
            //}

            Floor fl = doc.Create.NewFloor(curve, true);

            //Getting Floor Geometry
            Face face = null;
            Options geomOptions = new Options();
            geomOptions.ComputeReferences = true;
            geomOptions.DetailLevel = ViewDetailLevel.Medium;

            Element final_floor = doc.GetElement(fl.Id);
            Floor ff = final_floor as Floor;

            GeometryElement faceGeom = ff.get_Geometry(geomOptions);

            //foreach (GeometryObject geomObj in faceGeom)
            //{
            //    Solid geomSolid = geomObj as Solid;
            //    if (null != geomSolid)
            //    {
            //        foreach (Face geomFace in geomSolid.Faces)
            //        {
            //            face = geomFace;
            //            break;
            //        }
            //        break;
            //    }
            //}

            Parameter area_par = ff.LookupParameter("Площадь");
            return area_par.AsDouble() / 10.7639104166;
        }

        public double GetExternalPerimeterArea(Level level)
        {
            double area = 0.0;

            FilteredElementCollector walls = new FilteredElementCollector(this.doc)
            .OfCategory(BuiltInCategory.OST_Walls)
            .WhereElementIsNotElementType();

            List<Element> external_walls = new List<Element>();
            List<Curve> _ext_curves = new List<Curve>();

            foreach (Element w in walls)
            {

                if (w.LevelId != level.Id) continue;

                WallType wt = (WallType)this.doc.GetElement(w.GetTypeId());
                if (wt.Function != WallFunction.Exterior) continue;

                external_walls.Add(w);
                
                LocationCurve lc = w.Location as LocationCurve;
                _ext_curves.Add(lc.Curve);
            }

            Dictionary<string, XYZ> _corner = new Dictionary<string, XYZ>()
            {
                {"Xmax", new XYZ(0,0,0) },
                {"Xmin", new XYZ(0,0,0) },
                {"Ymax", new XYZ(0,0,0) },
                {"Ymin", new XYZ(0,0,0) }
            };

            foreach (Curve curve in _ext_curves)
            {
                XYZ start = curve.GetEndPoint(0);
                XYZ end = curve.GetEndPoint(1);

                if (start.X > _corner["Xmax"].X) _corner["Xmax"] = start;
                if (start.X < _corner["Xmin"].X) _corner["Xmin"] = start;
                if (start.Y > _corner["Ymax"].Y) _corner["Ymax"] = start;
                if (start.Y < _corner["Ymin"].Y) _corner["Ymin"] = start;

                if (end.X > _corner["Xmax"].X) _corner["Xmax"] = end;
                if (end.X < _corner["Xmin"].X) _corner["Xmin"] = end;
                if (end.Y > _corner["Ymax"].Y) _corner["Ymax"] = end;
                if (end.Y < _corner["Ymin"].Y) _corner["Ymin"] = end;
            }

            double width = 304.8 * (_corner["Xmax"].X - _corner["Xmin"].X) / 1000;
            double height = 304.8 * (_corner["Ymax"].Y - _corner["Ymin"].Y) / 1000;

            area = width * height;

            return area;
        }

        public Curve FindClosestCurve(XYZ _point, List<Curve> _curves)
        {
            SortedDictionary<double, Curve> _distances = new SortedDictionary<double, Curve>();

            foreach (Curve _curve in _curves)
            {
                double dist_min = 0;

                double dist_start = _point.DistanceTo(_curve.GetEndPoint(0));
                double dist_end = _point.DistanceTo(_curve.GetEndPoint(1));

                if(dist_start < dist_end) dist_min = dist_start;
                else dist_min = dist_end;

                _distances[dist_min] = _curve;
            }

            return _distances.First().Value;
        }
    
    }
}
