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
    [SerializeField] private CameraInfo Camera;
    private CombatPhase _currentPhase = CombatPhase.None;
    public CombatantSlot FocusingSlot { get; private set; }

    public void FocusSlot(CombatantSlot slot){
        
    }

    public void HoverSlot(CombatantSlot slot, bool isHoveredOn){

    }

    public void SetRoundInfo(int currentRound){
        _combatPanel.InfoPanel.RoundCounter.text = $"Round {currentRound}";
    }

    public void SetMoveFeedbackText(String text){
        _combatPanel.FeedbackPanel.FeedbackText.text = text;
    }   

    public void SetMovePanelInfo(Combatant unit){
        foreach(ActionButton moveButton in _combatPanel.PlayerTurnPanel.ActionButtons){
            moveButton.AssignedMove = unit.Move;
            moveButton.MoveNameText.text = unit.Move.MoveName;
            moveButton.MoveCostText.text = 
                $"{unit.Move.MoveCostBase + (int)((float)unit.Level * unit.Move.MoveCostGrowthRate)}SP";
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
    private void SetCamera(String camPosition, bool updateDefaultPosition, bool cameraControllable){
        MoveCameraTo(camPosition, true, updateDefaultPosition);
        SetCameraControl(cameraControllable);
    }
    private void MoveCameraTo(String camPosition, bool isPriority = false, bool updateDefaultPosition = false){
        if(Camera.CurrentPosition.CompareTo(camPosition) == 0)
            return;
        
        if(!updateDefaultPosition && !Camera.IsControllable) 
            return;
        
        if(updateDefaultPosition)
            Camera.DefaultPosition = camPosition;

        Camera.CurrentPosition = camPosition;
        Camera.Camera.MoveCamTo(Camera.SlotsDict[camPosition], isPriority);
    }
    private void SetCameraControl(bool value){
        Camera.IsControllable = value;
        if(!value) ResetCamView();
    }

    private void ResetCamView(bool isPriority = false){
        MoveCameraTo(Camera.DefaultPosition, isPriority);
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
                Camera.SlotsDict.Add(slot.GetSlotTag(), slot.GetCamPosition());
        }

        foreach(CameraSlot slot in Camera.CameraSlots){
            if(slot.Transform != null)
                Camera.SlotsDict.Add(slot.SlotTag, slot.Transform);
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
    }
    private void SetRoundEndView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        SetCamera("Overhead", true, false);
    }
    private void SetTurnStartView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        SetCamera(phaseHandler.TurnUnitSlot.GetSlotTag(), true, false);
        SetMovePanelInfo(phaseHandler.TurnUnit);
    }
    private void SetWaitingPlayerMoveView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){

    }
    private void SetTurnEndView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){

    }
    private void SetWaitingEnemySubmitView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        MoveCameraTo("Overhead", true, true);
        SetCameraControl(true);
    }
    private void SetProcessingMovesView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        
        MoveCameraTo("Overhead", true, true);
        SetCameraControl(true);
    }
    private void SetProcessingMovesEndView(CombatPhaseHandler phaseHandler, CombatantHandler combatantHandler){
        
        MoveCameraTo("Overhead", true, true);
        SetCameraControl(true);
    }
}
