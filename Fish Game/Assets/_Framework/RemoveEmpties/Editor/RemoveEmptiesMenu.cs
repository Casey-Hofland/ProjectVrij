using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[InitializeOnLoad]
public class RemoveEmptiesMenu
{
	static RemoveEmptiesMenu()
	{
		EditorApplication.delayCall += InitializeBuildMenu;
	}

	private static readonly string uniquePref = PlayerSettings.companyName + "." + PlayerSettings.productName + "/";

    private const string MENU_PATH = "Tools/";
    private const string REMOVE_PATH = MENU_PATH + "Remove Empty GameObjects/";
    private const string ONALL_PATH = REMOVE_PATH + "On All";
    private const string ONPLAY_PATH = REMOVE_PATH + "On Play";
    private const string ONDEVELOPMENTBUILD_PATH = REMOVE_PATH + "On Development Build";
    private const string ONBUILD_PATH = REMOVE_PATH + "On Build";

    public static bool OnPlayPref { get { return EditorPrefs.GetBool(uniquePref + ONPLAY_PATH, true); } }
    public static bool OnDevelopmentBuildPref { get { return EditorPrefs.GetBool(uniquePref + ONDEVELOPMENTBUILD_PATH, true); } }
    public static bool OnBuildPref { get { return EditorPrefs.GetBool(uniquePref + ONBUILD_PATH, true); } }

    private static readonly Dictionary<string, bool> onRemovePaths = new Dictionary<string, bool>()
    {
        {ONPLAY_PATH, OnPlayPref},
        {ONDEVELOPMENTBUILD_PATH, OnDevelopmentBuildPref},
        {ONBUILD_PATH, OnBuildPref}
    };

    // Sets the Checks on the menu items on initialization, else they will appear blank when you start the Editor.
    private static void InitializeBuildMenu()
    {
        foreach (KeyValuePair<string, bool> path in onRemovePaths)
        {
            Menu.SetChecked(path.Key, path.Value);
        }
    }

    [MenuItem(ONALL_PATH, priority = 901)]
    private static void ToggleRemoveOnAll()
    {
        bool check = onRemovePaths.ContainsValue(false);

        for (int i = 0; i < onRemovePaths.Count; i++)
        {
            string path = onRemovePaths.Keys.ElementAt(i);
            SetRemove(path, check);
        }
    }

    [MenuItem(ONPLAY_PATH, priority = 902)]
    private static void ToggleRemoveOnPlay()
    {
        ToggleRemove(ONPLAY_PATH);
    }

    [MenuItem(ONDEVELOPMENTBUILD_PATH, priority = 903)]
    private static void ToggleRemoveOnDevelopmentBuild()
    {
        ToggleRemove(ONDEVELOPMENTBUILD_PATH);
    }

    [MenuItem(ONBUILD_PATH, priority = 904)]
    private static void ToggleRemoveOnBuild()
    {
        ToggleRemove(ONBUILD_PATH);
    }

    private static void SetRemove(string path, bool check)
    {
        EditorPrefs.SetBool(uniquePref + path, check);
        Menu.SetChecked(path, check);

        if (onRemovePaths.ContainsKey(path))
            onRemovePaths[path] = check;
    }

    private static void ToggleRemove(string path)
    {
        bool check = !Menu.GetChecked(path);
        SetRemove(path, check);
    }
}
