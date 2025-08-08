#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion

namespace ColorPipes
{
    [Transaction(TransactionMode.Manual)]
    public class EraseFilters : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            // Get all structural framing elements
            ICollection<ElementId> filterIds = doc.ActiveView.GetFilters();

            foreach (ElementId id in filterIds)
            {
                using (Transaction trans = new Transaction(doc, "Delete Element"))
                {
                    trans.Start();
                    
                    Element ele = doc.GetElement(id);
                    String eleName = ele.Name;

                    doc.Delete(id);

                    TaskDialog tDialog = new TaskDialog("Delete Element");
                    tDialog.MainContent = $"Are you sure you want to delete \"{eleName}\" filter ?";
                    tDialog.CommonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel;

                    if (tDialog.Show() == TaskDialogResult.Ok)
                    {
                        trans.Commit();
                    }
                    else
                    {
                        trans.RollBack();
                    }
                }
            }
            

            return Result.Succeeded;
        }

    }
}
