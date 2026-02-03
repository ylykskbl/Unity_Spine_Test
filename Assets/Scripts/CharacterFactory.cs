// 方案 A：使用共用 SkeletonData + 皮肤名创建角色（图集按角色分，在 SkeletonDataAsset 中配置）
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class CharacterFactory : MonoBehaviour
{
    [System.Serializable]
    public class CharacterConfig
    {
        [Tooltip("角色唯一 ID，用于 CreateCharacter(characterId, position)")]
        public string characterId;
        [Tooltip("Spine 中该角色对应的 Skin 名称")]
        public string skinName;
        [Tooltip("角色缩放")]
        public Vector3 scale = Vector3.one;
    }

    [Header("共用资源")]
    [Tooltip("共用骨骼动画数据（1 个 .skel，内含多套 Skin；图集在 Inspector 中按角色配置，支持按需加载）")]
    public SkeletonDataAsset commonSkeletonData;

    [Header("角色配置")]
    public List<CharacterConfig> characterConfigs = new List<CharacterConfig>
    {
        new CharacterConfig { characterId = "1", skinName = "1", scale = Vector3.one },
        new CharacterConfig { characterId = "2", skinName = "2", scale = Vector3.one },
        new CharacterConfig { characterId = "3", skinName = "3", scale = Vector3.one },
        new CharacterConfig { characterId = "4", skinName = "4", scale = Vector3.one },
        new CharacterConfig { characterId = "5", skinName = "5", scale = Vector3.one },
        new CharacterConfig { characterId = "6", skinName = "6", scale = Vector3.one },
        new CharacterConfig { characterId = "7", skinName = "7", scale = Vector3.one },
    };

    /// <summary>根据 characterId 创建使用共用动画、指定皮肤的角色</summary>
    public GameObject CreateCharacter(string characterId, Vector3 position)
    {
        var config = characterConfigs.Find(c => c.characterId == characterId);
        if (config == null)
        {
            Debug.LogError($"[CharacterFactory] Character config not found: {characterId}");
            return null;
        }

        if (commonSkeletonData == null)
        {
            Debug.LogError("[CharacterFactory] commonSkeletonData is not assigned.");
            return null;
        }

        var character = new GameObject($"Character_{characterId}");
        character.transform.position = position;
        character.transform.localScale = config.scale;

        var skeletonAnimation = character.AddComponent<SkeletonAnimation>();
        skeletonAnimation.skeletonDataAsset = commonSkeletonData;
        skeletonAnimation.initialSkinName = config.skinName;  // 必须在 Initialize 之前设置，否则会显示 Default
        skeletonAnimation.loop = true;   // 必须先设 loop，再设 AnimationName，否则 setter 里 SetAnimation 会用默认的 false
        skeletonAnimation.AnimationName = "idle";   // 使用官方 API，Inspector 会显示 idle 且会实际播放

        var animManager = character.AddComponent<SpineAnimationManager>();
        animManager.commonSkeletonData = commonSkeletonData;
        animManager.skinName = config.skinName;

        var controller = character.AddComponent<CharacterController2D>();
        controller.animationManager = animManager;

        return character;
    }
}
