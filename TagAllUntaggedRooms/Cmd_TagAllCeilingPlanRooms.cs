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
    public class Cmd_TagAllCeilingPlanRooms : IExternalCommand
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

            var allCeilingPlanViews = MyUtils.GetAllCeilingPlanViews(doc);

            int count = 0;
            using (Transaction t = new Transaction(doc, "Tagged All CeilingPlan Rooms"))
            {
                t.Start();
                foreach (var ceilingPlanView in allCeilingPlanViews)
                {
                    count += MyUtils.TagUntaggedRoomsInView(doc, uidoc, ceilingPlanView);
                }
                t.Commit();
            }
            TaskDialog.Show("Info", $"CeilingPlan Rooms tagged: {count}");
            return Result.Succeeded;
        }

    }
}
