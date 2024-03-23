using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions {
    public static GameObject GetOrCreateGameObjectByName(this GameObject go, string name) {
		foreach (Transform child in go.transform) {
            if (child.name == name) {
                return child.gameObject;
            }
        }
        GameObject newChild = new GameObject(name);
        newChild.transform.parent = go.transform;
        return newChild;
    }

    public static Transform DestroyImmediateChildren(this Transform transform)	{
        // Unity doesn't want to destroy objects while OnValidate
        // Known workaround: https://forum.unity.com/threads/onvalidate-and-destroying-objects.258782/#post-1710165
        foreach (Transform child in transform) {
            UnityEditor.EditorApplication.delayCall += () => {
                GameObject.DestroyImmediate(child.gameObject);
            };
        }
		return transform;
	}

    public static IEnumerable<GameObject> GetChildren(this GameObject go) {
        foreach (Transform t in go.transform) {
            yield return t.gameObject;
        }
    }
}
