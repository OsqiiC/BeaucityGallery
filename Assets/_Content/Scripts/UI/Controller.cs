using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Controller
{
    public bool IsOpened { get; private set; }

    public Controller(View view)
    {
        view.Initialize();
        IsOpened = false;
    }

    public virtual void Open()
    {
        IsOpened = true;
    }
    public virtual void Close()
    {
        IsOpened = false;
    }
}
