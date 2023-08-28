using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using Document = Autodesk.Revit.DB.Document;

namespace Revit_product_check
{
    public class FloorCreator
    {
        public FloorCreator(Document doc)
        {
            Transaction trans = new Transaction(doc);
            try
            {
                trans.Start();
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

                Floor fl = doc.Create.NewFloor(curve, true);

            }
            catch (Exception e)
            {
                trans.RollBack();
                //msg = e.Message;
                //return Result.Failed;
            }
            trans.Commit();
            //return Result.Succeeded;
        }
    }
}
