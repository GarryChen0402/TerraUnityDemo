# Terra Unity — 开发进度文档

> **更新日期**: 2026-05-19  
> **当前状态**: 第 1-2 阶段已基本完成（Week 1-10），第 3 阶段部分完成（Week 13 存档系统已实现）  
> **参考文档**: Terra_Unity_DevPlan.md、Terra_Unity_GDD.md

---

## 总览

| 阶段 | 周期 | 状态 | 完成度 |
|------|------|------|--------|
| 前置准备 | 开始前 | ✅ 完成 | 100% |
| 第 0 阶段 | 第 1-2 天 | ✅ 完成 | 100% |
| 第 1 阶段：核心原型 | Week 1-4 | ✅ 完成 | 100% |
| 第 2 阶段：内容系统 | Week 5-10 | ✅ 完成 | 100% |
| 第 3 阶段：进阶内容 | Week 11-14 | 🔄 进行中 | ~25% |
| 第 4 阶段：打磨 | Week 15-16 | ❌ 未开始 | 0% |

**整体完成度：约 69%（11/16 周）**

---

## 逐周进度详情

### Week 1：Unity 基础 + Tilemap ✅ 完成

| 计划任务 | 状态 | 实际实现 |
|----------|------|----------|
| Unity 2022.3 LTS 安装 | ✅ | 2022.3.62f3 |
| 2D Core 模板创建项目 | ✅ | `TerraUnityDemo` |
| Tilemap 绘制地形 | ✅ | `Tilemap_Ground` + Tilemap Collider 2D |
| 玩家角色显示与基础移动 | ✅ | `PlayerController.cs`（含完整移动+跳跃） |
| 地面碰撞检测 | ✅ | Layer "Ground" + `OnCollisionEnter2D/Exit2D` |

**备注**: 实际的 `PlayerController` 已包含 Week 4 的 Coyote Time 和跳跃缓冲，属于合并实现。地面检测改为 `Physics2D.OverlapCircle` + `GroundCheck` 子对象的方式，更精确。

---

### Week 2：摄像机跟随 + 挖掘基础 ✅ 完成

| 计划任务 | 状态 | 实际实现 |
|----------|------|----------|
| Cinemachine 安装与配置 | ✅ | `com.unity.cinemachine` 2.10.7 已安装 |
| TileData ScriptableObject | ✅ | `Assets/Scripts/Tiles/TileData.cs` |
| 挖掘进度条 | ✅ | `TileInteraction.cs`，Slider 进度条 |
| 挖掘距离限制 | ✅ | `mineRange = 3f` |
| 工具等级检查 | ✅ | `requiredMiningLevel`，从 `PlayerEquipment` 读取 |
| 挖掘掉落 | ✅ | `itemDropPrefab` 生成 `ItemDrop` |

**备注**: `TileDataManager` 作为单例管理 `TileBase → TileData` 映射。挖掘力由 `PlayerEquipment.GetMiningPower()/GetMiningLevel()` 动态提供，而非 Inspector 手动设置，更灵活。

---

### Week 3：程序化世界生成 + Chunk 系统 ✅ 完成

| 计划任务 | 状态 | 实际实现 |
|----------|------|----------|
| Perlin Noise 地形生成 | ✅ | `WorldGenerator.FillChunk()`，使用 `Mathf.PerlinNoise` |
| Chunk 数据结构（16×16） | ✅ | `ChunkData.cs`，`int[,] tileIDs` |
| ChunkManager 动态加载/卸载 | ✅ | `ChunkManager.cs`，`activeChunkRadius = 2` |
| 世界规格 4200×1200 | ✅ | `worldWidth = 4200`, `worldHeight = 1200` |
| 洞穴生成 | ✅ | `IsCaveAt()`，双层 Perlin Noise 叠加 |
| 五层地形结构 | ✅ | 空气(-1)/草地(0)/泥土(1)/石头(2)/洞穴 |
| 随机种子 | ✅ | `worldSeed = Random.Range(0, 999999)` |

