# Spine动画复用方案

## 实施说明

- **思路**：1 个 .skel（1 套骨骼 + 1 套动画 + N 个 Skin）+ 1 个 .atlas（多页，每页对应一个角色 Skin），通过 On Demand Texture Loader 实现按页按需加载。
- **Spine 导出**：在一个 Spine 项目里做好 1 套骨骼与动画，为每个角色建一个 Skin，所有 Skin 共用同一个 Atlas（该 Atlas 包含多页，每页对应一个 Skin 的纹理）。导出一个 .skel + 1 个 .atlas（包含多页 PNG）。
- **Unity**：一个 SkeletonDataAsset 引用该 .skel 和该 Atlas（单 Atlas 多页）。若使用 On Demand Texture Loader，可做到只加载当前显示皮肤对应的那一页。运行时用 `SetSkin(skinName)` 切换角色外观。
- **脚本位置**：`Assets/Scripts/SpineAnimationManager.cs`、`CharacterFactory.cs`、`CharacterController2D.cs`。需安装 [Spine-Unity Runtime](https://esotericsoftware.com/spine-unity)。

**Spine-Unity 安装说明（若 Package Manager 用 Git URL 安装失败或导致 Unity 卡死）**

请用 **.unitypackage** 方式安装，不要改 `Packages/manifest.json` 用 Git URL（大仓库会导致 Unity 解析时卡死）。

1. **下载** spine-unity 4.2 的 .unitypackage（任选其一）：
   - 官方下载页：[spine-unity Download](https://en.esotericsoftware.com/spine-unity-download)
   - 4.2 直链：`https://esotericsoftware.com/files/runtimes/unity/spine-unity-4.2-2025-12-10.unitypackage`
2. **导入**：在 Unity 中双击下载好的 `.unitypackage`，或在 Project 面板里把该文件拖入，在弹出窗口中点 **Import**（或 **Import All**）。
3. 导入完成后，Project 里会出现 `Assets/Spine` 等目录，脚本即可正常引用 Spine-Unity。
4. **URP 项目**（你当前是 URP）：如需 Spine 的 URP 着色器，再单独下载 URP Shaders 包：[spine.urp-shaders 4.2](https://en.esotericsoftware.com/files/runtimes/unity/com.esotericsoftware.spine.urp-shaders-4.2-Unity2019.3-2025-12-09.zip)，解压后把包内容放到项目 `Packages` 目录，或在 Package Manager 里选「Add package from disk...」指向解压出的 `package.json`。

**使用步骤（你完成 Spine 导出后）**

1. 在 Spine 中：一个项目，1 套骨骼 + 1 套动画 + 3 个 Skin（如 default、monster1、monster2），所有 Skin 共用同一个 Atlas（该 Atlas 包含多页，每页对应一个 Skin 的纹理）；导出一个 .skel + 1 个 .atlas（包含多页 PNG，每页对应一个 Skin）。
2. 在 Unity 中：导入 .skel 与图集，创建 SkeletonDataAsset，绑定该 .skel 和该 Atlas（单 Atlas 多页）。若使用 On Demand Texture Loader，可做到只加载当前显示皮肤对应的那一页。
3. 场景中放一个空物体，挂 CharacterFactory，指定 commonSkeletonData，在 characterConfigs 里填 3 条：characterId（如 "monster1"）、skinName（与 Spine 中 Skin 名一致）、scale。
4. 代码中调用 `CharacterFactory.CreateCharacter("monster1", position)` 即可生成对应皮肤的角色。

---

## 一、基础架构设计

### 1. 资源组织结构

```
Resources/
├── SpineAnimations/
│   ├── SLGMonster(动画).skel.bytes    # 共用骨骼 + 动画 + 多套 Skin 定义
│   ├── SLGMonster(动画).atlas.txt     # 单 Atlas，包含多页（8 个 Material/PNG）
│   ├── SLGMonster(动画).png           # 第 0 页
│   ├── SLGMonster(动画)_2.png         # 第 1 页（Skin "1"）
│   ├── SLGMonster(动画)_3.png         # 第 2 页（Skin "2"）
│   └── ...
```

### 2. 核心实现方案

- 一个 SkeletonDataAsset：绑定上述 .skel 和该 Atlas（单 Atlas 多页）。若使用 On Demand Texture Loader，可做到只加载当前显示皮肤对应的那一页。
- 每个角色实例：共用该 SkeletonDataAsset，通过 `skinName` 指定要用的 Skin。

## 二、动画控制器与状态机

```csharp
// 共用骨骼 + 多 Skin，按皮肤名切换（实现见 Assets/Scripts/SpineAnimationManager.cs）

void ApplyCharacterSkin()
{
    if (commonSkeletonData == null || string.IsNullOrEmpty(skinName)) return;
    if (skeletonAnimation == null) return;

    var skeleton = skeletonAnimation.Skeleton;
    if (skeleton == null) return;

    var data = skeleton.Data;
    if (data == null) return;
    var skin = data.FindSkin(skinName);  // 从同一 SkeletonData 中按名称取 Skin
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

/// <summary>运行时切换皮肤（供测试或 UI 切换角色用）</summary>
public void SetSkin(string newSkinName)
{
    skinName = newSkinName;
    ApplyCharacterSkin();
    // 与 SkeletonAnimation 的 initialSkinName 保持同步
    if (skeletonAnimation != null)
        skeletonAnimation.initialSkinName = newSkinName;
}

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

public float GetAnimationDuration(string animationName)
{
    if (skeletonAnimation == null) return 0f;
    var skeleton = skeletonAnimation.Skeleton;
    if (skeleton == null || skeleton.Data == null) return 0f;
    var anim = skeleton.Data.FindAnimation(animationName);
    return anim != null ? anim.Duration : 0f;
}
```

## 三、角色工厂模式

- 配置中只填 `characterId`、`skinName`、`scale`；图集使用同一个 Atlas（多页），通过 On Demand Texture Loader 实现按页按需加载。
- 实现见 `Assets/Scripts/CharacterFactory.cs`。

```csharp
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
    skeletonAnimation.initialSkinName = config.skinName;  // 必须在 Initialize 之前设置
    skeletonAnimation.loop = true;   // 必须先设 loop，再设 AnimationName
    skeletonAnimation.AnimationName = "idle";   // 使用官方 API，Inspector 会显示 idle 且会实际播放

    var animManager = character.AddComponent<SpineAnimationManager>();
    animManager.commonSkeletonData = commonSkeletonData;
    animManager.skinName = config.skinName;

    var controller = character.AddComponent<CharacterController2D>();
    controller.animationManager = animManager;

    return character;
}
```

## 四、2D角色控制器示例

```csharp
// 控制角色移动和动画状态
public class CharacterController2D : MonoBehaviour
{
    public SpineAnimationManager animationManager;
    public float moveSpeed = 5f;
    
    private Vector2 movement;
    private bool isGrounded = true;
    private bool isDead = false;
    
    void Update()
    {
        if (isDead) return;
        
        HandleInput();
        UpdateAnimationState();
    }
    
    void HandleInput()
    {
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");
        
        // 跳跃检测
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            PlayJumpAnimation();
        }
    }
    
    void UpdateAnimationState()
    {
        if (animationManager == null) return;

        if (movement.magnitude > 0.1f)
        {
            // 移动中
            if (Mathf.Abs(movement.x) > 0)
            {
                // 转向
                var ls = transform.localScale;
                transform.localScale = new Vector3(
                    movement.x > 0 ? Mathf.Abs(ls.x) : -Mathf.Abs(ls.x),
                    ls.y,
                    ls.z
                );
            }
            
            // 选择移动动画
            if (Input.GetKey(KeyCode.LeftShift))
            {
                animationManager.CrossFadeAnimation("run", 0.2f);
            }
            else
            {
                animationManager.CrossFadeAnimation("walk", 0.2f);
            }
        }
        else
        {
            // idle动画
            animationManager.CrossFadeAnimation("idle", 0.2f);
        }
    }
    
    void PlayJumpAnimation()
    {
        if (animationManager == null) return;
        animationManager.PlayAnimation("jump", false);
        StartCoroutine(WaitForAnimationComplete("jump", () =>
        {
            if (animationManager != null)
                animationManager.CrossFadeAnimation("idle", 0.2f);
        }));
    }
    
    public void PlayDeathAnimation()
    {
        if (animationManager == null) return;
        isDead = true;
        animationManager.PlayAnimation("death", false);
        StartCoroutine(WaitForAnimationComplete("death", () => gameObject.SetActive(false)));
    }
    
    IEnumerator WaitForAnimationComplete(string animName, System.Action onComplete)
    {
        float duration = animationManager != null ? animationManager.GetAnimationDuration(animName) : 1f;
        if (duration <= 0f) duration = 1f;  // 兜底
        yield return new WaitForSeconds(duration);
        onComplete?.Invoke();
    }
}
```

## 五、高级复用技巧

### 1. 动画事件系统

```csharp
// 动画事件处理器
public class SpineAnimationEventHandler : MonoBehaviour
{
    public void RegisterAnimationEvents(SkeletonAnimation skeletonAnim)
    {
        skeletonAnim.AnimationState.Event += OnAnimationEvent;
    }
    
    void OnAnimationEvent(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        switch (e.Data.Name)
        {
            case "footstep":
                PlayFootstepSound();
                break;
            case "attack_hit":
                ApplyAttackDamage();
                break;
            case "effect_spawn":
                SpawnVisualEffect(e.String);
                break;
        }
    }
}
```

### 2. 动画层级系统

```csharp
// 实现上下半身独立动画
public class AnimationLayeringSystem : MonoBehaviour
{
    private SkeletonAnimation skeletonAnim;
    
    void Start()
    {
        skeletonAnim = GetComponent<SkeletonAnimation>();
        
        // 下半身：基础移动动画 (track 0)
        skeletonAnim.AnimationState.SetAnimation(0, "walk", true);
        
        // 上半身：攻击动画 (track 1，可混合)
        skeletonAnim.AnimationState.SetAnimation(1, "idle_upper", true);
    }
    
    public void PlayUpperBodyAttack()
    {
        // 只在上半身轨道播放攻击动画
        skeletonAnim.AnimationState.SetAnimation(1, "attack", false);
        skeletonAnim.AnimationState.AddAnimation(1, "idle_upper", true, 0);
    }
}
```

## 六、Spine编辑器设置要点

### 骨骼标准化：

- 所有角色使用相同的骨骼命名规范
- 保持骨骼层级结构一致
- 使用相同的插槽（Slot）名称

### 动画制作规范：

- 所有动画名称标准化（如：walk、run、jump、death）
- 确保动画长度和循环设置一致
- 使用相同的动画事件名称

### 皮肤系统使用：

- 为每个角色创建单独的皮肤（Skin）
- 确保皮肤中的附件（Attachment）命名一致

## 七、性能优化建议

### 资源管理：

```csharp
// 预加载共用动画
void PreloadAnimations()
{
    commonSkeletonData.GetSkeletonData(true);
}

// 对象池重用
public class CharacterPool : MonoBehaviour
{
    private Queue<GameObject> characterPool = new Queue<GameObject>();
    
    public GameObject GetCharacter(string characterId)
    {
        if (characterPool.Count > 0)
        {
            var character = characterPool.Dequeue();
            character.SetActive(true);
            return character;
        }
        return CreateNewCharacter(characterId);
    }
}
```

### 内存优化：

- 使用纹理图集减少Draw Call
- 实现按需加载皮肤资源
- 使用AssetBundle分发角色资源

### 图集内存与 On Demand 按需加载

**当前项目结构：单 Atlas 多页**

- 本项目采用「单 Atlas 多页」方案：一个 `SLGMonster(动画).atlas.txt` 包含多页纹理（例如 8 个 Material 对应 8 张 PNG：`SLGMonster(动画).png`、`_2`…`_8`），所有 Skin 共用这一个 Atlas。

**默认（Normal）模式下的内存行为**

- 只要该 Atlas 被 SkeletonDataAsset 引用，在**首次需要骨骼数据时**（如创建角色、调用 `GetSkeletonData()`），Spine-Unity 会加载该 Atlas 关联的**所有** Material 及其 `mainTexture`。
- 因此：**即使当前只显示 1 个皮肤**（例如只用 `SLGMonster(动画)_2` 这一页），**其它皮肤对应的图片（_3、_4、…、_8 及第 0 页）也会一起被加载进内存**。无法在「单 Atlas 多页」的前提下仅加载当前皮肤那一页，除非使用 On Demand。

**On Demand 模式（按需加载）**

- 将 Atlas 的 **Texture Loading Mode** 设为 **On Demand**，并为其指定一个 **On Demand Texture Loader**（继承 `Spine.Unity.OnDemandTextureLoader` 的 ScriptableObject）。
- 运行时只有在**首次渲染用到某一页**时，才会通过 Loader 加载该页对应的纹理，其它页不预先加载，从而**降低内存占用**。
- 本项目已实现：
  - 脚本：`Assets/Scripts/SLGMonsterOnDemandLoader.cs`（按页从 `Resources/SpineAnimations/` 加载）。
  - 在 Unity 菜单执行一次 **Spine > Create SLG Monster On Demand Loader**，会创建 Loader 资产并绑定到 `SLGMonster(动画)_Atlas`，同时将 Atlas 的 Texture Loading Mode 设为 On Demand。
  - Loader 的 **Skeleton Data Asset** 可留空；仅在使用 blend mode 材质且未挂在 Atlas 时才需填写。

## 八、扩展功能

### 1. 换装系统

```csharp
public class EquipmentSystem : MonoBehaviour
{
    public void ChangeEquipment(string slotName, string attachmentName)
    {
        var skeleton = GetComponent<SkeletonAnimation>().Skeleton;
        var slot = skeleton.FindSlot(slotName);
        var attachment = skeleton.GetAttachment(slotName, attachmentName);
        slot.Attachment = attachment;
    }
}
```

### 2. 动画混合树

```csharp
// 类似Animator Controller的状态机
public class SpineAnimationBlendTree
{
    public void BlendMovement(float speed, float direction)
    {
        // 根据参数混合walk、run等动画
    }
}
```
