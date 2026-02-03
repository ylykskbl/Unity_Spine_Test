# Unity Spine 动画复用测试项目

Unity中Spine资源的动画复用方案

## 项目简介

这是一个 Unity 项目，用于测试和演示 Spine 动画的复用方案。通过共享骨骼和动画数据，使用不同的 Skin 来实现多个角色的外观切换，从而减少内存占用和提高资源复用率。

## 环境要求

- **Unity 版本**: 2022.3.61f1
- **渲染管线**: URP (Universal Render Pipeline)
- **依赖**: Spine-Unity Runtime 4.2

## 项目结构

```
Assets/
├── Scripts/                    # 核心脚本
│   ├── SpineAnimationManager.cs      # Spine 动画管理器
│   ├── CharacterFactory.cs           # 角色工厂（创建角色实例）
│   ├── CharacterController2D.cs      # 2D 角色控制器
│   ├── SpineTestSpawner.cs           # 测试生成器
│   └── SLGMonsterOnDemandLoader.cs   # On Demand 纹理加载器
├── Resources/
│   └── SpineAnimations/       # Spine 资源文件
│       ├── SLGMonster(动画).skel.bytes    # 骨骼和动画数据
│       ├── SLGMonster(动画).atlas.txt     # 图集定义（多页）
│       └── SLGMonster(动画).png           # 纹理图集（多页）
├── Scenes/
│   └── Main.unity             # 主场景
└── Editor/
    └── CreateSLGMonsterOnDemandLoader.cs  # 编辑器工具
```

## 核心特性

### 1. 动画复用方案
- **单骨骼多皮肤**: 一个 `.skel` 文件包含所有角色的骨骼和动画数据
- **多页图集**: 一个 `.atlas` 文件包含多个角色的纹理（每页对应一个 Skin）
- **按需加载**: 使用 On Demand Texture Loader 实现纹理的按需加载，降低内存占用

### 2. 角色工厂模式
通过 `CharacterFactory` 组件配置多个角色，运行时动态创建：
- 配置角色 ID、Skin 名称和缩放比例
- 调用 `CreateCharacter(characterId, position)` 创建角色实例

### 3. 动画管理
- 支持播放、切换、淡入淡出动画
- 支持动画事件系统
- 支持动画层级（上下半身独立动画）

## 安装步骤

### 1. 安装 Spine-Unity Runtime

**重要**: 请使用 `.unitypackage` 方式安装，不要使用 Git URL（大仓库会导致 Unity 解析时卡死）。

1. 下载 spine-unity 4.2 的 `.unitypackage`:
   - 官方下载页: [spine-unity Download](https://en.esotericsoftware.com/spine-unity-download)
   - 4.2 直链: `https://esotericsoftware.com/files/runtimes/unity/spine-unity-4.2-2025-12-10.unitypackage`

2. 在 Unity 中导入:
   - 双击下载好的 `.unitypackage` 文件
   - 或在 Project 面板中拖入文件
   - 点击 **Import** 或 **Import All**

3. URP 着色器（可选）:
   - 下载 URP Shaders 包: [spine.urp-shaders 4.2](https://en.esotericsoftware.com/files/runtimes/unity/com.esotericsoftware.spine.urp-shaders-4.2-Unity2019.3-2025-12-09.zip)
   - 解压后通过 Package Manager 的 "Add package from disk..." 安装

### 2. 设置 On Demand Texture Loader

1. 在 Unity 菜单栏执行: **Spine > Create SLG Monster On Demand Loader**
2. 这会自动创建 Loader 资产并绑定到 Atlas

### 3. 配置场景

1. 打开 `Assets/Scenes/Main.unity`
2. 场景中应有一个空物体挂载了 `CharacterFactory` 组件
3. 在 Inspector 中配置 `commonSkeletonData` 和 `characterConfigs`

## 使用方法

### 创建角色

```csharp
// 通过 CharacterFactory 创建角色
CharacterFactory factory = FindObjectOfType<CharacterFactory>();
GameObject character = factory.CreateCharacter("monster1", new Vector3(0, 0, 0));
```

### 切换皮肤

```csharp
SpineAnimationManager animManager = character.GetComponent<SpineAnimationManager>();
animManager.SetSkin("monster2");
```

### 播放动画

```csharp
// 播放动画
animManager.PlayAnimation("walk", loop: true);

// 淡入淡出切换动画
animManager.CrossFadeAnimation("run", fadeDuration: 0.3f);
```

## 技术文档

详细的技术实现方案请参考: [Spine动画复用方案.md](Spine动画复用方案.md)

## 注意事项

1. **资源文件**: 项目中的 Spine 资源文件（`.skel.bytes`、`.atlas.txt`、`.png`）已包含在仓库中
2. **Unity 版本**: 请使用 Unity 2022.3.61f1，其他版本可能存在兼容性问题
3. **内存优化**: 项目已配置 On Demand Texture Loader，运行时只加载当前使用的纹理页

## 许可证

本项目仅供学习和测试使用。

## 贡献

欢迎提交 Issue 和 Pull Request！
