using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private uint _roundCount = 0;
    [SerializeField] private uint _turnCount = 0;
    [SerializeField] private int _processingUnitIndex = -1;
    [SerializeField] private CameraMover _combatCamera;
    [SerializeField] private GameObject _combatPanel;

    private bool
        _turnPlaying = false,
        _roundPlaying = false,
        _processingRoundMoves = false,
        _processingSingleMove = false;

    [SerializeField] private bool _debugStartCombat;
    [SerializeField] private bool _debugEndCombat;
    [SerializeField] private bool _debugPlayerSubmitMove;
    [SerializeField] private bool _debugEnemySubmittedMoves;
    [SerializeField] private bool _debugEndProcessMove;
    [SerializeField] private bool _debugTransitionCam;
    [SerializeField] private String _debugTransitionSlotTag;
    [SerializeField] private String _debugSubmittedMoveTag;
    [SerializeField] private List<CombatantSlot> _playerSlots = new();
    [SerializeField] private List<CombatantSlot> _enemySlots = new();
    [SerializeField] private List<Combatant> _playerTeam = new();
    [SerializeField] private List<Combatant> _enemyTeam = new();
    [SerializeField] private List<Combatant> _movingCombatants = new();

    public void StartCombat(){
        if(IsCombatActive()){
            Debug.LogWarning("[WARN]: Trying to start combat while combat is running");
            return;
        }

        _combatPanel.SetActive(true);
        _roundCount = 1;
        UpdateSlots();
        StartCoroutine(PlayCombat());
    }
    public void SubmitMove(String MoveTag){
        if(MoveTag.CompareTo("") == 0){
            Debug.LogWarning("[WARN]: Tried to submit null move");
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
    public bool IsCombatActive(){
        return _roundCount != 0;
    }
    
    private IEnumerator PlayCombat(){
        Debug.Log("[COMBAT]: Combat Started");

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
            if(!_turnPlaying && !PlayerSubmittedMoves()) 
                StartCoroutine(PlayTurn());
            else if(PlayerSubmittedMoves()){
                _combatCamera.MoveCameraToSlot("Overhead");
            }
            yield return new WaitForEndOfFrame();
        }
        
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

        _combatCamera.MoveCameraToSlot(
            "P" + (_turnCount + 1)
        );

        while(!ShouldTurnEnd()){
            yield return new WaitForEndOfFrame();
        }

        EndTurn();
    }
    private IEnumerator ProcessMoves(){
        _processingRoundMoves = true;
        _movingCombatants.Clear();

        foreach(Combatant unit in _playerTeam){
            if(!unit.HasDied && unit.SubmittedMoveTag.CompareTo("") != 0) 
                _movingCombatants.Add(unit);
        }
        foreach(Combatant unit in _enemyTeam){
            if(!unit.HasDied && unit.SubmittedMoveTag.CompareTo("") != 0) 
                _movingCombatants.Add(unit);
        }

        _movingCombatants.Sort(delegate(Combatant unitA, Combatant unitB){
            if(unitB.CurrentSpeed() == unitA.CurrentSpeed()){
                int random = UnityEngine.Random.Range(0,1);
                if(random == 0) return -1;
                else return 1;
            }

            return unitB.CurrentSpeed() - unitA.CurrentSpeed();
        });

        _processingUnitIndex = 0;
        while(_processingUnitIndex < _movingCombatants.Count){

            if(!_processingSingleMove){
                Debug.Log($"[COMBAT]: ({_processingUnitIndex}) {_movingCombatants[_processingUnitIndex].UnitTag} (Speed: {_movingCombatants[_processingUnitIndex].CurrentSpeed()})");
                StartCoroutine(ProcessMove(_movingCombatants[_processingUnitIndex]));
            }

            yield return new WaitForEndOfFrame();
        }
        
        _processingRoundMoves = false;
    }
    private IEnumerator ProcessMove(Combatant combatant){
        Debug.Log($"[COMBAT]: Processing {combatant.UnitTag}'s {combatant.SubmittedMoveTag}");
        _processingSingleMove = true;

        while(!_debugEndProcessMove){
            yield return new WaitForEndOfFrame();
        }

        _debugEndProcessMove = false;

        _processingUnitIndex++;
        _processingSingleMove = false;
    }

    private void EndTurn(){
        Debug.Log($"[COMBAT]: Ending Turn {_turnCount}");
        
        _turnPlaying = false;

        _turnCount++;
    }
    private void EndRound(){
        Debug.Log($"[COMBAT]: Ending Round {_roundCount}");
        _debugEnemySubmittedMoves = false;

        _roundPlaying = false;
        
        foreach(Combatant unit in _playerTeam)
            unit.SubmittedMoveTag = "";
        foreach(Combatant unit in _enemyTeam)
            unit.SubmittedMoveTag = "";

        _roundCount++;
    }
    private void EndCombat(){
        Debug.Log("[COMBAT]: Ending Combat");

        _roundCount = 0;
    }

    private bool ShouldTurnEnd(){
        // Returns that the turn should end if the unit is either dead or the unit has a submitted move
        return 
            TurnUnit().HasDied || 
            TurnUnit().SubmittedMoveTag.CompareTo("") != 0
        ;
    }
    private bool ShouldRoundEnd(){
        return 
            _debugEnemySubmittedMoves && 
            PlayerSubmittedMoves();
    }
    private bool ShouldCombatEnd(){
        if(_roundCount == 0){
            Debug.LogWarning("[WARN]: Round count reached exactly 0, aborting combat");
            return false;
        }

        return _debugEndCombat;
    }
    private bool PlayerSubmittedMoves(){
        return _turnCount >= _playerTeam.Count;
    }

    private Combatant TurnUnit(){
        return _playerTeam[(int)_turnCount];
    }

    private void UpdateSlots(){
        int slotCount = _playerSlots.Count;
        for(int i = 0; i < slotCount && i < _playerTeam.Count; i++){
            _playerSlots[i].UpdateCombatant(_playerTeam[i]);
        }
        slotCount = _enemySlots.Count;
        for(int i = 0; i < slotCount && i < _enemyTeam.Count; i++){
            _enemySlots[i].UpdateCombatant(_enemyTeam[i]);
        }
    }

    private void Setup(){
        _combatPanel.SetActive(false);
    }

    private void DebugUpdate(){
        if(_debugStartCombat){
            _debugStartCombat = false;
            StartCombat();
        }
        if(_debugTransitionCam){
            _debugTransitionCam = false;
            _combatCamera.MoveCameraToSlot(_debugTransitionSlotTag);
        }
        if(_debugPlayerSubmitMove){
            _debugPlayerSubmitMove = false;
            SubmitMove(_debugSubmittedMoveTag);
        }
    }

    void Update()
    {
        DebugUpdate();
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
}
