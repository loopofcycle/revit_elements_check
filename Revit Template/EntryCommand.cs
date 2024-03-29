#region Namespaces

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#endregion


namespace Revit_product_check
{
    /// <summary>
    /// This is the ExternalCommand which gets executed from the ExternalApplication. In a WPF context,
    /// this can be lean, as it just needs to show the WPF. Without a UI, this could contain the main
    /// order of operations for executing the business logic.
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class EntryCommand : IExternalCommand
    {
        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                App.ThisApp.ShowForm(commandData.Application);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}