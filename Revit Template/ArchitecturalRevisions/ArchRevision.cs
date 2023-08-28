using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_product_check.ArchitecturalRevisions
{
    public class ArchRevision
    {
        public List<Ui.DataObject> ResultDataObject = new List<Ui.DataObject>();
        
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
    }
}
