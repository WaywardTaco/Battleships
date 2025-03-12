using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class CombatPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private List<ActionButton> _actionButtons = new();

    public void ActionButtonCallback(ActionButton button){
        if(button.AssignedMove.CompareTo("") != 0){
            CombatManager.Instance.SubmitMove(button.AssignedMove);
        }
    }

    public void LoadButtonInfo(List<String> moveTags){
        foreach(ActionButton button in _actionButtons){
            button.AssignedMove = "";
        }

        for(int i = 0; i < moveTags.Count && i < _actionButtons.Count; i++){
            _actionButtons[i].AssignedMove = moveTags[i];
        }
    }   

    public void ActionButtonHover(ActionButton button, bool isHoveredOn){
        if(!isHoveredOn){
            _descriptionText.text = "";
            return;
        }
        
        if(button.AssignedMove.CompareTo("") != 0){
            MoveData data = DataLoader.Instance.GetMoveData(button.AssignedMove);
            if(data == null)
                _descriptionText.text = "";
            else
                _descriptionText.text = data.MoveDescription;
        }
    }

}
