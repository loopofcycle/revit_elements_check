using System;
using System.Collections.Generic;
using System.Linq;



using System.Collections.ObjectModel;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Input;
using System.Data;

namespace Revit_MP_check
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
        public class DataObject
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public string Result { get; set; }
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
            this.dataGrid1.ItemsSource = list;

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
            MessageBox.Show("Non-External Method Executed Successfully", "Non-External Method");

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

        //private void BNonExternal_choose_Click(object sender, RoutedEventArgs e)
        //{
        //    Methods.SelectElement(this, _doc);
        //    UserAlert();
        //}

        #endregion
    }
}