using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatStateHandler : MonoBehaviour
{
    
    [Serializable] public class BattlefieldCameraSlot {
        public String SlotTag;
        public Transform CamSlot;
    }
    
    [SerializeField] private float _cameraReturnTime = 0;
    [SerializeField] protected uint _roundCount = 0;
    [SerializeField] protected uint _turnCount = 0;
    [SerializeField] protected int _processingUnitIndex = -1;
    [SerializeField] protected CameraMover _combatCamera;
    [SerializeField] protected CombatPanel _combatPanel;

    protected bool
        _turnPlaying = false,
        _roundPlaying = false,
        _processingRoundMoves = false,
        _cameraControllable = false;
    protected String _defaultCamPosition = "";
    public String ControlledCamTag = "";

    [SerializeField] protected bool _debugStartCombat;
    [SerializeField] protected bool _debugEndCombat;
    [SerializeField] protected bool _debugEnemySubmittedMoves;
    [SerializeField] private List<BattlefieldCameraSlot> _cameraSlots = new();
    [SerializeField] protected List<CombatantSlot> _playerSlots = new();
    [SerializeField] protected List<CombatantSlot> _enemySlots = new();
    [SerializeField] protected List<Combatant> _playerTeam = new();
    [SerializeField] protected List<Combatant> _enemyTeam = new();
    [SerializeField] protected List<Combatant> _movingCombatants = new();
    protected Dictionary<String, BattlefieldCameraSlot> _cameraSlotDict = new();

    public void SelectMove(String MoveTag){
        if(MoveTag.CompareTo("") == 0){
            Debug.Log("[COMBAT]: No Move Selected");
            return;
        }

        if(!IsCombatActive()){
            Debug.LogWarning("[WARN]: Trying to submit move with no combat active");
            return;
        }

        if(!_turnPlaying){
            Debug.LogWarning("[WARN]: Trying to submit move with no active turn");
            return;
        }

        TurnUnit().SubmittedMoveTag = MoveTag;
    }

    public void SubmitTarget(Combatant combatant){
        if(!IsAwaitingTarget()) return;
        if(!MoveProcessor.Instance.HasValidTarget(TurnUnit())) return; 

        Debug.Log("[COMBAT]: Valid Target Submitted " + combatant);
        TurnUnit().SubmittedTarget = combatant.UnitTag;
    }

    public bool IsCombatActive(){
        return _roundCount != 0;
    }

    protected bool DidPlayerSubmitMoves(){
        return _turnCount >= _playerTeam.Count;
    }
    protected bool DidEnemySubmitMoves(){
        if(_debugEnemySubmittedMoves) return true;

        foreach(Combatant unit in _enemyTeam){
            if(unit.HasDied) continue;
            if(
                unit.SubmittedMoveTag.CompareTo("") == 0 ||
                unit.SubmittedTarget.CompareTo("") == 0
            ) return false;
        }

        return true;
    }
    protected bool IsAwaitingTarget(){
        // Says that a target is being waited for if the unit is alive, has a move, but no target
        return 
            !TurnUnit().HasDied &&
            (
                TurnUnit().SubmittedMoveTag.CompareTo("") != 0 &&
                TurnUnit().SubmittedTarget.CompareTo("") == 0
            )
        ;
    }
    protected bool ShouldTurnEnd(){
        // Returns that the turn should end if the unit is either dead or the unit has a submitted move and target
        return 
            TurnUnit().HasDied || 
            (
                TurnUnit().SubmittedMoveTag.CompareTo("") != 0 &&
                TurnUnit().SubmittedTarget.CompareTo("") != 0
            )
        ;
    }
    protected bool ShouldRoundEnd(){
        return 
            DidEnemySubmitMoves() && 
            DidPlayerSubmitMoves();;
    }
    protected bool ShouldCombatEnd(){
        if(_roundCount == 0){
            Debug.LogWarning("[WARN]: Round count reached exactly 0, aborting combat");
            return false;
        }

        return _debugEndCombat;
    }
    protected Combatant TurnUnit(){
        if(_turnCount >= _playerTeam.Count) return null;

        return _playerTeam[(int)_turnCount];
    }

    protected void UpdateSlots(){
        int slotCount = _playerSlots.Count;
        for(int i = 0; i < slotCount && i < _playerTeam.Count; i++){
            _playerSlots[i].UpdateCombatant(_playerTeam[i]);
        }
        slotCount = _enemySlots.Count;
        for(int i = 0; i < slotCount && i < _enemyTeam.Count; i++){
            _enemySlots[i].UpdateCombatant(_enemyTeam[i]);
        }
    }
    protected void UpdateMovingCombatants(){
        _movingCombatants.Clear();

        foreach(Combatant unit in _playerTeam){
            if(!unit.HasDied && unit.SubmittedMoveTag.CompareTo("") != 0) 
                _movingCombatants.Add(unit);
        }
        foreach(Combatant unit in _enemyTeam){
            if(!unit.HasDied && unit.SubmittedMoveTag.CompareTo("") != 0) 
                _movingCombatants.Add(unit);
        }

        // Sorts moving combatants by faster speed, tied speeds result in a random order
        _movingCombatants.Sort(
            delegate(Combatant unitA, Combatant unitB){
                if(unitB.CurrentSpeed() == unitA.CurrentSpeed()){
                    int random = UnityEngine.Random.Range(0,1);
                    if(random == 0) return -1;
                    else return 1;
                }

                return unitB.CurrentSpeed() - unitA.CurrentSpeed();
            }
        );
    }

    protected void InitializeCameraSlotDict(){
        _cameraSlotDict.Clear();

        foreach(BattlefieldCameraSlot slot in _cameraSlots){
            _cameraSlotDict.Add(slot.SlotTag, slot);
        }
    }

}
