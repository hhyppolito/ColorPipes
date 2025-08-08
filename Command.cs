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
            List<Element> pipes = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeCurves).WhereElementIsNotElementType().ToList();


            // Filtered element collector is iterable

            List<double> names = new List<double>();

            foreach (Element pipe in pipes)
                {
                    double diameterInFeet = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble();
                    // Convert the diameter to inches for easier use (1 foot = 12 inches)
                    //double diameterInmilimiter = diameterInFeet * 304.8;
                    names.Add(diameterInFeet);
                }
                        
            names = names.Distinct().ToList();


            // Modify document 
            int i = 0;
            foreach (double name in names)
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
               
            }

            TaskDialog.Show("Filter creation", $"{i} filters already exist.");
            return Result.Succeeded;
        }
 

        void createFilterView(Autodesk.Revit.DB.Document doc, Autodesk.Revit.DB.View view, Autodesk.Revit.DB.Color objColor, double name)
        {

                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Transaction Name");
                    List<ElementId> cats = new List<ElementId>();
                    cats.Add(new ElementId(BuiltInCategory.OST_PipeCurves));;
                 
                    ElementParameterFilter filter = new ElementParameterFilter(ParameterFilterRuleFactory.CreateEqualsRule(new ElementId(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM),name, 0.00000001));

                    ParameterFilterElement parameterFilterElement = ParameterFilterElement.Create(doc, $"Ø{name * 304.8}", cats, filter);

                    OverrideGraphicSettings filterSettings = new OverrideGraphicSettings();
                    filterSettings.SetSurfaceForegroundPatternColor(objColor);
                    filterSettings.SetSurfaceForegroundPatternId(ElementId.Parse("4"));
                    filterSettings.SetSurfaceBackgroundPatternVisible(true);
                    view.AddFilter(parameterFilterElement.Id);
                    view.SetFilterOverrides(parameterFilterElement.Id, filterSettings);

                    tx.Commit();
                }
        }
    }
}
