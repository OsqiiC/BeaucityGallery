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
    [SerializeField]
    private AspectRatioFitter aspectRatioFitter;

    public GalleryCardView Initialize(TextureData photoData)
    {
        this.photoData = photoData;
        gameObject.SetActive(true);
        contentImage.texture = photoData.texture;
      //  SetContentSize();
        UpdateAspectRatio();
        checkMark.gameObject.SetActive(selected);
        cardButton.onClick.AddListener(()=> OnCardClick?.Invoke(this));
        return this;
    }

    public override void OnSelected()
    {
        UpdateAspectRatio();
        checkMark.gameObject.SetActive(selected);
    }

    public override void OnSelectionEnabled(bool value)
    {
        checkMark.gameObject.SetActive(!value);
        cardButton.gameObject.SetActive(!value);
    }

    public void UpdateAspectRatio()
    {
        aspectRatioFitter.aspectRatio = (float)photoData.texture.width / photoData.texture.height;
    }

    public class TextureData
    {
        public Texture2D texture;
        public PhotoModel.PhotoData photoData;
        public string filePath;
    }
}
