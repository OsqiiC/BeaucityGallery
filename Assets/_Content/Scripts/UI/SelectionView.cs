using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectionView
{
    public UnityEvent<bool> OnSelectionEnabled = new UnityEvent<bool>();
    public UnityEvent<SelectableCardView> OnCardSelected = new UnityEvent<SelectableCardView>();

    private SelectAllButton selectAllButton;

    private List<SelectableCardView> cards = new List<SelectableCardView>();
    private UnityEvent<SelectableCardView> OnCardAdded = new();

    public SelectionView(SelectAllButton selectAllButton, List<SelectableCardView> cards)
    {
        this.selectAllButton = selectAllButton;
        this.cards = cards;

        selectAllButton.Initialize(this);

        selectAllButton.button.onClick.AddListener(() =>
        {
            SelectAll(!selectAllButton.IsOn);
        });

        OnCardAdded.AddListener((card) =>
        {
            card.OnSelect.AddListener((card)=>OnCardSelected?.Invoke(card));
        });
    }

    public void AddCard(SelectableCardView selectableCard) 
    {
        cards.Add(selectableCard);
        OnCardAdded?.Invoke(selectableCard);
        selectableCard.OnSelect.AddListener((card)=> 
        {
            OnCardSelected?.Invoke(card);
        });
    }

    public void RemoveCard(SelectableCardView selectableCard)
    {
        cards.Remove(selectableCard);
    }

    public List<SelectableCardView> GetSelected() 
    {
        List<SelectableCardView> result = new();

        foreach (var item in cards)
        {
            if (item.selected)
            {
                result.Add(item);
            }
        }

        return result;
    }


    public List<SelectableCardView> GetAllCards() 
    {
        List<SelectableCardView> result = new();

        foreach (var item in cards)
        {
            result.Add(item);
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
        OnSelectionEnabled.Invoke(value);
    }

    public void SelectAll(bool value)
    {
        foreach (var item in cards)
        {
            item.Select(value);
        }
    }

    [System.Serializable]
    public class SelectAllButton
    {
        public Image activeImage;
        public Image inactiveImage;
        public Button button;

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

        private bool isOn;
        private SelectionView selectionView;

        public void Initialize(SelectionView selectionView)
        {
            this.selectionView = selectionView;
            selectionView.OnCardAdded.AddListener((card)=> 
            {
                card.OnSelect.AddListener(CheckAllSelected); 
            });
        }

        private void CheckAllSelected(SelectableCardView selectableCardView)
        {
            IsOn = true;
            foreach (var item in selectionView.cards)
            {
                if (!item.selected)
                {
                    IsOn = false;
                }
            }
        }
    }
}
