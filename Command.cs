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

            names = names.Distinct().ToList();


            // Modify document 
            int i = 0;
            foreach (string name in names)
            {
                Random random = new Random();
                byte randomNumber1 = (byte)random.Next(0, 256);
                byte randomNumber2 = (byte)random.Next(0, 256);
                byte randomNumber3 = (byte)random.Next(0, 256);



                Color color = new Color(randomNumber1, randomNumber2, randomNumber3);
                try
                {
                    createFilterView(doc, doc.ActiveView, color, name);
                }
                catch
                { i++; }
                //ElementParameterFilter filter = new ElementParameterFilter(ParameterFilterRuleFactory.CreateContainsRule(new ElementId(BuiltInParameter.ALL_MODEL_TYPE_NAME), name));
                ////ParameterFilterRuleFactory.CreateContainsRule(new ElementId(BuiltInParameter.ALL_MODEL_TYPE_NAME), name, true)
                //ParameterFilterElement filterElement = ParameterFilterElement.Create(doc, name, categories, filter);
                //doc.ActiveView.AddFilter(filterElement.Id);
                //doc.ActiveView.SetFilterVisibility(filterElement.Id, false);

            }

            TaskDialog.Show("Filter creation", $"{i} filters already exist.");
            return Result.Succeeded;
        }
        //static Byte ramdomByte()
        //{
        //    Random random = new Random();
        //    byte randomByte = (byte)random.Next(0, 256);
        //    return (byte)randomByte;
        //}

        void createFilterView(Autodesk.Revit.DB.Document doc, Autodesk.Revit.DB.View view, Autodesk.Revit.DB.Color objColor, string name)
        {
            //try
            //{
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Transaction Name");
                    List<ElementId> cats = new List<ElementId>();
                    cats.Add(new ElementId(BuiltInCategory.OST_StructuralColumns));
                    cats.Add(new ElementId(BuiltInCategory.OST_StructuralFraming));

                    //FilteredElementCollector parameterCollector = new FilteredElementCollector(doc, view.Id);

                    ////
                    ////Dim parameter As Parameter = parameterCollector.OfClass(GetType(Rebar)).FirstElement().LookupParameter("RelNo")

                    //List<FilterRule> filterRules = new List<FilterRule>();
                    
                    ElementParameterFilter filter = new ElementParameterFilter(ParameterFilterRuleFactory.CreateContainsRule(new ElementId(BuiltInParameter.ALL_MODEL_TYPE_NAME), name));

                    //filterRules.Add(ParameterFilterRuleFactory.CreateEqualsRule(new ElementId(BuiltInParameter.ALL_MODEL_TYPE_NAME), name));

                    ParameterFilterElement parameterFilterElement = ParameterFilterElement.Create(doc, $"{name}_", cats, filter);

                    OverrideGraphicSettings filterSettings = new OverrideGraphicSettings();
                    filterSettings.SetSurfaceForegroundPatternColor(objColor);
                    filterSettings.SetSurfaceForegroundPatternId(ElementId.Parse("4"));
                    filterSettings.SetSurfaceBackgroundPatternVisible(true);
                    view.AddFilter(parameterFilterElement.Id);
                    view.SetFilterOverrides(parameterFilterElement.Id, filterSettings);

                    tx.Commit();
                }
            // }

            //catch
            //{
               // TaskDialog.Show("Message", "Task not complited");
            //}

        }
    }
}
