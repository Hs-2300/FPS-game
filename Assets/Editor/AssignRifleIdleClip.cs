using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AssignRifleIdleClip
{
    private const string ClipPath = "Assets/Shooter Pack/rifle aiming idle.fbx";

    [MenuItem("Tools/Animation/Assign Rifle Idle to Ukrainian Soldier")]
    private static void Assign()
    {
        var clip = AssetDatabase.LoadAllAssetsAtPath(ClipPath)
            .OfType<AnimationClip>()
            .FirstOrDefault(item => !item.name.StartsWith("__preview__"));

        if (clip == null)
        {
            Debug.LogError("rifle aiming idle.fbx 没有导入 Animation Clip。请检查该 FBX 的 Animation 页是否启用了 Import Animation。", AssetDatabase.LoadMainAssetAtPath(ClipPath));
            return;
        }

        GameObject soldier = null;
        for (int i = 0; i < SceneManager.sceneCount && soldier == null; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            foreach (var root in scene.GetRootGameObjects())
            {
                var match = root.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.name == "Ukrainian_Soldier1");
                if (match != null) { soldier = match.gameObject; break; }
            }
        }

        if (soldier == null)
        {
            Debug.LogError("当前打开的场景中找不到 Ukrainian_Soldier1。");
            return;
        }

        var display = soldier.GetComponent<Display>();
        if (display == null) display = Undo.AddComponent<Display>(soldier);
        Undo.RecordObject(display, "Assign rifle idle clip");
        display.clip = clip;
        EditorUtility.SetDirty(display);
        EditorSceneManager.MarkSceneDirty(soldier.scene);
        Selection.activeGameObject = soldier;
        Debug.Log("已将 " + clip.name + " 分配给 Ukrainian_Soldier1 的 Display。请保存场景并进入 Play 模式。", soldier);
    }
}
