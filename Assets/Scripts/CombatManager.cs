using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CombatManager : CombatStateHandler {
    public void StartCombat(){
        if(IsCombatActive()){
            Debug.LogWarning("[WARN]: Trying to start combat while combat is running");
            return;
        }

        _combatPanel.gameObject.SetActive(true);
        _defaultCamPosition = "Overhead";
        _roundCount = 1;
        UpdateSlots();
        StartCoroutine(PlayCombat());
    }
    public void MoveCamTo(Transform target, bool priority = true){
        _combatCamera.MoveCamTo(target, priority);
    }
    public void MoveCamToDefault(String slotTag = "", bool priority = true){
        if(slotTag.CompareTo("") != 0)
            _defaultCamPosition = slotTag;

        ControlledCamTag = "";
    }
    public CombatantSlot GetCombatantSlot(String slotTag){
        if(!_slotDict.ContainsKey(slotTag)) return null;

        return _slotDict[slotTag];
    }
    
    private IEnumerator PlayCombat(){
        // Debug.Log("[COMBAT]: Combat Started");

        while(!ShouldCombatEnd()){
            if(!_roundPlaying) 
                StartCoroutine(PlayRound());
                
            yield return new WaitForEndOfFrame();
        }

        EndCombat();
    }
    private IEnumerator PlayRound(){
        Debug.Log($"[COMBAT]: Round {_roundCount} Started");

        _roundPlaying = true;
        _turnCount = 0;

        while(!ShouldRoundEnd()){
            if(!_turnPlaying && !DidPlayerSubmitMoves()) 
                StartCoroutine(PlayTurn());
            else if(DidPlayerSubmitMoves()){
                _defaultCamPosition = "Overhead";
            }

            yield return new WaitForEndOfFrame();
        }
        
        _cameraControllable = false;
        _defaultCamPosition = "Overhead";

        do {
            if(!_processingRoundMoves)
                StartCoroutine(ProcessMoves());

            yield return new WaitForEndOfFrame();
        } while(_processingRoundMoves);

        EndRound();
    }
    private IEnumerator PlayTurn(){
        Debug.Log($"[COMBAT]: Playing Turn {_turnCount}");
        _turnPlaying = true;
        _cameraControllable = true;
        _defaultCamPosition = "P" + (_turnCount + 1);

        UnitData unitData = DataLoader.Instance.GetUnitData(TurnUnit().UnitTag);
        _combatPanel.LoadButtonInfo(TurnUnit(), unitData.MoveList);

        while(!ShouldTurnEnd()){
            yield return new WaitForEndOfFrame();
        }

        EndTurn();
    }
    private IEnumerator ProcessMoves(){
        _processingRoundMoves = true;
        UpdateMovingCombatants();
        
        _processingUnitIndex = 0;
        while(_processingUnitIndex < _movingCombatants.Count){
            if(!MoveProcessor.Instance.IsProcessingMove){
                MoveProcessor.Instance.Process(_movingCombatants[_processingUnitIndex]);
                _processingUnitIndex++;
            }

            yield return new WaitForEndOfFrame();
        }
        
        _processingRoundMoves = false;
    }
    private void EndTurn(){
        // Debug.Log($"[COMBAT]: Ending Turn {_turnCount}");
        
        _cameraControllable = false;
        _turnPlaying = false;

        _turnCount++;
    }
    private void EndRound(){
        // Debug.Log($"[COMBAT]: Ending Round {_roundCount}");
        _debugEnemySubmittedMoves = false;

        _roundPlaying = false;
        
        foreach(Combatant unit in _playerTeam){
            unit.SubmittedMoveTag = "";
            unit.SubmittedSlotTargetTag = "";
        }
        foreach(Combatant unit in _enemyTeam){
            unit.SubmittedMoveTag = "";
            unit.SubmittedSlotTargetTag = "";
        }

        _roundCount++;
    }
    private void EndCombat(){
        Debug.Log("[COMBAT]: Ending Combat");
        
        _defaultCamPosition = "";
        _roundCount = 0;
    }

    
    private void UpdateCameraPosition(){
        if(_defaultCamPosition.CompareTo("") == 0) return;

        if(!_cameraControllable || ControlledCamTag.CompareTo("") == 0){
            if(_defaultCamPosition.CompareTo("Overhead") == 0)
                _combatCamera.MoveCamTo(_overheadCam, true);
            else
                _combatCamera.MoveCamTo(_slotDict[_defaultCamPosition].CameraPosition, true);
            return;
        }

        if(ControlledCamTag.CompareTo("Overhead") == 0)
            _combatCamera.MoveCamTo(_overheadCam, false);
        else
            _combatCamera.MoveCamTo(_slotDict[ControlledCamTag].CameraPosition, false);
    }

    public static CombatManager Instance { get; private set;}
    void Start(){
        if(Instance == null){
            Instance = this;
        } else {
            Destroy(this);
        }

        Setup();
    }
    void OnDestroy()
    {
        if(Instance == this){
            Instance = null;
        }
    }
    private void Setup(){
        _combatPanel.gameObject.SetActive(false);
        InitializeSlotDict();
    }
    void Update()
    {
        if(_debugStartCombat){
            _debugStartCombat = false;
            StartCombat();
        }
        UpdateCameraPosition();
    }
}
