using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatViewHandler : MonoBehaviour
{
    [Serializable] public class CameraSlot {
        public String SlotTag;
        public Transform Transform;
    }

    [Serializable] public class CameraInfo {
        public CameraMover Camera;
        public List<CameraSlot> CameraSlots = new();
        public Dictionary<String, Transform> SlotsDict = new(); 
        [HideInInspector] public String DefaultPosition = "";
        [HideInInspector] public String CurrentPosition = "";
        [HideInInspector] public bool IsControllable = false;
    }

    public Vector3 HIGHLIGHTED_SLOT_SIZE_SCALER;
    [SerializeField] private CombatPanel _combatPanel;
    [SerializeField] private CameraInfo _camera;
    private CombatPhase _currentPhase = CombatPhase.None;
    public CombatantSlot FocusingSlot { get; private set; }

    public String LookingAt { 
        get {
            return _camera.CurrentPosition;
        } 
    }

    public void FocusSlot(CombatantSlot slot){
        FocusingSlot = slot;
        SetCamera(slot.GetSlotTag(), false, true);
    }

    public void HoverSlot(CombatantSlot slot, bool isHoveredOn){
        _combatPanel.StatPopup.gameObject.SetActive(isHoveredOn);
        if(isHoveredOn){
            _combatPanel.StatPopup.HpText.text = $"{slot.AssignedCombatant.CurrentHealth}/{slot.AssignedCombatant.MaxHP}";
            _combatPanel.StatPopup.SpText.text = $"{slot.AssignedCombatant.CurrentStamina}/{slot.AssignedCombatant.MaxSP}";
        }
    }

    public void HoverAction(ActionButton action, bool isHoveredOn){
        if(!isHoveredOn){
            _combatPanel.PlayerTurnPanel.MoveDescriptionText.text = "";
        } else {
            _combatPanel.PlayerTurnPanel.MoveDescriptionText.text = action.AssignedMove.MoveDescription;
        }
    }

    public void SetRoundInfo(int currentRound){
        _combatPanel.InfoPanel.RoundCounter.text = $"Round {currentRound}";
    }

    public void SetMoveFeedbackText(String text){
        _combatPanel.FeedbackPanel.FeedbackText.text = text;
    }   

    public void SetMovePanelInfo(Combatant unit){
        if(unit == null) return;

        int buttonCount = _combatPanel.PlayerTurnPanel.ActionButtons.Count;
        int moveCount = unit.Info.MoveList.Count;
        for(int i = 0 ; i < buttonCount && i < moveCount; i++){
            ActionButton moveButton = _combatPanel.PlayerTurnPanel.ActionButtons[i];
            MoveData move = DataLoader.Instance.GetMoveData(unit.Info.MoveList[i]);

            if(move == null){
                Debug.LogWarning($"[WARN]: Trying to load bad move {unit.Info.MoveList[i]}");
                continue;
            }

            moveButton.AssignedMove = move;
            moveButton.MoveNameText.text = move.MoveName;
            moveButton.MoveCostText.text = 
                $"{move.MoveCostBase + (int)((float)unit.Level * move.MoveCostGrowthRate)}SP";
        }
    }

    public void UpdateLookingAtStats(Combatant combatant){
        if(combatant == null){
            _combatPanel.StatPopup.gameObject.SetActive(false);
        }

        _combatPanel.StatPopup.gameObject.SetActive(true);
        _combatPanel.StatPopup.HpText.text = $"HP {combatant.CurrentHealth}/{combatant.MaxHP}";
        _combatPanel.StatPopup.SpText.text = $"SP {combatant.CurrentStamina}/{combatant.MaxSP}";
    }

    private void TurnOffAllCombatPanels(){
        _combatPanel.InfoPanel.gameObject.SetActive(false);
        _combatPanel.StatPopup.gameObject.SetActive(false);
        _combatPanel.PlayerTurnPanel.gameObject.SetActive(false);
        _combatPanel.FeedbackPanel.gameObject.SetActive(false);
        _combatPanel.AwaitingEnemyPanel.gameObject.SetActive(false);
    }

    /* CAMERA CONTROL */
    public void SetCamera(String camPosition, bool updateDefaultPosition, bool cameraControllable){
        MoveCameraTo(camPosition, true, updateDefaultPosition);
        SetCameraControl(cameraControllable);
    }
    private void MoveCameraTo(String camPosition, bool isPriority = false, bool updateDefaultPosition = false){
        if(_camera.CurrentPosition.CompareTo(camPosition) == 0)
            return;
        
        if(!updateDefaultPosition && !_camera.IsControllable) 
            return;
        
        if(updateDefaultPosition)
            _camera.DefaultPosition = camPosition;

        _camera.CurrentPosition = camPosition;
        _camera.Camera.MoveCamTo(_camera.SlotsDict[camPosition], isPriority);
    }
    private void SetCameraControl(bool value){
        _camera.IsControllable = value;
        if(!value) ResetCamView();
    }

    public void ResetCamView(bool isPriority = false){
        MoveCameraTo(_camera.DefaultPosition, isPriority);
    }


    /**********************
    **  INITIALIZATIONS  **
    **********************/    

    public void Initialize(List<CombatantSlot> combatantSlots){
        _combatPanel.gameObject.SetActive(false);
        InitializeCameraSlots(combatantSlots);
    }

    private void InitializeCameraSlots(List<CombatantSlot> combatantSlots){
        foreach(CombatantSlot slot in combatantSlots){
            if(slot.GetCamPosition() != null)
                _camera.SlotsDict.Add(slot.GetSlotTag(), slot.GetCamPosition());
        }

        foreach(CameraSlot slot in _camera.CameraSlots){
            if(slot.Transform != null)
                _camera.SlotsDict.Add(slot.SlotTag, slot.Transform);
        }
    }

    public void SetView(CombatPhase currentPhase, CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        _currentPhase = currentPhase;
        SetView(phaseHandler, combatantHandler);   
    }

    public void SetView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        switch(_currentPhase){
            case CombatPhase.None:                      SetCombatOffView(phaseHandler, combatantHandler);             break;
            case CombatPhase.Combat_Start:              SetCombatStartView(phaseHandler, combatantHandler);           break;
            case CombatPhase.Round_Start:               SetRoundStartView(phaseHandler, combatantHandler);            break;
            case CombatPhase.Turn_Start:                SetTurnStartView(phaseHandler, combatantHandler);             break;
            case CombatPhase.Waiting_For_Player_Move:   SetWaitingPlayerMoveView(phaseHandler, combatantHandler);     break;
            case CombatPhase.Waiting_Player_Target:     SetWaitingPlayerTarget(phaseHandler, combatantHandler);       break;
            case CombatPhase.Turn_Ending:               SetTurnEndView(phaseHandler, combatantHandler);               break;
            case CombatPhase.Waiting_Enemy_Submit:      SetWaitingEnemySubmitView(phaseHandler, combatantHandler);    break;
            case CombatPhase.Processing_Moves:          SetProcessingMovesView(phaseHandler, combatantHandler);       break;
            case CombatPhase.Process_Moves_Ending:      SetProcessingMovesEndView(phaseHandler, combatantHandler);    break;
            case CombatPhase.Round_Ending:              SetRoundEndView(phaseHandler, combatantHandler);              break;
            case CombatPhase.Combat_End:                SetCombatEndView(phaseHandler, combatantHandler);             break;
        }
    }

    /********************
    **  VIEW SETTINGS  **
    ********************/
    private void SetCombatOffView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        _combatPanel.gameObject.SetActive(false);
    }
    private void SetCombatStartView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        SetCamera("Overhead", true, false);
        _combatPanel.gameObject.SetActive(true);
        TurnOffAllCombatPanels();
    }
    private void SetCombatEndView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        SetCamera("Overhead", true, false);
        TurnOffAllCombatPanels();
    }
    private void SetRoundStartView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        SetCamera("Overhead", true, false);
        SetRoundInfo((int) phaseHandler.RoundCount);
        _combatPanel.InfoPanel.gameObject.SetActive(true);
    }
    private void SetRoundEndView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        SetCamera("Overhead", true, false);
    }
    private void SetTurnStartView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        SetCamera("Overhead", true, false);
        SetMovePanelInfo(phaseHandler.TurnUnit);
        _combatPanel.AwaitingEnemyPanel.gameObject.SetActive(false);
        _combatPanel.PlayerTurnPanel.gameObject.SetActive(true);
    }
    private void SetWaitingPlayerMoveView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        SetCamera(phaseHandler.TurnUnitSlot.GetSlotTag(), true, false);
    }
    private void SetWaitingPlayerTarget(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        SetCamera("Overhead", true, true);
    }
    private void SetTurnEndView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        _combatPanel.PlayerTurnPanel.gameObject.SetActive(false);
    }
    private void SetWaitingEnemySubmitView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        SetCamera("Overhead", true, true);
        _combatPanel.AwaitingEnemyPanel.gameObject.SetActive(true);
    }
    private void SetProcessingMovesView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        SetCamera("Overhead", true, true);
        _combatPanel.AwaitingEnemyPanel.gameObject.SetActive(false);
        _combatPanel.FeedbackPanel.gameObject.SetActive(true);
    }
    private void SetProcessingMovesEndView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        SetCamera("Overhead", true, true);
        _combatPanel.FeedbackPanel.gameObject.SetActive(false);
    }
}