**备注**: 与 DevPlan 设计高度一致。`UpdateActiveChunks()` 仅在玩家移动到新 Chunk 时触发更新，避免每帧重建。Chunk 数据在内存中保留，仅从 Tilemap 中清除/绘制。

---

### Week 4：完善玩家控制 + Coyote Time ✅ 完成

| 计划任务 | 状态 | 实际实现 |
|----------|------|----------|
| Coyote Time（0.15s） | ✅ | `coyoteTime = 0.15f` |
| Jump Buffer（0.1s） | ✅ | `jumpBufferTime = 0.1f` |
| 可变高度跳跃 | ✅ | `Input.GetKeyUp(KeyCode.Space)` 时 y 速度 ×0.5 |
| 角色翻转 | ✅ | `spriteRenderer.flipX` |
| GroundCheck 子对象 | ✅ | 使用 `Physics2D.OverlapCircle` 检测 |

**备注**: 实现完整，包含 Debug.Log 输出用于调试跳跃逻辑。移动放在 `FixedUpdate`，输入检测放在 `Update`，符合 Unity 物理最佳实践。

---

### Week 5：物品系统基础 ✅ 完成

| 计划任务 | 状态 | 实际实现 |
|----------|------|----------|
| ItemData ScriptableObject | ✅ | `ItemData.cs`（itemID/名称/类型/稀有度/图标/堆叠） |
| ItemType 枚举 | ✅ | Material/Tool/Weapon/Armor/Consumable/Misc |
| ItemRarity + 颜色 | ✅ | 5 级稀有度（Common→Legendary），`RarityColors.GetColor()` |
| 掉落物 ItemDrop | ✅ | `ItemDrop.cs`（拾取延迟、自动吸引、距离触发拾取） |
| PlayerCurrency 四级货币 | ✅ | `PlayerCurrency.cs`（铜/银/金/铂，内部用 long 存铜币） |
| CoinDisplay HUD | ✅ | `CoinDisplay.cs`，每帧读取 `FormatCurrency()` |
| 击败怪物获得金币 | ✅ | `EnemyBase.Die()` → `PlayerCurrency.EarnFromEnemy()` |

**备注**: `ItemDrop` 在拾取时同时处理货币（`coinValue > 0` 直接 Earn）和物品（`AddItem` 到背包），逻辑完整。`EnemyBase.Die()` 中已调用 `EarnFromEnemy`。

**创建的实际物品资源**（`Assets/Sources/ScriptObjects/`）：
- Dirt.asset, DirtItem.asset
- Grass.asset
- Stone.asset
- FiberItem.asset
- WoodPickaxeItem.asset, WoodSwordItem.asset
- NPCSummonToken.asset

---

### Week 6：背包系统 UI ✅ 完成

| 计划任务 | 状态 | 实际实现 |
|----------|------|----------|
| PlayerInventory（40 格） | ✅ | `PlayerInventory.cs`，`List<InventorySlot>` |
| InventoryUI | ✅ | `InventoryUI.cs`（纯代码构建，B 键开关） |
| 物品堆叠 | ✅ | 先堆叠已有同类物品，再放入空格 |
| ConsumeItem | ✅ | 从各个 slot 依次扣除 |
| PlayerEquipment（装备槽） | ✅ | `PlayerEquipment.cs`，9 槽（比计划多 1 个 tool 槽） |
| GetAttackBonus/GetDefense/GetMiningPower | ✅ | 遍历装备计算加成 |

**备注**: UI 全部通过 C# 代码动态创建，未使用 Prefab，与 DevPlan 在 Unity Editor 中手动搭建 UI 的方式不同。装备槽位实际为 9 个（helmet/chestplate/leggings/boots/offhand/accessory1/accessory2/weapon/tool），比 GDD 的 8 槽多了一个独立的 tool 槽位。`Equip()` 目前仅处理 Weapon/Armor/Tool 三种类型。

---

### Week 7：敌人 AI（史莱姆）✅ 完成

