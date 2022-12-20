using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Zenject;

public class PhotoModeInstaller : MonoInstaller
{
    [SerializeField]
    private PhotoModeView photoModeView;
    [SerializeField]
    private ColorSwitch colorSwitch;

    public override void InstallBindings()
    {
        Container.BindInstance(photoModeView);
        Container.BindInstance(colorSwitch);
        Container.BindInterfacesAndSelfTo<PhotoModeController>().AsSingle();
    }
}
