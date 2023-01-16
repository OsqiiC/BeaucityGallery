using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TempInstaller : MonoInstaller
{
    [SerializeField]
    private TempView tempView;

    public override void InstallBindings()
    {
        Container.BindInstance(tempView);
        Container.Bind<TempController>().AsSingle().NonLazy();

        Watch.StartWatch();
    }
}
