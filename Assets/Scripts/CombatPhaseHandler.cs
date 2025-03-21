using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CombatPhase {
    None, Combat_Start, Combat_End, 
    Round_Start, Waiting_Enemy_Submit, Round_Ending, Round_Ended,
    Turn_Start, Waiting_For_Player_Move, Turn_Ending, Turn_Ended,
    Processing_Moves, Process_Moves_Ending,
}
public class CombatPhaseHandler : MonoBehaviour
{
    private CombatPhase _currentPhase = CombatPhase.None;
    [SerializeField] private bool _debugStartCombat;
    [SerializeField] private bool _debugEndCombat;

    [HideInInspector] public uint RoundCount { get; private set; } = 0;
    private int _currentTurn = 0;
    private CombatantHandler _combatantHandler;
    private CombatViewHandler _viewHandler;
    private MoveProcessor _moveProcessor;
    
    public bool IsCombatActive { get { return _currentPhase != CombatPhase.None; }}
    private bool IsRoundPlaying {
        get {
            return 
                _currentPhase == CombatPhase.Round_Start || 
                IsTurnPlaying ||
                _currentPhase == CombatPhase.Waiting_Enemy_Submit ||
                _currentPhase == CombatPhase.Processing_Moves ||
                _currentPhase == CombatPhase.Process_Moves_Ending ||
                _currentPhase == CombatPhase.Round_Ending;
        }
    }
    private bool IsTurnPlaying { 
        get {
            return 
                _currentPhase == CombatPhase.Turn_Start ||
                _currentPhase == CombatPhase.Waiting_For_Player_Move ||
                _currentPhase == CombatPhase.Turn_Ending;
        }
    }
    public bool IsWaitingPlayerMove {
        get { return _currentPhase == CombatPhase.Waiting_For_Player_Move;}
    }

    public Combatant TurnUnit{ 
        get { return _combatantHandler.GetCombatant(_currentTurn, true); }
    }
    public CombatantSlot TurnUnitSlot{ 
        get { return _combatantHandler.GetCombatantSlot(TurnUnit); }
    }

    void Update()
    {
        if(_debugStartCombat){
            _debugStartCombat = false;
            CombatManager.Instance.StartCombat();
        }
    }

    public IEnumerator PlayCombat(){
        /* START OF COMBAT CODE */
        _currentPhase = CombatPhase.Combat_Start;
        _combatantHandler.InitializeForCombat();
        RoundCount = 1;

        CombatManager.Instance.UpdateView(_currentPhase);

        // Keep playing rounds until the combat should end
        while(!ShouldCombatEnd()){
            Debug.Log($"Current Combat Phase: {_currentPhase}");
            if(!IsRoundPlaying) StartCoroutine(PlayRound());
            yield return new WaitForEndOfFrame();
        }

        /* END OF COMBAT CODE */
        _currentPhase = CombatPhase.Combat_End;
        Debug.Log("[COMBAT]: Ending Combat");
        RoundCount = 0;
        CombatManager.Instance.UpdateView(_currentPhase);

        _currentPhase = CombatPhase.None;
        CombatManager.Instance.UpdateView(_currentPhase);
    }
    private IEnumerator PlayRound(){
        /* START OF ROUND CODE */
        _currentPhase = CombatPhase.Round_Start;
        _currentTurn = 0;

        CombatManager.Instance.UpdateView(_currentPhase);

        // Plays turns while player has turns to make
        while(!_combatantHandler.DidPlayerSubmitMoves){
            if(!IsTurnPlaying) StartCoroutine(PlayTurn());
            yield return new WaitForEndOfFrame();
        }

        // Shifts to waiting enemy to submit view if enemy hasn't submitted
        while(!_combatantHandler.DidEnemySubmitMoves){
            if(_currentPhase != CombatPhase.Waiting_Enemy_Submit){
                _currentPhase = CombatPhase.Waiting_Enemy_Submit;
                CombatManager.Instance.UpdateView(_currentPhase);
            }
            yield return new WaitForEndOfFrame();
        }
        
        /* MOVE PROCESSING PHASE CODE */
        _currentPhase = CombatPhase.Processing_Moves;
        _combatantHandler.UpdateMovingCombatants();
        CombatManager.Instance.UpdateView(_currentPhase);

        // Keep processing round moves as necessary
        while(_combatantHandler.MovingCombatant != null){
            if(!_moveProcessor.IsProcessingMove)
                _moveProcessor.Process(_combatantHandler.MovingCombatant, _combatantHandler, _viewHandler);
            yield return new WaitForEndOfFrame();
        }

        /* END PROCESSING CODE */
        _currentPhase = CombatPhase.Process_Moves_Ending;
        CombatManager.Instance.UpdateView(_currentPhase);

        /* END OF ROUND CODE */
        _currentPhase = CombatPhase.Round_Ending;

        _combatantHandler.ClearCombatantMoves();

        CombatManager.Instance.UpdateView(_currentPhase);
        RoundCount++;

        _currentPhase = CombatPhase.Round_Ended;
    }
    private IEnumerator PlayTurn(){
        /* START OF TURN CODE */
        _currentPhase = CombatPhase.Turn_Start;
        CombatManager.Instance.UpdateView(_currentPhase);

        // Keeps the player 'in the turn' while they are considered in it (ie. they have but have not selected a valid move)
        _currentPhase = CombatPhase.Waiting_For_Player_Move;
        CombatManager.Instance.UpdateView(_currentPhase);
        while(!(TurnUnit.HasDied || TurnUnit.HasMoveAndTarget)) 
            yield return new WaitForEndOfFrame();
        
        /* END OF TURN CODE */
        _currentPhase = CombatPhase.Turn_Ending;
        CombatManager.Instance.UpdateView(_currentPhase);

        _currentTurn++;

        _currentPhase = CombatPhase.Turn_Ended;
    }

    /***********************
    **  HELPER FUNCTIONS  **
    ************************/

    protected bool ShouldCombatEnd(){
        // Debug.Log("[TODO]: Combata automatic end detection still WIP");

        if(RoundCount == 0){
            Debug.LogWarning("[WARN]: Round count reached exactly 0, aborting combat");
            return false;
        }

        return _debugEndCombat;
    }
    
    public void Initialize(CombatantHandler combatantHandler, MoveProcessor moveProcessor, CombatViewHandler viewHandler){
        _combatantHandler = combatantHandler;
        _moveProcessor = moveProcessor;
        _viewHandler = viewHandler;
    }
}
