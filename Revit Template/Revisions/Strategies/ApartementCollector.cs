using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace Revit_product_check.Revisions
{
    public class ApartementCollector: ArchRevision
    {
        private class Apartement
        {
            public int number;
            public Level level;
            public Dictionary<ElementId, double> rooms_area = new Dictionary<ElementId, double>();
            public double total_area = 0;

            public Apartement(int number)
            {
                this.number = number;
            }

            public void AddRoom(ElementId room_id, double area)
            {
                this.rooms_area.Add(room_id, area);
                this.total_area += area;
            }
        }

        Dictionary<int, Apartement> Apartements = new Dictionary<int, Apartement>();

        public ApartementCollector(Document doc)
        {
            FilteredElementCollector rooms = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType();

            foreach (Element room in rooms)
            {
                Room r = room as Room;

                int number = 0;
                Parameter p = r.LookupParameter("ADSK_Номер квартиры");
                if (p != null)
                {
                    string ap_number = p.AsString();
                    int.TryParse(ap_number, out number);
                }

                var room_geo = this.GetRoomGeometry(r);
                if (!Apartements.ContainsKey(number))
                {
                    Apartement ap = new Apartement(number);
                    ap.AddRoom(r.Id, room_geo["area"]);
                    Apartements[number] = ap;
                }
                else
                {
                    Apartements[number].AddRoom(r.Id, room_geo["area"]);
                }
            }
        }
        
        private Apartement GetApartement(ElementId room_id)
        {
            Apartement result_Ap = null;

            foreach (var ap in Apartements)
            {
                if (ap.Value.rooms_area.ContainsKey(room_id))
                {
                    result_Ap = ap.Value;
                }
            }

            return result_Ap;
        }

        public string GetApartementSummary(ElementId room_id)
        {
            Apartement apartement = GetApartement(room_id);
            string result = "";
            result += "номер кв. " + apartement.number.ToString();
            result += "\nвсего комнат " + apartement.rooms_area.Keys.Count.ToString();
            result += "\nобщ. площадь " + String.Format("{0:0.00}", apartement.total_area.ToString());
            return result;
        }

        public double GetApartementArea(ElementId room_id)
        {
            Apartement apartement = GetApartement(room_id);
            return apartement.total_area;
        }

    }
}
