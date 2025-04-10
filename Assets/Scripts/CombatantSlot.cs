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
    private const float DMG_LINGERTIME = 0.5f;
    private const float DIE_ANIMTIME = 0.5f;
    private const float DIE_ROT_DISPLACEMENT = 90.0f;
    private Quaternion _initialRotation;
    [SerializeField] private GameObject _dmgPanel;
    [SerializeField] private TMP_Text _dmgText;
    [SerializeField] private GameObject _healPanel;
    [SerializeField] private TMP_Text _healText;
    [SerializeField] private GameObject _staminaPanel;
    [SerializeField] private TMP_Text _staminaText;
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

    public void AffectStat(string stat, int amount){
        GameObject panel;
        TMP_Text text;
        if(amount >= 0){
            panel = _healPanel;
            text = _healText;
            text.text = $"+{amount} {stat}";
            panel.SetActive(true);
            StartCoroutine(DelayedHealingDisappear());
        } else {
            panel = _dmgPanel;
            text = _dmgText;
            text.text = $"{amount} {stat}";
            panel.SetActive(true);
            StartCoroutine(DelayedDmgDisappear());
        }    
    }

    public void DealDamage(int amount){
        if(amount < 0)
            _dmgText.text = $"{amount}";
        else
            _dmgText.text = $"-{amount}";
        _dmgPanel.SetActive(true);
        StartCoroutine(DelayedDmgDisappear());
    }

    public void DieAnim(){
        StartCoroutine(DieAnimAsync());
    }

    private IEnumerator DieAnimAsync(){
        Quaternion aliveRot = _initialRotation;
        Quaternion dieRot = Quaternion.Euler(aliveRot.eulerAngles.x + DIE_ROT_DISPLACEMENT, aliveRot.eulerAngles.y, aliveRot.eulerAngles.z);

        float elapsedTime = 0.0f;
        while(elapsedTime < DIE_ANIMTIME){
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(aliveRot, dieRot, elapsedTime / DIE_ANIMTIME);
        }

        transform.rotation = dieRot;
    }

    public void ReviveAnim(){
        StartCoroutine(ReviveAnimAsync());
    }
    
    public IEnumerator ReviveAnimAsync(){
        Quaternion aliveRot = _initialRotation;
        Quaternion dieRot = Quaternion.Euler(aliveRot.eulerAngles.x + DIE_ROT_DISPLACEMENT, aliveRot.eulerAngles.y, aliveRot.eulerAngles.z);

        float elapsedTime = 0.0f;
        while(elapsedTime < DIE_ANIMTIME){
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(dieRot, aliveRot, elapsedTime / DIE_ANIMTIME);
        }

        transform.rotation = aliveRot;
    }

    public void Heal(int amount){
        _healText.text = $"+{amount}";
        _healPanel.SetActive(true);
        StartCoroutine(DelayedHealingDisappear());
    }

    public void AffectStamina(int amount){
        if(amount >= 0)
            _staminaText.text = $"+{amount}";
        else
            _staminaText.text = $"{amount}";
        _staminaPanel.SetActive(true);
        StartCoroutine(DelayedStaminaDisappear());

    }

    private IEnumerator DelayedDmgDisappear(){
        yield return new WaitForSeconds(DMG_LINGERTIME);

        _dmgPanel.SetActive(false);
        _dmgText.text = "-DMG";
    }
    private IEnumerator DelayedHealingDisappear(){
        yield return new WaitForSeconds(DMG_LINGERTIME);

        _healPanel.SetActive(false);
        _healText.text = "+HP";
    }
    private IEnumerator DelayedStaminaDisappear(){
        yield return new WaitForSeconds(DMG_LINGERTIME);

        _staminaPanel.SetActive(false);
        _staminaText.text = "+SP";
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
        _healPanel.SetActive(false);
        _staminaPanel.SetActive(false);
        _initialRotation = transform.rotation;
    }
}
