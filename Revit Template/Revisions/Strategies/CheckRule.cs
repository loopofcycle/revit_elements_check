using System.Collections.Generic;

namespace Revit_product_check
{
    public class CheckRule
    {
        public int ID { get; set; }
        public string Text { get; set; }

        public Dictionary<string, double> geometry_dict
            = new Dictionary<string, double>();

        public CheckRule(int id, string text, KeyValuePair<string, double> _kv_pair)
        {
            this.ID = id;
            this.Text = text;
            this.geometry_dict.Add(_kv_pair.Key, _kv_pair.Value);
        }
    }
}
