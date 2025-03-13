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
    public String SlotTag;
    public Combatant AssignedCombatant;
    public Transform CameraPosition;
    [SerializeField] private Image _unitSprite;
    [SerializeField] private Transform _forwardLocation;
    [SerializeField] private Transform _rearLocation;
    private bool _hoveringOn = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        _hoveringOn = false;
        CombatManager.Instance.SubmitTarget(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoveringOn = true;
        CombatManager.Instance.ControlledCamTag = SlotTag;
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

        AssignedCombatant = combatant;

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
