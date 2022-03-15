using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Modules.WDCore;

public class ExtensionCoroutine : Singleton<ExtensionCoroutine>
{
    Coroutine lastRoutine = null;

    public IEnumerator StartExtendedCoroutine(IEnumerator enumerator)
    {            
        yield return StartCoroutine(enumerator);            
    }

    public void StartExtendedCoroutineNoWait(IEnumerator enumerator)
    {
        StartCoroutine(enumerator);
    }

    public void StopExtendedCoroutine()
    {
        if (lastRoutine != null)
            StopCoroutine(lastRoutine);       
    }
}
