#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

#endregion

namespace Module01_Challenge_230719
{
    [Transaction(TransactionMode.Manual)]
    public class FizzBuzz : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // convert meters to feet
            double meters = 4;
            double metersTofeet = meters * 3.28084;
            // convert mm to feet
            double mm = 3500;
            double mmToFeet = mm / 304.8;

            // find the reminder when dividing (ie. the modulo or mod)
            //double reminder1 = 100 % 10; // equals 0 (100 divided by 10 = 10)
            //double reminder2 = 100 % 9; // equals 1 (100 divided by 9 = 11 with reminder

            // Variables
            double numberVariable = 250;
            double startingElevation = 0;
            double floorHeight = 15;

            string mod3_FloorPlanName = "FIZZ_";
            string mod5_CeilingPlanName = "BUZZ_";
            string mod3And5_SheetName = "FIZZBUZZ_";

            // Mod Lists
            List<Level> mod3 = new List<Level>();
            List<Level> mod5 = new List<Level>();
            List<Level> mod3And5 = new List<Level>();
            //           List<Level> notMod3And5 = new List<Level>();

            //Create Level Transaction
            Transaction t1 = new Transaction(doc);
            t1.Start("Create levels");

            for (double i = 0; i <= numberVariable; i++)
            {
                startingElevation += i;

                if ((startingElevation % 3 == 0) && !(startingElevation % 5 == 0))
                {
                    Level levelMod3 = Level.Create(doc, startingElevation + floorHeight);
                    levelMod3.Name = mod3_FloorPlanName + startingElevation.ToString();// +"_" +levelMod3.Elevation.ToString();
                    mod3.Add(levelMod3);
                }
                if ((startingElevation % 5 == 0) && !(startingElevation % 3 == 0))
                {
                    Level levelMod5 = Level.Create(doc, startingElevation + floorHeight);
                    levelMod5.Name = mod5_CeilingPlanName + startingElevation.ToString();// + "_" + levelMod5.Elevation.ToString();
                    mod5.Add(levelMod5);
                }
                if ((startingElevation % 3 == 0) && (startingElevation % 5 == 0))
                {
                    Level levelMod3And5 = Level.Create(doc, startingElevation + floorHeight);
                    levelMod3And5.Name = mod3And5_SheetName + startingElevation.ToString();// + "_" + levelMod3And5.Elevation.ToString();
                    mod3And5.Add(levelMod3And5);
                }
            }

            t1.Commit();
            t1.Dispose();

            //Filter View Family Types and obtaining Floor & Ceiling Plans
            FilteredElementCollector collector1 = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType));
            ViewFamilyType floorPlanVFT = null;
            ViewFamilyType ceilingPlanVFT = null;

            //Create Floor Plan Views Transaction
            Transaction t2 = new Transaction(doc);
            t2.Start("Create Floor Plans");

            foreach (ViewFamilyType fVFT in collector1)
            {
                if (fVFT.ViewFamily == ViewFamily.FloorPlan)
                {
                    floorPlanVFT = fVFT;
                    foreach (Level imod3 in mod3)
                    {
                        ViewPlan newFloorPlan = ViewPlan.Create(doc, floorPlanVFT.Id, imod3.Id);
                        newFloorPlan.Name = imod3.Name.ToString();
                    }
                    break;
                }
            }
            t2.Commit();
            t2.Dispose();

            //Create Ceiling Plan Views Transaction
            Transaction t3 = new Transaction(doc);
            t3.Start("Create Ceiling Plans");

            foreach (ViewFamilyType cVFT in collector1)
            {
                if (cVFT.ViewFamily == ViewFamily.CeilingPlan)
                {
                    ceilingPlanVFT = cVFT;
                    foreach (Level imod5 in mod5)
                    {
                        ViewPlan newCeilingPlan = ViewPlan.Create(doc, ceilingPlanVFT.Id, imod5.Id);
                        newCeilingPlan.Name = imod5.Name.ToString(); ;
                    }
                    break;
                }
            }
            t3.Commit();
            t3.Dispose();

            //Create Floor Plan Views for Sheets and place them on sheet Transtacion
            Transaction t4 = new Transaction(doc);
            t4.Start("Create Floor Plans and place Viewports on Sheets");

            List<ViewPlan> viewsmod3And5 = new List<ViewPlan>();

            FilteredElementCollector collector2 = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_TitleBlocks);
            XYZ viewInsertPoint = new XYZ(420 / 304.8, 297.0 / 304.8, 0);

            foreach (ViewFamilyType fVFT in collector1)
            {
                if (fVFT.ViewFamily == ViewFamily.FloorPlan)
                {
                    floorPlanVFT = fVFT;
                    foreach (Level imod3And5 in mod3And5)
                    {
                        ViewPlan newFloorPlan = ViewPlan.Create(doc, floorPlanVFT.Id, imod3And5.Id);
                        newFloorPlan.Name = imod3And5.Name.ToString();
                        viewsmod3And5.Add(newFloorPlan);

                        ViewSheet fizzBuzzSheet = ViewSheet.Create(doc, collector2.FirstElementId());
                        fizzBuzzSheet.Name = "FizzBuzz";
                        fizzBuzzSheet.SheetNumber = imod3And5.Name.ToString();

                        Viewport newViewport = Viewport.Create(doc, fizzBuzzSheet.Id, newFloorPlan.Id, viewInsertPoint);
                    }
                    break;
                }
            }

            //           FilteredElementCollector collector2 = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_TitleBlocks);
            //           XYZ viewInsertPoint = new XYZ(420, 297.0, 0);
            //
            //           ViewSheet fizzBuzzSheet = ViewSheet.Create(doc, collector2.FirstElementId());
            //           fizzBuzzSheet.Name = "FizzBuzz Name";
            //           fizzBuzzSheet.SheetNumber = "FizzBuzz Number";
            //
            //           Viewport newViewport = Viewport.Create(doc, fizzBuzzSheet.Id, newFloorPlan.Id, viewInsertPoint)

            t4.Commit();
            t4.Dispose();


            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
