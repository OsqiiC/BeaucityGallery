using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GalleryController : Controller
{
    private GalleryView galleryView;
    private NavigationModel navigationModel;
    private PhotoModel photoModel;
    private ProjectModel projectModel;

    public GalleryController(GalleryView projectGalleryView, NavigationModel navigationModel,
        PhotoModel photoModel, ProjectModel projectModel) : base(projectGalleryView)
    {
        galleryView = projectGalleryView;
        this.navigationModel = navigationModel;
        this.photoModel = photoModel;
        this.projectModel = projectModel;

        Initialize();
    }

    private void Initialize()
    {
        if(photoModel ==null || projectModel == null || navigationModel == null)
        {
            Debug.LogError("install PhotoModel ProjectModel NavigationModel in projectContext");
        }
        galleryView.OnBackPressed.AddListener(Back);

        galleryView.selectedActionButton.OnCick.AddListener(() =>
        {
            if (galleryView.selectionMode == GalleryView.SelectionMode.Delete)
            {
                foreach (var item in galleryView.GetSelectedCards())
                {
                    photoModel.DeletePhoto(item.photoData.photoData);
                }
                galleryView.RemoveSelectedCards();
            }
            else if (galleryView.selectionMode == GalleryView.SelectionMode.Share)
            {
                List<string> photoPaths = new();

                foreach (var item in galleryView.GetSelectedCards())
                {
                    photoPaths.Add(item.photoData.photoData.fullFilePath);
                }

                FileSharer.ShareFile(photoPaths);
            }
        });
    }

    public override void Close()
    {
        base.Close();
        galleryView.Close();
    }

    public override void Open()
    {
        base.Open();
        List<GalleryCardView.TextureData> photoTextures = new List<GalleryCardView.TextureData>();

        foreach (var item in photoModel.GetPhotos(projectModel.GetCurrentProject().projectID))
        {
            photoTextures.Add(new GalleryCardView.TextureData()
            {
                texture = photoModel.GetPhotoTexture(item),
                photoData = item
            });
        }

        galleryView.Open(photoTextures);
    }

    public void Back()
    {
        if (!IsOpened) return;

        if (galleryView.fullScreenViewOpened)
        {
            galleryView.CloseFullscreenView();
            return;
        }
        if(galleryView.selectionMode != GalleryView.SelectionMode.Disabled)
        {
            galleryView.EnableCardsSelection(GalleryView.SelectionMode.Disabled);
            return;
        }

        navigationModel.TryRemoveController(out Controller controller);
        navigationModel.GetCurrentController().Open();
        Close();
    }
}
