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
    [SerializeField] private Transform _cameraFocusPosition;
    [SerializeField] private Transform _forwardLocation;
    [SerializeField] private Transform _rearLocation;
    private bool _hoveringOn = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked");
        _hoveringOn = false;
        CombatManager.Instance.SubmitTarget(_assignedCombatant);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("HoverOn");
        // if(!_hoveringOn)
        _hoveringOn = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("HoverOff");
        _hoveringOn = false;
        CombatManager.Instance.MoveCamToDefault();
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
        if(_hoveringOn){
            CombatManager.Instance.MoveCamTo(_cameraFocusPosition, false);
        }
    }
}
