using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectionView
{
    public UnityEvent<List<SelectableCardView>> OnCardsSelectionStateChanged = new UnityEvent<List<SelectableCardView>>();

    private SelectAllButton selectAllButton;
    private List<SelectableCardView> cards = new List<SelectableCardView>();

    public void Initialize(SelectAllButton selectAllButton)
    {
        this.selectAllButton = selectAllButton;
        selectAllButton.button.onClick.AddListener(()=> 
        {
            SelectAll(!selectAllButton.IsOn);
        });
    }

    public void AddCard(SelectableCardView selectableCard) 
    {
        selectableCard.OnSelect.AddListener(OnCardSelectionStateChanged);
        cards.Add(selectableCard);
    }

    public void RemoveCard(SelectableCardView selectableCard)
    {
        selectableCard.OnSelect.RemoveAllListeners();
        cards.Remove(selectableCard);
    }

    public void RemoveAll()
    {
        while (cards.Count > 0)
        {
            RemoveCard(cards[0]);
        }
    }

    public List<T> GetSelected<T>() where T : SelectableCardView
    {
        List<T> result = new List<T>();

        foreach (var item in cards)
        {
            if (item.selected)
            {
                result.Add(item as T);
            }
        }

        return result;
    }

    public List<T> GetAllCards<T>() where T : SelectableCardView
    {
        List<T> result = new List<T>();

        foreach (var item in cards)
        {
            result.Add(item as T);
        }

        return result; 
    }

    public void EnableCardsSelection(bool value)
    {
        foreach (var item in cards)
        {
            item.EnableSelection(value);
        }
        selectAllButton.IsOn = false;
        selectAllButton.button.gameObject.SetActive(value);
    }

    public void SelectAll(bool value)
    {
        foreach (var item in cards)
        {
            item.Select(value);
        }
    }

    public void Select(SelectableCardView selectableCardView,bool value)
    {
        foreach (var item in cards)
        {
            if(selectableCardView == item)
                item.Select(value);
        }
    }

    private void OnCardSelectionStateChanged(SelectableCardView selectableCard)
    {
        OnCardsSelectionStateChanged?.Invoke(GetSelected<SelectableCardView>());
        bool IsOn = true;
        foreach (var item in cards)
        {
            if (!item.selected)
            {
                IsOn = false;
            }
        }
        selectAllButton.IsOn = IsOn;
    }

    [System.Serializable]
    public class SelectAllButton
    {
        public Image activeImage;
        public Image inactiveImage;
        public Button button;

        private bool isOn;

        public bool IsOn
        {
            get
            {
                return isOn;
            }
            set
            {
                activeImage.gameObject.SetActive(value);
                isOn = value;
            }
        }
    }
}
