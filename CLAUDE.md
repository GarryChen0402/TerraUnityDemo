# CLAUDE.md

本文件为 Claude Code (claude.ai/code) 在此仓库中工作时提供指导。

## 项目概览

TerraUnity Demo — 一个 2D 类泰拉瑞亚沙盒游戏，使用 **Unity 2022.3.62f3** 构建，基于内置 2D Tilemap 系统和 TextMeshPro 实现 UI。所有游戏代码位于 `Assets/Scripts/` 目录下。

## 构建与开发

```bash
# 在 Unity Editor 中打开项目（未配置 CLI 构建）
# 在 IDE 中打开 TerraUnityDemo.sln 进行 C# 编辑
```

没有 CI/CD、测试运行器或 CLI 构建流水线。Unity Test Framework 包（`com.unity.test-framework`）已安装但尚未编写测试。所有开发工作均在 Unity Editor 中进行——打开 `Assets/Scenes/Default.unity` 作为主场景（也是唯一场景）。

## 架构

### 世界系统

世界采用**基于区块的 2D tilemap**。`ChunkManager`（单例，挂载在 `WorldManager` GameObject 上）维护一个 `Dictionary<Vector2Int, ChunkData>`，每个区块为 16×16。玩家周围 `activeChunkRadius` 范围内的区块会被渲染到单个 `Tilemap_Ground` Tilemap 上；超出范围的区块从 tilemap 中清除但保留在内存中。

`WorldGenerator` 是一个**静态类**（同时也是 MonoBehaviour，挂载在同一 GameObject 上以获取 Inspector 参数）。`FillChunk()` 使用柏林噪声放置草地（地表）、泥土（地表下 0-5 格）和石头（更深层），并生成洞穴空洞。Tile ID 映射到 `ChunkManager` 上的 `TileBase[] tileAssets` 数组。

`TileData`（ScriptableObject）保存每种 Tile 的属性：硬度、所需挖掘等级、掉落物。`TileDataManager` 通过序列化列表建立 `TileBase → TileData` 的映射。

### 玩家系统

全部挂载在 Player GameObject（Tag 为 `"Player"`）上：

| 脚本 | 职责 |
|---|---|
| `PlayerController` | 2D 移动、土狼时间、跳跃缓冲、可变高度跳跃 |
| `PlayerCombat` | 鼠标左键近战攻击（OverlapCircle 检测）、暴击系统 |
| `PlayerStats` | HP/MP 及回复、无敌帧、伤害事件 |
| `PlayerInventory` | 单例，`List<InventorySlot>`，最多 40 格，堆叠 |
| `PlayerEquipment` | 单例，武器/工具/护甲槽位，提供挖掘力/挖掘等级 |
| `PlayerCurrency` | 单例，铜币/银币/金币/铂金币多级货币体系 |
| `TileInteraction` | 按住鼠标左键挖掘 Tile，带进度条 |
| `StatusEffectManager` | 单例，持续伤害/增益效果（中毒/燃烧/减速/回复） |

### 物品与合成系统（ScriptableObject）

- `ItemData` — 定义物品，包含 `itemID`、类型（`ItemType` 枚举）、稀有度、图标、堆叠上限、硬币价值和工具/武器属性。
- `CraftingRecipe` — 引用 `ItemData` 作为材料和产出，以及所需 `CraftingStationType`。`CanCraft()` 检查 `PlayerInventory.GetItemCount()`。`Craft()` 消耗材料并添加产出。
- `CraftingStation` — 放置在世界中，拥有 `CraftingStationType`（Hand/Workbench/Furnace/Anvil）。`CraftingUI` 根据范围内最近的工作站过滤配方。

所有 ScriptableObject 资源存放在 `Assets/Sources/ScriptObjects/` 下。

### UI 构建模式

所有主要 UI 面板（`InventoryUI`、`CraftingUI`、`ShopUI`、`DebugItemPanel`）**完全通过代码构建**——不使用 Prefab。它们调用 `FindObjectOfType<Canvas>()` 并以编程方式实例化 GameObject，包括 RectTransform、Image、TextMeshProUGUI 文本、Button 以及 ScrollRect/GridLayoutGroup 布局。如果 Canvas 上已存在匹配的子对象，则复用它；否则从零创建。这意味着修改 UI 布局需要编辑 C# 代码而非 Unity 场景。

快捷键：**B** = 背包，**C** = 合成，**E** = 与 NPC 交互，**F1** = 调试物品面板，**F5** = 手动存档（槽位 1），**F9** = 读档（优先级 1→2→3→0），**M** = 全屏地图，**U** = 召唤 NPC。

### 敌人系统

`EnemyBase` 是一个抽象类，包含状态机（`Idle/Patrol/Chase/Attack/Hurt/Dead`）。子类实现 `OnPatrol()`、`OnChase()`、`OnAttack()`。基类处理基于玩家距离的状态转换、击退、受击闪烁协程、死亡及掉落物。目前唯一的具象敌人是 `SlimeEnemy`，通过跳跃移动。

### NPC 与房间系统

`NPC` — 漫游（左右移动，定时转向），玩家靠近时高亮显示，按 E 打开商店（委托给 `ShopUI.Instance.OpenShop(this)`）。夜间则显示警告消息。

`RoomDetector` — 从玩家所在格子使用洪水填充算法寻找封闭区域（四周有墙、未通向地表）。验证最小尺寸（6×10）和必要的家具 Tile（门、桌子、椅子、光源），家具放置在 `Tilemap_Furniture` 上。按 **U** 消耗 NPC 召唤令牌，在有效房间中生成 NPC。

`ShopData`（ScriptableObject）— 包含 `ShopItemEntry` 列表（物品 + 库存数量，-1 = 无限）和出售价格倍率。

