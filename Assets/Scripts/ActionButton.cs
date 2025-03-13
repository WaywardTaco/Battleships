using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public String AssignedMove = "";
    public int MoveCost = 0;

    [SerializeField] private TMP_Text _moveNameText;
    [SerializeField] private TMP_Text _moveCostText;

    public UnityEvent<ActionButton, bool> _HoverCallbackHandler;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _HoverCallbackHandler.Invoke(this, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _HoverCallbackHandler.Invoke(this, false);
    }

    void Update()
    {
        if(AssignedMove.CompareTo("") != 0){
            MoveData data = DataLoader.Instance.GetMoveData(AssignedMove);
            if(data == null){
                _moveNameText.text = "";
                _moveCostText.text = "";
                MoveCost = 0;
            }
            else{
                _moveNameText.text = data.MoveName;
                _moveCostText.text = $"{MoveCost}SP";
            }
        } else {
            _moveNameText.text = "";
            _moveCostText.text = "";
            MoveCost = 0;
        }
    }
}
