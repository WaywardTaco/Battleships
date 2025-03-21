using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatPanel : MonoBehaviour, IPointerClickHandler
{
    [Serializable] public class BattleInfoPanel {
        public GameObject gameObject;
        public TMP_Text RoundCounter;
    }
    [Serializable] public class MoveFeedbackPanel {
        public GameObject gameObject;
        public TMP_Text FeedbackText;
    }
    [Serializable] public class TurnPanel {
        public GameObject gameObject;
        public TMP_Text MoveDescriptionText;
        public List<ActionButton> ActionButtons = new();
    }
    [Serializable] public class WaitingEnemyPanel {
        public GameObject gameObject;
        public TMP_Text AwaitingText;
    }
    [Serializable] public class StatPanel {
        public GameObject gameObject;
        public TMP_Text HpText;
        public TMP_Text SpText;
    }

    public BattleInfoPanel InfoPanel;
    public StatPanel StatPopup;
    public TurnPanel PlayerTurnPanel;
    public MoveFeedbackPanel FeedbackPanel;
    public WaitingEnemyPanel AwaitingEnemyPanel;

    public void OnPointerClick(PointerEventData eventData){
        CombatManager.Instance.ClickOffCallback();
    }
}
