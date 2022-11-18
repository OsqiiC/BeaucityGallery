using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(PhotoModel))]
public class ProjectContextInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<PhotoModel>().FromInstance(GetComponent<PhotoModel>()).AsSingle();
        Container.Bind<NavigationModel>().FromInstance(GetComponent<NavigationModel>()).AsSingle();
        Container.Bind<ProjectModel>().FromInstance(GetComponent<ProjectModel>()).AsSingle();
    }
}
