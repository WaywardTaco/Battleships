using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatantHandler : MonoBehaviour
{
    [SerializeField] protected bool _debugEnemySubmittedMoves;
    [SerializeField] private List<Combatant> _playerTeam = new();
    [SerializeField] private List<Combatant> _enemyTeam = new();
    private List<Combatant> _combatantMoveOrder = new();
    
    [SerializeField] private List<CombatantSlot> _playerSlots = new();
    [SerializeField] private List<CombatantSlot> _enemySlots = new();
    private Dictionary<String, CombatantSlot> _slotDict = new();
    private int _playerSubmittedMovesCount = 0;
    private int _enemySubmittedMovesCount = 0;
    
    public bool DidPlayerSubmitMoves {
        get {
            int aliveUnitCount = 0;
            foreach(Combatant unit in _playerTeam)
                if(!unit.HasDied) aliveUnitCount++;
            
            return _playerSubmittedMovesCount >= aliveUnitCount;
        }
    }

    public bool DidEnemySubmitMoves {
        get {
            if(_debugEnemySubmittedMoves){
                _debugEnemySubmittedMoves = false;
                return _debugEnemySubmittedMoves;
            }

            int aliveUnitCount = 0;
            foreach(Combatant unit in _enemyTeam)
                if(!unit.HasDied) aliveUnitCount++;
            
            return _enemySubmittedMovesCount >= aliveUnitCount;
        }
    }

    public void SubmitTeam(Dictionary<String, int> teamUnits, bool isAlly){
        if(isAlly)  _playerTeam.Clear();
        else        _enemyTeam.Clear();

        foreach(var unit in teamUnits){
            Combatant newUnit = new Combatant();
            newUnit.UnitTag = unit.Key;
            newUnit.Level = unit.Value;
            newUnit.IsAlly = isAlly;

            if(isAlly) _playerTeam.Add(newUnit);
            else _enemyTeam.Add(newUnit);
        }
    }

    public void SubmitMove(Combatant unit, MoveData move){
        unit.MoveTag = move.MoveTag;
        if(unit.IsAlly) _playerSubmittedMovesCount++;
        else            _enemySubmittedMovesCount++;
    }

    /// <summary>
    /// Returns the next combatant that should move, returns null if none exist
    /// </summary>
    public Combatant MovingCombatant{
        get {
            foreach(Combatant unit in _combatantMoveOrder){
                // returns the first unit that is still alive, hasn't moved and has a move submitted
                if(!unit.HasDied && !unit.HasMoved && unit.MoveTag.CompareTo("") != 0)
                    return unit;
            }

            return null;
        }
    }

    public void UpdateMovingCombatants(){
        // Sorts moving combatants by move priority, then faster speed, tied speeds result in a random order
        _combatantMoveOrder.Sort(
            delegate(Combatant unitA, Combatant unitB){
                if(unitB.Move == null) return -1;

                if(unitB.Move.MovePriority == unitA.Move.MovePriority){
                    if(unitB.CurrentSpeed() == unitA.CurrentSpeed()){
                        int random = UnityEngine.Random.Range(0,1);
                        if(random == 0) return -1;
                        else return 1;
                    }

                    return unitB.CurrentSpeed() - unitA.CurrentSpeed();
                }

                return unitB.Move.MovePriority - unitA.Move.MovePriority;
            }
        );
    }

    public void ClearCombatantMoves(){
        foreach(Combatant unit in _playerTeam){
            unit.MoveTag = "";
            unit.TargetSlotTag = "";
            unit.HasMoved = false;
        }
        foreach(Combatant unit in _enemyTeam){
            unit.MoveTag = "";
            unit.TargetSlotTag = "";
            unit.HasMoved = false;
        }

        _playerSubmittedMovesCount = 0;
        _enemySubmittedMovesCount = 0;
    }

    /************************
    **  HELPER FUNCTIONS  **
    ************************/

    public CombatantSlot GetCombatantSlot(Combatant combatant){
        foreach(var slot in _slotDict){
            if(slot.Value.AssignedCombatant == combatant)
                return slot.Value;
        }

        return null;
    }
    public CombatantSlot GetCombatantSlot(String slotTag){
        if(!_slotDict.ContainsKey(slotTag)) return null;
        return _slotDict[slotTag];
    }
    public Combatant GetCombatant(int index, bool fromPlayer = true){
        if(fromPlayer){
            if(index >= _playerSlots.Count) return null;
            return _playerSlots[index].AssignedCombatant;
        } else {
            if(index >= _enemySlots.Count) return null;
            return _enemySlots[index].AssignedCombatant;
        }
    }

    public List<CombatantSlot> GetAllSlots(){
        List<CombatantSlot> allSlots = new();
        foreach(CombatantSlot slot in _playerSlots)
            allSlots.Add(slot);
        foreach(CombatantSlot slot in _enemySlots)
            allSlots.Add(slot);

        if(allSlots.Count == 0)
            return null;

        return allSlots;
    }
    
    public void Initialize(){
        InitializeSlotDict();
    }

    public void InitializeForCombat(){
        InitializeCombatants();
    }

    private void InitializeSlotDict(){
        _slotDict.Clear();

        foreach(CombatantSlot slot in _playerSlots)
            _slotDict.Add(slot.GetSlotTag(), slot);
        foreach(CombatantSlot slot in _enemySlots)
            _slotDict.Add(slot.GetSlotTag(), slot);
    }

    private void InitializeCombatants(){
        _combatantMoveOrder.Clear();

        foreach(Combatant unit in _playerTeam)
            _combatantMoveOrder.Add(unit);
        foreach(Combatant unit in _enemyTeam)
            _combatantMoveOrder.Add(unit);

        int slotCount = _playerSlots.Count;
        for(int i = 0; i < slotCount && i < _playerTeam.Count; i++){
            _playerTeam[i].IsAlly = true;
            _playerTeam[i].Initialize();
            _playerSlots[i].UpdateCombatant(_playerTeam[i]);
        }
        slotCount = _enemySlots.Count;
        for(int i = 0; i < slotCount && i < _enemyTeam.Count; i++){
            _enemyTeam[i].IsAlly = false;
            _enemyTeam[i].Initialize();
            _enemySlots[i].UpdateCombatant(_enemyTeam[i]);
        }
    }
}
