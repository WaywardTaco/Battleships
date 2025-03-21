using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ActionButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text MoveNameText;
    public TMP_Text MoveCostText;
    [HideInInspector] public MoveData AssignedMove;
    
    public void OnPointerClick(PointerEventData eventData){
        CombatManager.Instance.ActionButtonClickCallback(this);
    }

    public void OnPointerEnter(PointerEventData eventData){
        CombatManager.Instance.ActionButtonHoverCallback(this, true);
    
    }

    public void OnPointerExit(PointerEventData eventData){
        CombatManager.Instance.ActionButtonHoverCallback(this, false);
    }
}
