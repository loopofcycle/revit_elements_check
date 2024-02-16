using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_product_check
{
    internal class FluctuationEvaluator
    {
        public string evaluation = "";
        
        public FluctuationEvaluator(double fluctuation)
        {
            //if (fluctuation <= 0)
            //    this.evaluation = "\n отклонение";

            if (0 < fluctuation && fluctuation < 5)
                this.evaluation = "\n допустимое отклонение";

            if (5 <= fluctuation && fluctuation <= 29)
                this.evaluation = "\n необходимость предоставления обоснования техническому заказчику";

            if (30 <= fluctuation)
                this.evaluation = "\n требуется пересчет";
        }
    }
}
