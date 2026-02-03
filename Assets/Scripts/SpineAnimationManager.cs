// 方案 A：共用骨骼 + 多 Skin，按皮肤名切换（图集在 SkeletonDataAsset 中按角色配置，支持按需加载）
using Spine;
using Spine.Unity;
using UnityEngine;

public class SpineAnimationManager : MonoBehaviour
{
    [Header("动画资源配置")]
    [Tooltip("共用骨骼动画数据（1 个 .skel，内含多套 Skin）")]
    public SkeletonDataAsset commonSkeletonData;

    [Header("皮肤配置")]
    [Tooltip("当前角色使用的皮肤名，与 Spine 中 Skin 名称一致")]
    public string skinName = "default";

    private SkeletonAnimation skeletonAnimation;

    void Awake()
    {
        InitializeCharacter();
    }

    void InitializeCharacter()
    {
        if (commonSkeletonData == null)
        {
            Debug.LogWarning("[SpineAnimationManager] commonSkeletonData is not assigned.");
            return;
        }

        skeletonAnimation = GetComponent<SkeletonAnimation>();
        if (skeletonAnimation == null)
            skeletonAnimation = gameObject.AddComponent<SkeletonAnimation>();

        skeletonAnimation.skeletonDataAsset = commonSkeletonData;
        skeletonAnimation.Initialize(false);

        ApplyCharacterSkin();
        // 初始化后立即设置 idle，避免 Inspector 显示 Animation Name 为 None
        PlayAnimation("idle", true);
        RefreshAfterSkinChange();
    }

    void Start()
    {
        ApplyCharacterSkin();
        RefreshAfterSkinChange();
        // 再次确保 idle 在播（解决运行时创建角色时 Animation Name 仍为 None 的问题）
        PlayAnimation("idle", true);
        RefreshAfterSkinChange();
    }

    /// <summary>运行时切换皮肤（供测试或 UI 切换角色用）</summary>
    public void SetSkin(string newSkinName)
    {
        skinName = newSkinName;
        ApplyCharacterSkin();
        // 与 SkeletonAnimation 的 initialSkinName 保持同步，避免渲染或后续逻辑仍用旧皮肤
        if (skeletonAnimation != null)
            skeletonAnimation.initialSkinName = newSkinName;
    }

    /// <summary>
    /// 方案 A：从同一 SkeletonData 中按名称取 Skin 并应用（无需合并图集）
    /// </summary>
    void ApplyCharacterSkin()
    {
        if (commonSkeletonData == null || string.IsNullOrEmpty(skinName))
            return;
        if (skeletonAnimation == null)
            return;

        var skeleton = skeletonAnimation.Skeleton;
        if (skeleton == null)
            return;

        var data = skeleton.Data;
        if (data == null) return;
        var skin = data.FindSkin(skinName);
        if (skin != null)
        {
            skeleton.SetSkin(skin);
            skeleton.SetSlotsToSetupPose();
            // 先应用当前动画到骨骼，再刷新 mesh，确保新皮肤的 slot 参与生成
            if (skeletonAnimation.AnimationState != null)
                skeletonAnimation.AnimationState.Apply(skeleton);
            skeletonAnimation.LateUpdateMesh();
        }
        else
            Debug.LogWarning($"[SpineAnimationManager] Skin not found: '{skinName}'");
    }

    /// <summary>设置皮肤后强制应用动画并刷新 mesh，否则可能只显示轮廓不显示贴图</summary>
    void RefreshAfterSkinChange()
    {
        if (skeletonAnimation == null) return;
        var skeleton = skeletonAnimation.Skeleton;
        var state = skeletonAnimation.AnimationState;
        if (skeleton != null && state != null)
        {
            state.Apply(skeleton);
            skeletonAnimation.LateUpdateMesh();
        }
    }

    /// <summary>播放共用动画</summary>
    public void PlayAnimation(string animationName, bool loop = true, float timeScale = 1f)
    {
        if (skeletonAnimation == null) return;
        var state = skeletonAnimation.AnimationState;
        if (state != null)
        {
            var entry = state.SetAnimation(0, animationName, loop);
            if (entry != null) entry.TimeScale = timeScale;
        }
    }

    /// <summary>动画混合（如从走路到跑步的平滑过渡）</summary>
    public void CrossFadeAnimation(string toAnimation, float fadeDuration = 0.3f)
    {
        if (skeletonAnimation == null) return;
        var state = skeletonAnimation.AnimationState;
        if (state != null)
        {
            var entry = state.SetAnimation(0, toAnimation, true);
            if (entry != null) entry.MixDuration = fadeDuration;
        }
    }

    /// <summary>获取当前动画时长（用于等待动画结束等逻辑）</summary>
    public float GetAnimationDuration(string animationName)
    {
        if (skeletonAnimation == null) return 0f;
        var skeleton = skeletonAnimation.Skeleton;
        if (skeleton == null || skeleton.Data == null) return 0f;
        var anim = skeleton.Data.FindAnimation(animationName);
        return anim != null ? anim.Duration : 0f;
    }
}
