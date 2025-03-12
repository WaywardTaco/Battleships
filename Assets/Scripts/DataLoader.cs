using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;
using UnityEngine.UI;

public class DataLoader : MonoBehaviour
{
    [SerializeField] private string UNIT_FILEPATH = "/StatBlocks/Units/";
    [SerializeField] private string MOVES_FILEPATH = "/StatBlocks/Moves/";
    [SerializeField] private string UNIT_SPRITES_FILEPATH = "/Sprites/Units/";
    [SerializeField] private Vector2 UNIT_SPRITE_PIVOT = new(0.5f, 0.5f);
    [SerializeField] private float UNIT_SPRITE_PIXELS_PER_UNIT = 100.0f;

    [SerializeField] private bool _debugLoadUnits;
    [SerializeField] private bool _debugLoadMoves;
    [SerializeField] private bool _debugOutputUnits;
    [SerializeField] private bool _debugOutputMoves;

    [SerializeField] private List<UnitData> _loadedUnits = new();
    [SerializeField] private List<MoveData> _loadedMoves = new();
    [SerializeReference] private List<Sprite> _loadedSprites = new();
    private Dictionary<String, UnitData> _unitDict = new();
    private Dictionary<String, MoveData> _movesDict = new();
    private Dictionary<String, Sprite> _spritesDict = new();

    public UnitData GetUnitData(String unitTag){
        if(!_unitDict.ContainsKey(unitTag)) return null;

        return _unitDict[unitTag];
    }

    public Sprite GetUnitSprite(String filename){
        if(_spritesDict.ContainsKey(filename)) 
            return _spritesDict[filename];

        string spritePath = Path.Combine(Application.streamingAssetsPath, UNIT_SPRITES_FILEPATH, filename);

        if(!Directory.Exists(spritePath)){
            Debug.LogWarning($"[WARN]: Trying to access non-existent sprite: \"{spritePath}\"");
            return null;
        }

        byte[] imageData = File.ReadAllBytes(spritePath);

        Texture2D texture = new Texture2D(2,2);
        texture.LoadImage(imageData);

        Sprite newSprite = Sprite.Create(
            texture, 
            new Rect(0.0f, 0.0f, texture.width, texture.height),
            UNIT_SPRITE_PIVOT,
            UNIT_SPRITE_PIXELS_PER_UNIT
        );

        _loadedSprites.Add(newSprite);
        _spritesDict.Add(filename, newSprite);

        return newSprite;
    }

    private void LoadUnits(){
        _loadedUnits.Clear();
        _unitDict.Clear();

        DirectoryInfo unitDir = new DirectoryInfo(Application.streamingAssetsPath + UNIT_FILEPATH);
        FileInfo[] unitFiles = unitDir.GetFiles();
        foreach(FileInfo file in unitFiles){
            if(Path.GetExtension(file.FullName) != ".txt") continue;

            string[] importLines = File.ReadAllLines(file.FullName);
            string importString = string.Join("", importLines);

            // Debug.Log($"[DEBUG]: Importing Unit \"{importString}\"");
            UnitData unit = new UnitData();
            unit = JsonConvert.DeserializeObject<UnitData>(importString);
            _loadedUnits.Add(unit);
            _unitDict.Add(unit.UnitTag, unit);
        }

        Debug.Log($"[DEBUG]: Finished inputting unit files from \"{Application.streamingAssetsPath + UNIT_FILEPATH}\"");
    }
    private void OutputUnits(){
        foreach(UnitData unit in _loadedUnits){
            if(unit == null) continue;

            string outputString = JsonConvert.SerializeObject(unit, Formatting.Indented);
            StreamWriter file = File.CreateText(
                Application.streamingAssetsPath + UNIT_FILEPATH + unit.UnitTag + ".txt"
            );
            file.WriteLine(outputString);
            file.Close();
        }

        Debug.Log($"[DEBUG]: Finished outputting unit files to \"{Application.streamingAssetsPath + UNIT_FILEPATH}\"");
    }

    private void LoadMoves(){
        _loadedMoves.Clear();
        _movesDict.Clear();

        DirectoryInfo moveDir = new DirectoryInfo(Application.streamingAssetsPath + MOVES_FILEPATH);
        FileInfo[] moveFiles = moveDir.GetFiles();
        foreach(FileInfo file in moveFiles){
            if(Path.GetExtension(file.FullName) != ".txt") continue;

            string[] importLines = File.ReadAllLines(file.FullName);
            string importString = string.Join("", importLines);

            // Debug.Log($"[DEBUG]: Importing Move: \"{importString}\"");
            MoveData move = JsonConvert.DeserializeObject<MoveData>(importString);
            _loadedMoves.Add(move);
            _movesDict.Add(move.MoveTag, move);
        }

        Debug.Log($"[DEBUG]: Finished inputting move files from \"{Application.streamingAssetsPath + MOVES_FILEPATH}\"");
    }
    private void OutputMoves(){
        foreach(MoveData move in _loadedMoves){
            if(move == null) continue;

            string outputString = JsonConvert.SerializeObject(move, Formatting.Indented);
            StreamWriter file = File.CreateText(
                Application.streamingAssetsPath + MOVES_FILEPATH + move.MoveTag + ".txt"
            );
            file.WriteLine(outputString);
            file.Close();
        }

        Debug.Log($"[DEBUG]: Finished outputting move files to \"{Application.streamingAssetsPath + MOVES_FILEPATH}\"");
    }

    private void Setup(){
        
    }

    private void DebugUpdate(){
        if(_debugLoadUnits){
            _debugLoadUnits = false;
            LoadUnits();
        }

        if(_debugOutputUnits){
            _debugOutputUnits = false;
            OutputUnits();
        }

        if(_debugLoadMoves){
            _debugLoadMoves = false;
            LoadMoves();
        }

        if(_debugOutputMoves){
            _debugOutputMoves = false;
            OutputMoves();
        }
    }

    void Update(){
        DebugUpdate();
    }

    public static DataLoader Instance { get; private set;}
    void Start(){
        if(Instance == null){
            Instance = this;
        } else {
            Destroy(this);
        }

        Setup();
    }
    void OnDestroy(){
        if(Instance == this)
            Instance = null;
    }
}
