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
using System.Reflection;
using System.Windows.Media.Animation;

#endregion

namespace Revit_Addins_Bootcamp
{
    [Transaction(TransactionMode.Manual)]
    public class Module01Challenge : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Your code goes here
            int countVar = 1;
            int numVar = 250;
            double newElev = 0;
            double elevHeightVar = 15;

            //Create collector for levels
            FilteredElementCollector projLevels = new FilteredElementCollector(doc);
            projLevels.OfClass(typeof(ViewFamilyType));
            
            //Look foor all floor plans 
            ViewFamilyType currentFP = null;
            foreach (ViewFamilyType viewFamily in projLevels)
            {
                if (viewFamily.ViewFamily == ViewFamily.FloorPlan)
                {
                    currentFP = viewFamily;
                    break;
                }
            }

            //Look foor all ceiling plans 
            ViewFamilyType currentRCP = null;
            foreach (ViewFamilyType viewFamily in projLevels)
            {
                if (viewFamily.ViewFamily == ViewFamily.CeilingPlan)
                {
                    currentRCP = viewFamily;
                    break;
                }
            }

            //Create collector for title blocks
            FilteredElementCollector tbcollector = new FilteredElementCollector(doc);
            tbcollector.OfCategory(BuiltInCategory.OST_TitleBlocks);

            Transaction t = new Transaction(doc);
            t.Start("Create levels");

            for (int i = 1; i <= numVar; i++)
            {
                Level currentLev = Level.Create(doc, newElev);
                newElev = newElev + elevHeightVar;
                currentLev.Elevation = newElev;
                currentLev.Name = ("FB-" + i.ToString());

                //create a floor plan and name it "FIZZ_#"
                if (countVar % 3 == 0)
                {
                    ViewPlan fizzPlan = ViewPlan.Create(doc, currentFP.Id, currentLev.Id);
                    fizzPlan.Name = "FIZZ" + i.ToString();
                }

                //create a ceiling plan and name it "BUZZ_#"
                if (countVar % 5 == 0)
                {
                    ViewPlan buzzPlan = ViewPlan.Create(doc, currentRCP.Id, currentLev.Id);
                    buzzPlan.Name = "BUZZ" + i.ToString();
                }

                //create a sheet and name it "FIZZBUZZ_#"
                if (countVar % 3 == 0 && countVar % 5 == 0)
                {
                    ViewSheet newSheet = ViewSheet.Create(doc, tbcollector.FirstElementId());
                    newSheet.Name = "FIZZBUZZ_" + i.ToString();
                    newSheet.SheetNumber = "FB-" + i.ToString();

                    //Create plan for FIZZBUZZ
                    ViewPlan fizzbuzzPlan = ViewPlan.Create(doc, currentFP.Id, currentLev.Id);
                    fizzbuzzPlan.Name = "FIZZBUZZ" + i.ToString();

                    //Add FIZZBUZZ plan to sheet
                    XYZ viewInsPt = new XYZ(1, 0.5, 0);

                    Viewport.Create(doc, newSheet.Id, fizzbuzzPlan.Id, viewInsPt);
                }

                countVar++;
            }

            t.Commit();
            t.Dispose();

            return Result.Succeeded;
        }

        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }
    }
}
