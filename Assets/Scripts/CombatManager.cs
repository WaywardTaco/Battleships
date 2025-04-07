using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CombatManager : MonoBehaviour {
    
    [SerializeField] private CombatPhaseHandler _phaseHandler;
    [SerializeField] private CombatantHandler _combatantHandler;
    [SerializeField] private MoveProcessor _moveProcessor;
    [SerializeField] private CombatViewHandler _viewHandler;

    public void StartCombat(){
        if(_phaseHandler.IsCombatActive){
            Debug.LogWarning("[WARN]: Trying to start combat while combat is running");
            return;
        }

        StartCoroutine(_phaseHandler.PlayCombat());
    }

    public void SubmitTeam(string data, bool isAlly){

    }

    public void SubmitEnemyMoves(string data){

    }

    public void UpdateStatus(string data){
        
    }

    public void UpdateView(CombatPhase currentPhase){
        _viewHandler.SetView(currentPhase, _phaseHandler, _combatantHandler);
    }

    public void SlotClickCallback(CombatantSlot slot){
        if(_phaseHandler.IsWaitingPlayerMove){
            if(_viewHandler.LookingAt.CompareTo("Overhead") == 0)
                _viewHandler.ResetCamView();
            else
                _viewHandler.SetCamera("Overhead", false, true);
        }

        else if (_phaseHandler.IsWaitingTargetSelect){
            SubmitTarget(slot);
        }
    }

    public void SlotHoverCallback(CombatantSlot slot, bool isHoveredOn){
        _viewHandler.HoverSlot(slot, isHoveredOn);
    }

    public void ActionButtonClickCallback(ActionButton button){
        if(button.AssignedMove.MoveTag.CompareTo(_phaseHandler.TurnUnit.MoveTag) == 0)
            _combatantHandler.ResetMove(_phaseHandler.TurnUnit);
        // else if (_viewHandler.FocusingSlot.AssignedCombatant != _phaseHandler.TurnUnit)
        //     _viewHandler.ResetCamView();
        else
            SubmitMoveCallback(button.AssignedMove);
    }
    public void ActionButtonHoverCallback(ActionButton button, bool isHoveredOn){
        _viewHandler.HoverAction(button, isHoveredOn);
    }

    public void ClickOffCallback(){
        if(_phaseHandler.IsWaitingTargetSelect)
            _combatantHandler.ResetMove(_phaseHandler.TurnUnit);
    }

    private void SubmitMoveCallback(MoveData move){
        if(move == null){
            Debug.Log($"[COMBAT]: Non existent move selected");
            return;
        }

        if(!_phaseHandler.IsCombatActive){
            Debug.LogWarning("[WARN]: Trying to submit move with no combat active");
            return;
        }

        if(!_phaseHandler.IsWaitingPlayerMove && !_phaseHandler.IsWaitingTargetSelect){
            Debug.LogWarning("[WARN]: Trying to submit move while not waiting for a move");
            return;
        }

        _combatantHandler.SubmitMove(_phaseHandler.TurnUnit, move);
    }

    private void SubmitTarget(CombatantSlot slot){
        if(_phaseHandler.IsWaitingPlayerMove) return;
        if(!_moveProcessor.IsMoveTargetValid(_phaseHandler.TurnUnit, slot)){
            Debug.Log("[COMBAT]: Invalid Target Submitted " + slot.AssignedCombatant);
            _viewHandler.SetView(_phaseHandler, _combatantHandler);
            return; 
        }

        Debug.Log("[COMBAT]: Valid Target Submitted " + slot.AssignedCombatant);
        _combatantHandler.SubmitTarget(_phaseHandler.TurnUnit, slot);
    }

    public static CombatManager Instance { get; private set;}
    void Start(){
        if(Instance == null) Instance = this;
        else Destroy(this);

        Initialize();
    }
    void OnDestroy(){
        if(Instance == this) Instance = null;
    }
    
    private void Initialize(){
        _combatantHandler.Initialize();
        _moveProcessor.Initialize();
        _viewHandler.Initialize(_combatantHandler.GetAllSlots());
        _phaseHandler.Initialize(_combatantHandler, _moveProcessor, _viewHandler);
        _viewHandler.SetView(_phaseHandler, _combatantHandler);
    }
}
