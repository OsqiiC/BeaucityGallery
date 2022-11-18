using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NavigationModel : MonoBehaviour
{
    public UnityEvent CloseCall;

    private List<Controller> controllers = new List<Controller>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCall?.Invoke();
        }
    }

    public void AddController(Controller controller)
    {
        if (controllers.Contains(controller))
        {
            while (controllers[controllers.Count - 1] != controller)
            {
                controllers.RemoveAt(controllers.Count - 1);
            }
            controllers.RemoveAt(controllers.Count - 1);
        }

        controllers.Add(controller);
    }

    public Controller GetCurrentController()
    {
        //if (controllers.Count < 1) return null;
        //return controllers[controllers.Count - 1];
        return GetController(controllers.Count - 1);
    }

    public Controller GetPreviousController()
    {
        return GetController(controllers.Count - 2);
    }

    public bool TryRemoveController(out Controller controller)
    {
        if (controllers.Count <= 1)
        {
            controller = null;
            return false;
        }

        controller = controllers[controllers.Count - 1];
        controllers.Remove(controller);
        Debug.Log("controllers count = " + controllers.Count);
        return true;
    }

    private Controller GetController(int index)
    {
        if (controllers.Count < index) return null;
        return controllers[index];
    }
}
