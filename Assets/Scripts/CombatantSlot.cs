using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatantSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private const float DMG_LINGERTIME = 0.3f;
    [SerializeField] private GameObject _dmgPanel;
    [SerializeField] private TMP_Text _dmgText;
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

    public void DealDamage(int amount){
        _dmgText.text = $"{-amount}";
        _dmgPanel.SetActive(true);
        StartCoroutine(DelayedDmgDisappear());
    }

    private IEnumerator DelayedDmgDisappear(){
        yield return new WaitForSeconds(DMG_LINGERTIME);

        _dmgPanel.SetActive(false);
        _dmgText.text = "-DMG";
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

    void Start()
    {
        _dmgPanel.SetActive(false);
    }
}
