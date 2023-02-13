using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TempController : Controller
{
    private NavigationModel navigationModel;
    private TempView tempView;

    private GalleryController galleryController;
    private PhotoModeController photoModeController;

    public TempController(NavigationModel navigationModel, TempView tempView, GalleryController galleryController, PhotoModeController photoModeController) : base(tempView)
    {
        this.navigationModel = navigationModel;
        this.tempView = tempView;
        this.galleryController = galleryController;
        this.photoModeController = photoModeController;

        Initialize();
    }

    private void Initialize()
    {
        Application.targetFrameRate = 60;

        tempView.galleryButton.onClick.AddListener(() =>
        {
            navigationModel.AddController(galleryController);
            galleryController.Open();
            Close();
        });
        tempView.photoModeButton.onClick.AddListener(() =>
        {
            navigationModel.AddController(photoModeController);
            photoModeController.Open();
            Close();
        });
        tempView.OnBackPressed.AddListener(Back);
        Open();
        navigationModel.AddController(this);
    }

    public void Back()
    {
        Debug.Log("Application closed");
    }

    public override void Close()
    {
        base.Close();
        tempView.Close();
    }

    public override void Open()
    {
        base.Open();
        tempView.Open();
    }
}