| 计划任务 | 状态 | 实际实现 |
|----------|------|----------|
| EnemyBase 抽象类（FSM） | ✅ | `EnemyBase.cs`，6 状态（Idle/Patrol/Chase/Attack/Hurt/Dead） |
| SlimeEnemy 具象类 | ✅ | `SlimeEnemy.cs`（跳跃式移动/追击） |
| 状态转换逻辑 | ✅ | `UpdateStateTransitions()`，基于距离判定 |
| 受击/死亡/掉落 | ✅ | `TakeDamage()` + 击退 + 闪烁 + `Die()` + `DropItems()` |
| 接触伤害 | ✅ | `OnCollisionEnter2D` 中调用 `PlayerStats.TakeDamage()` |

**备注**: 敌人 FSM 实现完整。SlimeEnemy 的 `OnAttack()` 为空（接触伤害代替独立攻击动作）。受击效果包含击退（knockback）和白色闪烁（HitFlash 协程），比 DevPlan 描述更丰富。

**补充 — EnemySpawner**（后续追加，commit `749fea1`）：
- `EnemySpawner.cs` 挂载在场景中，定期在摄像机视野外、地表上方生成史莱姆
- 参数：`spawnInterval=5s`，`maxEnemies=10`，`spawnDistMin=3f / Max=20f`
- 生成位置计算：沿摄像机左右边缘外随机距离 → `FindSurfaceY()` 向下扫描地表 → 在地表上方 1.5 格生成
- 最多尝试 15 次找到有效位置，避免生成在墙体内部

---

### Week 8：战斗系统 + HP/MP + 状态效果 ✅ 完成

| 计划任务 | 状态 | 实际实现 |
|----------|------|----------|
| PlayerStats（HP/MP） | ✅ | `PlayerStats.cs`，maxHP=100, maxMP=20 |
| HP/MP 自然回复 | ✅ | HP 回复（hpRegenRate, 默认 0），MP 回复（mpRegenRate=1/s） |
| 无敌帧 iFrame（0.5s） | ✅ | `invincibilityDuration = 0.5f`，半透明闪烁动画 |
| PlayerCombat（近战） | ✅ | `PlayerCombat.cs`，OverlapCircleAll 检测 |
| 暴击系统 | ✅ | `critChance = 0.04`（4%），`critMultiplier = 2.0` |
| 伤害公式 | ✅ | `(基础伤害 × 0.85~1.15) × 暴击系数 - 防御` |
| 魔法攻击 | ✅ | `MagicAttack(spellDamage, mpCost)`，2 倍范围 |
| 武器伤害集成 | ✅ | `equipment.weapon?.damage` + `playerStats.baseAttack` |
| StatusEffect 基类 | ✅ | `StatusEffect.cs`（duration/tickInterval/DoT 框架） |
| 具体状态效果 | ✅ | PoisonEffect/BurnEffect/SlowEffect/RegenEffect |
| StatusEffectManager | ✅ | `StatusEffectManager.cs`，同类型刷新不叠加 |
| IncreaseMaxHP/MP | ✅ | HP 上限 400，MP 上限 200 |

**备注**: 战斗和 HP/MP 系统实现非常完整，严格遵循 GDD §5.2 伤害公式。`OnHPChanged`/`OnMPChanged`/`OnPlayerDeath` 事件已定义，可供 UI 订阅。

---

### Week 9：合成系统 ✅ 完成

| 计划任务 | 状态 | 实际实现 |
|----------|------|----------|
| CraftingRecipe ScriptableObject | ✅ | `CraftingRecipe.cs`（材料/产出/所需合成台） |
| CraftingUI | ✅ | `CraftingUI.cs`（纯代码构建，C 键开关） |
| CraftingStation | ✅ | `CraftingStation.cs`，交互范围内高亮 |
| CraftingStationType 枚举 | ✅ | Hand/Workbench/Furnace/Anvil |
| 配方过滤 | ✅ | 根据 `activeStation.stationType` 筛选 |
| 材料检查（红/绿色） | ✅ | UI 中用 `#8f8` / `#f66` 颜色标注 |
| 合成后自动装备 | ✅ | 武器/工具/护甲类物品合成后自动调用 `Equip()` |

