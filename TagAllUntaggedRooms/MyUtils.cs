using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
namespace TagAllUntaggedRooms
{
    public static class MyUtils
    {
        public static ICollection<Level> GetAllLevels1(Document doc)
        {
            // Retrieve all Level elements from the document
            ICollection<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();

            return levels;
        }
        public static List<Level> GetAllLevels(Document doc)
        {
            // Get the Level category
            Category levelCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Levels);

            // Retrieve all elements of the Level category
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Levels);
            ICollection<Element> levelElements = collector.ToElements();

            // Filter out only the Level elements
            List<Level> levels = new List<Level>();
            foreach (Element element in levelElements)
            {
                Level level = element as Level;
                if (level != null)
                {
                    levels.Add(level);
                }
            }

            return levels;
        }
        public static List<Room> GetRoomsByLevel(Document doc, Level level)
        {
            // Get the Level Id
            ElementId levelId = level.Id;

            // Create a filter for rooms on the specified level
            ElementLevelFilter levelFilter = new ElementLevelFilter(levelId);

            // Retrieve all rooms in the document that are on the specified level
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);
            collector.WherePasses(levelFilter);

            // Convert the collected elements to Room objects
            List<Room> rooms = new List<Room>();
            foreach (Element element in collector)
            {
                Room room = element as Room;
                if (room != null)
                {
                    rooms.Add(room);
                }
            }

            return rooms;
        }
        public static ICollection<View> GetSelectedViews(Document doc)
        {
            UIDocument uidoc = new UIDocument(doc);

            // Get the selected elements from the UIDocument
            ICollection<ElementId> selectedElementIds = uidoc.Selection.GetElementIds();

            // Filter out the views from the selected elements
            ICollection<View> selectedViews = new List<View>();
            foreach (ElementId elementId in selectedElementIds)
            {
                Element element = doc.GetElement(elementId);
                if (element is View view)
                {
                    selectedViews.Add(view);
                }
            }

            return selectedViews;
        }
        public static void TagUntaggedRoomsOnLevel(Document doc, Level level)
        {
            // Get the Level Id
            ElementId levelId = level.Id;

            // Create a filter for rooms on the specified level
            ElementLevelFilter levelFilter = new ElementLevelFilter(levelId);

            // Create a filter for untagged rooms on the specified level
            ElementClassFilter roomFilter = new ElementClassFilter(typeof(SpatialElement));
            LogicalAndFilter untaggedRoomsFilter = new LogicalAndFilter(roomFilter, levelFilter);

            // Retrieve all untagged rooms on the specified level
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.WherePasses(untaggedRoomsFilter);
            collector.OfClass(typeof(SpatialElement));

            // Create a new tag for the untagged rooms
            ElementId tagTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TagNoteType);
            XYZ tagInsertionPoint = new XYZ(0, 0, 0); // Adjust the insertion point as needed

            // Tag each untagged room
            foreach (SpatialElement room in collector)
            {
                // Create a tag for each room
                IndependentTag tag = IndependentTag.Create(doc, tagTypeId, doc.ActiveView.Id, new Reference(room), false, TagOrientation.Horizontal, tagInsertionPoint);
                if (tag != null)
                {
                    // Optionally set other properties of the tag, such as leader and leader end
                    // tag.HasLeader = true;
                    // tag.LeaderEndCondition = LeaderEndCondition.Free;
                    // tag.LeaderEnd = ...;
                }
            }
        }
        public static ICollection<ViewSheet> GetAllViewSheets(Document doc)
        {
            // Retrieve all ViewSheet elements from the document
            ICollection<ViewSheet> viewSheets = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .OrderBy(sheet => sheet.Name)
                .ToList();

            foreach (ViewSheet sheet in viewSheets)
            {
                Debug.Print($"-------------------------  ViewSheet Name: {sheet.Name} ");
                // Check if the ViewSheet is a Floor Plan
                if (sheet.ViewType == ViewType.FloorPlan)
                {
                    Debug.Print($"Floor Plan - ViewSheet Name: {sheet.Name}");
                }

            }

            return viewSheets;
        }

        public static ICollection<View> GetAllFloorPlanViews(Document doc)
        {
            // Retrieve all views of ViewType.FloorPlan
            ICollection<View> floorPlanViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(view => view.ViewType == ViewType.FloorPlan)
                .ToList();

            return floorPlanViews;
        }

        public static ICollection<View> GetAllCeilingPlanViews(Document doc)
        {
            // Retrieve all views of ViewType.FloorPlan
            ICollection<View> ceilingPlanViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(view => view.ViewType == ViewType.CeilingPlan)
                .ToList();

            return ceilingPlanViews;
        }


        public static ICollection<ViewSheet> GetAllFloorPlanViewSheets(Document doc)
        {
            // Retrieve all ViewSheet elements from the document
            ICollection<ViewSheet> viewSheets = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .OrderBy(sheet => sheet.Name)
            .ToList();

            List<ViewSheet> floorPlanViews = new List<ViewSheet>();
            foreach (ViewSheet sheet in viewSheets)
            {
                // Check if the ViewSheet is a Floor Plan
                if (sheet.ViewType == ViewType.FloorPlan)
                {
                    Debug.Print($"Floor Plan - ViewSheet Name: {sheet.Name}");
                    floorPlanViews.Add(sheet);
                }

            }

            return floorPlanViews;
        }
        public static void TagUntaggedRoomsOnCurrentActiveView(Document doc, UIDocument uidoc)
        {
            // Get the current active view
            View activeView = doc.ActiveView;

            // Check if the active view is a level view
            if (activeView.ViewType == ViewType.FloorPlan || activeView.ViewType == ViewType.CeilingPlan)
            {
                // Retrieve all the rooms on the current level
                ICollection<ElementId> roomIds = new FilteredElementCollector(doc, activeView.Id)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElementIds();

                // Filter out the already tagged rooms
                ICollection<ElementId> untaggedRoomIds = new List<ElementId>();
                foreach (ElementId roomId in roomIds)
                {
                    Room room = doc.GetElement(roomId) as Room;

                    if (room != null && room.GetParameters("Tag").FirstOrDefault() == null)
                    {
                        untaggedRoomIds.Add(roomId);
                    }
                }

                // Get the loaded tag family
                FilteredElementCollector tagCollector = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_RoomTags)
                    .WhereElementIsElementType();

                FamilySymbol tagSymbol = tagCollector.Cast<FamilySymbol>().FirstOrDefault();

                // Create a new tag for each untagged room
                List<ElementId> tagIds = new List<ElementId>();
                foreach (ElementId roomId in untaggedRoomIds)
                {
                    Room room = doc.GetElement(roomId) as Room;
                    if (room != null)
                    {
                        // Create a reference for the room
                        Reference roomReference = new Reference(room);
                        // Get the room location point
                        XYZ roomLocation = (room.Location as LocationPoint)?.Point;
                        // Create a new tag at the room location
                        IndependentTag newTag = IndependentTag.Create(
                            doc,
                            tagSymbol.Id,
                            activeView.Id,
                            roomReference,
                            false,
                            TagOrientation.Horizontal,
                            roomLocation);

                        // Assign the tag family symbol to the newly created tag
                        newTag.ChangeTypeId(tagSymbol.Id);
                        tagIds.Add(newTag.Id);
                    }
                }

                // Select the newly created tags
                if (tagIds.Count > 0)
                {
                    // Set the selection to the newly created tags
                    uidoc.Selection.SetElementIds(tagIds);

                    // Show a message indicating the number of tags created
                    TaskDialog.Show("Tag Untagged Rooms",
                        $"Successfully tagged {tagIds.Count} untagged rooms.");
                }
                else
                {
                    TaskDialog.Show("Tag Untagged Rooms", "No untagged rooms found.");
                }
            }
            else
            {
                // Handle the case when the active view is not a level view
                TaskDialog.Show("Error", "Please switch to a floor plan or ceiling plan view.");
            }
        }

        public static int TagUntaggedRoomsInView(Document doc, UIDocument uidoc, View view)
        {
            if (doc == null || uidoc == null)
            {
                // Handle null document or UIDocument
                return 0;
            }

            if (view == null || !IsValidFloorPlanView(view))
            {
                // Handle invalid or non-floor plan view
                return 0;
            }

            int processedViewsCount = 0;

            try
            {
                // Retrieve all the rooms within the view
                ICollection<ElementId> roomIds = new FilteredElementCollector(doc, view.Id)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElementIds();

                // Filter out the already tagged rooms
                ICollection<ElementId> untaggedRoomIds = new List<ElementId>();
                foreach (ElementId roomId in roomIds)
                {
                    Room room = doc.GetElement(roomId) as Room;

                    if (room != null && room.GetParameters("Tag").FirstOrDefault() == null)
                    {
                        untaggedRoomIds.Add(roomId);
                    }
                }

                // Get the loaded tag family
                FilteredElementCollector tagCollector = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_RoomTags)
                    .WhereElementIsElementType();

                FamilySymbol tagSymbol = tagCollector.Cast<FamilySymbol>().FirstOrDefault();

                // Create a new tag for each untagged room
                foreach (ElementId roomId in untaggedRoomIds)
                {
                    Room room = doc.GetElement(roomId) as Room;
                    if (room != null)
                    {
                        // Create a reference for the room
                        Reference roomReference = new Reference(room);

                        // Get the room location point
                        XYZ roomLocation = (room.Location as LocationPoint)?.Point;

                        // Create a new tag at the room location
                        IndependentTag newTag = IndependentTag.Create(
                            doc,
                            tagSymbol.Id,
                            view.Id,
                            roomReference,
                            false,
                            TagOrientation.Horizontal,
                            roomLocation);

                        // Assign the tag family symbol to the newly created tag
                        newTag.ChangeTypeId(tagSymbol.Id);

                        // Increment the count of tagged rooms
                        processedViewsCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and log or display error message
                Debug.Print($"Error in TagUntaggedRoomsInView: {ex.Message}");
                return 0;
            }

            return processedViewsCount;
        }

        private static bool IsValidFloorPlanView(View view)
        {
            return view.ViewType == ViewType.FloorPlan || view.ViewType == ViewType.CeilingPlan;
        }

        public static int TagUntaggedRoomsInView5(Document doc, UIDocument uidoc, View view)
        {
            int processedViewsCount = 0;

            // Check if the provided view is valid for element iteration
            if (view.CanBePrinted)
            {
                // Retrieve all the rooms within the view
                ICollection<ElementId> roomIds = new FilteredElementCollector(doc, view.Id)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElementIds();

                // Filter out the already tagged rooms
                ICollection<ElementId> untaggedRoomIds = new List<ElementId>();
                foreach (ElementId roomId in roomIds)
                {
                    Room room = doc.GetElement(roomId) as Room;

                    if (room != null && room.GetParameters("Tag").FirstOrDefault() == null)
                    {
                        untaggedRoomIds.Add(roomId);
                    }
                }

                // Get the loaded tag family
                FilteredElementCollector tagCollector = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_RoomTags)
                    .WhereElementIsElementType();

                FamilySymbol tagSymbol = tagCollector.Cast<FamilySymbol>().FirstOrDefault();

                // Create a new tag for each untagged room
                List<ElementId> tagIds = new List<ElementId>();
                foreach (ElementId roomId in untaggedRoomIds)
                {
                    Room room = doc.GetElement(roomId) as Room;
                    if (room != null)
                    {
                        // Create a reference for the room
                        Reference roomReference = new Reference(room);

                        // Get the room location point
                        XYZ roomLocation = (room.Location as LocationPoint)?.Point;

                        // Create a new tag at the room location
                        IndependentTag newTag = IndependentTag.Create(
                            doc,
                            tagSymbol.Id,
                            view.Id,
                            roomReference,
                            false,
                            TagOrientation.Horizontal,
                            roomLocation);

                        // Assign the tag family symbol to the newly created tag
                        newTag.ChangeTypeId(tagSymbol.Id);
                        tagIds.Add(newTag.Id);
                        // Increment the count of tagged rooms
                        processedViewsCount++;
                    }
                }
            }
            return processedViewsCount;
        }

        public static int TagUntaggedRoomsInView4(Document doc, UIDocument uidoc, View view)
        {
            int processedViewsCount = 0;

            // Check if the provided view is valid for element iteration
            if (view.CanBePrinted)
            {
                // Retrieve all the rooms within the view
                ICollection<ElementId> roomIds = new FilteredElementCollector(doc, view.Id)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElementIds();

                // Filter out the already tagged rooms
                ICollection<ElementId> untaggedRoomIds = new List<ElementId>();
                foreach (ElementId roomId in roomIds)
                {
                    Room room = doc.GetElement(roomId) as Room;

                    if (room != null && room.GetParameters("Tag").FirstOrDefault() == null)
                    {
                        untaggedRoomIds.Add(roomId);
                    }
                }

                // Get the loaded tag family
                FilteredElementCollector tagCollector = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_RoomTags)
                    .WhereElementIsElementType();

                FamilySymbol tagSymbol = tagCollector.Cast<FamilySymbol>().FirstOrDefault();

                // Create a new tag for each untagged room
                List<ElementId> tagIds = new List<ElementId>();
                foreach (ElementId roomId in untaggedRoomIds)
                {
                    Room room = doc.GetElement(roomId) as Room;
                    if (room != null)
                    {
                        // Create a reference for the room
                        Reference roomReference = new Reference(room);

                        // Get the room location point
                        XYZ roomLocation = (room.Location as LocationPoint)?.Point;

                        // Create a new tag at the room location
                        IndependentTag newTag = IndependentTag.Create(
                            doc,
                            tagSymbol.Id,
                            view.Id,
                            roomReference,
                            false,
                            TagOrientation.Horizontal,
                            roomLocation);

                        // Assign the tag family symbol to the newly created tag
                        newTag.ChangeTypeId(tagSymbol.Id);
                        tagIds.Add(newTag.Id);
                        //increment the count of tagged rooms
                        processedViewsCount++;
                    }
                }
            }
            return processedViewsCount;
        }

        public static int TagUntaggedRoomsInView3(Document doc, UIDocument uidoc, View view)
        {
            int processedRoomsCount = 0;

            // Check if the provided view is a floor plan or ceiling plan
            if (view.ViewType == ViewType.FloorPlan || view.ViewType == ViewType.CeilingPlan)
            {
                // Retrieve all the rooms within the view
                ICollection<ElementId> roomIds = new FilteredElementCollector(doc, view.Id)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElementIds();

                // Get the loaded tag family
                FilteredElementCollector tagCollector = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_RoomTags)
                    .WhereElementIsElementType();

                FamilySymbol tagSymbol = tagCollector.Cast<FamilySymbol>().FirstOrDefault();

                // Filter out the already tagged rooms
                ICollection<ElementId> untaggedRoomIds = roomIds
                    .Select(roomId => doc.GetElement(roomId) as Room)
                    .Where(room => room != null && room.GetParameters("Tag").FirstOrDefault() == null)
                    .Select(room => room.Id)
                    .ToList();

                // Create a new tag for each untagged room
                List<ElementId> tagIds = new List<ElementId>();
                foreach (ElementId roomId in untaggedRoomIds)
                {
                    Room room = doc.GetElement(roomId) as Room;
                    if (room != null)
                    {
                        // Create a reference for the room
                        Reference roomReference = new Reference(room);

                        // Get the room location point
                        XYZ roomLocation = (room.Location as LocationPoint)?.Point;

                        // Create a new tag at the room location
                        IndependentTag newTag = IndependentTag.Create(
                            doc,
                            tagSymbol.Id,
                            view.Id,
                            roomReference,
                            false,
                            TagOrientation.Horizontal,
                            roomLocation);

                        // Assign the tag family symbol to the newly created tag
                        newTag.ChangeTypeId(tagSymbol.Id);
                        tagIds.Add(newTag.Id);

                        // Increment the count of tagged rooms
                        processedRoomsCount++;
                    }
                }

                // Set the selection to the newly created tags
                if (tagIds.Count > 0)
                {
                    uidoc.Selection.SetElementIds(tagIds);
                }
            }

            return processedRoomsCount;
        }

        public static int TagUntaggedRoomsInView2(Document doc, UIDocument uidoc, View view)
        {
            int processedViewsCount = 0;

            // Check if the provided view is a floor plan or ceiling plan
            if (view.ViewType == ViewType.FloorPlan || view.ViewType == ViewType.CeilingPlan)
            {
                // Retrieve all the rooms within the view
                ICollection<ElementId> roomIds = new FilteredElementCollector(doc, view.Id)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElementIds();

                // Filter out the already tagged rooms
                ICollection<ElementId> untaggedRoomIds = new List<ElementId>();
                foreach (ElementId roomId in roomIds)
                {
                    Room room = doc.GetElement(roomId) as Room;

                    if (room != null && room.GetParameters("Tag").FirstOrDefault() == null)
                    {
                        untaggedRoomIds.Add(roomId);
                    }
                }

                // Get the loaded tag family
                FilteredElementCollector tagCollector = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_RoomTags)
                    .WhereElementIsElementType();

                FamilySymbol tagSymbol = tagCollector.Cast<FamilySymbol>().FirstOrDefault();

                // Create a new tag for each untagged room
                List<ElementId> tagIds = new List<ElementId>();
                foreach (ElementId roomId in untaggedRoomIds)
                {
                    Room room = doc.GetElement(roomId) as Room;
                    if (room != null)
                    {
                        // Create a reference for the room
                        Reference roomReference = new Reference(room);

                        // Get the room location point
                        XYZ roomLocation = (room.Location as LocationPoint)?.Point;

                        // Create a new tag at the room location
                        IndependentTag newTag = IndependentTag.Create(
                            doc,
                            tagSymbol.Id,
                            view.Id,
                            roomReference,
                            false,
                            TagOrientation.Horizontal,
                            roomLocation);

                        // Assign the tag family symbol to the newly created tag
                        newTag.ChangeTypeId(tagSymbol.Id);
                        tagIds.Add(newTag.Id);
                        //increment the count of tagged rooms
                        processedViewsCount++;

                    }
                }
            }
            return processedViewsCount;
        }


    }// End of MyUtils
}

