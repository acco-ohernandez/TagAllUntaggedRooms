#region Namespaces
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace TagAllUntaggedRooms
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_TagAllFloorPlanRooms : IExternalCommand
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

            //var allFloorPlanViews = MyUtils.GetAllFloorPlanViewSheets(doc);
            var allFloorPlanViews = MyUtils.GetAllFloorPlanViews(doc);

            int count = 0;
            using (Transaction t = new Transaction(doc, "Tagged All FloorPlan Rooms"))
            {
                t.Start();
                foreach (var floorPlanView in allFloorPlanViews)
                {
                    count += MyUtils.TagUntaggedRoomsInView(doc, uidoc, floorPlanView);
                }
                t.Commit();
            }
            TaskDialog.Show("Info", $"FloorPlan Rooms tagged: {count}");
            return Result.Succeeded;
        }

    }
}