**备注**: 合成 UI 同样通过 C# 代码动态构建（ScrollView + 配方列表 + Craft 按钮）。配方默认从 `Resources.LoadAll<CraftingRecipe>()` 自动加载。`CraftingUI.DetectNearbyStation()` 每帧检测最近的工作站。

**创建的实际配方资源**:
- WoodPickaxeRecipe.asset, WoodSwordRecipe.asset

---

### Week 10：NPC 基础 + 白天黑夜 ✅ 完成

| 计划任务 | 状态 | 实际实现 |
|----------|------|----------|
| DayNightCycle | ✅ | `DayNightCycle.cs`，dayDuration=300s，Sin 曲线亮度 |
| 夜晚遮罩 | ✅ | 全屏 `nightOverlay` Image（黑色 + 可变 alpha） |
| 夜晚判定 | ✅ | `IsNight`：TimeOfDay < 0.2 或 > 0.8 |
| NPC 类 | ✅ | `NPC.cs`（漫游/高亮/交互/夜间提示） |
| ShopData ScriptableObject | ✅ | `ShopData.cs`（商品列表/买入卖出价格） |
| ShopUI | ✅ | `ShopUI.cs`（纯代码构建，网格展示/信息/购买按钮） |
| RoomDetector（房间检测） | ✅ | `RoomDetector.cs`（Flood Fill + 家具检查 + 最小尺寸） |
| NPC 召唤令牌 | ✅ | `NPCSummonToken.asset`，按 U 在有效房间中召唤 |
| 房间要求 | ✅ | 墙/门/桌/椅/灯 + 最小 6×10 |
| 自动设置 | ✅ | `RuntimeWeek10Setup.cs` + `Week10Setup.cs`（Editor 菜单） |
| NPC 夜间行为 | ✅ | 夜晚显示警告消息而非打开商店 |

**备注**: Week 10 实现远超 DevPlan 基本要求。RoomDetector 完整实现了 Flood Fill 算法（BFS + 开口检测 + 5000 格上限）。自动设置系统分编辑器版本（`Tools/Setup Week 10 - Complete` 菜单）和运行时版本（`[RuntimeInitializeOnLoadMethod]`），通过反射自动填充私有字段。

**创建的实际资源**:
- GeneralStore.asset（商店数据）
- DoorTile.asset, TableTile.asset, ChairTile.asset, LightTile.asset（家具 Tile）
- NPC.prefab（NPC 预制体）

---

### Week 11：更多地形 + 多生物群系 ❌ 未开始

| 计划任务 | 状态 |
|----------|------|
| RuleTile 自动匹配 | ❌ |
| 多生物群系（Forest/Desert/Snow/Mushroom/Corruption） | ❌ |
| 矿脉生成（铜/铁/金团簇） | ❌ |
| 洞穴优化 | ❌ |

---

### Week 12：Boss 1 — 眼球之王 ❌ 未开始

| 计划任务 | 状态 |
|----------|------|
| Boss 类（两阶段 AI） | ❌ |
| Boss HP 条 UI | ❌ |
| 召唤机制 | ❌ |
| 掉落物配置 | ❌ |

---

### Week 13：存档系统 ✅ 完成

| 计划任务 | 状态 | 实际实现 |
|----------|------|----------|
| SaveManager | ✅ | `SaveManager.cs`，单例，DontDestroyOnLoad |
| JSON 序列化 | ✅ | `JsonUtility.ToJson/FromJson`，prettyPrint |
| 自动存档（5 分钟） | ✅ | `autoSaveTimer`，每 300 秒自动存到 slot 0 |
| 手动存档（F5） | ✅ | 按 F5 存到 slot 1 |
| 手动读档（F9） | ✅ | 按 F9 自动选择存档（优先级：slot 1→2→3→0） |
| 3 个存档槽 | ✅ | slot 0（自动）/ slot 1（F5）/ slot 2/3（预留） |
| 世界数据存档 | ✅ | 世界种子 + 时间 + 已修改 Chunk（`modifiedChunks`） |
| 玩家数据存档 | ✅ | 位置/HP/MP/属性/货币/背包/装备 |
| NPC 状态存档 | ✅ | 位置/巡逻方向/计时器/朝向/夜间消息状态 |
| 已击杀敌人追踪 | ✅ | `DeadEnemyData`（类型+位置），读档时销毁对应敌人 |
| Chunk 修改持久化 | ✅ | `ChunkData.FlattenTileIDs()/UnflattenTileIDs()`，`hasBeenModified` 标记 |

