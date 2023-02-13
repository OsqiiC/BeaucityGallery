using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class FullscreenPhotoView : MonoBehaviour
{
    public Button exitButton;

    [SerializeField]
    private float doubleTapScaling;
    [SerializeField]
    private Image imagePrefab;
    [SerializeField]
    private FullscreenImage fullscreenImage;
    [SerializeField]
    private HorizontalScrollSnap horizontalScrollSnap;
    [SerializeField]
    private ScrollRect scrollRect;
    [SerializeField]
    private CanvasScaler canvasScaler;
    [SerializeField]
    private TMPro.TMP_Text pageCounter;
    [SerializeField]
    private TouchInput touchInput;

    private FullscreenImage currentImage;
    private List<FullscreenImage> images = new List<FullscreenImage>();
    private bool isScaling = false;
    private RectTransform currentRectTransform => currentImage.rawImage.rectTransform;
    private float canvasHeight => canvasScaler.referenceResolution.x * ((float)Screen.height / Screen.width);
    private float canvasWidth => canvasScaler.referenceResolution.x;

    public void Open(List<GalleryCardView.TextureData> photoData, int itemIndex)
    {
        gameObject.SetActive(true);
        EnableScrollView(true);

        horizontalScrollSnap.OnSelectionPageChangedEvent.AddListener(SelectionPageChanged);
        touchInput.OnDrag.AddListener(OnDrag);
        touchInput.OnDoubleTap.AddListener(DoubleTapScale);
        touchInput.OnScaleStart.AddListener(ScaleStart);
        touchInput.OnDeltaScaleChanged.AddListener(DeltaScaleChanged);
        touchInput.OnScaleEnd.AddListener(ScaleEnd);

        foreach (var item in photoData)
        {
            AddImage(item);
        }

        IEnumerator kal()
        {
            yield return null;
            horizontalScrollSnap.ChangePage(itemIndex);
            ResetImageSizeDelta(images);
        }

        StartCoroutine(kal());

    }

    public void Close()
    {
        horizontalScrollSnap.OnSelectionPageChangedEvent.RemoveListener(SelectionPageChanged);
        touchInput.OnDrag.RemoveListener(OnDrag);
        touchInput.OnDoubleTap.RemoveListener(DoubleTapScale);
        touchInput.OnScaleStart.RemoveListener(ScaleStart);
        touchInput.OnDeltaScaleChanged.RemoveListener(DeltaScaleChanged);
        touchInput.OnScaleEnd.RemoveListener(ScaleEnd);

        gameObject.SetActive(false);
        DeleteImages();
    }

    //public void AddImage(GalleryCardView.TextureData photoData)
    //{
    //    Watch.ResetWatch();

    //    Sprite sprite = Sprite.Create(photoData.texture, new Rect(0, 0, photoData.photoData.width, photoData.photoData.height), new Vector2(0.5f, 0.5f));
    //    Image image = Instantiate(imagePrefab, scrollRect.content);
    //    RoflanImage roflanImage = new RoflanImage() { image = image, sizeDelta = new Vector2(photoData.photoData.width, photoData.photoData.height) };
    //    image.sprite = sprite;
    //    images.Add(roflanImage);

    //    horizontalScrollSnap.AddChild(image.gameObject);
    //    Watch.LogTime("gallery opening time");
    //}  

    public void AddImage(GalleryCardView.TextureData photoData)
    {


        FullscreenImage image = Instantiate(fullscreenImage, scrollRect.content);
        image.rawImage.texture = photoData.texture;

        images.Add(image);

        horizontalScrollSnap.AddChild(image.gameObject);
    }

    private void SelectionPageChanged(int pageIndex)
    {
        currentImage = images[pageIndex];

        ResetImageSizeDelta(images);
        pageCounter.text = $"{pageIndex + 1}/{horizontalScrollSnap.ChildObjects.Length}";
    }

    private void OnDrag(Vector2 offset)
    {
        if (isScaling) return;
        if (!horizontalScrollSnap.isSettled) return;
        currentRectTransform.anchoredPosition += offset;
        MoveTowardsScreen();
    }

    private void ScaleStart()
    {
        if (!horizontalScrollSnap.isSettled) return;
        isScaling = true;
        EnableScrollView(false);
    }

    private void DeltaScaleChanged(float delta, Vector2 position)
    {
        if (currentRectTransform.localScale.x > 4 && delta > 0) return;
        if (currentRectTransform.localScale.x < 0.5f && delta < 0) return;
        if (!horizontalScrollSnap.isSettled)
        {
            horizontalScrollSnap.transitionSpeed = 100;
            scrollRect.enabled = false;
            horizontalScrollSnap.ChangePage(horizontalScrollSnap.CurrentPage);
            scrollRect.enabled = true;
            horizontalScrollSnap.transitionSpeed = 25;
            return;
        }

        if (currentRectTransform.localScale.x > 1)
        {
            MovePivotToPositionOnScreen(position * canvasWidth / Screen.width, currentRectTransform);
            ScaleImageAdditive(delta / canvasWidth);
            MoveTowardsScreen();
        }
        else
        {
            currentRectTransform.anchoredPosition = new Vector2(canvasWidth * (horizontalScrollSnap.CurrentPage + 0.5f), 0);
            currentRectTransform.pivot = Vector2.one * 0.5f;
            ScaleImageAdditive(delta / canvasWidth);
        }
    }

    private void ScaleEnd()
    {
        isScaling = false;
        if (currentRectTransform.localScale.x < 1) ResetImage();
    }

    private void DeleteImages()
    {
        GameObject[] toDestroy;
        horizontalScrollSnap.RemoveAllChildren(out toDestroy);
        for (int i = 0; i < toDestroy.Length; i++)
        {
            Destroy(toDestroy[i]);
        }
        images.Clear();
    }

    private void EnableScrollView(bool value)
    {
        scrollRect.enabled = value;
        horizontalScrollSnap.enabled = value;
        ResetImageSizeDelta(images);
    }

    private void DoubleTapScale(Vector2 position)
    {
        if (isScaling) return;
        if (!horizontalScrollSnap.isSettled) return;

        if (currentRectTransform.localScale == Vector3.one)
        {
            StartCoroutine(ScaleImage(doubleTapScaling, 0.2f, position));
        }
        else
        {
            ResetImage();
        }
    }

    private void MovePivotToPositionOnScreen(Vector2 input, RectTransform rect)
    {
        if (currentRectTransform.localScale.x < 1) return;
        Vector2 pageOffset = new Vector2(-canvasWidth * (horizontalScrollSnap.CurrentPage), canvasHeight / 2);
        Vector2 prevPivot = rect.pivot;
        Vector2 size = rect.sizeDelta * rect.localScale.x;
        rect.pivot += (input - rect.anchoredPosition - pageOffset) / size;
        currentRectTransform.anchoredPosition += (rect.pivot - prevPivot) * size;
        //  AddPositionClamped((rect.pivot - prevPivot) * size);
    }

    private void ScaleImageAdditive(float delta)
    {
        if (delta > 0 && currentRectTransform.localScale.x > 4) return;
        if (delta < 0 && currentRectTransform.localScale.x < 0.5f) return;
        if (!horizontalScrollSnap.isSettled) return;

        ResetImageSizeDelta(currentImage);

        currentRectTransform.localScale += Vector3.one * delta;

        if (currentRectTransform.localScale != Vector3.one)
        {
            EnableScrollView(false);
        }
    }

    private IEnumerator ScaleImage(float scale, float duration, Vector2 position)
    {
        isScaling = true;

        MovePivotToPositionOnScreen(position * canvasWidth / Screen.width, currentRectTransform);
        yield return null;

        Vector2 pageOffset = new Vector2(-canvasWidth * (horizontalScrollSnap.CurrentPage), canvasHeight / 2);
        Vector2 startPosition = currentRectTransform.anchoredPosition;
        Vector2 targetPosition = new Vector2(canvasWidth / 2, canvasHeight / 2) - pageOffset;
        float startScale = currentRectTransform.localScale.x;

        for (float i = 0; i < duration; i += Time.deltaTime)
        {
            Vector3 scaleVector = Vector3.one * Mathf.Lerp(startScale, scale, i / duration);
            currentRectTransform.localScale = scaleVector;

            currentRectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, i / duration);

            MoveTowardsScreen();
            yield return null;
        }

        currentRectTransform.localScale = scale * Vector3.one;
        currentRectTransform.anchoredPosition = new Vector2(canvasWidth / 2, canvasHeight / 2) - pageOffset;
        MoveTowardsScreen();

        if (currentRectTransform.localScale != Vector3.one)
        {
            EnableScrollView(false);
        }

        isScaling = false;
    }

    private void ResetImage()
    {
        if (isScaling) return;

        IEnumerator temp()
        {
            Rect worldRect = GetWorldSpaceRect(currentRectTransform);
            Vector2 pageOffset = new Vector2(-canvasWidth * (horizontalScrollSnap.CurrentPage), canvasHeight / 2);
            yield return ScaleImage(1, 0.2f, worldRect.center + pageOffset);
            currentRectTransform.pivot = Vector2.up * 0.5f;
            currentRectTransform.anchoredPosition = new Vector2(canvasWidth * horizontalScrollSnap.CurrentPage, 0);
            EnableScrollView(true);
            ResetImageSizeDelta(currentImage);
            currentRectTransform.localScale = Vector3.one;
        }

        StartCoroutine(temp());
    }

    private void ResetImageSizeDelta(FullscreenImage image)
    {
        image.rawImage.rectTransform.sizeDelta = new Vector2(image.rawImage.rectTransform.sizeDelta.x, image.rawImage.rectTransform.sizeDelta.x * (image.textureResolution.y / image.textureResolution.x));
    }

    private void ResetImageSizeDelta(List<FullscreenImage> images)
    {
        foreach (var image in images)
        {
            image.rawImage.rectTransform.sizeDelta = new Vector2(image.rawImage.rectTransform.sizeDelta.x, image.rawImage.rectTransform.sizeDelta.x * (image.textureResolution.y / image.textureResolution.x));
        }
    }

    private void AddPositionClamped(Vector2 offset)
    {
        if (scrollRect.enabled || currentRectTransform.localScale.x <= 1) return;

        Vector2 position = currentRectTransform.anchoredPosition + offset;
        position = ClampPositionToWorldRect(currentRectTransform, new Vector2(canvasWidth, canvasHeight), position);
        currentRectTransform.anchoredPosition = position;
    }

    private void MoveTowardsScreen()
    {
        Vector2 screenSize = new Vector2(canvasWidth, canvasHeight);
        Vector2 target = currentRectTransform.anchoredPosition;
        target = ClampPositionToWorldRect(currentRectTransform, screenSize, target);

        currentRectTransform.anchoredPosition = target;
    }

    private Vector2 ClampPositionToWorldRect(RectTransform rect, Vector2 screenSize, Vector2 position)
    {
        Rect worldRect = GetWorldSpaceRect(rect);
        Vector2 pageOffset = new Vector2(-canvasWidth * (horizontalScrollSnap.CurrentPage), canvasHeight / 2);
        Vector2 upRightOffset = (Vector2.one - rect.pivot) * worldRect.size + pageOffset;
        Vector2 downLeftOffset = -rect.pivot * worldRect.size + pageOffset;

        if (worldRect.width > screenSize.x)
        {
            if (position.x + upRightOffset.x < screenSize.x) position.x = screenSize.x - upRightOffset.x;
            if (position.x + downLeftOffset.x > 0) position.x = -downLeftOffset.x;
        }
        else
        {
            if (position.x + upRightOffset.x > screenSize.x) position.x = screenSize.x - upRightOffset.x;
            if (position.x + downLeftOffset.x < 0) position.x = -downLeftOffset.x;
        }
        if (worldRect.height > screenSize.y)
        {
            if (position.y + upRightOffset.y < screenSize.y) position.y = screenSize.y - upRightOffset.y;
            if (position.y + downLeftOffset.y > 0) position.y = -downLeftOffset.y;
        }
        else
        {
            if (position.y + upRightOffset.y > screenSize.y) position.y = screenSize.y - upRightOffset.y;
            if (position.y + downLeftOffset.y < 0) position.y = -downLeftOffset.y;
        }

        return position;
    }

    private Rect GetWorldSpaceRect(RectTransform rectTransform)
    {
        Vector2 downLeftCorner = rectTransform.anchoredPosition - rectTransform.pivot * rectTransform.sizeDelta * rectTransform.localScale;
        return new Rect(downLeftCorner, rectTransform.localScale * rectTransform.sizeDelta);
    }

}
