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
    public class Cmd_TagCurrentViewRooms : IExternalCommand
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

            #region 

            // Get all the ViewSheets in the document
            ICollection<ViewSheet> viewSheets = MyUtils.GetAllViewSheets(doc);

            // Process each ViewSheet
            foreach (ViewSheet sheet in viewSheets)
            {
                Debug.Print($"-------------------------  ViewSheet Name: {sheet.Name} ");
                // Check if the ViewSheet is a Floor Plan
                if (sheet.ViewType == ViewType.FloorPlan)
                {
                    Debug.Print($"Floor Plan - ViewSheet Name: {sheet.Name}");
                }

            }
            #endregion
            using (Transaction t = new Transaction(doc, "Tag Rooms"))
            {
                t.Start();
                MyUtils.TagUntaggedRoomsOnCurrentActiveView(doc, uidoc);
                t.Commit();
            }

            return Result.Succeeded;
        }

    }
}
