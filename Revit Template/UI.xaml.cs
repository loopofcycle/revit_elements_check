using System;
using System.Collections.Generic;
using System.Linq;



using System.Collections.ObjectModel;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Input;
using System.Data;
using System.Windows.Forms;

namespace Revit_product_check
{
    /// <summary>
    /// Interaction logic for UI.xaml
    /// </summary>
    public partial class Ui : Window
    {
        private Document _doc;

        //private readonly UIApplication _uiApp;
        //private readonly Autodesk.Revit.ApplicationServices.Application _app;
        private UIDocument _uiDoc;

        private EventHandlerWithStringArg _mExternalMethodStringArg;
        private EventHandlerWithWpfArg _mExternalMethodWpfArg;

        // id of element in revit
        public class OldDataObject
        {
            public int ID { get; set; }
            public int Check_ID { get; set; }
            public string Check_Text { get; set; }
            public string Category { get; set; }
            public string Standart { get; set; }
            public string Value { get; set; }
            public string Name { get; set; }
            public string Result { get; set; }
            public string Group { get; set; }
            public string Level { get; set; }
            public string Count { get; set; }
            public string Summ { get; set; }
            public string Fluctuation { get; set; }
        }

        public class DataObject
        {
            public int ID { get; set; }
            public int Check_ID { get; set; }
            public string Check_Text { get; set; }
            public string Title { get; set; }
            public string Project { get; set; }
            public string Section { get; set; }
            public string Doc_complect { get; set; }
            public string Levels_quantity { get; set; }
            public string Category { get; set; }
            public string Standart { get; set; }
            public string Value { get; set; }
            public string Name { get; set; }
            public string Result { get; set; }
            public string Group { get; set; }
            public string Level { get; set; }
            public string Count { get; set; }
            public string Summ { get; set; }
            public string Fluctuation { get; set; }

        }

        public Ui(UIApplication uiApp,
            EventHandlerWithStringArg evExternalMethodStringArg,
            EventHandlerWithWpfArg eExternalMethodWpfArg)
        {
            _uiDoc = uiApp.ActiveUIDocument;
            _doc = _uiDoc.Document;
            //_app = _doc.Application;
            //_uiApp = _doc.Application;
            Closed += MainWindow_Closed;

            InitializeComponent();

            _mExternalMethodStringArg = evExternalMethodStringArg;
            _mExternalMethodWpfArg = eExternalMethodWpfArg;

            var list = new ObservableCollection<DataObject>();
            //list.Add(new DataObject() {});


            DataGridView dataGridView1 = new DataGridView();

            this.dataGrid1.ItemsSource = list;
            //this.dataGridView1.DataSource = list;
            
            dataGridView1.CellFormatting +=
            new System.Windows.Forms.DataGridViewCellFormattingEventHandler(
            this.dataGridView1_CellFormatting);

        }


        private void dataGridView1_CellFormatting(object sender,
        System.Windows.Forms.DataGridViewCellFormattingEventArgs e)
        {
            // Set the background to red for negative values in the Balance column.
            //if (dataGridView1.Columns[e.ColumnIndex].Name.Equals("Отклонение"))
            //{
            //    Int32 intValue;
            //    if (Int32.TryParse((String)e.Value, out intValue) &&
            //        (intValue > 0))
            //    {
            //        e.CellStyle.BackColor = System.Drawing.Color.Red;
            //        e.CellStyle.SelectionBackColor = System.Drawing.Color.DarkRed;
            //    }
            //}
        }


        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Close();
        }

        #region External Project Methods

        private void BExtString_Click(object sender, RoutedEventArgs e)
        {
            // Raise external event with a string argument. The string MAY
            // be pulled from a Revit API context because this is an external event
            _mExternalMethodStringArg.Raise($"Title: {_doc.Title}");
        }

        private void BExternalMethod1_Click(object sender, RoutedEventArgs e)
        {
            // Raise external event with this UI instance (WPF) as an argument
            _mExternalMethodWpfArg.Raise(this);
        }

        private void BNonExternal_choose_Click(object sender, RoutedEventArgs e)
        {
            _mExternalMethodWpfArg.Raise(this);
        }

        #endregion

        #region Non-External Project Methods

        private void UserAlert()
        {
            //TaskDialog.Show("Non-External Method", "Non-External Method Executed Successfully");
            System.Windows.Forms.MessageBox.Show("Non-External Method Executed Successfully", "Non-External Method");

            //Dispatcher.Invoke(() =>
            //{
            //    TaskDialog mainDialog = new TaskDialog("Non-External Method")
            //    {
            //        MainInstruction = "Hello, Revit!",
            //        MainContent = "Non-External Method Executed Successfully",
            //        CommonButtons = TaskDialogCommonButtons.Ok,
            //        FooterText = "<a href=\"http://usa.autodesk.com/adsk/servlet/index?siteID=123112&id=2484975 \">"
            //                     + "Click here for the Revit API Developer Center</a>"
            //    };


            //    TaskDialogResult tResult = mainDialog.Show();
            //    Debug.WriteLine(tResult.ToString());
            //});
        }

        private void BNonExternal3_Click(object sender, RoutedEventArgs e)
        {
            // the sheet takeoff + delete method won't work here because it's not in a valid Revit api context
            // and we need to do a transaction
            // Methods.SheetRename(this, _doc); <- WON'T WORK HERE
            UserAlert();
        }

        private void BNonExternal_check_Click(object sender, RoutedEventArgs e)
        {
            List<DataObject> results =  Methods.CheckElements(this, _doc);

            var list = new ObservableCollection<DataObject>();
            foreach (DataObject id in results)
            {
                list.Add( id );
            }
            this.dataGrid1.ItemsSource = results;

            //UserAlert();
        }

        private void BNonExternal_export_Click(object sender, RoutedEventArgs e)
        {
            List<DataObject> results = Methods.CheckElements(this, _doc);
            //Methods.ExportResultstoHTML(results, _doc);
            //Methods.ExportResultstoXLS(results, _doc);
            Methods.ExportToExcel(results, _doc);
            //UserAlert();
        }

        private void BNonExternal_export_DB_Click(object sender, RoutedEventArgs e)
        {
            List<DataObject> results = Methods.CheckElements(this, _doc);
            //Methods.ExportResultstoHTML(results, _doc);
            //Methods.ExportResultstoXLS(results, _doc);
            Methods.ExportToDB(results, _doc);
            //UserAlert();
        }

        #endregion
    }
}