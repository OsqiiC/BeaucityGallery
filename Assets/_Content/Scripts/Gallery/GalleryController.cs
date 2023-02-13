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
        if (photoModel == null || projectModel == null || navigationModel == null)
        {
            Debug.LogError("install PhotoModel ProjectModel NavigationModel in projectContext");
        }
        galleryView.OnBackPressed.AddListener(Back);

        galleryView.selectedActionButton.OnCick.AddListener(() =>
        {
            if (galleryView.selectionMode == GalleryView.SelectionMode.Delete)
            {
                foreach (var item in galleryView.SelectionView.GetSelected<GalleryCardView>())
                {
                    photoModel.DeletePhoto(item.photoData.photoData);
                    galleryView.DeleteCard(item);
                }
                if (galleryView.GetCardsCount() == 0)
                {
                    galleryView.EnableCardsSelection(GalleryView.SelectionMode.Disabled);
                    galleryView.SetActiveNoPhotoText(true);
                }
            }
            else if (galleryView.selectionMode == GalleryView.SelectionMode.Share)
            {
                List<string> photoPaths = new();

                foreach (var item in galleryView.SelectionView.GetSelected<GalleryCardView>())
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
        Resources.UnloadUnusedAssets();
    }

    public override async void Open()
    {

        base.Open();
        galleryView.gameObject.SetActive(true);
        await System.Threading.Tasks.Task.Yield();
        //foreach (var item in photoModel.GetPhotos(projectModel.GetCurrentProject().projectID))
        //{
        //    photoTextures.Add(new GalleryCardView.TextureData()
        //    {
        //        texture = photoModel.GetPhotoTexture(item),
        //        filePath = item.fullFilePath,
        //        photoData = item
        //    });
        //}

        IEnumerator govno()
        {
            Watch.ResetWatch();
            List<PhotoModel.PhotoData> photoDatas = photoModel.GetPhotos(projectModel.GetCurrentProject().projectID);

            for (int i = 0; i < photoDatas.Count; i++)
            {
                PhotoModel.PhotoData item = photoDatas[i];
                if (i < 8)
                {
                    Watch.ResetWatch();
                    galleryView.AddCard(new GalleryCardView.TextureData()
                    {
                        texture = photoModel.GetPhotoTexture(item),
                        filePath = item.fullFilePath,
                        photoData = item
                    });
                    Watch.LogTime();
                }
                else
                {
                    yield return new WaitForSeconds(0.1f);
                    yield return photoModel.GetTexture(item, (texture) =>
                    {
                        galleryView.AddCard(new GalleryCardView.TextureData()
                        {
                            texture = texture,
                            filePath = item.fullFilePath,
                            photoData = item
                        });
                    });
                }
            }
            Watch.LogTime();
        }

        galleryView.Open(govno());
    }

    public void Back()
    {
        if (!IsOpened) return;

        if (galleryView.FullScreenViewOpened)
        {
            galleryView.CloseFullscreenView();
            return;
        }
        if (galleryView.selectionMode != GalleryView.SelectionMode.Disabled)
        {
            galleryView.EnableCardsSelection(GalleryView.SelectionMode.Disabled);
            return;
        }

        navigationModel.TryRemoveController(out Controller controller);
        navigationModel.GetCurrentController().Open();
        Close();
    }
}
