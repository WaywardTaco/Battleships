using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MoveScriptable AssignedMove;

    [SerializeField] private TMP_Text _moveNameText;
    [SerializeField] private TMP_Text _movePPText;

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
        if(AssignedMove != null){
            _moveNameText.text = AssignedMove.MoveName;
        }
    }
}