**实现架构**：
```
SaveData.cs（数据结构）          SaveManager.cs（管理器）
├── WorldData                   ├── Save(slot) — 收集所有单例数据，序列化写入文件
│   ├── worldSeed               ├── Load(slot) — 读取文件，反序列化，逐系统恢复
│   ├── timeOfDay               ├── Auto-save — Update 中每 300s 调用 Save(0)
│   └── modifiedChunks[]        └── 输入 — F5 存档 / F9 读档
├── PlayerData
│   ├── 位置/HP/MP/属性
│   ├── totalCopper
│   ├── inventory[]
│   └── equipment[] (9槽, itemID)
├── NpcData[]
│   └── 位置/巡逻/朝向/状态
└── DeadEnemyData[]
    └── 类型+位置 Key
```

**各系统存档集成点**：
| 系统 | 存盘方法 | 读盘方法 |
|------|---------|---------|
| PlayerStats | 直接读取 `currentHP/MP/maxHP/MP/attack/defense` | `LoadState(curHP, curMP, maxHP, maxMP, atk, def)` |
| PlayerCurrency | 读取 `TotalCopper` (long) | `SetTotalCopper(amount)` |
| PlayerInventory | 遍历 `slots` 输出非空格子 | `ClearAll()` → `LoadSlot(index, item, amount)` |
| PlayerEquipment | `GetEquipmentItemIDs()`(int[9]) | `LoadFromItemIDs(ids, itemDB)` |
| NPC | 暴露 `WanderDirection/Timer/HasShownNightMessage/FlipX` | `LoadState(posX, posY, wDir, wTimer, nightMsg, flipX)` |
| EnemyBase | `DeathKey` = `"{TypeName}|{posX:F1}|{posY:F1}"` | 读档时销毁匹配 DeathKey 的敌人 |
| ChunkManager | `GetAllChunks()` → 筛选 `hasBeenModified` → `FlattenTileIDs()` | `ClearAllChunks()` → `ForceRegenerateAround()` → `ApplySavedChunk()` |
| DayNightCycle | 读取 `TimeOfDay` | 直接设置 `TimeOfDay` |
| WorldGenerator | 读取 `WorldSeed` | 直接设置 `WorldSeed` |

**备注**: 实现远超 DevPlan 基本要求。DevPlan 仅设计了玩家位置/HP/游戏时间的存档；实际实现覆盖了世界修改追踪、NPC 完整状态、已击杀敌人持久化、装备和背包的完整恢复、修改过的 Chunk 增量保存。存档路径位于 `Application.persistentDataPath/saves/save_slot_{slot}.json`。物品数据库通过 `Resources.LoadAll<ItemData>()` 在 SaveManager.Awake 时构建，用于读档时 itemID → ItemData 的映射。

---

### Week 14：UI 完善 + 小地图 ❌ 未开始

| 计划任务 | 状态 |
|----------|------|
| 小地图（Fog of War） | ❌ |
| 死亡界面 | ❌ |
| HUD 血条/魔法条 | ❌ |
| 快捷栏 | ❌ |

**备注**: 有一个额外的 `DebugItemPanel.cs`（F1 键）作为开发调试工具，可以快速获取所有已知物品，不在原始 DevPlan 中。

---

### Week 15：音效与特效 ❌ 未开始

| 计划任务 | 状态 |
|----------|------|
| AudioManager | ❌ |
| 挖掘/跳跃/受伤音效 | ❌ |
| 背景音乐 | ❌ |

---

### Week 16：性能优化 + Bug 修复 + 最终测试 ❌ 未开始

| 计划任务 | 状态 |
|----------|------|
| 对象池（Object Pool） | ❌ |
| Profiler 性能分析 | ❌ |
| Bug 修复 | ❌ |
| 构建测试 | ❌ |

