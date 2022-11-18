using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GalleryInstaller : MonoInstaller
{
    [SerializeField]
    private GalleryView projectGalleryView;

    public override void InstallBindings()
    {
        Container.BindInstance(projectGalleryView);
        Container.BindInterfacesAndSelfTo<GalleryController>().AsSingle();
    }
}
