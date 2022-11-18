using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Zenject;

public class GalleryView : View
{
    public SelectionMode selectionMode { get; private set; }

    public UnityEvent OnBackPressed;

    public Button backButton;
    public Button deleteButton;
    public Button shareButton;
    public SelectedActionButton selectedActionButton;


    [SerializeField]
    private SelectionView.SelectAllButton selectAllButton;
    [SerializeField]
    private GalleryCardView cardPrefab;
    [SerializeField]
    private FullscreenPhotoView fullscreenPhotoView;
    [SerializeField]
    private Transform contentPlaceholder;
    [SerializeField]
    private GridLayoutGroup contentGridLayout;
    [SerializeField]
    private TMPro.TMP_Text header;
    [SerializeField]
    private ScrollRect contentScroll;

    private SelectionView selectionView;
    private List<SelectableCardView> selectableCardViews = new List<SelectableCardView>();
    public bool fullScreenViewOpened { get; private set; }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackPressed?.Invoke();
        }
    }

    public override void Initialize()
    {
        fullScreenViewOpened = false;
        shareButton.onClick.AddListener(() =>
        {
            EnableCardsSelection(SelectionMode.Share);
        });

        deleteButton.onClick.AddListener(() =>
        {
            EnableCardsSelection(SelectionMode.Delete);
        });

        backButton.onClick.AddListener(() =>
        {
            OnBackPressed?.Invoke();
        });

        selectedActionButton.Initialize(this);
        fullscreenPhotoView.exitButton.onClick.AddListener(CloseFullscreenView);
        selectionView = new SelectionView(selectAllButton, selectableCardViews);
        selectionView.OnSelectionEnabled.AddListener(OnSelectionEnabled);
    }

    public void Open(List<GalleryCardView.TextureData> photoDatas)
    {
        gameObject.SetActive(true);

        selectionView = new SelectionView(selectAllButton, selectableCardViews);
        selectionView.OnCardSelected.AddListener(OnCardSelected);

        foreach (var item in photoDatas)
        {
            AddCard(item);
        }

        EnableCardsSelection(SelectionMode.Disabled);
    }

    public void Close()
    {
        selectionView.OnCardSelected.RemoveAllListeners();

        gameObject.SetActive(false);

        selectableCardViews = selectionView.GetAllCards();
        DeleteAllCards();
        selectableCardViews.Clear();
    }

    public void CloseFullscreenView()
    {
        fullscreenPhotoView.Close();
        fullScreenViewOpened = false;
    }

    public void AddCard(GalleryCardView.TextureData photoData)
    {
        GalleryCardView cardView = Instantiate(cardPrefab, contentPlaceholder);
        cardView.OnCardClick.AddListener(OpenFullScreenView);
        selectionView.AddCard(cardView);
        cardView.Initialize(photoData);
    }

    public void RemoveSelectedCards()
    {
        List<SelectableCardView> cards = selectionView.GetSelected();
        foreach (var item in cards)
        {
            selectionView.RemoveCard(item);
            Destroy(item.gameObject);
        }
        selectedActionButton.ChangeHeader(selectionView.GetSelected().Count);
    }

    public void EnableCardsSelection(SelectionMode selectionMode)
    {
        bool selectionEnabled = selectionMode == SelectionMode.Delete || selectionMode == SelectionMode.Share ? true : false;
        this.selectionMode = selectionMode;

        deleteButton.gameObject.SetActive(!selectionEnabled);
        shareButton.gameObject.SetActive(!selectionEnabled);

        selectedActionButton.button.gameObject.SetActive(selectionEnabled);
        selectedActionButton.ChangeHeader(selectionView.GetSelected().Count);
        selectionView.EnableCardsSelection(selectionEnabled);
    }

    public List<GalleryCardView> GetSelectedCards()
    {
        List<GalleryCardView> result = new List<GalleryCardView>();

        foreach (var item in selectionView.GetAllCards())
        {
            result.Add(item as GalleryCardView);
        }


        return result;
    }

    private void OpenFullScreenView(GalleryCardView cardView)
    {
        fullScreenViewOpened = true;
        List<GalleryCardView> cards = new List<GalleryCardView>();

        foreach (var item in selectionView.GetAllCards())
        {
            cards.Add(item as GalleryCardView);
        }

        for (int i = 0; i < cards.Count; i++)
        {
            if (cardView == cards[i])
            {
                List<GalleryCardView.TextureData> textures = new();

                foreach (var item in cards)
                {
                    textures.Add(item.photoData);
                }

                fullscreenPhotoView.Open(textures, i);
                return;
            }
        }

        throw new System.Exception("!ATTENTION! critical EGOR occured");
    }

    private void DeleteAllCards()
    {
        foreach (var item in selectableCardViews)
        {
            selectionView.RemoveCard(item);
            Destroy(item.gameObject);
        }
    }
    private void MoveDownContentScroll(SelectionMode selectionMode)
    {
        if (selectionMode == SelectionMode.Disabled)
        {
            contentGridLayout.padding.top = 0;
        }
        else
        {
            contentGridLayout.padding.top = 100;
        }
    }

    private void ChangeHeader(SelectionMode selectionMode)
    {
        switch (selectionMode)
        {
            case SelectionMode.Disabled:
                header.text = "Фото";
                break;
            case SelectionMode.Delete:
                header.text = "Удалить фото";
                break;
            case SelectionMode.Share:
                header.text = "Поделиться фото";
                break;
            default:
                break;
        }
    }

    private void OnSelectionEnabled(bool value)
    {
        selectedActionButton.button.gameObject.SetActive(value);
        selectedActionButton.ChangeHeader(0);

        deleteButton.gameObject.SetActive(!value);
        shareButton.gameObject.SetActive(!value);

        ChangeHeader(selectionMode);
        MoveDownContentScroll(selectionMode);
    }

    private void OnCardSelected(SelectableCardView selectableCard)
    {
        selectedActionButton.ChangeHeader(selectionView.GetSelected().Count);
    }

    public enum SelectionMode
    {
        Disabled, Delete, Share
    }

    [System.Serializable]
    public class SelectedActionButton
    {
        public TMPro.TMP_Text header;
        public Button button;
        public UnityEvent OnCick;

        private GalleryView projectGalleryView;

        public void Initialize(GalleryView projectGalleryView)
        {
            this.projectGalleryView = projectGalleryView;

            button.onClick.AddListener(() =>
            {
                OnCick?.Invoke();
            });
        }

        public void ChangeHeader(int selectedCount)
        {
            if (projectGalleryView.selectionMode == SelectionMode.Delete)
            {
                header.text = $"Удалить ({selectedCount})";
            }
            else if (projectGalleryView.selectionMode == SelectionMode.Share)
            {
                header.text = $"Поедлиться ({selectedCount})";
            }
        }
    }
}