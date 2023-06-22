using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit_EIR_check
{
    internal class ParameterRevisionStrategy
    {

        public string Result { get; set; }

        public ParameterRevisionStrategy(Element e, string parameter_name, string condition)
        {
            Parameter p = e.LookupParameter(parameter_name);
            if (p != null )
            {
                if(p.HasValue == false)
                {
                    this.Result = $"параметр не заполнен";
                }
                else
                {
                    switch (condition)
                    {
                        case "not_empty":
                            break;
                        
                        case ">1":
                            if ( p.AsInteger() < 1) 
                            {
                                this.Result = $"параметр меньше единицы";
                            }
                            break;
                    }
                }
            }
            else
            {
                this.Result = $"параметр отсутствует у элемента";
            }
        }
    }
}
