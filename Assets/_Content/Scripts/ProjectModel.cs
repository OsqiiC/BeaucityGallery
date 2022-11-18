using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectModel : MonoBehaviour
{
    private Project currentProject;
    private List<Project> projects;

    private void Awake()
    {
        projects = new List<Project>();
        projects.Add(new Project(null) { projectID = "test" });
        projects.Add(new Project(null) { projectID = "tESt" });
        currentProject = projects[0];
    }

    public void CreateProject(Project project)
    {
        projects.Add(project);
    }

    public void OpenProject(Project project)
    {
        currentProject = GetProject(project);
    }

    public void DeleteProject(Project project)
    {
        projects.Remove(GetProject(project));
    }

    public Project GetCurrentProject()
    {
        return new Project(currentProject);
    }

    public List<Project> GetProjects()
    {
        List<Project> result = new List<Project>();

        foreach (var item in projects)
        {
            result.Add(new Project(item));
        }

        return result;
    }

    private Project GetProject(Project project)
    {
        foreach (var item in projects)
        {
            if(item.projectID == project.projectID)
            {
                return item;
            }
        }

        throw new System.Exception("roject not found");
    }

    public class Project
    {
        public string projectID;

        public Project(Project project)
        {
            projectID = project?.projectID;
        }

        public Project()
        {

        }
    }
}
