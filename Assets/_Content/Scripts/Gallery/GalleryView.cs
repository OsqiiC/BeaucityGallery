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
    private GameObject noPhotoText;
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

    public bool FullScreenViewOpened { get; private set; }
    public SelectionView SelectionView => selectionView;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackPressed?.Invoke();
        }
    }

    public override void Initialize()
    {
        FullScreenViewOpened = false;
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
        selectionView = new SelectionView();
        selectionView.Initialize(selectAllButton);
        selectionView.EnableCardsSelection(false);
        selectionView.OnCardsSelectionStateChanged.AddListener((cards) =>
        {
            selectedActionButton.ChangeHeader(cards.Count);
        });
    }

    public void Open(List<GalleryCardView.TextureData> photoDatas)
    {
      gameObject.SetActive(true);

        foreach (var item in photoDatas)
        {
            AddCard(item);
        }

        EnableCardsSelection(SelectionMode.Disabled);
        SetActiveNoPhotoText(photoDatas.Count == 0);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        DeleteCards(selectionView.GetAllCards<GalleryCardView>());
        selectableCardViews.Clear();
    }

    public void CloseFullscreenView()
    {
        fullscreenPhotoView.Close();
        FullScreenViewOpened = false;
    }

    public void EnableCardsSelection(SelectionMode selectionMode)
    {
        bool selectionEnabled = selectionMode == SelectionMode.Delete || selectionMode == SelectionMode.Share ? true : false;
        this.selectionMode = selectionMode;

        deleteButton.gameObject.SetActive(!selectionEnabled);
        shareButton.gameObject.SetActive(!selectionEnabled);

        contentScroll.content.GetComponent<GridLayoutGroup>().padding.top = selectionEnabled ? 230 : 80;
        selectedActionButton.button.gameObject.SetActive(selectionEnabled);
        selectedActionButton.ChangeHeader(selectionView.GetSelected<SelectableCardView>().Count);
        selectionView.EnableCardsSelection(selectionEnabled);
    }

    public void DeleteCards(List<GalleryCardView> galleryCardViews)
    {
        foreach (var item in galleryCardViews)
        {
            Destroy(item.gameObject);
            selectionView.RemoveCard(item);
        }
    }

    public void DeleteCard(GalleryCardView galleryCardView)
    {
        Destroy(galleryCardView.gameObject);
        selectionView.RemoveCard(galleryCardView);
    }

    public void SetActiveNoPhotoText(bool value)
    {
        noPhotoText.SetActive(value);
        shareButton.gameObject.SetActive(!value);
        deleteButton.gameObject.SetActive(!value);
    }

    private void AddCard(GalleryCardView.TextureData photoData)
    {
        GalleryCardView cardView = Instantiate(cardPrefab, contentPlaceholder);
        cardView.OnCardClick.AddListener(OpenFullScreenView);
        selectionView.AddCard(cardView);
        cardView.Initialize(photoData);
    }

    public int GetCardsCount()
    {
        return selectionView.GetAllCards<GalleryCardView>().Count;
    }

    private void OpenFullScreenView(GalleryCardView cardView)
    {
        FullScreenViewOpened = true;
        List<GalleryCardView> cards = new List<GalleryCardView>();

        foreach (var item in selectionView.GetAllCards<GalleryCardView>())
        {
            cards.Add(item);
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
        selectedActionButton.ChangeHeader(selectionView.GetSelected<GalleryCardView>().Count);
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