### 存档系统

`SaveManager`（单例）使用 `JsonUtility` 序列化 `SaveData` 到 `Application.persistentDataPath/saves/save_slot_{n}.json`。

- **自动存档**：每 300 秒自动保存到槽位 0。
- **手动存档**：按 **F5** 保存到槽位 1。
- **读档**：按 **F9** 按优先级 1→2→3→0 查找存档并加载。
- **物品数据库**：`BuildItemDatabase()` 从 `Resources.LoadAll<ItemData>()` 构建 `Dictionary<int, ItemData>`，用于读档时通过 itemID 恢复物品引用。
- **死亡敌人追踪**：`deadEnemyKeys` 集合存储 `EnemyBase.DeathKey`（格式：`"{TypeName}|{spawnX:F1}|{spawnY:F1}"`），读档时销毁匹配的敌人对象。
- **存档内容**：世界种子、昼夜时间、已修改区块的 tile 数据、玩家位置/属性/货币/背包/装备、NPC 状态（位置、漫游方向、夜间消息标记）、死亡敌人列表。
- **加载流程**：先设置玩家位置 → 清空并重新生成区块 → 应用已修改区块的 tile 覆盖 → 恢复玩家属性/货币/背包/装备 → 恢复 NPC 状态 → 销毁已死亡敌人。

### 小地图系统

`Minimap`（单例）使用 CPU 端纹理渲染实现**战争迷雾**和**小地图/全屏地图**。按 **M** 切换全屏地图（关闭时显示角落地图）。

- **坐标系**：1 纹理像素 = 10 世界单位，迷雾纹理 420×120（覆盖 4200×1200 世界）。
- **颜色映射**：`tileID` -1 = 空气（深色），0 = 草地（绿），1 = 泥土（棕），2 = 石头（灰）；未探索区域显示为深蓝色。
- **小地图**：150×150 的 RawImage，固定在右上角，以玩家为中心显示 `minimapViewRadius`（默认 100）范围内的区域。
- **全屏地图**：600×380 面板，悬浮在屏幕中央，显示整个探索过的世界。包含图例和"显示敌人"切换按钮。敌人可以在全屏地图中隐藏以仅查看地形。
- **实体标记**：玩家（青色十字）、NPC（绿色点）、敌人（红色点）——每帧从 `FindObjectsOfType` 动态绘制。
- **UI 构建**：完全通过代码构建（Canvas 子对象），与其他 UI 面板风格一致。

### 敌人刷新

`EnemySpawner` 在摄像机视野外的地表上生成史莱姆。通过从高空向下扫描 `Tilemap_Ground` 查找地表 Y 坐标，确保生成位置在 Tile 上方且未被阻挡。受 `maxEnemies`（默认 10）和 `spawnInterval`（默认 5 秒）限制。

### 掉落物

`ItemDrop` 在世界中以 SpriteRenderer 形式存在，显示物品图标。0.5 秒延迟后启用拾取，玩家靠近时磁吸移动（`pickupRange * 3` 范围内），接触时自动加入背包。同时处理硬币价值和物品入包，支持部分拾取（背包满时剩余数量留在地上）。

### 状态效果

`StatusEffect` 是一个可序列化类（非 MonoBehaviour），子类通过重写 `OnTick()` 实现具体效果：
- `PoisonEffect`：10 秒，每秒造成 2 点伤害
- `BurnEffect`：5 秒，每 0.5 秒造成 5 点伤害
- `SlowEffect`：3 秒，仅改变移动速度
- `RegenEffect`：8 秒，每秒回复 2 点 HP

`StatusEffectManager`（单例，挂载在 Player 上）维护 `List<StatusEffect>`，每帧调用 `Update()` 并清理过期效果。

### 昼夜循环

`DayNightCycle` 在 `dayDurationSeconds`（默认 300 秒）内将 `TimeOfDay` 从 0 循环到 1。驱动 `Camera.main.backgroundColor` 在白天/夜晚颜色之间过渡，并控制全屏黑色 `nightOverlay` Image 的透明度。夜晚定义为 `TimeOfDay < 0.2 || TimeOfDay > 0.8`。

### 设置自动化

- **编辑器**：`Tools > Setup Week 10 - Complete` 菜单项（位于 `Assets/Editor/Week10Setup.cs`）创建所有必需的场景对象、ScriptableObject 资源、家具 Tile 和 NPC Prefab。
- **运行时**：`RuntimeWeek10Setup`（标记了 `[RuntimeInitializeOnLoadMethod]`）在首次场景加载时通过反射设置私有序列化字段，自动创建缺失的管理器（DayNightCycle、RoomDetector、ShopUI、SaveManager、EnemySpawner、Minimap、家具 Tilemap）。

### 单例模式

几乎所有管理器类都通过在 `Awake()` 中设置静态 `Instance` 属性使用 Unity 单例模式，并包含重复销毁逻辑。主要单例：`ChunkManager`、`PlayerInventory`、`PlayerEquipment`、`PlayerCurrency`、`PlayerStats`、`StatusEffectManager`、`TileDataManager`、`CraftingUI`、`InventoryUI`、`ShopUI`、`DayNightCycle`、`RoomDetector`、`SaveManager`、`Minimap`。

## 关键包

- `com.unity.feature.2d` — 2D 精灵/Tilemap 支持
- `com.unity.textmeshpro` — UI 文本渲染
- `com.unity.cinemachine` — 摄像机控制（已安装但实际使用情况不明）
- `com.unity.test-framework` — 测试运行器（尚未编写测试）
- `com.unity.ugui` — Unity UI（Canvas、Image、Button、ScrollRect 等）
