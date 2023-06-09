﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit_MP_check
{
    /// <summary>
    /// Create methods here that need to be wrapped in a valid Revit Api context.
    /// Things like transactions modifying Revit Elements, etc.
    /// </summary>
    internal class Methods
    {
        /// <summary>
        /// Method for collecting sheets as an asynchronous operation on another thread.
        /// </summary>
        /// <param name="doc">The Revit Document to collect sheets from.</param>
        /// <returns>A list of collected sheets, once the Task is resolved.</returns>
        private static async Task<List<ViewSheet>> GetSheets(Document doc)
        {
            return await Task.Run(() =>
            {
                Util.LogThreadInfo("Get Sheets Method");
                return new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewSheet))
                    .Select(p => (ViewSheet) p).ToList();
            });
        }

        /// <summary>
        /// Rename all the sheets in the project. This opens a transaction, and it MUST be executed
        /// in a "Valid Revit API Context", otherwise the add-in will crash. Because of this, we must
        /// wrap it in a ExternalEventHandler, as we do in the App.cs file in this template.
        /// </summary>
        /// <param name="ui">An instance of our UI class, which in this template is the main WPF
        /// window of the application.</param>
        /// <param name="doc">The Revit Document to rename sheets in.</param>
        public static void SheetRename(Ui ui, Document doc)
        {
            Util.LogThreadInfo("Sheet Rename Method");

            // get sheets - note that this may be replaced with the Async Task method above,
            // however that will only work if we want to only PULL data from the sheets,
            // and executing a transaction like below from an async collection, will crash the app
            List<ViewSheet> sheets = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .Select(p => (ViewSheet) p).ToList();

            // report results - push the task off to another thread
            Task.Run(() =>
            {
                Util.LogThreadInfo("Sheet Rename Show Results");

                // report the count
                string message = $"There are {sheets.Count} Sheets in the project";
                ui.Dispatcher.Invoke(() =>
                    ui.TbDebug.Text += "\n" + (DateTime.Now).ToLongTimeString() + "\t" + message);
            });

            // rename all the sheets, but first open a transaction
            using (Transaction t = new Transaction(doc, "Rename Sheets"))
            {
                Util.LogThreadInfo("Sheet Rename Transaction");

                // start a transaction within the valid Revit API context
                t.Start("Rename Sheets");

                // loop over the collection of sheets using LINQ syntax
                foreach (string renameMessage in from sheet in sheets
                    let renamed = sheet.LookupParameter("Sheet Name")?.Set("TEST")
                    select $"Renamed: {sheet.Title}, Status: {renamed}")
                {
                    ui.Dispatcher.Invoke(() =>
                        ui.TbDebug.Text += "\n" + (DateTime.Now).ToLongTimeString() + "\t" + renameMessage);
                }

                t.Commit();
                t.Dispose();
            }

            // invoke the UI dispatcher to print the results to report completion
            ui.Dispatcher.Invoke(() =>
                ui.TbDebug.Text += "\n" + (DateTime.Now).ToLongTimeString() + "\t" + "SHEETS HAVE BEEN RENAMED");
        }

        /// <summary>
        /// Print the Title of the Revit Document on the main text box of the WPF window of this application.
        /// </summary>
        /// <param name="ui">An instance of our UI class, which in this template is the main WPF
        /// window of the application.</param>
        /// <param name="doc">The Revit Document to print the Title of.</param>
        public static List<Ui.DataObject> CheckElements(Ui ui, Document doc)
        {
            ui.Dispatcher.Invoke(() => ui.TbDebug.Text += "\n" + (DateTime.Now).ToLongTimeString() + "\t" + doc.Title);
            Util.LogThreadInfo("Parameters checking");

            // creating results with summary revision
            List<Ui.DataObject> results = new List<Ui.DataObject>();
            
            var topo_rev = new TopoRevision(doc);
            if (topo_rev.Result != null)
            {
                results.Add(topo_rev.Result);
            }
            
            var dwg_rev = new DWGRevision(doc);
            if (dwg_rev.Result != null)
            {
                results.Add(dwg_rev.Result);
            }

            results.Add(new Ui.DataObject()
            {
                ID = -1,
                Name = "Активный файл revit",
                Category = "Наличие файла dwg общей подложки",
                Result = "Не найден файл dwg",
            });

            results.Add(new Ui.DataObject()
            {
                ID = -1,
                Name = "Активный файл revit",
                Category = "Наличие файла dwg подложки Инж.сетей",
                Result = "Не найден файл dwg",
            });

            results.Add(new Ui.DataObject()
            {
                ID = -1,
                Name = "Активный файл revit",
                Category = "Наличие файла 2d-эскиза",
                Result = "Не найден файл 2d-эскиза",
            });

            results.Add(new Ui.DataObject()
            {
                ID = -1,
                Name = "Активный файл revit",
                Category = "Наличие файла растровой- подложки визуализации",
                Result = "Отсутствует файл с подложкой",
            });



            // format the message to show the number of walls in the project
            string message = $"There are {results.Count} errors in the project";

            // invoke the UI dispatcher to print the results to the UI
            ui.Dispatcher.Invoke(() =>
            ui.TbDebug.Text += "\n" + (DateTime.Now).ToLongTimeString() + "\t" + message);

            return results;
        }

        /// <summary>
        /// Count the walls in the Revit Document, and print the count
        /// on the main text box of the WPF window of this application.
        /// </summary>
        /// <param name="ui">An instance of our UI class, which in this template is the main WPF
        /// window of the application.</param>
        /// <param name="doc">The Revit Document to count the walls of.</param>
        public static void SelectElement(Ui ui, Document doc)
        {
            Ui.DataObject dataObj = (Ui.DataObject)ui.dataGrid1.SelectedItem;
            string message = $"Choosen element is {dataObj.ID}";

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