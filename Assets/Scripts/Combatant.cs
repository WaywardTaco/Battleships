using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class Combatant {
    private const float ATK_STAGE_MULT = 0.5f;
    private const float SPA_STAGE_MULT = 0.5f;
    private const float DEF_STAGE_MULT = 0.5f;
    private const float SPD_STAGE_MULT = 0.5f;
    private const float SPE_STAGE_MULT = 0.25f;

    public string UnitTag = "";
    public int Level = 10;
    [HideInInspector] public string MoveTag = "";
    [HideInInspector] public string TargetSlotTag = "";
    public int CurrentHealth = 0;
    public int CurrentStamina = 0;
    [HideInInspector] public int ATKStage = 0;
    [HideInInspector] public int SPAStage = 0;
    [HideInInspector] public int DEFStage = 0;
    [HideInInspector] public int SPDStage = 0;
    [HideInInspector] public int SPEStage = 0;
    [HideInInspector] public bool IsAlly = false;
    [HideInInspector] public bool HasDied = false;
    [HideInInspector] public bool HasMoved = false;
    public UnitData Info {
        get { return DataLoader.Instance.GetUnitData(UnitTag); }
    }
    public MoveData Move {
        get { return DataLoader.Instance.GetMoveData(MoveTag); }
    }
    public bool HasMoveAndTarget {
        get {
            return MoveTag.CompareTo("") != 0 && TargetSlotTag.CompareTo("") != 0;
        }
    }
    public Sprite GetSprite(int index){
        return DataLoader.Instance.GetUnitSprite(Info.SpriteList[index]);
    }

    public void Initialize(){
        CurrentHealth = MaxHP;
        CurrentStamina = MaxSP;
        HasDied = false;
    }

    public int CurrentAttack(){
        return (int)(((float)Info.ATK_Base + (Info.ATK_Growth * (float)Level)) * (((float)ATKStage * ATK_STAGE_MULT) + 1.0f));
    }
    public int CurrentSpecialAttack(){
        return (int)(((float)Info.SPA_Base + (Info.SPA_Growth * (float)Level)) * (((float)SPAStage * SPA_STAGE_MULT) + 1.0f));
    }
    public int CurrentDefense(){
        return (int)(((float)Info.DEF_Base + (Info.DEF_Growth * (float)Level)) * (((float)DEFStage * DEF_STAGE_MULT) + 1.0f));
    }
    public int CurrentSpecialDefense(){
        return (int)(((float)Info.SPD_Base + (Info.SPD_Growth * (float)Level)) * (((float)SPDStage * SPD_STAGE_MULT) + 1.0f));
    }
    public int CurrentSpeed(){
        return (int)(((float)Info.SPE_Base + (Info.SPE_Growth * (float)Level)) * (((float)SPEStage * SPE_STAGE_MULT) + 1.0f));
    }

    public int MaxHP{
        get {
            return (int)((float)Info.HP_Base + (Info.HP_Growth * (float)Level));
        }
    }
    public int MaxSP{
        get {
            return (int)((float)Info.SP_Base + (Info.SP_Growth * (float)Level));
        }
    }

    public void DealDamage(int amount){
        if(amount < 0) amount = 0;

        if(CurrentHealth - amount > 0){
            CurrentHealth -= amount;
        } else {
            CurrentHealth = 0;
            HasDied = true;
        }
    }

    public void Heal(int amount, bool doesRevive = false){
        if(amount < 0) amount = 0;

        if(HasDied && !doesRevive) return;

        if(CurrentHealth + amount < MaxHP){
            CurrentHealth+= amount;
        } else {
            CurrentHealth = MaxHP;
        }

        if(CurrentHealth > 0 && doesRevive) 
            HasDied = false;
    }
}
