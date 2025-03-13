using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatantSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image _unitSprite;
    [SerializeField] private Combatant _assignedCombatant;
    
    [SerializeField] private String _camSlotTag;
    [SerializeField] private Transform _cameraFocusPosition;
    [SerializeField] private Transform _forwardLocation;
    [SerializeField] private Transform _rearLocation;
    private bool _hoveringOn = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        _hoveringOn = false;
        CombatManager.Instance.SubmitTarget(_assignedCombatant);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoveringOn = true;
        CombatManager.Instance.ControlledCamTag = _camSlotTag;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hoveringOn = false;
        // CombatManager.Instance.ControlledCamTag = "";
        // CombatManager.Instance.MoveCamToDefault();
    }

    public void UpdateCombatant(Combatant combatant, bool isAlly = false){
        if(!isAlly){
            _unitSprite.sprite = DataLoader.Instance.GetUnitSprite(
                combatant.Data.SpriteList[0]
            );
        } else {
            _unitSprite.sprite = DataLoader.Instance.GetUnitSprite(
                combatant.Data.SpriteList[1]
            );
        }

        _assignedCombatant = combatant;

        // if(combatant.isInRear){
        //     _unitSprite.parent
        // }
    }

    void Update()
    {
        // if(_hoveringOn){
        // }
    }
}
