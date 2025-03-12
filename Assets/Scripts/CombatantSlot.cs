using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CombatantSlot : MonoBehaviour
{
    [SerializeField] private Image _unitSprite;
    [SerializeField] private Transform _forwardLocation;
    [SerializeField] private Transform _rearLocation;

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

        // if(combatant.isInRear){
        //     _unitSprite.parent
        // }
    }
}
