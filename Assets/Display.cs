using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>在 Play 模式下循环预览 Inspector 中指定的动画。</summary>
public class Display : MonoBehaviour
{
    [Tooltip("拖入要测试的 Animation Clip（也可以从 FBX 展开后拖入）")]
    public AnimationClip clip;

    private Animator animator;
    private PlayableGraph graph;
    private AnimationClipPlayable clipPlayable;
    private AnimationClip playingClip;

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        PlaySelectedClip();
    }

    private void Update()
    {
        if (clip != playingClip)
            PlaySelectedClip();

        if (!graph.IsValid() || !clipPlayable.IsValid() || playingClip == null || playingClip.length <= 0f)
            return;

        if (clipPlayable.GetTime() >= playingClip.length)
            clipPlayable.SetTime(clipPlayable.GetTime() % playingClip.length);
    }

    private void PlaySelectedClip()
    {
        if (graph.IsValid())
            graph.Destroy();

        playingClip = clip;
        if (clip == null)
            return;

        if (animator == null)
        {
            Debug.LogWarning("Display: 找不到 Animator，请把脚本挂在模型根物体或其父物体上。", this);
            return;
        }

        graph = PlayableGraph.Create("Display Animation Preview");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        clipPlayable = AnimationClipPlayable.Create(graph, clip);
        var output = AnimationPlayableOutput.Create(graph, "Animation", animator);
        output.SetSourcePlayable(clipPlayable);
        graph.Play();
    }

    private void OnDisable()
    {
        if (graph.IsValid())
            graph.Destroy();
        playingClip = null;
    }
}
