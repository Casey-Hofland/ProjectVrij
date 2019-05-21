using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RemoveEmpties : IProcessSceneWithReport
{
    /*
     * An empty gameobject is SPECIFICALLY refering to a gameobject with
     * ONLY a Transform Component! (RectTransforms won't be included)
    */

    public int callbackOrder { get { return 0; } }

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        // Check if the scene(s) should remove all their empty gameobjects.
        if (!report) { if (!RemoveEmptiesMenu.OnPlayPref) return; }
        else if (report.summary.options.HasFlag(BuildOptions.Development)) { if (!RemoveEmptiesMenu.OnDevelopmentBuildPref) return; }
        else if (!RemoveEmptiesMenu.OnBuildPref) return;

        // Querry for retreiving empty gameobjects.
        IEnumerable<Transform> emptyGameObjects =
            from gameObject in Object.FindObjectsOfType<Transform>()
            where gameObject.GetComponents<Component>().Length == 1 && 
				gameObject.transform.GetType() != typeof(RectTransform)
            select gameObject;

        // Detaches and orders all the children for each empty gameobject before destroying them.
        foreach (Transform transform in emptyGameObjects)
        {
            int count = transform.childCount;
            int siblingIndex = transform.GetSiblingIndex();
            for (int i = 0; i < count; i++)
            {
                Transform child = transform.GetChild(0);
                child.SetParent(transform.parent);
                child.SetSiblingIndex(siblingIndex + i);
            }

            Object.DestroyImmediate(transform.gameObject);
        }
    }
}
