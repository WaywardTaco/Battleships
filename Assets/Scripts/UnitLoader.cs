using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class UnitLoader : MonoBehaviour
{
    private const string UNIT_FILEPATH = "/GameResources/Units/";

    [SerializeField] private bool _debugLoadUnits;
    [SerializeField] private bool _debugOutputUnits;

    [SerializeField] private List<TextAsset> _textfiles;
    [SerializeReference] private List<UnitScriptable> _loadedUnits;

    private void LoadUnits(){

    }

    private void OutputUnits(){
        foreach(UnitScriptable unit in _loadedUnits){
            if(unit == null) continue;

            string outputString = JsonConvert.SerializeObject(unit, Formatting.Indented);
            var file = File.CreateText(
                Application.dataPath + UNIT_FILEPATH + unit.UnitTag + ".txt"
            );
            file.WriteLine(outputString);
            file.Close();
        }
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
    }

    void Update(){
        DebugUpdate();
    }

    public static UnitLoader Instance { get; private set;}
    void Start(){
        if(Instance == null){
            Instance = this;
        } else {
            Destroy(this);
        }
    }
    void OnDestroy(){
        if(Instance == this)
            Instance = null;
    }
}
