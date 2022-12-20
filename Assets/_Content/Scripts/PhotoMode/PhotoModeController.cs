using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class PhotoModeController : Controller
{
    private PhotoModel photoModel;
    private PhotoModeView photoModeView;
    private ProjectModel projectModel;
    private NavigationModel navigationModel;
    private ColorSwitch colorSwitch;

    public PhotoModeController(PhotoModel photoModel, PhotoModeView photoModeView, NavigationModel navigationModel,
        ProjectModel projectModel, ColorSwitch colorSwitch) : base(photoModeView)
    {
        this.photoModel = photoModel;
        this.photoModeView = photoModeView;
        this.navigationModel = navigationModel;
        this.projectModel = projectModel;
        this.colorSwitch = colorSwitch;

        Initialize();
    }

    private void Initialize()
    {
        photoModeView.OnBackPressed.AddListener(Back);
        photoModeView.PostPhoto.AddListener((texture) => photoModel.AddPhoto(texture, projectModel.GetCurrentProject().projectID));
        photoModeView.PrePhoto.AddListener(colorSwitch.SwitchColors);
    }

    public override void Close()
    {
        base.Close();
        photoModeView.Close();
    }

    public override void Open()
    {
        base.Open();
        photoModeView.Open();
    }

    public  void Back()
    {
        if (!IsOpened) return;
        navigationModel.TryRemoveController(out Controller controller);
        navigationModel.GetCurrentController().Open();
        Close();
    }
}
