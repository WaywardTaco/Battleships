using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _descriptionText;

    public void ActionButtonCallback(ActionButton button){
        if(button.AssignedMove != null){
            // button.AssignedMove.UseMove();
        }
    }

    public void ActionButtonHover(ActionButton button, bool isHoveredOn){
        if(!isHoveredOn){
            _descriptionText.text = "";
            return;
        }
        
        if(button.AssignedMove != null){
            _descriptionText.text = button.AssignedMove.MoveDescription;
        }
    }

}
