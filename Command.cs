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

namespace ColorSection
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
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

            // Retrieve elements from database
            ICollection<FilteredElementCollector> Elements = new List<FilteredElementCollector>();
            FilteredElementCollector beam = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfCategory(BuiltInCategory.OST_StructuralFraming);
            FilteredElementCollector columns = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfCategory(BuiltInCategory.OST_StructuralColumns);
            Elements.Add(beam);
            Elements.Add(columns);

            // Retrive categories
            List<ElementId> categories = new List<ElementId>();
            categories.Add(new ElementId(BuiltInCategory.OST_StructuralColumns));
            categories.Add(new ElementId(BuiltInCategory.OST_StructuralFraming));


            // Filtered element collector is iterable

            List<string> names = new List<string>();

            foreach (FilteredElementCollector e in Elements)
            {
                foreach (Element e1 in e)
                {
                    names.Add(e1.Name.ToString());
                }

            }

            names.Distinct().ToList();


            // Modify document within a transaction
            try
            {
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Transaction Name");

                    //Apply filter
                    foreach (string name in names)
                    {
                        ElementParameterFilter filter = new ElementParameterFilter(ParameterFilterRuleFactory.CreateContainsRule(new ElementId(BuiltInParameter.ALL_MODEL_TYPE_NAME),name));
                        //ParameterFilterRuleFactory.CreateContainsRule(new ElementId(BuiltInParameter.ALL_MODEL_TYPE_NAME), name, true)
                        ParameterFilterElement filterElement = ParameterFilterElement.Create(doc, name, categories, filter);
                        doc.ActiveView.AddFilter(filterElement.Id);
                        doc.ActiveView.SetFilterVisibility(filterElement.Id, false);

                    }


                    tx.Commit();
                }
                return Result.Succeeded;
            }

            catch
            {
                return Result.Failed;
            }



        }

    }
}
