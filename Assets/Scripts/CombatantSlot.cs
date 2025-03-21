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
    [SerializeField] private String _slotTag = "";
    [SerializeField] private Image _unitSprite;
    [SerializeField] private Transform _cameraPosition;
    [HideInInspector] public Combatant AssignedCombatant { get; private set; }

    public String GetSlotTag(){
        return _slotTag;
    }

    public Transform GetCamPosition(){
        return _cameraPosition;
    }

    public void UpdateCombatant(Combatant combatant = null){
        if(combatant != null)
            AssignedCombatant = combatant;

        if(combatant.IsAlly){
            _unitSprite.sprite = AssignedCombatant.GetSprite(1);

        } else {
            _unitSprite.sprite = AssignedCombatant.GetSprite(0);

        }
    }

    public void OnPointerClick(PointerEventData eventData){
        CombatManager.Instance.SlotClickCallback(this);
    }
    public void OnPointerEnter(PointerEventData eventData){
        CombatManager.Instance.SlotHoverCallback(this, true);
    }
    public void OnPointerExit(PointerEventData eventData){
        CombatManager.Instance.SlotHoverCallback(this, false);
    }
}
