using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PhotoModeView : View
{
    public Button exitButton;
    public Button photoButton;

    public UnityEvent OnBackPressed;
    public UnityEvent PrePhoto;
    public UnityEvent<Texture2D> PostPhoto;

    [SerializeField]
    private List<GameObject> hiddableUI;
    [SerializeField]
    private Image flashImage;
    [SerializeField]
    private float flashDuration;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackPressed?.Invoke();
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator Screenshot(float duraion)
    {
        foreach (var item in hiddableUI)
        {
            item.gameObject.SetActive(false);
        }

        flashImage.gameObject.SetActive(true);
        flashImage.rectTransform.localScale = Vector3.one * 1.070f;
        for (float i = 0; i < duraion; i+=Time.deltaTime)
        {
            yield return null;
            flashImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.070f, 1.00f, i / duraion * i / duraion);
        }
        flashImage.rectTransform.localScale = Vector3.one;
        flashImage.gameObject.SetActive(false);

        yield return TakeScreenshotCoro(1);

        foreach (var item in hiddableUI)
        {
            item.gameObject.SetActive(true);
        }
    }

    private IEnumerator TakeScreenshotCoro(int superSize)
    {
        PrePhoto?.Invoke();
        yield return new WaitForEndOfFrame();
        Texture2D text = ScreenCapture.CaptureScreenshotAsTexture(superSize);
        PostPhoto?.Invoke(text);
    }

    public override void Initialize()
    {
        photoButton.onClick.AddListener(() => { StartCoroutine(Screenshot(flashDuration)); });
        exitButton.onClick.AddListener(() => OnBackPressed?.Invoke());
    }
}
