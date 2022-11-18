using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class SelectableCardView : MonoBehaviour
{
    public UnityEvent<SelectableCardView> OnSelect = new(); 
    
    [SerializeField]
    private Button selectionButton;

    public bool selected { get; private set; }
    public bool selectionEnabled { get; private set; }

    public abstract void OnSelected();
    public abstract void OnSelectionEnabled(bool value);

    private void OnEnable()
    {
        selectionButton.onClick.AddListener(SelectionButtonClick);
    }

    private void OnDestroy()
    {
        selectionButton.onClick.RemoveAllListeners();
    }

    public void SelectionButtonClick()
    {
        Select(!selected);
    }

    public void Select(bool value)
    {
        selected = value;
        OnSelected();
        OnSelect.Invoke(this);
    }

    public void EnableSelection(bool value)
    {
        selectionButton.gameObject.SetActive(value);
        selectionEnabled = value;
        selected = false;
        OnSelectionEnabled(value);
    }
}
