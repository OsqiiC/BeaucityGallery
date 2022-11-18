using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

public class GalleryCardView : SelectableCardView
{
    public UnityEvent<GalleryCardView> OnCardClick = new();

    public TextureData photoData { get; private set; }

    [SerializeField]
    private RawImage contentImage;
    [SerializeField]
    private RectTransform cardRect;
    [SerializeField]
    private Image checkBox;
    [SerializeField]
    private Image checkMark;
    [SerializeField]
    private Button cardButton;

    public GalleryCardView Initialize(TextureData photoData)
    {
        this.photoData = photoData;
        gameObject.SetActive(true);
        contentImage.texture = photoData.texture;
        SetContentSize();
        checkMark.gameObject.SetActive(selected);
        cardButton.onClick.AddListener(()=> OnCardClick?.Invoke(this));
        return this;
    }

    public override void OnSelected()
    {
        SetContentSize();
        checkMark.gameObject.SetActive(selected);
    }

    public override void OnSelectionEnabled(bool value)
    {
        checkMark.gameObject.SetActive(!value);
        cardButton.gameObject.SetActive(!value);
    }

    private void SetContentSize()
    {
        float widthToHeightRatio = (float)Screen.width / Screen.height;
        if (contentImage.texture.width > contentImage.texture.height) widthToHeightRatio = 1 / widthToHeightRatio;

        if (widthToHeightRatio < 1)
        {
            contentImage.rectTransform.sizeDelta = new Vector2(cardRect.rect.width, cardRect.rect.width * (1f / widthToHeightRatio));
        }
        else
        {
            contentImage.rectTransform.sizeDelta = new Vector2(cardRect.rect.height * widthToHeightRatio, cardRect.rect.height);
        }

        contentImage.rectTransform.localPosition = Vector3.zero;
    }

    public class TextureData
    {
        public Texture2D texture;
        public PhotoModel.PhotoData photoData;
    }
}
