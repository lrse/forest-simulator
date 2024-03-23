using UnityEngine;
using System.Collections;

public static class StaticCoroutine {
    private class CoroutineHolder : MonoBehaviour { }

    //lazy singleton pattern. Note that I don't set it to dontdestroyonload - you usually want corotuines to stop when you load a new scene.
    private static CoroutineHolder _runner;
    private static CoroutineHolder runner {
        get {
            if (_runner == null) {
                _runner = new GameObject("Static Corotuine Runner").AddComponent<CoroutineHolder>();
            }
            return _runner;
        }
    }

    public static void StartCoroutine(IEnumerator corotuine) {
        runner.StartCoroutine(corotuine);
    }
}
