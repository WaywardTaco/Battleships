using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CombatantSlot : MonoBehaviour
{
    [SerializeField] private Sprite _unitSprite;
    [SerializeField] private Transform _forwardLocation;
    [SerializeField] private Transform _rearLocation;

    public void UpdateCombatant(Combatant combatant, bool isAlly = false){
        if(!isAlly){
            _unitSprite = DataLoader.Instance.GetUnitSprite(
                DataLoader.Instance.GetUnitData(combatant.UnitTag).SpriteList[0]
            );
        } else {
            _unitSprite = DataLoader.Instance.GetUnitSprite(
                DataLoader.Instance.GetUnitData(combatant.UnitTag).SpriteList[1]
            );
        }

        // if(combatant.isInRear){
        //     _unitSprite.parent
        // }
    }
}
