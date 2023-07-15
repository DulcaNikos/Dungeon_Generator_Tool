using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class DungeonTool : EditorWindow
{
    // The ToolData object that will hold the data from the JSON file
    ToolData toolData = new ToolData();

    // Game object references for each type of room
    GameObject Room_E;
    GameObject Room_ES;
    GameObject Room_ESW;
    GameObject Room_EW;
    GameObject Room_N;
    GameObject Room_NE;
    GameObject Room_NES;
    GameObject Room_NEW;
    GameObject Room_NS;
    GameObject Room_NSW;
    GameObject Room_NW;
    GameObject Room_S;
    GameObject Room_SW;
    GameObject Room_W;

    //100, 0 ,100
    // The size of the spawn area 
    Vector3Int DungeonSize;

    // 100
    // The number of rooms the tool will try to create
    int iterations;

    // The position of the currently generated room
    Vector3 roomPosition;

    // The empty room prefab that will be instantiated to fill empty spaces
    GameObject RoomEmpty;

    // Reference to the root game object of the dungeon
    GameObject root;

    // The index of the last visited tile
    int lastVisitedTileIndex;

    // The dimensions of the dungeon
    int Length;
    int Width;
    int size;

    // An array to hold the generated path
    int[] path;

    // An array to hold the positions of each room
    Vector3Int[] roomPositions;

    // A list to hold the cells that have been visited during generation
    List<Cell> VisitedRooms = new List<Cell>();

    // The menu item to enable the dungeon generator window
    [MenuItem("My Tools/Dungeon Generator", false, 20)]
    public static void EnableWindow()
    {
        GetWindow(typeof(DungeonTool));
    }

    // Called when the script is loaded
    private void OnEnable()
    {
        // Load the data from the JSON file
        LoadDataFromJSON();

        // Find the root gameobject of the dungeon in the scene by name
        root = GameObject.Find("Dungeon");
    }

    // Loads the data from the JSON file into the toolData object
    void LoadDataFromJSON()
    {
        // Get the file path of the JSON file
        string filePath = Application.dataPath + "/DungeonTool.json";
        //string filePath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "/DungeonTool.json";

        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Read the file contents
            string json = File.ReadAllText(filePath);

            // Deserialize the JSON data into the toolData object
            toolData = JsonConvert.DeserializeObject<ToolData>(json);

            // Load to the room prefabs the file paths stored in the toolData object
            if (!string.IsNullOrEmpty(toolData.Room_E_Path)) Room_E = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_E_Path);
            if (!string.IsNullOrEmpty(toolData.Room_ES_Path)) Room_ES = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_ES_Path);
            if (!string.IsNullOrEmpty(toolData.Room_ESW_Path)) Room_ESW = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_ESW_Path);
            if (!string.IsNullOrEmpty(toolData.Room_EW_Path)) Room_EW = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_EW_Path);
            if (!string.IsNullOrEmpty(toolData.Room_N_Path)) Room_N = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_N_Path);
            if (!string.IsNullOrEmpty(toolData.Room_NE_Path)) Room_NE = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_NE_Path);
            if (!string.IsNullOrEmpty(toolData.Room_NES_Path)) Room_NES = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_NES_Path);
            if (!string.IsNullOrEmpty(toolData.Room_NEW_Path)) Room_NEW = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_NEW_Path);
            if (!string.IsNullOrEmpty(toolData.Room_NS_Path)) Room_NS = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_NS_Path);
            if (!string.IsNullOrEmpty(toolData.Room_NSW_Path)) Room_NSW = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_NSW_Path);
            if (!string.IsNullOrEmpty(toolData.Room_NW_Path)) Room_NW = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_NW_Path);
            if (!string.IsNullOrEmpty(toolData.Room_S_Path)) Room_S = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_S_Path);
            if (!string.IsNullOrEmpty(toolData.Room_SW_Path)) Room_SW = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_SW_Path);
            if (!string.IsNullOrEmpty(toolData.Room_W_Path)) Room_W = AssetDatabase.LoadAssetAtPath<GameObject>(toolData.Room_W_Path);

            //Assign iterations and DungeonSize values from loaded data to current values
            iterations = toolData.iterations;
            DungeonSize = toolData.DungeonSize;
        }
    }

    void SaveDataToJson()
    {
        // Save Room assets paths to toolData if they are not null
        if (Room_E != null) toolData.Room_E_Path = AssetDatabase.GetAssetPath(Room_E);
        if (Room_ES != null) toolData.Room_ES_Path = AssetDatabase.GetAssetPath(Room_ES);
        if (Room_ESW != null) toolData.Room_ESW_Path = AssetDatabase.GetAssetPath(Room_ESW);
        if (Room_EW != null) toolData.Room_EW_Path = AssetDatabase.GetAssetPath(Room_EW);
        if (Room_N != null) toolData.Room_N_Path = AssetDatabase.GetAssetPath(Room_N);
        if (Room_NE != null) toolData.Room_NE_Path = AssetDatabase.GetAssetPath(Room_NE);
        if (Room_NES != null) toolData.Room_NES_Path = AssetDatabase.GetAssetPath(Room_NES);
        if (Room_NEW != null) toolData.Room_NEW_Path = AssetDatabase.GetAssetPath(Room_NEW);
        if (Room_NS != null) toolData.Room_NS_Path = AssetDatabase.GetAssetPath(Room_NS);
        if (Room_NSW != null) toolData.Room_NSW_Path = AssetDatabase.GetAssetPath(Room_NSW);
        if (Room_NW != null) toolData.Room_NW_Path = AssetDatabase.GetAssetPath(Room_NW);
        if (Room_S != null) toolData.Room_S_Path = AssetDatabase.GetAssetPath(Room_S);
        if (Room_SW != null) toolData.Room_SW_Path = AssetDatabase.GetAssetPath(Room_SW);
        if (Room_W != null) toolData.Room_W_Path = AssetDatabase.GetAssetPath(Room_W);

        // Assign iterations and DungeonSize values to toolData
        toolData.iterations = iterations;
        toolData.DungeonSize = DungeonSize;

        // Save toolData to DungeonTool.json file

        string filePath = Application.dataPath + "/DungeonTool.json";
        //string filePath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "/DungeonTool.json";

        string json = JsonConvert.SerializeObject(toolData, Formatting.Indented);
     
        File.WriteAllText(filePath, json);
    }

    void DeleteDataFronJson()
    {
        // Delete DungeonTool.json file and its meta file if they exist
        string filePath = Application.dataPath + "/DungeonTool.json";
        //string filePath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "/DungeonTool.json";

        string jsonMeta = filePath + ".meta";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            File.Delete(jsonMeta);
        }
        // Refresh the AssetDatabase
        AssetDatabase.Refresh();
    }

    // This method is called when the GUI is being rendered in the Unity Editor.
    private void OnGUI()
    {
        #region Rooms Prefabs
        // This label is used to group the fields for the room prefabs.
        EditorGUILayout.LabelField("Rooms Prefab", EditorStyles.boldLabel);

        // These fields are used to set the room prefabs for each direction.
        // The user can drag and drop GameObjects onto these fields in the Unity Editor.
        Room_E = (GameObject)EditorGUILayout.ObjectField("Room_E", Room_E, typeof(GameObject), true);
        Room_ES = (GameObject)EditorGUILayout.ObjectField("Room_ES", Room_ES, typeof(GameObject), true);
        Room_ESW = (GameObject)EditorGUILayout.ObjectField("Room_ESW", Room_ESW, typeof(GameObject), true);
        Room_EW = (GameObject)EditorGUILayout.ObjectField("Room_EW", Room_EW, typeof(GameObject), true);
        Room_N = (GameObject)EditorGUILayout.ObjectField("Room_N", Room_N, typeof(GameObject), true);
        Room_NE = (GameObject)EditorGUILayout.ObjectField("Room_NE", Room_NE, typeof(GameObject), true);
        Room_NES = (GameObject)EditorGUILayout.ObjectField("Room_NES", Room_NES, typeof(GameObject), true);
        Room_NEW = (GameObject)EditorGUILayout.ObjectField("Room_NEW", Room_NEW, typeof(GameObject), true);
        Room_NS = (GameObject)EditorGUILayout.ObjectField("Room_NS", Room_NS, typeof(GameObject), true);
        Room_NSW = (GameObject)EditorGUILayout.ObjectField("Room_NSW", Room_NSW, typeof(GameObject), true);
        Room_NW = (GameObject)EditorGUILayout.ObjectField("Room_NW", Room_NW, typeof(GameObject), true);
        Room_S = (GameObject)EditorGUILayout.ObjectField("Room_S", Room_S, typeof(GameObject), true);
        Room_SW = (GameObject)EditorGUILayout.ObjectField("Room_SW", Room_SW, typeof(GameObject), true);
        Room_W = (GameObject)EditorGUILayout.ObjectField("Room_W", Room_W, typeof(GameObject), true);
        #endregion

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        // This field is used to set the size of the the spawn area.
        DungeonSize = EditorGUILayout.Vector3IntField(new GUIContent("Dungeon Size", "Set the boundaries of the dungeon. X and Z axis is recommended to be equal and Y axis should be 0"), DungeonSize);

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        // This slider is used to set the number of how many rooms the dungeon will try to have".
        EditorGUILayout.LabelField(new GUIContent("Iterations", "Select how many rooms the dungeon will try to have"));
        iterations = EditorGUILayout.IntSlider(iterations, 0, 100);

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        #region Buttons
        // Generate the dungeon.
        // The user must first set the room prefabs, dungeon size, and iterations before generating the dungeon.
        // If the user has already generated a dungeon, pressing this button again will delete the previous dungeon.
        if (GUILayout.Button(new GUIContent("Generate Dungeon", "This button will create a dungeon, you have to assign first the rooms, the boundaries of the dungeon and the iterations. If you have created one and you press the button again the previous one will be deleted")))
        {
            GenerateDungeon();

            // This line marks the scene as dirty so that any changes made to the scene will be saved.
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            //OR you can move only the dungeon to a new scene with SceneManager.MoveGameObjectToScen
        }

        // Delete the current dungeon.
        if (GUILayout.Button(new GUIContent("Delete Dungeon", "Delete the dungeon that you've created")))
        {
            DeleteDungeon();

            // This line marks the scene as dirty so that any changes made to the scene will be saved.
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        // Save any changes made to the dungeon as a JSON file
        if (GUILayout.Button(new GUIContent("Save Data", "Save your changes on a JSON file")))
        {
            SaveDataToJson();
        }

        // Delete the previously saved JSON file
        if (GUILayout.Button(new GUIContent("Delete Data", "Delete the JSON file")))
        {
            DeleteDataFronJson();
        }
        #endregion
    }

    void GenerateDungeon()
    {
        // Delete any previously generated dungeon and clear the list of visited rooms
        DeleteDungeon();
        VisitedRooms.Clear();

        // Create a new game object to act as the root of the dungeon
        root = new GameObject();
        root.name = "Dungeon";

        // Determine the size of the dungeon based on the provided DungeonSize vector
        Length = DungeonSize.x;
        Width = DungeonSize.z;
        size = Length * Width;

        // Clear the path array that will be used to keep track of the path through the dungeon
        path = new int[size];
        for (int i = 0; i < size; i++) path[i] = 0;

        // Set up the positions of all the rooms in the dungeon
        roomPositions = new Vector3Int[size];

        for (int z = 0; z < Width; z++)
        {
            for (int x = 0; x < Length; x++)
            {
                int i = x + z * Width;
                // Calculate the position of the room at this index using x and z values and set it in the roomPositions array
                // The scale of each room is assumed to be 6 units for each axis, so the position is scaled accordingly
                roomPositions[i] = new Vector3Int(x * 6, 0, z * 6);
                //SOS
                //you can change x * 6 or z * 6 to something like  * 10 or * 20
                //but you have to check that this values corresponds to prefabs room scales
            }
        }

        // Start the path through the dungeon at the first tile (index 0)
        lastVisitedTileIndex = 0;

        // Iterate through all possible rooms, marking which ones can be connected to the path
        IterateThroughPossibleRoomms();

        // Place the correct rooms at each position in the dungeon based on the results of the iteration
        PlaceCorrectRooms();

    }

    void DeleteDungeon()
    {
        // Deletes the dungeon by destroying the root game object if exists.
        if (root) DestroyImmediate(root);
    }

    void IterateThroughPossibleRoomms()
    {
        //Iterates through possible rooms for a given number of iterations.
        for (int i = 0; i < iterations; i++)
        {
            FindTheNextRoom();
        }
    }

    // Finds the next room by randomly picking a direction and checking if there's a room there.
    void FindTheNextRoom()
    {
        for (int i = 0; i < 8; i++)
        {
            // Pick a random direction
            int randDirection = UnityEngine.Random.Range(0, 4);
            // North
            if (randDirection == 0)
            {
                // Find the cell above
                int index = lastVisitedTileIndex + Length;
                // if the cell is within our dungeon and has not been visited before
                if (index < size && !HasRoomBeenVisited(index))
                {
                    BuildRoom(index);
                    break; ;
                }
            }

            // East
            if (randDirection == 1)
            {
                int index = lastVisitedTileIndex + 1;

                // if the cell is within our dungeon, is on the same row as the previous room and has not been visited before
                if (index < size && roomPositions[index].z == roomPositions[lastVisitedTileIndex].z && !HasRoomBeenVisited(index))
                {
                    BuildRoom(index);
                    break;
                }
            }
            // South
            if (randDirection == 2)
            {
                int index = lastVisitedTileIndex - Length;
                // if the cell is within our dungeon and has not been visited before
                if (index >= 0 && !HasRoomBeenVisited(index))
                {
                    BuildRoom(index);
                    break;
                }
            }

            // West
            if (randDirection == 3)
            {
                int index = lastVisitedTileIndex - 1;
                // if the cell is within our dungeon, is on the same column as the previous room and has not been visited before
                if (index >= 0 && roomPositions[index].x == roomPositions[lastVisitedTileIndex].x && !HasRoomBeenVisited(index))
                {
                    BuildRoom(index);
                    break;
                }
            }
        }
    }

    //This function checks if a room has already been visited before by looking at the path array.
    //If the value at the given index is 0, it means the room has not been visited yet, and the function returns false.
    //Otherwise, it returns true.
    bool HasRoomBeenVisited(int index)
    {
        if (path[index] == 0) return false;
        else return true;
    }

    void BuildRoom(int index)
    {
        // Mark it as visited
        path[index] = 1;
        // Store it as last visited
        lastVisitedTileIndex = index;
        // Add the room to the list of visited rooms
        AddCellToList(index);
    }

    void AddCellToList(int index)
    {
        //creates a new Cell struct with information about the visited room,
        //such as its index in the roomPositions array and its position in the X and Z axes.
        //It sets the visited flag to true, and adds the struct to the VisitedRooms list.
        Cell newCell;
        newCell.index = index;
        newCell.posX = roomPositions[index].x;
        newCell.posZ = roomPositions[index].z;
        newCell.visited = true;

        VisitedRooms.Add(newCell);
    }

    // This function is responsible for building the correct rooms in the correct positions.
    void PlaceCorrectRooms()
    {
        // Instantiate the corresponding game object of the first room
        GameObject Go0 = RoomEmpty;

        //Determine the type of the next room to be built
        string roomType0 = DetermineLocationOfNextRoom(0);


        if (roomType0 == "N") Go0 = Room_N;

        if (roomType0 == "E") Go0 = Room_E;

        if (roomType0 == "S") Go0 = Room_S;

        if (roomType0 == "W") Go0 = Room_W;

        // Set the position of the first room and instantiate it
        Vector3 pos0 = new Vector3(VisitedRooms[0].posX, 0, VisitedRooms[0].posZ);
        Instantiate(Go0, pos0, Quaternion.identity, root.transform);

        // Instantiate the corresponding game object of the last room
        GameObject GoL = RoomEmpty;

        // Determine the type of the previous room to be built
        string roomTypeL = DetermineLocationOfPreviousRoom(VisitedRooms.Count - 1);

        if (roomTypeL == "N") GoL = Room_N;

        if (roomTypeL == "E") GoL = Room_E;

        if (roomTypeL == "S") GoL = Room_S;

        if (roomTypeL == "W") GoL = Room_W;

        // Set the position of the last room and instantiate it
        Vector3 posL = new Vector3(VisitedRooms[VisitedRooms.Count - 1].posX, 0, VisitedRooms[VisitedRooms.Count - 1].posZ);
        Instantiate(GoL, posL, Quaternion.identity, root.transform);

        // Build Path
        // Determine the type of each intermediate room to be built and instantiate it
        for (int i = 1; i < VisitedRooms.Count - 1; i++)
        {
            GameObject Go = RoomEmpty;
            string roomType = DetermineLocationOfNextRoom(i) + DetermineLocationOfPreviousRoom(i);

            if (roomType == "NE" || roomType == "EN") Go = Room_NE;

            if (roomType == "NS" || roomType == "SN") Go = Room_NS;

            if (roomType == "NW" || roomType == "WN") Go = Room_NW;

            if (roomType == "ES" || roomType == "SE") Go = Room_ES;

            if (roomType == "EW" || roomType == "WE") Go = Room_EW;

            if (roomType == "SW" || roomType == "WS") Go = Room_SW;

            if (roomType == "NES") Go = Room_SW;

            if (roomType == "NEW") Go = Room_NEW;

            if (roomType == "NSW") Go = Room_NSW;

            if (roomType == "ESW") Go = Room_ESW;

            Vector3 pos = new Vector3(VisitedRooms[i].posX, 0, VisitedRooms[i].posZ);
            Instantiate(Go, pos, Quaternion.identity, root.transform);
        }

    }
    // Determines the direction of the next room relative to the current room at index i in VisitedRooms
    // It returns a string of two characters representing the direction:
    // "N" for north, "S" for south, "E" for east, "W" for west
    string DetermineLocationOfNextRoom(int i)
    {
        string str1 = "";
        string str2 = "";

        // Check if the next room is north or south of the current room
        if (VisitedRooms[i].posZ < VisitedRooms[i + 1].posZ) str1 = "N";
        else if (VisitedRooms[i].posZ > VisitedRooms[i + 1].posZ) str1 = "S";

        // Check if the next room is east or west of the current room
        if (VisitedRooms[i].posX < VisitedRooms[i + 1].posX) str2 = "E";
        else if (VisitedRooms[i].posX > VisitedRooms[i + 1].posX) str2 = "W";

        // Combine the two direction strings and return the result
        return str1 + str2;
    }

    // Determines the direction of the previous room relative to the current room at index i in VisitedRooms
    // It returns a string of two characters representing the direction:
    // "N" for north, "S" for south, "E" for east, "W" for west
    string DetermineLocationOfPreviousRoom(int i)
    {
        string str1 = "";
        string str2 = "";

        // Check if the previous room is north or south of the current room
        if (VisitedRooms[i].posZ < VisitedRooms[i - 1].posZ) str1 = "N";
        else if (VisitedRooms[i].posZ > VisitedRooms[i - 1].posZ) str1 = "S";

        // Check if the previous room is east or west of the current room
        if (VisitedRooms[i].posX < VisitedRooms[i - 1].posX) str2 = "E";
        else if (VisitedRooms[i].posX > VisitedRooms[i - 1].posX) str2 = "W";

        // Combine the two direction strings and return the result
        return str1 + str2;
    }
}

// This is a struct representing a cell in the dungeon grid
// It contains an index, x and z position, and a boolean flag indicating whether it has been visited or not
[System.Serializable]
public struct Cell
{
    public int index;
    public int posX;
    public int posZ;
    public bool visited;
}

// This is a class representing the data for a procedural dungeon generation tool
// It contains strings representing the paths to each type of room prefab,
// as well as the size of the dungeon grid and the number of iterations to run the generator for
[System.Serializable]
public class ToolData
{
    public string Room_E_Path;
    public string Room_ES_Path;
    public string Room_ESW_Path;
    public string Room_EW_Path;
    public string Room_N_Path;
    public string Room_NE_Path;
    public string Room_NES_Path;
    public string Room_NEW_Path;
    public string Room_NS_Path;
    public string Room_NSW_Path;
    public string Room_NW_Path;
    public string Room_S_Path;
    public string Room_SW_Path;
    public string Room_W_Path;

    public Vector3Int DungeonSize;

    public int iterations;
}