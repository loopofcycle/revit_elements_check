using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using Revit_product_check.Revisions;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Revit_product_check.ArchitecturalRevisions;
using Revit_product_check.ConstructionRevisions;

namespace Revit_product_check
{
    /// <summary>
    /// Create methods here that need to be wrapped in a valid Revit Api context.
    /// Things like transactions modifying Revit Elements, etc.
    /// </summary>
    internal class Methods
    {
        /// <summary>
        /// Check all elements according to product rules
        /// </summary>
        public static List<Ui.DataObject> CheckElements(Ui ui, Document doc)
        {
            ui.Dispatcher.Invoke(() => ui.TbDebug.Text += "\n" + (DateTime.Now).ToLongTimeString() + "\t" + doc.Title);
            Util.LogThreadInfo("Product checkup");

            // creating results with summary revision
            List<Ui.DataObject> results = new List<Ui.DataObject>();

            if (doc.Title.Contains("КЖ"))
            {
                results.Add(new Ui.DataObject
            {
                Name = "Наименование",
                Category = "Категория",
                Standart =  "Стандартное значение",
                Value = "Расчетное значение",
                Fluctuation = "Отклонение",
                Result = "Результат проверки",
                Group = ":Группа элементов",
                Level = ":Уровень размещения",
                Count = "Количество",
                Summ = "Сумма",
            });
                switch (MetallRevision.GetGroup(doc.Title))
                {
                    case "ВК":
                        WallMetallRevision walls_rev = new WallMetallRevision(doc);
                        results.Add(walls_rev.ResultDataObject);

                        WallConcreteRevision walls_concrete_rev = new WallConcreteRevision(doc);
                        results.Add(walls_concrete_rev.ResultDataObject);
                        break;
                    
                    case "ПП":
                        FloorMetallRevision floor_rev = new FloorMetallRevision(doc);
                        results.Add(floor_rev.ResultDataObject);

                        FloorsConcreteRevision floor_concrete_rev = new FloorsConcreteRevision(doc);
                        results.Add(floor_concrete_rev.ResultDataObject);
                        break;
                    
                    case "ФП":
                        FoundationMetallRevision foundation_rev = new FoundationMetallRevision(doc);
                        results.Add(foundation_rev.ResultDataObject);

                        FoundationConcreteRevision foundation_concrete_rev = new FoundationConcreteRevision(doc);
                        results.Add(foundation_concrete_rev.ResultDataObject);
                        break;
                }
            }

            if (doc.Title.Contains("АР"))
            {
                results.Add(new Ui.DataObject
                {
                    Name = "Наименование",
                    Category = "Категория",
                    Standart = "Стандартное значение",
                    Value = "Расчетное значение",
                    Result = "Результат проверки",
                    Group = "Элементы",
                    Level = "Уровень размещения",
                    Count = "Количество",
                    Summ = "Сумма",
                });

                MP_Revision mp_rev = new MP_Revision(doc);
                foreach (var data_obj in mp_rev.ResultDataObject)
                    results.Add(data_obj);

                RoomRevision room_rev = new RoomRevision(doc);
                foreach (var data_obj in room_rev.ResultDataObject)
                    results.Add(data_obj);
            }

            // format the message to  show the number of walls in the project
            string message = $"There are {results.Count} rows in the results";

            // invoke the UI dispatcher to print the results to the UI
            ui.Dispatcher.Invoke(() =>
            ui.TbDebug.Text += "\n" + (DateTime.Now).ToLongTimeString() + "\t" + message);

            return results;
        }

        /// <summary>
        /// Export results of analysis to html file with table.
        /// </summary>
        public static void ExportResultstoHTML(List<Ui.DataObject> results, Document doc)
        {

            DataTable results_dt = new DataTable("Results");
            results_dt.Columns.Add("ID", typeof(int));
            results_dt.Columns.Add("Name", typeof(string));
            results_dt.Columns.Add("Category", typeof(string));
            results_dt.Columns.Add("Result", typeof(string));
            results_dt.Columns.Add("Value", typeof(double));

            foreach (Ui.DataObject obj in results)
            {
                DataRow row = results_dt.NewRow();
                row["ID"] = obj.ID;
                row["Name"] = obj.Name;
                row["Category"] = obj.Category;
                row["Result"] = obj.Result;
                row["Value"] = obj.Value;
                results_dt.Rows.Add(row);
            }

            string result_string = DTtoHTML.toHTML_Table(results_dt);
            File.WriteAllText(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    doc.Title + "_check.html"), 
                result_string);
        }
        
        /// <summary>
        /// Select elemtn on model
        /// <param name="ui">An instance of our UI class, which in this template is the main WPF
        /// window of the application.</param>
        /// <param name="doc">The Revit Document to count the walls of.</param>
        /// </summary>
        public static void SelectElement(Ui ui, Document doc)
        {
            Ui.DataObject dataObj = (Ui.DataObject)ui.dataGrid1.SelectedItem;
            string message = $"Choosen element is {dataObj.ID}";

            FloorCreator fl = new FloorCreator(doc);

            ElementId eid = new ElementId(dataObj.ID);
            ICollection<ElementId> ids = new List<ElementId>();
            ids.Add(eid);
            UIDocument uiDoc = new UIDocument(doc);

            using (Transaction t = new Transaction(doc, "Showing element"))
            {
                Util.LogThreadInfo("Selecting element");

                // start a transaction within the valid Revit API context
                t.Start("Rename Sheets");

                uiDoc.Selection.SetElementIds(ids);
                uiDoc.ShowElements(ids);

                t.Commit();
                t.Dispose();
            }

            ui.Dispatcher.Invoke(() => ui.TbDebug.Text += "\n" + (DateTime.Now).ToLongTimeString() + "\t" + message);

        }

    }
}