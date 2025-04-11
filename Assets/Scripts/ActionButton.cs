using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text MoveNameText;
    public TMP_Text MoveCostText;
    [HideInInspector] public MoveData AssignedMove;
    private Button _button;
    
    public void OnPointerClick(PointerEventData eventData){
        if(_button != null){
            if(!_button.interactable) return;
        }
        CombatManager.Instance.ActionButtonClickCallback(this);
    }

    public void OnPointerEnter(PointerEventData eventData){
        CombatManager.Instance.ActionButtonHoverCallback(this, true);
    
    }

    public void OnPointerExit(PointerEventData eventData){
        CombatManager.Instance.ActionButtonHoverCallback(this, false);
    }

    void Start(){
        _button = GetComponent<Button>();
    }
}
