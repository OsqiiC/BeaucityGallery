using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Zenject;

public class PhotoModeInstaller : MonoInstaller
{
    [SerializeField]
    private PhotoModeView photoModeView;

    public override void InstallBindings()
    {
        Container.BindInstance(photoModeView);
        Container.BindInterfacesAndSelfTo<PhotoModeController>().AsSingle();
    }
}
