using System;
using System.Collections.Generic;
using System.Data;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using Revit_product_check.DB_connection;
using System.Linq;

using Revit_product_check.Revisions;

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

            string GetProjectLocal(string project_number)
            {
                string result = "не определен";

                var db_copy = new Dictionary<string, string>()
                {
                    {"03101-ТМН-ЕБ2-01", "С. Европейский берег (29 Га) ГП-1" },
                    { "СТ-01", "С. Сибирский тракт 2 этап" },
                    { "03101", "Европейский берег (29 Га)" },
                    { "ШИФР", "Тестовый проект" },
                    { "03302_Р_", "С. Ирбитская ГП-2" },
                    { "3101-ТМН-ЕБ2-01", "Европейский берег (29 Га)" },
                };

                if (db_copy.Keys.Contains(project_number))
                    result = db_copy[project_number];

                return result;
            }

            ui.Dispatcher.Invoke(() => ui.TbDebug.Text += "\n" + (DateTime.Now).ToLongTimeString() + "\t" + doc.Title);
            Util.LogThreadInfo("Product checkup");

            var title = new TitleAnalyzer(doc.Title);
            
            // creating results with summary revision
            List<Ui.DataObject> results = new List<Ui.DataObject>();

            if (!title.decoded)
            {
                results.Add(new Ui.DataObject
                {
                    Name = "Не распознано имя файла",
                });
            }
            else if (!title.chapter.Contains("АР") && !title.chapter.Contains("КЖ"))
            {
                results.Add(new Ui.DataObject
                {
                    Name = "Запустите проверку на файле АР или КЖ",
                });
            }
            else if (title.decoded && title.chapter.Contains("АР"))
            {

                RoomPlacementRevision room_plcmnt_rev = new RoomPlacementRevision(doc);
                var placement_objs = room_plcmnt_rev.GetResultObject(doc);
                
                RoomGeometryRevision room_geom_rev = new RoomGeometryRevision(doc);
                var geometry_objs = room_geom_rev.GetResultObject(doc);
                
                if((placement_objs.Count + geometry_objs.Count) > 0)
                {
                    results.Add(new Ui.DataObject
                {
                    Name = "Наименование",
                    Check_Text = "Описание",
                    Category = "Категория",
                    Standart = "Стандартное\nзначение",
                    Value = "Расчетное\nзначение",
                    Fluctuation = "Отклонение (%)",
                    Result = "Результат",
                    Group = "Группа",
                    Level = "Уровень",
                    Count = "Количество",
                    Summ = "Сумма",
                });
                    results.AddRange(placement_objs);
                    results.AddRange(geometry_objs);
                }
                else
                {
                    results.Add(new Ui.DataObject
                    {
                        Name = "Все проверки успешно пройдены",
                        Check_Text = "",
                        Category = "",
                        Standart = "",
                        Value = "",
                        Fluctuation = "",
                        Result = "",
                        Group = "",
                        Level = "",
                        Count = "",
                        Summ = "",
                    });
                }
            }
            else if (title.decoded && title.chapter.Contains("КЖ"))
            {
                results.Add(new Ui.DataObject
                {
                    Name = "Наименование",
                    Check_Text = "Описание",
                    Category = "Категория",
                    Standart = "Стандартное\nзначение",
                    Value = "Расчетное\nзначение",
                    Fluctuation = "Отклонение (%)",
                    Result = "Результат",
                    Group = "Группа",
                    Level = "Уровень",
                    Count = "Количество",
                    Summ = "Сумма",
                });

                string project = GetProjectLocal(doc.ProjectInformation.Number);

                if (title.stage == "П" && title.group == "ALL")
                {
                    var summ_concrete = new SummaryConcreteRevision(doc, project);
                    results.AddRange(summ_concrete.Results);
                }

                else if (title.stage == "Р" && 
                    (title.group == "Г*-Г*" || title.group == "В*-В*"))
                {
                    var summ_metal = new SummaryMetallRevision(doc, project);
                    results.AddRange(summ_metal.Results);

                    var summ_concrete = new SummaryConcreteRevision(doc, project);
                    results.AddRange(summ_concrete.Results);
                }
                
                else if (title.stage == "Р" && title.group == "В*")
                {
                    var wall_metal = new WallMetallRevision(doc, project);
                    results.Add(wall_metal.GetResultObject(doc, project));

                    var wall_concrete = new WallConcreteRevision(doc, project);
                    results.Add(wall_concrete.GetResultObject(doc, project));
                }
                
                else if (title.stage == "Р" && title.group == "Г*")
                {
                    var floor_metal = new FloorMetallRevision(doc, project);
                    results.Add(floor_metal.GetResultObject(doc, project));

                    var floor_concrete = new FloorsConcreteRevision(doc, project);
                    results.Add(floor_concrete.GetResultObject(doc, project));
                }
                
                else if (title.stage == "Р" && title.group == "ФП")
                {
                    var foundation_metal = new FoundationMetallRevision(doc, project);
                    results.Add(foundation_metal.GetResultObject(doc, project));
                }

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
            DataTableBuilder dt_builder = new DataTableBuilder(results, doc.Title);

            string result_string = DTtoHTML.toHTML_Table(dt_builder.table);
            File.WriteAllText(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    doc.Title + "_check.html"), 
                result_string);
        }

        /// <summary>
        /// Export results of analysis to XLS file with table.
        /// </summary>
        public static void ExportToExcel(List<Ui.DataObject> results, Document doc)
        {
            DataTableBuilder dt_builder = new DataTableBuilder(results, doc.Title);
            DataTable tbl = dt_builder.table;

            try
            {
                if (tbl == null || tbl.Columns.Count == 0)
                    throw new Exception("ExportToExcel: Null or empty input table!\n");

                // load excel, and create a new workbook
                var excelApp = new Excel.Application();
                excelApp.Workbooks.Add();

                // single worksheet
                Excel._Worksheet workSheet = excelApp.ActiveSheet;

                // column headings
                for (var i = 0; i < tbl.Columns.Count; i++)
                {
                    workSheet.Cells[1, i + 1] = tbl.Columns[i].ColumnName;
                }

                // rows
                for (var i = 0; i < tbl.Rows.Count; i++)
                {
                    // to do: format datetime values before printing
                    for (var j = 0; j < tbl.Columns.Count; j++)
                    {
                        workSheet.Cells[i + 2, j + 1] = tbl.Rows[i][j];
                    }
                }

                string excelFilePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    doc.Title + "_check.xls");

                // check file path
                if (!string.IsNullOrEmpty(excelFilePath))
                {
                    try
                    {
                        workSheet.SaveAs(excelFilePath);
                        excelApp.Quit();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("ExportToExcel: Excel file could not be saved! Check filepath.\n"
                                            + ex.Message);
                    }
                }
                else
                { // no file path is given
                    excelApp.Visible = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ExportToExcel: \n" + ex.Message);
            }

        }

        /// <summary>
        /// Export results of analysis to DB with table.
        /// </summary>
        public static void ExportToDB(List<Ui.DataObject> results, Document doc)
        {
            DataTableBuilder dt_builder = new DataTableBuilder(results, doc.Title);

            DB_Uploader _uploader = new DB_Uploader(dt_builder.table);

        }

        /// <summary>
        /// Select element on model
        /// <param name="ui">An instance of our UI class, which in this template is the main WPF
        /// window of the application.</param>
        /// <param name="doc">The Revit Document to count the walls of.</param>
        /// </summary>
        public static void SelectElement(Ui ui, Document doc)
        {
            Ui.DataObject dataObj = (Ui.DataObject)ui.dataGrid1.SelectedItem;
            string message = $"Choosen element is {dataObj.ID}";

            //FloorCreator fl = new FloorCreator(doc);

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