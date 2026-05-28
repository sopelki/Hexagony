using System.Collections.Generic;
using System.Linq;
using HexagonScripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Field
{
    public class FieldGenerator : MonoBehaviour
    {
        public TileBase basicTile;

        [SerializeField]
        private LevelAsset currentLevel;

        [Header("TileRender")]
        public List<TileMapping> tileMappings;

        [Header("Object Render")]
        [Tooltip("Prefab names should start with objectMapping IDs")]
        public List<ObjectMapping> objectMappings;
        public Transform objectsContainer;

        private Field currentField;
        private Tilemap myTilemap;
        private Dictionary<TileBase, HexagonType> tileToTypeDict;
        private Dictionary<HexagonType, TileBase> typeToTileDict;

        private void Awake()
        {
            myTilemap = GetComponent<Tilemap>();
            SetupDictionaries();
        }

        public Field GetFieldFromAsset()
        {
            if (currentLevel == null || currentLevel.jsonFile == null)
            {
                Debug.LogError("currentLevel or jsonFile is null!");
                return null;
            }

            var json = currentLevel.jsonFile.text;
            var data = JsonUtility.FromJson<FieldData>(json);
            return ConvertDataToField(data);
        }


        public void Initialize(Field field)
        {
            currentField = field;

            ClearGrid();
            ClearDecorations();
            DrawHexagons();
            DrawObjects();
        }

        private static Field ConvertDataToField(FieldData data)
        {
            var field = new Field();

            foreach (var hex in data.savedHexes)
                field.AddHexagon(hex.offset.x, hex.offset.y, hex.type);

            field.MapObjects = data.savedObjects ?? new List<MapObjectData>();
            return field;
        }


        private void DrawHexagons()
        {
            if (myTilemap == null)
                myTilemap = GetComponent<Tilemap>();

            SetupDictionaries();

            foreach (var hexagon in currentField.Hexagons.Values)
            {
                if (typeToTileDict.TryGetValue(hexagon.type, out var tileToDraw))
                    myTilemap.SetTile(hexagon.offset, tileToDraw);
            }
        }

        private void DrawObjects()
        {
            if (objectsContainer == null || currentField.MapObjects == null)
                return;

            foreach (var objData in currentField.MapObjects)
            {
                var mapping = objectMappings.Find(m => m.id == objData.objectId);
                if (mapping != null && mapping.prefab != null)
                {
                    var worldPos = myTilemap.GetCellCenterWorld(objData.position);
                    var newObj = Instantiate(mapping.prefab, worldPos, Quaternion.identity, objectsContainer);
                    newObj.name = mapping.id;
                }
            }
        }

        private void ClearGrid()
        {
            if (myTilemap == null)
                myTilemap = GetComponent<Tilemap>();

            myTilemap?.ClearAllTiles();
        }

        private void ClearDecorations()
        {
            if (objectsContainer == null)
                return;

            for (var i = objectsContainer.childCount - 1; i >= 0; i--)
            {
                var obj = objectsContainer.GetChild(i).gameObject;
                if (Application.isPlaying)
                    Destroy(obj);
                else
                    DestroyImmediate(obj);
            }
        }

        private void ReadHexagonsFromBrush()
        {
            if (myTilemap == null)
                myTilemap = GetComponent<Tilemap>();

            SetupDictionaries();

            var bounds = myTilemap.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                var tileOnScene = myTilemap.GetTile(pos);
                if (tileOnScene != null && tileToTypeDict.TryGetValue(tileOnScene, out var foundType))
                    currentField.AddHexagon(pos.x, pos.y, foundType);
            }
        }

        private void ReadObjectsFromScene()
        {
            currentField.MapObjects.Clear();
            if (objectsContainer == null)
                return;

            foreach (Transform child in objectsContainer)
            {
                var mapping = objectMappings.Find(m => child.name.StartsWith(m.id));
                if (mapping != null)
                {
                    currentField.MapObjects.Add(new MapObjectData
                    {
                        position = myTilemap.WorldToCell(child.position),
                        objectId = mapping.id
                    });
                }
            }
        }

        private void SetupDictionaries()
        {
            typeToTileDict = new Dictionary<HexagonType, TileBase>();
            tileToTypeDict = new Dictionary<TileBase, HexagonType>();

            foreach (var mapping in tileMappings.Where(m => m.tileAsset != null))
            {
                typeToTileDict[mapping.type] = mapping.tileAsset;
                if (!tileToTypeDict.ContainsKey(mapping.tileAsset))
                    tileToTypeDict.Add(mapping.tileAsset, mapping.type);
            }
        }

#if UNITY_EDITOR

        [ContextMenu("Load Field From File")]
        public void LoadAndDraw()
        {
            // ClearField();

            var loadedField = SaveLoadManager.LoadMapFromFile();

            if (loadedField != null)
            {
                Initialize(loadedField);
                Debug.Log(
                    $"Field Loaded: {currentField.Hexagons.Count} tiles, {currentField.MapObjects.Count} objects.");
            }
            else
            {
                Debug.LogWarning("Load Field From File: File not found. Generating default grid.");
                // GenerateAndDraw();
            }
        }

        [ContextMenu("Save Field To File")]
        public void SaveGrid()
        {
            currentField = new Field();

            ReadHexagonsFromBrush();
            ReadObjectsFromScene();

            if (currentField.Hexagons.Count > 0)
            {
                SaveLoadManager.SaveMapToFile(currentField);
                Debug.Log($"Saved {currentField.Hexagons.Count} tiles and {currentField.MapObjects.Count} objects.");
            }
            else
                Debug.LogWarning("Save Field To File: Tilemap is empty.");
        }

        [ContextMenu("Clear Field (Editor Only)")]
        public void ClearField()
        {
            ClearGrid();
            ClearDecorations();

            Debug.Log("Field cleared (Editor mode).");
        }

        // [ContextMenu("Generate Random Grid")]
        // public void GenerateAndDraw()
        // {
        //     ClearField();
        //     CurrentField = new Field();
        //     CurrentField.GenerateFieldData(FieldWidth, FieldHeight);
        //     DrawHexagons(CurrentField);
        // }
#endif
    }
}