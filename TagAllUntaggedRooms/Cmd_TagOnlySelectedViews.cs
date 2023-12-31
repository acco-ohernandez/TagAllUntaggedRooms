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
    public class Cmd_TagOnlySelectedViews : IExternalCommand
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

            // Get Only the selected Views
            var SelectedViews = MyUtils.GetSelectedViews(doc);

            int count = 0;
            using (Transaction t = new Transaction(doc, "Tagged All CeilingPlan Rooms"))
            {
                t.Start();
                foreach (var curSelectedView in SelectedViews)
                {
                    count += MyUtils.TagUntaggedRoomsInView(doc, uidoc, curSelectedView);
                }
                t.Commit();
            }
            TaskDialog.Show("Info", $"Selected Views Rooms tagged: {count}");
            return Result.Succeeded;
        }

    }
}
