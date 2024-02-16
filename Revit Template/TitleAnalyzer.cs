using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace Revit_product_check
{
    public class TitleAnalyzer
    {
        public bool decoded = false;

        public string code;
        public string stage;
        public string chapter;
        public string section;
        public string standart;
        public string bim_version;
        public string comment;
        public string group = null;

        public Dictionary<string, string> parts = new Dictionary<string, string>()
        {
            {"code", ""},
            {"stage", ""},
            {"chapter", ""},
            {"section", ""},
            {"standart", ""},
            {"bim_version", ""},
            {"comment", ""}
        };

        public TitleAnalyzer(string title)
        {
            var splited_title = title.Split('_');
            if (splited_title.Length > 6)
            {
                this.code = splited_title[0];
                this.stage = splited_title[1];
                this.chapter = splited_title[2];
                this.section = splited_title[3];
                this.standart = splited_title[4];
                this.bim_version = splited_title[5];
                this.comment = splited_title[6];
                this.decoded = true;

                if(stage == "Р" && chapter.Contains("КЖ"))
                {
                    this.group = GetGroup(title);
                }

            }
            else if (splited_title.Length == 6)
            {
                this.code = splited_title[0];
                this.stage = splited_title[1];
                this.chapter = splited_title[2];
                this.section = splited_title[3];
                this.standart = splited_title[4];
                this.bim_version = splited_title[5];
                this.decoded = true;

                if (stage == "Р" && chapter.Contains("КЖ"))
                {
                    this.group = GetGroup(title);
                }

            }
            else
            {
                this.decoded = false;
            }
        }
        public string GetGroup(string title)
        {
            string result = null;

            if (this.comment.Contains("ALL"))
                result = "ALL";

            if (this.comment.Contains("ФП"))
                result = "ФП";

            if (this.comment.StartsWith("В") && !this.comment.Contains("-"))
                result = "В*";

            if (this.comment.StartsWith("В") && this.comment.Contains("-"))
                result = "В*-В*";

            if (this.comment.StartsWith("Г") && !this.comment.Contains("-"))
                result = "Г*";

            if (this.comment.StartsWith("Г") && this.comment.Contains("-"))
                result = "Г*-Г*";

            if (this.comment.Contains("П"))
                result = "ФП";

            if (this.comment.StartsWith("Ростверк"))
                result = "ФП";

            return result;
        }
    }
}