---

## 实现与计划的主要差异

### 架构差异

| 方面 | DevPlan 设计 | 实际实现 |
|------|-------------|----------|
| UI 构建方式 | Unity Editor 中手动搭建 Prefab | 全部通过 C# 代码动态构建 |
| 输入系统 | 新版 Input System | 旧版 Input Manager（`Input.GetKeyDown` 等） |
| 昼夜实现 | Universal RP Light2D | Camera.backgroundColor + 全屏遮罩 Image |
| 代码组织 | 分目录（World/Player/Combat 等） | 平铺在 `Assets/Scripts/` 下 |
| 装备槽数量 | 8 槽 | 9 槽（多了独立 tool 槽） |
| 玩家控制器 | Week 1 基础版 + Week 4 改进版分开 | 一次性实现完整版 |

### 功能差异

| 方面 | DevPlan/GDD 要求 | 实际状态 |
|------|-----------------|----------|
| 敌人类 | 多种敌人（骷髅/蝙蝠/飞眼等） | 仅 SlimeEnemy 一种，但有 EnemySpawner 自动生成 |
| 合成台 | 5 种（工作台/熔炉/铁砧/法术台/暗铁台） | 4 种枚举值（Hand/Workbench/Furnace/Anvil） |
| NPC 类型 | 6 种（商人/护士/爆破师/法师/武器商/向导） | 仅 1 种（Merchant 商人） |
| 物品数量 | 矿石链完整（木材→铜→铁→金→暗铁→虚空） | 基础物品（泥土/石块/草/纤维/木镐/木剑） |
| 世界尺寸 | 4200×1200 | 参数上已设置，但 ChunkManager 可能未在实际场景中使用 |
| 多生物群系 | 5 种群系 | 仅基础噪声地形（草地/泥土/石头/洞穴） |
| 存档系统 | Week 13 待实现 | ✅ 已完整实现，覆盖世界/玩家/NPC/敌人/装备/Chunk |

---

## 已定义的 ScriptableObject 资源清单

### 物品（ItemData）
| 文件 | 说明 |
|------|------|
| `Dirt.asset` | 泥土（Tile 掉落源？） |
| `DirtItem.asset` | 泥土物品 |
| `Grass.asset` | 草地（Tile 掉落源？） |
| `Stone.asset` | 石头（Tile 掉落源？） |
| `FiberItem.asset` | 纤维 |
| `WoodPickaxeItem.asset` | 木镐 |
| `WoodSwordItem.asset` | 木剑 |
| `NPCSummonToken.asset` | NPC 召唤令牌 |

### 配方（CraftingRecipe）
| 文件 | 说明 |
|------|------|
| `WoodPickaxeRecipe.asset` | 木镐配方 |
| `WoodSwordRecipe.asset` | 木剑配方 |

### Tile（TileBase）
| 文件 | 说明 |
|------|------|
| `BasicTile_0.asset` ~ `BasicTile_3.asset` | 基础 Tile（草地/泥土/石头等） |
| `DoorTile.asset` | 门 Tile |
| `TableTile.asset` | 桌子 Tile |
| `ChairTile.asset` | 椅子 Tile |
| `LightTile.asset` | 光源 Tile |

### 商店（ShopData）
| 文件 | 说明 |
|------|------|
| `GeneralStore.asset` | 通用商店 |

---

## 下一步建议

1. **Week 11**：实现多生物群系系统，基于 x 坐标分段切换地表 Tile 类型
2. **Week 12**：实现第一个 Boss（眼球之王），继承 EnemyBase 扩展两阶段 AI
3. **Week 14**：小地图（Fog of War）+ 死亡界面 + HUD 完善
4. 存档系统（Week 13）已完成，可在此基础上继续迭代（如存档槽 UI、存档预览）
5. 在此之前，建议先完善已有功能的测试和 Bug 修复，确保 Week 1-10 的内容稳定运行

---

*本文档基于代码审查生成，反映截至 2026-05-19 的实际开发状态。*
