# Terra Unity — 新手详细开发计划文档

> **配套文档**: Terra_Unity_GDD.md  
> **版本**: v1.1 | **日期**: 2026-05-18（GDD 对齐优化版）  
> **适用对象**: Unity 初学者，有基础 C# 语法知识  
> **总周期**: 16 周（每周约 15~20 小时投入）

---

## 文档说明

本文档将 GDD 中的 16 周开发计划拆解为**每周具体任务清单**，包含：
- 本周**需要学会的 Unity 知识点**
- **每天的具体工作任务**
- **关键代码示例**（带详细注释）
- **常见新手坑**和避坑方法
- **自检清单**（完成标准）

**学习建议**：
- 每天至少投入 2~3 小时
- 每完成一个功能，立刻**玩一遍**验证手感
- 遇到问题先查 Unity 官方文档，再查 StackOverflow，最后才问 AI
- 每周末写 100 字"本周总结"留存

---

## 前置准备（开始前完成）

### 安装清单
- [ ] **Unity Hub** 最新版（从 unity.com/download 下载）
- [ ] **Unity 2022.3 LTS**（在 Unity Hub 中安装，选 LTS 版本最稳定）
  - 安装时勾选：`Windows Build Support`、`2D Sprite`、`2D Tilemap Editor`
- [ ] **Visual Studio 2022** 或 **VS Code**（Unity 编辑器会自动推荐）
- [ ] **Git**（版本控制，强烈推荐）+ GitHub 账号（免费）
- [ ] **Aseprite**（约 ¥14，像素画工具）或免费替代 **LibreSprite**

### 新建项目
1. 打开 Unity Hub → 新建项目
2. 选择模板：**2D Core**
3. 项目名：`TerraUnity`
4. 保存位置：你的工作目录
5. 点击"Create project"

### C# 基础检查（必须先掌握）
在开始前，确保你理解以下概念：
- 变量、if/else、for 循环、while 循环
- 类（class）和方法（function/method）
- 访问修饰符（public/private）
- 列表（List<T>）和数组（Array）

如果不熟悉，先花 1 周看：**《C# 黄宝书》前 6 章** 或 B 站搜索"C# 入门教程"

---

## 第 0 阶段：环境熟悉（第 1-2 天，不计入正式周期）

### 目标
熟悉 Unity 编辑器界面，不要急着写代码。

### 任务
1. **认识编辑器各面板**
   - `Scene`（场景编辑视图）
   - `Game`（游戏运行视图）
   - `Hierarchy`（场景层级，所有 GameObject 列表）
   - `Project`（项目文件）
   - `Inspector`（选中对象的属性）
   - `Console`（日志输出，报错在这里看）

2. **做一个超简单测试**
   - 在 Hierarchy 中右键 → 2D Object → Sprite
   - 在 Inspector 中给它添加一个 Rigidbody2D 组件
   - 点击运行，看精灵下落
   - 理解：Unity 里所有东西都是 GameObject，功能由 Component 组成

3. **学会 Git 基础操作**
   ```bash
   git init
   git add .
   git commit -m "Initial commit"
   ```
   > 建议每完成一个功能点就 commit 一次，这是你的"存档点"

---

## 第 1 阶段：核心原型（第 1-4 周）

---

### Week 1：Unity 基础 + 创建第一个可动场景

**本周学习目标**
- 理解 MonoBehaviour 生命周期（Awake、Start、Update）
- 学会使用 Tilemap 绘制地形
- 实现玩家精灵在场景中显示

---

#### Day 1-2：Tilemap 入门

**学习内容**
- Tilemap 是 Unity 提供的 2D 瓦片地图系统
- 每个格子（Tile）是 16×16 或 32×32 像素的图片

**操作步骤**

1. Hierarchy → 右键 → 2D Object → Tilemap → Rectangular
   - 这会自动创建 `Grid` 父对象 和 `Tilemap` 子对象
2. 在 Project 面板 → 右键 → Create → Folder，命名 `Art/Tiles`
3. 暂时用 Unity 内置的方块色块代替美术资源：
   - Project → 右键 → Create → 2D → Sprites → Square
   - 将这个方块拖入 Tile Palette

4. 打开 Tile Palette（菜单 Window → 2D → Tile Palette）
5. 用画笔工具在 Scene 中涂地形

**新建 Tilemap 结构（Hierarchy）**
```
Grid
  ├── Tilemap_Ground   (地面层，有碰撞)
  ├── Tilemap_Back     (背景装饰层，无碰撞)
  └── Tilemap_Foreground (前景装饰)
```

**给 Tilemap_Ground 添加碰撞**
- 选中 `Tilemap_Ground`
- Inspector → Add Component → `Tilemap Collider 2D`
- 再添加 `Composite Collider 2D`（把所有 Tile 碰撞合并，性能更好）
- 在 Tilemap Collider 2D 中勾选 `Used By Composite`
- 在 Rigidbody2D（Composite Collider 自动添加）中设置 Body Type = `Static`

**常见坑**
> ⚠️ 忘记设置 Rigidbody2D 为 Static，导致整个地图会受重力下落！

---

#### Day 3-4：玩家角色显示

**任务**：在场景中创建玩家角色并让它显示

1. Hierarchy → 右键 → 2D Object → Sprite → 命名 `Player`
2. 在 Inspector 中：
   - 添加 `Rigidbody2D`（Gravity Scale 保持默认 1）
   - 添加 `Capsule Collider 2D`（调整大小贴合角色）
   - 勾选 Rigidbody2D 中的 `Constraints → Freeze Rotation Z`（防止角色倒下去）

3. 创建第一个脚本：
   - Project → Create → C# Script → 命名 `PlayerController`
   - 双击打开，输入以下代码：

```csharp
using UnityEngine;

// 这个类控制玩家的基础行为
// MonoBehaviour 是 Unity 脚本的基类，继承它才能挂到 GameObject 上
public class PlayerController : MonoBehaviour
{
    // [SerializeField] 让 private 变量在 Inspector 中可以调节
    // 这样不需要写 public，外部也看不到，但你在编辑器里能调数值
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;

    // 缓存组件引用，避免每帧调用 GetComponent（性能开销大）
    private Rigidbody2D rb;
    private bool isGrounded = false;  // 是否在地面上

    // Awake 在对象创建时调用，比 Start 更早
    // 用于初始化组件引用
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update 每帧调用一次（通常 60fps）
    // 处理输入检测
    private void Update()
    {
        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        // Input.GetAxisRaw("Horizontal") 返回 -1（左）、0（停）、1（右）
        // 比 GetAxis 更硬朗，没有缓动效果，适合平台游戏
        float horizontal = Input.GetAxisRaw("Horizontal");

        // 修改 Rigidbody2D 的速度：保持 Y 轴速度不变，只改 X
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
    }

    private void HandleJump()
    {
        // GetKeyDown 只在按下那一帧触发，不会持续触发
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // 设置向上速度实现跳跃
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }
    }

    // OnCollisionEnter2D 在碰撞开始时调用
    // 用来检测角色是否踩在地面上
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查碰撞物的 Layer 是否是 "Ground"
        // 之后我们会给 Tilemap 设置 Ground 层
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }
}
```

4. 将脚本拖到 Player 对象上（或直接从 Inspector → Add Component 搜索）
5. 给 `Tilemap_Ground` 设置 Layer = `Ground`（Edit → Project Settings → Tags and Layers 中新建）
6. 点击运行，测试左右移动和跳跃

**本周自检清单**
- [ ] 能在 Tilemap 上绘制地形
- [ ] 玩家可以左右移动
- [ ] 玩家可以跳跃，落到地面停止
- [ ] 地形有碰撞，玩家不会穿过

---

### Week 2：摄像机跟随 + 挖掘基础

**本周学习目标**
- 使用 Cinemachine 实现平滑摄像机跟随
- 实现带**进度条**的 Tile 挖掘功能（匹配 GDD §4.3）
- 理解 Tilemap API

---

#### Day 1-2：Cinemachine 摄像机

**安装 Cinemachine**
- 菜单 Window → Package Manager
- 搜索 `Cinemachine` → Install

**配置**
1. Hierarchy → 右键 → Cinemachine → 2D Camera
2. 在 `CinemachineCamera` Inspector 中：
   - `Follow` 字段：拖入你的 Player 对象
   - `Lens → Orthographic Size`：调整为 8~10（数值越小镜头越近）
3. 选中主 Camera，添加 `CinemachineBrain` 组件（通常自动添加）

**设置摄像机边界（防止看到世界外面）**
1. Hierarchy → 右键 → 2D Object → Physics → Composite Collider 2D，命名 `WorldBounds`
2. 在 CinemachineCamera → Add Extension → `CinemachineConfiner2D`
3. 把 WorldBounds 拖入 Confiner 的 `Bounding Shape 2D` 字段

---

#### Day 3-5：挖掘系统（带进度条）

这是游戏最核心的交互之一，需要仔细理解。

> **GDD 对齐**：GDD §4.3 要求挖掘有进度条（`mineTime = tile.hardness / tool.miningPower`），且工具等级不足时无法挖掘。

**第一步：给 Tile 添加硬度数据**

创建 `TileData.cs`，用 ScriptableObject 存储每种 Tile 的属性：

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileData", menuName = "TerraUnity/Tile Data")]
public class TileData : ScriptableObject
{
    public TileBase tile;           // 对应 Tile 资源
    public string tileName;
    public float hardness = 1f;     // 硬度（越大越难挖）
    public int requiredMiningLevel = 0;  // 所需工具等级（0=手挖）
    public ItemData dropItem;       // 掉落的物品
    public int dropAmount = 1;
}

// 在 WorldGenerator 中需要维护一个 Dictionary<TileBase, TileData> 映射表
```

**第二步：挖掘进度条 UI**

在 Canvas → HUD 下创建一个 `Slider`，命名 `MiningProgressBar`：
- 默认 `SetActive(false)`
- 填充颜色：绿色 → 黄色 → 红色（根据硬度）
- 跟随鼠标位置

**第三步：重写挖掘脚本（带进度条）**

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileInteraction : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private float mineRange = 3f;
    [SerializeField] private GameObject itemDropPrefab;
    [SerializeField] private Slider miningProgressBar;  // 挖掘进度条 UI

    [Header("玩家当前工具属性")]
    public int currentMiningPower = 1;   // 工具挖掘力（手挖=1，木镐=3，铜镐=6...）
    public int currentMiningLevel = 0;   // 工具等级

    private Camera mainCamera;
    private Vector3Int currentTargetCell;   // 当前正在挖的格子
    private float miningProgress = 0f;      // 当前挖掘进度

    private void Awake()
    {
        mainCamera = Camera.main;
        if (miningProgressBar != null)
            miningProgressBar.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
            TryMine();
        else
            ResetMining();  // 松开鼠标时重置进度
    }

    private void TryMine()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        float distance = Vector2.Distance(transform.position, mouseWorldPos);
        if (distance > mineRange) { ResetMining(); return; }

        Vector3Int cellPos = groundTilemap.WorldToCell(mouseWorldPos);
        TileBase tile = groundTilemap.GetTile(cellPos);
        if (tile == null) { ResetMining(); return; }

        // 获取该 Tile 的数据
        TileData tileData = TileDataManager.Instance.GetTileData(tile);
        if (tileData == null) { ResetMining(); return; }

        // 检查工具等级是否足够（GDD §4.3 规则）
        if (currentMiningLevel < tileData.requiredMiningLevel)
        {
            Debug.Log($"需要挖矿等级 {tileData.requiredMiningLevel} 以上的工具！");
            ResetMining();
            return;
        }

        // 如果切换了目标格子，重置进度
        if (cellPos != currentTargetCell)
        {
            currentTargetCell = cellPos;
            miningProgress = 0f;
        }

        // 显示进度条在鼠标旁边
        if (miningProgressBar != null)
        {
            miningProgressBar.gameObject.SetActive(true);
            miningProgressBar.transform.position = Input.mousePosition + new Vector3(0, 30, 0);
        }

        // 计算挖掘速度 = 工具挖掘力 / Tile 硬度（GDD §4.3 公式）
        float mineSpeed = currentMiningPower / tileData.hardness;
        miningProgress += mineSpeed * Time.deltaTime;

        // 更新进度条
        if (miningProgressBar != null)
            miningProgressBar.value = miningProgress;

        // 进度满 → 完成挖掘
        if (miningProgress >= 1f)
        {
            groundTilemap.SetTile(cellPos, null);

            // 生成掉落物
            Vector3 dropPos = groundTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
            GameObject drop = Instantiate(itemDropPrefab, dropPos, Quaternion.identity);
            drop.GetComponent<ItemDrop>().itemData = tileData.dropItem;
            drop.GetComponent<ItemDrop>().amount = tileData.dropAmount;

            ResetMining();
        }
    }

    private void ResetMining()
    {
        miningProgress = 0f;
        if (miningProgressBar != null)
            miningProgressBar.gameObject.SetActive(false);
    }
}
```

**本周自检清单**
- [ ] 摄像机平滑跟随玩家
- [ ] 鼠标按住 Tile 出现挖掘进度条
- [ ] 进度条走满后 Tile 消失
- [ ] 挖掘有距离限制（超出范围自动取消）
- [ ] 工具等级不够时无法挖掘（控制台提示）

---

### Week 3：程序化世界生成 + Chunk 系统

**本周学习目标**
- 理解 Perlin Noise（柏林噪声）
- 用代码生成地形，不手动绘制
- **实现 Chunk 分块加载**（GDD §3.3 和 §11.3 核心性能要求）

**这是难度最高的一周，需要耐心！**

> **GDD 对齐**: GDD 要求世界 4200×1200 Tile（§1.5）。全量生成会导致 ~500 万 Tile 的内存和启动开销，必须分 Chunk。

---

#### Day 1：理解 Perlin Noise

Perlin Noise 是一种"自然感"随机数生成算法。普通随机数每次完全不同，Perlin Noise 的相邻值是平滑过渡的，非常适合生成地形。

```csharp
// Perlin Noise 示例：
// Mathf.PerlinNoise(x, y) 返回 0~1 之间的浮点数
// 关键：相近的 x 值会返回相近的结果（平滑性）

float height = Mathf.PerlinNoise(x * 0.05f, 0f) * 20f;
// x * 0.05f 是"频率"——数值越小，地形变化越平缓
// * 20f 是"振幅"——数值越大，地形高低差越大
```

---

#### Day 2：Chunk 系统设计（GDD §3.3 & §11.3）

> **为什么需要 Chunk？** GDD 世界 4200×1200 = 504 万 Tile。一次性全部生成 → 内存爆炸 + 启动卡死。只激活玩家周围 5×5 Chunk = 80×80 = 6400 Tile 活跃。

**Chunk 数据结构**

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;

// 每个 Chunk 是 16×16 的 Tile 区域（GDD §3.3 规格）
public class ChunkData
{
    public Vector2Int chunkCoord;   // Chunk 坐标（世界坐标 / 16）
    public int[,] tileIDs;          // 16×16 的 Tile ID 数组
    public Tilemap tilemap;         // 该 Chunk 使用的 Tilemap（可能多个 Chunk 共享）
    public bool isActive;           // 是否在活跃区域
    public bool isGenerated;        // 是否已生成数据

    public ChunkData(Vector2Int coord)
    {
        chunkCoord = coord;
        tileIDs = new int[16, 16];  // -1 = 空
        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                tileIDs[x, y] = -1;
    }
}
```

**ChunkManager 框架**

```csharp
using UnityEngine;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    [Header("世界规模（GDD §1.5）")]
    public int worldWidth = 4200;       // GDD 规格
    public int worldHeight = 1200;

    [Header("Chunk 设置（GDD §3.3）")]
    public int chunkSize = 16;          // 每个 Chunk 16×16
    public int activeChunkRadius = 2;   // 玩家周围加载半径（总共 5×5 Chunk）

    [Header("Tile 资源")]
    public TileBase[] tileAssets;       // TileID → TileBase 映射表

    // 所有 Chunk 的字典（Key = Chunk 坐标）
    private Dictionary<Vector2Int, ChunkData> chunks = new Dictionary<Vector2Int, ChunkData>();
    private Transform playerTransform;
    private Vector2Int lastCenterChunk;

    private void Awake() => Instance = this;
    private void Start() => playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

    private void Update()
    {
        // 计算玩家所在的 Chunk
        Vector2Int playerChunk = WorldToChunk(playerTransform.position);

        // 只在玩家移动到新 Chunk 时才更新加载
        if (playerChunk != lastCenterChunk)
        {
            lastCenterChunk = playerChunk;
            UpdateActiveChunks(playerChunk);
        }
    }

    private Vector2Int WorldToChunk(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / chunkSize),
            Mathf.FloorToInt(worldPos.y / chunkSize)
        );
    }

    // 激活玩家周围的 Chunk，卸载远距离 Chunk
    private void UpdateActiveChunks(Vector2Int center)
    {
        // 需要激活的 Chunk 列表
        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();
        for (int cx = -activeChunkRadius; cx <= activeChunkRadius; cx++)
            for (int cy = -activeChunkRadius; cy <= activeChunkRadius; cy++)
                neededChunks.Add(new Vector2Int(center.x + cx, center.y + cy));

        // 激活新 Chunk，停用不需要的
        foreach (var kvp in chunks)
        {
            bool shouldBeActive = neededChunks.Contains(kvp.Key);
            if (kvp.Value.isActive && !shouldBeActive)
                DeactivateChunk(kvp.Key);   // 卸载
            else if (!kvp.Value.isActive && shouldBeActive)
                ActivateChunk(kvp.Key);     // 加载
        }

        // 创建尚未初始化的 Chunk
        foreach (var coord in neededChunks)
        {
            if (!chunks.ContainsKey(coord))
            {
                chunks[coord] = new ChunkData(coord);
                GenerateChunkData(coord);   // 生成地形数据
                ActivateChunk(coord);       // 显示到 Tilemap
            }
        }
    }

    private void ActivateChunk(Vector2Int coord)
    {
        var chunk = chunks[coord];
        chunk.isActive = true;
        // 将 tileIDs 写入 Tilemap（使用 WorldGenerator 的生成逻辑）
        ApplyChunkToTilemap(coord);
    }

    private void DeactivateChunk(Vector2Int coord)
    {
        var chunk = chunks[coord];
        chunk.isActive = false;
        // 从 Tilemap 中清除该 Chunk 的 Tile
        for (int x = 0; x < chunkSize; x++)
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int worldCell = new Vector3Int(
                    coord.x * chunkSize + x,
                    coord.y * chunkSize + y, 0);
                chunk.tilemap.SetTile(worldCell, null);
            }
    }

    private void GenerateChunkData(Vector2Int coord)
    {
        // 调用 WorldGenerator 静态方法为该 Chunk 生成 tileIDs
        WorldGenerator.FillChunk(chunks[coord]);
    }

    private void ApplyChunkToTilemap(Vector2Int coord) { /* 同 GenerateChunkData 后写入 Tilemap */ }
}
```

---

#### Day 3-5：世界生成器 + 整合 Chunk

**创建脚本 `WorldGenerator.cs`**（与 ChunkManager 协作）

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance { get; private set; }

    [Header("地形参数（GDD §3.1 五层结构）")]
    [SerializeField] private float noiseScale = 0.05f;
    [SerializeField] private int surfaceBaseHeight = 100;    // 地表基准 Y
    [SerializeField] private int surfaceVariation = 20;
    [SerializeField] private int worldSeed;

    [Header("生物群系（GDD §3.2）")]
    [SerializeField] private BiomeType[] biomeDistribution;  // 生物群系横向分布

    private void Awake() => Instance = this;

    private void Start()
    {
        worldSeed = Random.Range(0, 999999);
        // ChunkManager 会自动调用 FillChunk 按需生成
    }

    /// <summary>
    /// 为指定 Chunk 填充 tileIDs 数据（被 ChunkManager 调用）
    /// </summary>
    public static void FillChunk(ChunkData chunk)
    {
        int gx0 = chunk.chunkCoord.x * 16;  // 该 Chunk 在世界中的起始 X
        int gy0 = chunk.chunkCoord.y * 16;

        for (int lx = 0; lx < 16; lx++)    // 局部坐标
        {
            int worldX = gx0 + lx;
            for (int ly = 0; ly < 16; ly++)
            {
                int worldY = gy0 + ly;

                // 计算地表高度
                float noiseVal = Mathf.PerlinNoise(
                    (worldX + Instance.worldSeed) * Instance.noiseScale,
                    (worldY + Instance.worldSeed) * 0.02f);
                int surfaceY = Instance.surfaceBaseHeight +
                    Mathf.RoundToInt(noiseVal * Instance.surfaceVariation);

                int tileID;
                if (worldY > surfaceY)
                {
                    tileID = -1;  // 空气
                }
                else if (worldY == surfaceY)
                {
                    tileID = 1;  // 草地
                }
                else if (worldY > surfaceY - 5)
                {
                    tileID = 2;  // 泥土
                }
                else if (worldY > surfaceY - 30)
                {
                    // 深层：根据洞穴生成算法判断是否留空
                    tileID = IsCaveAt(worldX, worldY) ? -1 : 3; // 3 = 石块
                }
                else
                {
                    tileID = IsCaveAt(worldX, worldY) ? -1 : 3;
                }

                chunk.tileIDs[lx, ly] = tileID;
            }
        }
    }

    // 洞穴检测（GDD §3.3：多层 Noise 叠加 + Cellular Automata）
    private static bool IsCaveAt(int x, int y)
    {
        float n1 = Mathf.PerlinNoise((x + 500) * 0.08f, (y + 500) * 0.08f);
        float n2 = Mathf.PerlinNoise((x + 1000) * 0.04f, (y + 1000) * 0.04f);
        // 深层（y < 40）洞穴密度更高
        float threshold = y < 40 ? 0.6f : 0.5f;
        return n1 * n2 > threshold;
    }
}
```

**在场景中配置**
1. 创建空 GameObject，命名 `WorldManager`，挂载 `ChunkManager` 和 `WorldGenerator`
2. 在 Inspector 中拖入 Tile 资源数组（按 TileID 排列）
3. 给 `Tilemap_Ground` 添加 Composite Collider 2D（同 Week 1）

**本周自检清单**
- [ ] 世界随玩家移动动态加载/卸载 Chunk
- [ ] 地形有自然的起伏（不是直线）
- [ ] 地表是草地，地下是泥土+石块
- [ ] 不同种子生成不同地形（修改 worldSeed 测试）
- [ ] Scene 视图中能看到玩家周围 Chunk 自动切换

---

### Week 4：完善玩家控制 + Coyote Time

**本周学习目标**
- 改善玩家手感（Coyote Time、跳跃缓冲）
- 实现角色翻转（朝向镜像）
- 理解 FixedUpdate vs Update

---

#### 重写玩家控制器（完整版）

```csharp
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;

    [Header("跳跃手感优化")]
    [SerializeField] private float coyoteTime = 0.15f;    // 离开地面后还能跳跃的时间
    [SerializeField] private float jumpBufferTime = 0.1f; // 提前按跳跃键的缓冲时间

    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;         // 地面检测点（玩家脚底的子对象）
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;         // 地面 Layer（在 Inspector 选择）

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // 跳跃相关计时器
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private bool IsGrounded => Physics2D.OverlapCircle(
        groundCheck.position, groundCheckRadius, groundLayer);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update 处理输入（每帧）
    private void Update()
    {
        // 更新 Coyote Time 计时器
        if (IsGrounded)
            coyoteTimeCounter = coyoteTime;  // 在地面时重置
        else
            coyoteTimeCounter -= Time.deltaTime;  // 不在地面时倒计时

        // 更新跳跃缓冲计时器
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;  // 按下跳跃键时开始缓冲
        else
            jumpBufferCounter -= Time.deltaTime;

        // 当两个计时器都有效时执行跳跃
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;  // 消耗跳跃缓冲
            coyoteTimeCounter = 0f; // 消耗 Coyote Time（防止二段跳）
        }

        // 松开跳跃键时减小上升速度（可变高度跳跃）
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    // FixedUpdate 处理物理（固定时间步长，默认 0.02s）
    // 物理操作（修改 Rigidbody）必须放在 FixedUpdate，否则会抖动
    private void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        // 角色翻转：根据移动方向翻转精灵
        if (horizontal > 0)
            spriteRenderer.flipX = false;  // 面朝右
        else if (horizontal < 0)
            spriteRenderer.flipX = true;   // 面朝左（水平翻转）
    }
}
```

**创建 GroundCheck 子对象**
1. 在 Player 下创建空子对象，命名 `GroundCheck`
2. 将其移到玩家脚底位置（y 值降低）
3. 在脚本 Inspector 中将 `GroundCheck` 拖进 `Ground Check` 字段

**本周自检清单**
- [ ] 走到悬崖边缘后 0.15 秒内还能跳跃
- [ ] 提前按空格键会在落地瞬间跳跃
- [ ] 角色会根据移动方向翻转朝向
- [ ] 物理感觉顺滑，不抖动

---

## 第 2 阶段：内容系统（第 5-10 周）

---

### Week 5：物品系统基础

**本周学习目标**
- 理解 ScriptableObject（数据容器）
- 用 ScriptableObject 定义物品数据
- 实现掉落物和拾取

---

#### Day 1-2：ScriptableObject 物品数据

ScriptableObject 是 Unity 的一种数据容器，非常适合存储游戏数据（物品属性、敌人属性等），和 GameObject 分离，修改数据不需要改代码。

**创建 `ItemData.cs`**

```csharp
using UnityEngine;

// CreateAssetMenu 让这个 ScriptableObject 可以从 Project 面板右键创建
[CreateAssetMenu(fileName = "NewItem", menuName = "TerraUnity/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("基本信息")]
    public int itemID;           // 物品唯一 ID
    public string itemName;      // 物品名称
    public Sprite icon;          // 物品图标
    [TextArea] public string description;  // 描述（支持多行文字输入）

    [Header("堆叠")]
    public int maxStack = 999;   // 最大堆叠数量

    [Header("物品类型")]
    public ItemType itemType;
    public ItemRarity rarity = ItemRarity.Common;  // 稀有度（GDD §6.1），影响名称颜色

    [Header("货币价值（GDD §8.3）")]
    public int coinValue = 0;  // 在 NPC 商店中的买入/卖出价格（铜币单位）

    [Header("工具属性，物品类型为 Tool 时生效")]
    public int miningPower = 0;  // 挖掘力
    public int miningLevel = 0;  // 可挖掘 Tile 的最低等级

    [Header("武器属性，物品类型为 Weapon 时生效")]
    public int damage = 0;
    public float attackSpeed = 1f;
}

// 枚举：物品类型
public enum ItemType
{
    Material,   // 材料（木材、矿石等）
    Tool,       // 工具（镐、斧子等）
    Weapon,     // 武器（剑、弓等）
    Armor,      // 盔甲
    Consumable, // 消耗品（药水等）
    Misc        // 杂项
}

// 物品稀有度（GDD §6.1）
public enum ItemRarity
{
    Common,     // 白色 — 挖掘/怪物掉落
    Uncommon,   // 绿色 — 宝箱/深层怪物
    Rare,       // 蓝色 — Boss 掉落/特殊合成
    Epic,       // 紫色 — 最终阶段 Boss
    Legendary   // 橙色 — 终极 Boss 专属
}

// 稀有度对应颜色
public static class RarityColors
{
    public static Color GetColor(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Common => Color.white,
        ItemRarity.Uncommon => Color.green,
        ItemRarity.Rare => new Color(0.3f, 0.5f, 1f),     // 蓝色
        ItemRarity.Epic => new Color(0.7f, 0.2f, 1f),      // 紫色
        ItemRarity.Legendary => new Color(1f, 0.6f, 0f),   // 橙色
        _ => Color.white
    };
}
```

**创建物品数据资产**
1. Project → 右键 → Create → TerraUnity → Item Data
2. 填入木材的数据（itemID=1, itemName="木材", maxStack=999, itemType=Material）
3. 重复为铜矿、铁矿等创建数据

---

#### Day 3-5：掉落物系统

**创建 `ItemDrop.cs`**（掉落物 GameObject 的行为脚本）

```csharp
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    public ItemData itemData;  // 这个掉落物代表哪种物品
    public int amount = 1;     // 数量

    [SerializeField] private float pickupRange = 1f;    // 自动拾取距离
    [SerializeField] private float moveSpeed = 3f;       // 向玩家飞去的速度
    [SerializeField] private float pickupDelay = 0.5f;   // 生成后多久才能被拾取（防止刚掉落就被拾取）

    private Transform playerTransform;
    private bool canPickup = false;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // 设置图标显示为物品图标
        if (itemData != null && itemData.icon != null)
            spriteRenderer.sprite = itemData.icon;
    }

    private void Start()
    {
        // 查找玩家（通过 Tag）
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        // 延迟后才能被拾取
        Invoke(nameof(EnablePickup), pickupDelay);
    }

    private void EnablePickup() => canPickup = true;

    private void Update()
    {
        if (!canPickup || playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 在范围内向玩家移动
        if (distanceToPlayer < pickupRange * 3f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                playerTransform.position,
                moveSpeed * Time.deltaTime
            );
        }

        // 足够近时拾取
        if (distanceToPlayer < 0.3f)
        {
            PickupItem();
        }
    }

    private void PickupItem()
    {
        // 尝试加入背包（后续实现背包系统后完善）
        // PlayerInventory.Instance.AddItem(itemData, amount);

        // 播放拾取特效（后续添加）
        Debug.Log($"拾取了 {itemData.itemName} ×{amount}");
        Destroy(gameObject);
    }
}
```

**修改挖掘系统，生成掉落物**：在 `TileInteraction.cs` 的挖掘成功处：

```csharp
// 在挖掘成功的位置生成掉落物
Vector3 dropPosition = groundTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
GameObject drop = Instantiate(itemDropPrefab, dropPosition, Quaternion.identity);
drop.GetComponent<ItemDrop>().itemData = GetItemDataForTile(tile); // 根据 Tile 类型获取物品数据
```

**Day 5 补充：货币系统（GDD §8.3）**

GDD 定义了四级货币：铜币 100 = 银币 1，银币 100 = 金币 1，金币 100 = 铂金币 1。

```csharp
using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
    public static PlayerCurrency Instance { get; private set; }

    // 货币存储（只用铜币作为内部单位，显示时再转换）
    private long totalCopper = 100; // 初始给 100 铜币

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 获取各等级货币显示值
    public int Copper  => (int)(totalCopper % 100);
    public int Silver  => (int)((totalCopper / 100) % 100);
    public int Gold    => (int)((totalCopper / 10000) % 100);
    public int Platinum => (int)(totalCopper / 1000000);

    // 格式化显示（如 "12金 34银 56铜"）
    public string FormatCurrency()
    {
        if (Platinum > 0) return $"{Platinum}铂 {Gold}金 {Silver}银 {Copper}铜";
        if (Gold > 0)     return $"{Gold}金 {Silver}银 {Copper}铜";
        if (Silver > 0)   return $"{Silver}银 {Copper}铜";
        return $"{Copper}铜";
    }

    public bool CanAfford(long copperAmount) => totalCopper >= copperAmount;

    public bool Spend(long copperAmount)
    {
        if (!CanAfford(copperAmount)) return false;
        totalCopper -= copperAmount;
        return true;
    }

    public void Earn(long copperAmount) => totalCopper += copperAmount;

    // 击杀怪物时获得金币（由敌人基类调用）
    public void EarnFromEnemy(int monsterLevel) => Earn(Random.Range(5, 15) * (monsterLevel + 1));
}
```

> 在 HUD 的 CoinDisplay 中绑定 `PlayerCurrency.Instance.FormatCurrency()`，每帧刷新。

**本周自检清单**
- [ ] 挖掘 Tile 后生成掉落物精灵
- [ ] 靠近掉落物会自动被吸引拾取
- [ ] 控制台输出拾取信息
- [ ] HUD 显示货币数量
- [ ] 击败怪物获得铜币

---

### Week 6：背包系统 UI

**本周学习目标**
- Unity UI Canvas 系统
- GridLayoutGroup 制作格子背包
- 理解 UI 数据绑定

---

#### Day 1-2：Canvas 和背包面板

**创建 UI 层级结构**
```
Canvas (Screen Space - Overlay)
  ├── HUD
  │   ├── HPBar
  │   ├── HotBar (快捷栏，10格)
  │   └── CoinDisplay
  └── InventoryPanel (默认 SetActive = false)
      ├── Background
      ├── InventoryGrid (GridLayoutGroup, 4×10)
      └── EquipmentPanel
```

**GridLayoutGroup 设置**（选中 InventoryGrid 后）
- Cell Size：50×50
- Spacing：5×5
- Constraint：Fixed Column Count = 10

---

#### Day 3-5：背包逻辑

**创建 `PlayerInventory.cs`**

```csharp
using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    // 单例模式：保证全局只有一个背包实例，方便其他脚本访问
    public static PlayerInventory Instance { get; private set; }

    [SerializeField] private int inventorySize = 40;  // 背包格子数量

    // 背包数据：每格存储（物品数据 + 数量）
    public List<InventorySlot> slots = new List<InventorySlot>();

    private void Awake()
    {
        // 单例模式实现
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 初始化空背包
        for (int i = 0; i < inventorySize; i++)
            slots.Add(new InventorySlot());
    }

    // 添加物品到背包
    // 返回值：实际添加的数量（背包满了可能添加不完）
    public int AddItem(ItemData item, int amount)
    {
        int remaining = amount;

        // 先尝试堆叠到已有的同类物品格子
        foreach (var slot in slots)
        {
            if (remaining <= 0) break;
            if (slot.itemData == item && slot.amount < item.maxStack)
            {
                int canAdd = item.maxStack - slot.amount;
                int actualAdd = Mathf.Min(canAdd, remaining);
                slot.amount += actualAdd;
                remaining -= actualAdd;
            }
        }

        // 再放入空格子
        foreach (var slot in slots)
        {
            if (remaining <= 0) break;
            if (slot.itemData == null)
            {
                int actualAdd = Mathf.Min(item.maxStack, remaining);
                slot.itemData = item;
                slot.amount = actualAdd;
                remaining -= actualAdd;
            }
        }

        int addedAmount = amount - remaining;
        if (addedAmount > 0)
            Debug.Log($"背包获得 {item.itemName} ×{addedAmount}");

        return addedAmount;
    }

    // 消耗物品（用于合成）
    public bool ConsumeItem(ItemData item, int amount)
    {
        // 先检查是否有足够数量
        if (GetItemCount(item) < amount) return false;

        int remaining = amount;
        foreach (var slot in slots)
        {
            if (remaining <= 0) break;
            if (slot.itemData == item)
            {
                int consume = Mathf.Min(slot.amount, remaining);
                slot.amount -= consume;
                remaining -= consume;
                if (slot.amount <= 0)
                    slot.itemData = null;  // 清空格子
            }
        }
        return true;
    }

    // 统计某物品总数量
    public int GetItemCount(ItemData item)
    {
        int count = 0;
        foreach (var slot in slots)
            if (slot.itemData == item)
                count += slot.amount;
        return count;
    }
}

// 背包格子数据类（可序列化，方便存档）
[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;  // null 表示空格
    public int amount;
}
```

**本周自检清单**
- [ ] 按 E 键显示/隐藏背包
- [ ] 拾取物品后背包格子显示图标和数量
- [ ] 同类物品自动堆叠
- [ ] 背包满了掉落物留在地上

---

#### Day 5 补充：装备面板（GDD §4.4）

GDD 定义了 8 个装备槽位（头盔/胸甲/护腿/鞋子/副手/饰品1/饰品2/武器）。在背包 UI 右侧增加装备面板。

**创建 `PlayerEquipment.cs`**

```csharp
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public static PlayerEquipment Instance { get; private set; }

    // GDD §4.4 装备槽位
    public ItemData helmet;
    public ItemData chestplate;
    public ItemData leggings;
    public ItemData boots;
    public ItemData offhand;      // 盾牌/法器
    public ItemData accessory1;
    public ItemData accessory2;
    public ItemData weapon;       // 当前手持武器

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 装备一件物品
    public bool Equip(ItemData item)
    {
        if (item.itemType != ItemType.Tool && item.itemType != ItemType.Weapon
            && item.itemType != ItemType.Armor) return false;

        // 根据物品类型放入对应槽位（简化版，完整版需 item 自带槽位标记）
        switch (item.itemType)
        {
            case ItemType.Weapon: weapon = item; break;
            case ItemType.Armor: chestplate = item; break;  // 后续按子类型细分
            default: return false;
        }
        return true;
    }

    // 获取当前攻击力（基础 + 装备加成）
    public int GetAttackBonus()
    {
        int bonus = 0;
        if (weapon != null) bonus += weapon.damage;
        return bonus;
    }

    // 获取当前防御力
    public int GetDefense()
    {
        int defense = 0;
        // 遍历所有已装备的 Armor 类型物品累加防御力
        if (helmet != null && helmet.itemType == ItemType.Armor) defense += helmet.damage;
        if (chestplate != null && chestplate.itemType == ItemType.Armor) defense += chestplate.damage;
        if (leggings != null && leggings.itemType == ItemType.Armor) defense += leggings.damage;
        if (boots != null && boots.itemType == ItemType.Armor) defense += boots.damage;
        return defense;
    }
}
```

> UI 实现：在 InventoryPanel 右侧放 8 个装备槽 Image，拖拽物品到槽位触发 `Equip()`。具体拖拽实现参考 Unity EventSystems `IBeginDragHandler` / `IDropHandler` 接口，此处不展开。

---

### Week 7：敌人 AI（史莱姆）

**本周学习目标**
- 有限状态机（FSM）设计模式
- Physics2D 碰撞检测
- 敌人生成与死亡流程

---

#### 创建 `EnemyFSM.cs`（通用 FSM 基类）

```csharp
using UnityEngine;

// 状态枚举
public enum EnemyState { Idle, Patrol, Chase, Attack, Hurt, Dead }

// 敌人基类，所有敌人都继承这个
public abstract class EnemyBase : MonoBehaviour
{
    [Header("基础属性")]
    public int maxHP = 10;
    public int currentHP;
    public int damage = 5;
    public float moveSpeed = 2f;
    public float detectionRange = 6f;   // 发现玩家的距离
    public float attackRange = 1f;      // 攻击距离

    [Header("掉落")]
    public ItemData[] dropItems;        // 可能掉落的物品
    public int[] dropAmounts;
    [Range(0, 1)] public float[] dropChances;  // 每种物品的掉落概率

    protected EnemyState currentState = EnemyState.Patrol;
    protected Transform playerTransform;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHP = maxHP;
    }

    protected virtual void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    protected virtual void Update()
    {
        if (currentState == EnemyState.Dead) return;

        // 每帧执行当前状态逻辑
        switch (currentState)
        {
            case EnemyState.Patrol: OnPatrol(); break;
            case EnemyState.Chase: OnChase(); break;
            case EnemyState.Attack: OnAttack(); break;
        }

        // 检查状态转换条件
        UpdateStateTransitions();
    }

    // 子类实现具体行为
    protected abstract void OnPatrol();
    protected abstract void OnChase();
    protected abstract void OnAttack();

    // 状态转换逻辑
    protected virtual void UpdateStateTransitions()
    {
        if (playerTransform == null) return;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (currentState != EnemyState.Attack && distanceToPlayer <= attackRange)
            ChangeState(EnemyState.Attack);
        else if (currentState == EnemyState.Attack && distanceToPlayer > attackRange * 1.5f)
            ChangeState(EnemyState.Chase);
        else if (currentState == EnemyState.Patrol && distanceToPlayer <= detectionRange)
            ChangeState(EnemyState.Chase);
        else if (currentState == EnemyState.Chase && distanceToPlayer > detectionRange * 1.5f)
            ChangeState(EnemyState.Patrol);
    }

    protected void ChangeState(EnemyState newState)
    {
        currentState = newState;
    }

    // 受到伤害（由玩家攻击系统调用）
    public virtual void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        // 显示伤害数字（后续添加）
        if (currentHP <= 0)
            Die();
    }

    protected virtual void Die()
    {
        ChangeState(EnemyState.Dead);
        // 生成掉落物
        DropItems();
        // 播放死亡动画（后续）
        Destroy(gameObject, 0.5f);
    }

    private void DropItems()
    {
        for (int i = 0; i < dropItems.Length; i++)
        {
            if (Random.value <= dropChances[i])
            {
                // 生成掉落物（这里简化，实际要用对象池）
                // 省略生成代码，原理同挖掘掉落
                Debug.Log($"掉落 {dropItems[i].itemName} ×{dropAmounts[i]}");
            }
        }
    }
}
```

**创建 `SlimeEnemy.cs`（史莱姆，继承基类）**

```csharp
using UnityEngine;

public class SlimeEnemy : EnemyBase
{
    [Header("史莱姆特有设置")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float jumpInterval = 1.5f;  // 跳跃间隔

    private float jumpTimer;
    private bool isGrounded;
    private float patrolDirection = 1f;  // 初始巡逻方向
    private float patrolTimer;

    protected override void OnPatrol()
    {
        // 无目的地左右跳跃
        jumpTimer -= Time.deltaTime;
        patrolTimer -= Time.deltaTime;

        if (patrolTimer <= 0)
        {
            patrolDirection *= -1;  // 随机掉头
            patrolTimer = Random.Range(2f, 5f);
        }

        if (jumpTimer <= 0 && isGrounded)
        {
            rb.linearVelocity = new Vector2(patrolDirection * moveSpeed, jumpForce);
            jumpTimer = jumpInterval;
            isGrounded = false;
        }
        spriteRenderer.flipX = patrolDirection < 0;
    }

    protected override void OnChase()
    {
        // 朝玩家方向跳跃追击
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0 && isGrounded)
        {
            float dirX = (playerTransform.position.x - transform.position.x) > 0 ? 1f : -1f;
            rb.linearVelocity = new Vector2(dirX * moveSpeed * 1.3f, jumpForce);
            jumpTimer = jumpInterval * 0.8f;  // 追击时跳得更频繁
            isGrounded = false;
            spriteRenderer.flipX = dirX < 0;
        }
    }

    protected override void OnAttack()
    {
        // 史莱姆没有独立攻击动作，靠身体接触伤害
        // 接触伤害在 OnCollisionEnter2D 中处理
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 落地检测
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            isGrounded = true;

        // 撞到玩家造成伤害
        if (collision.gameObject.CompareTag("Player"))
        {
            // collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Debug.Log($"史莱姆对玩家造成 {damage} 伤害");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            isGrounded = false;
    }
}
```

**本周自检清单**
- [ ] 史莱姆在场景中跳跃
- [ ] 靠近玩家后追击
- [ ] 碰触玩家（控制台）显示伤害
- [ ] 被攻击（暂时用 Inspector 手动减 HP）后死亡消失

---

### Week 8：战斗系统 + HP/MP + 状态效果

**本周学习目标**
- 玩家攻击判定（近战挥砍碰撞框）
- HP/MP 系统（GDD §4.1）
- 无敌帧 iFrame + 闪烁动画（GDD §5.3）
- 伤害数字飘字
- 状态效果框架（GDD §5.4）

---

#### Day 1-2：HP/MP 系统

**创建 `PlayerStats.cs`**（符合 GDD §4.1 属性表）

```csharp
using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("生命值（GDD §4.1）")]
    public int maxHP = 100;         // 初始 100，可提升至 400
    public int currentHP;
    [SerializeField] private float hpRegenRate = 0f;  // 每秒回复（默认无，药水/装备可提供）

    [Header("魔法值（GDD §4.1）")]
    public int maxMP = 20;          // 初始 20，可提升至 200
    public int currentMP;
    [SerializeField] private float mpRegenRate = 1f;   // 每秒回复 1 MP

    [Header("战斗属性")]
    public int baseAttack = 5;      // 基础攻击力
    public int defense = 0;         // 防御力（由装备提供）

    [Header("无敌帧（GDD §5.3）")]
    [SerializeField] private float invincibilityDuration = 0.5f;  // 受击后无敌 0.5 秒
    private float invincibilityTimer;
    public bool IsInvincible => invincibilityTimer > 0f;

    // 事件：供 UI 和动画监听
    public event Action<int> OnHPChanged;
    public event Action<int> OnMPChanged;
    public event Action OnPlayerDeath;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        currentHP = maxHP;
        currentMP = maxMP;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // 无敌帧倒计时
        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
            // 闪烁效果：无敌时精灵半透明闪烁（GDD §5.3）
            float alpha = Mathf.PingPong(Time.time * 10f, 1f);
            spriteRenderer.color = new Color(1, 1, 1, 0.3f + alpha * 0.7f);
        }
        else
        {
            spriteRenderer.color = Color.white;  // 恢复正常
        }

        // MP 自然回复
        if (currentMP < maxMP)
        {
            currentMP = Mathf.Min(maxMP, currentMP + mpRegenRate * Time.deltaTime);
            OnMPChanged?.Invoke(currentMP);
        }

        // HP 自然回复（如果有回复装备/药水）
        if (currentHP < maxHP && hpRegenRate > 0)
        {
            currentHP = Mathf.Min(maxHP, currentHP + Mathf.RoundToInt(hpRegenRate * Time.deltaTime));
            OnHPChanged?.Invoke(currentHP);
        }
    }

    // 受到伤害（GDD §5.2 伤害公式）
    public void TakeDamage(int rawDamage)
    {
        if (IsInvincible) return;  // 无敌帧保护

        // 伤害公式：最终伤害 = (武器伤害 ±15%浮动) - 防御力
        float randomMultiplier = UnityEngine.Random.Range(0.85f, 1.15f);
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(rawDamage * randomMultiplier) - defense);

        currentHP = Mathf.Max(0, currentHP - finalDamage);
        invincibilityTimer = invincibilityDuration;

        OnHPChanged?.Invoke(currentHP);
        Debug.Log($"受到了 {finalDamage} 伤害！剩余 HP: {currentHP}");

        if (currentHP <= 0)
        {
            OnPlayerDeath?.Invoke();
            Debug.Log("玩家死亡！");
        }
    }

    // 恢复 HP（药水/NPC 护士）
    public void HealHP(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        OnHPChanged?.Invoke(currentHP);
    }

    // 消耗 MP（魔法武器使用）
    public bool ConsumeMP(int amount)
    {
        if (currentMP < amount) return false;
        currentMP -= amount;
        OnMPChanged?.Invoke(currentMP);
        return true;
    }

    // 永久提升最大 HP（GDD：击败怪物/使用物品）
    public void IncreaseMaxHP(int amount)
    {
        maxHP = Mathf.Min(400, maxHP + amount);  // GDD 上限 400
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        OnHPChanged?.Invoke(currentHP);
    }

    public void IncreaseMaxMP(int amount)
    {
        maxMP = Mathf.Min(200, maxMP + amount);  // GDD 上限 200
        currentMP = Mathf.Min(currentMP + amount, maxMP);
        OnMPChanged?.Invoke(currentMP);
    }
}
```

---

#### Day 3：状态效果系统（GDD §5.4）

```csharp
using UnityEngine;
using System.Collections.Generic;

// 状态效果基类
[System.Serializable]
public class StatusEffect
{
    public string effectName;
    public float duration;       // 剩余持续时间（秒）
    public float tickInterval;   // 每跳间隔（DoT 效果用）
    private float tickTimer;

    public bool IsExpired => duration <= 0f;

    // 每帧更新（由 StatusEffectManager 调用）
    public void Update(float deltaTime, PlayerStats target)
    {
        duration -= deltaTime;
        tickTimer -= deltaTime;
        if (tickTimer <= 0f && tickInterval > 0f)
        {
            tickTimer = tickInterval;
            OnTick(target);
        }
    }

    // 每跳效果（子类重写）
    protected virtual void OnTick(PlayerStats target) { }

    // 应用时立即生效的效果
    public virtual void OnApply(PlayerStats target) { }
    // 移除时清理效果
    public virtual void OnRemove(PlayerStats target) { }
}

// 具体状态效果示例（GDD §5.4）
public class PoisonEffect : StatusEffect
{
    public PoisonEffect() { effectName = "中毒"; duration = 10f; tickInterval = 1f; }
    protected override void OnTick(PlayerStats target) => target.TakeDamage(2);
}

public class BurnEffect : StatusEffect
{
    public BurnEffect() { effectName = "燃烧"; duration = 5f; tickInterval = 0.5f; }
    protected override void OnTick(PlayerStats target) => target.TakeDamage(5);
}

public class SlowEffect : StatusEffect
{
    public SlowEffect() { effectName = "减速"; duration = 3f; }
    public override void OnApply(PlayerStats target)
    {
        // 移动速度 -30%（需配合 PlayerController 读取此状态）
    }
}

public class RegenEffect : StatusEffect
{
    public RegenEffect() { effectName = "再生"; duration = 8f; tickInterval = 1f; }
    protected override void OnTick(PlayerStats target) => target.HealHP(2);
}

// 玩家身上的状态效果管理器
public class StatusEffectManager : MonoBehaviour
{
    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    private PlayerStats playerStats;

    private void Awake() => playerStats = GetComponent<PlayerStats>();

    private void Update()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].Update(Time.deltaTime, playerStats);
            if (activeEffects[i].IsExpired)
            {
                activeEffects[i].OnRemove(playerStats);
                activeEffects.RemoveAt(i);
            }
        }
    }

    public void ApplyEffect(StatusEffect effect)
    {
        // 同类型效果刷新持续时间（不叠加）
        var existing = activeEffects.Find(e => e.effectName == effect.effectName);
        if (existing != null)
            existing.duration = effect.duration;
        else
        {
            effect.OnApply(playerStats);
            activeEffects.Add(effect);
        }
    }
}
```

---

#### Day 4-5：近战攻击系统（整合 HP/MP/状态效果）

```csharp
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("近战设置")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1.2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("攻击冷却")]
    [SerializeField] private float attackCooldown = 0.5f;
    private float lastAttackTime;

    [Header("暴击（GDD §5.2）")]
    [Range(0, 1)] public float critChance = 0.04f;   // 默认 4% 暴击率
    public float critMultiplier = 2.0f;

    private PlayerStats playerStats;
    private PlayerEquipment equipment;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        equipment = GetComponent<PlayerEquipment>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time - lastAttackTime >= attackCooldown)
        {
            // 区分挖掘和攻击：手持工具时挖掘，手持武器时攻击
            if (equipment != null && equipment.weapon != null)
                MeleeAttack();
            // else：由 TileInteraction 处理挖掘（见 Week 2）
        }
    }

    private void MeleeAttack()
    {
        lastAttackTime = Time.time;

        // 获取武器基础伤害 + 玩家基础攻击力（GDD §5.2）
        int weaponDamage = equipment?.weapon?.damage ?? 5;
        int baseDamage = playerStats.baseAttack + weaponDamage;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position, attackRadius, enemyLayer);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            // 伤害公式 (GDD §5.2):
            // 最终伤害 = (基础伤害 + 随机浮动 ±15%) × 暴击系数 - 敌人防御
            float randomMultiplier = Random.Range(0.85f, 1.15f);
            bool isCrit = Random.value < critChance;
            float critMod = isCrit ? critMultiplier : 1f;
            int finalDamage = Mathf.RoundToInt(baseDamage * randomMultiplier * critMod);

            enemyCollider.GetComponent<EnemyBase>()?.TakeDamage(finalDamage);
            Debug.Log($"{(isCrit ? "暴击! " : "")}攻击了 {enemyCollider.name}，造成 {finalDamage} 伤害");
        }
    }

    // 魔法攻击（GDD §5.1 — 消耗 MP）
    public void MagicAttack(int spellDamage, int mpCost)
    {
        if (!playerStats.ConsumeMP(mpCost))
        {
            Debug.Log("MP 不足！");
            return;
        }

        // 魔法攻击判定（范围更大，伤害更高）
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position, attackRadius * 2f, enemyLayer);
        foreach (Collider2D hit in hitEnemies)
            hit.GetComponent<EnemyBase>()?.TakeDamage(spellDamage);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
```

**本周自检清单**
- [ ] 左键攻击有冷却时间
- [ ] 攻击范围内的敌人受到伤害（含暴击判定）
- [ ] HP 条随伤害减少，MP 条随使用减少
- [ ] 受击后 0.5 秒无敌帧（精灵闪烁）
- [ ] 玩家 HP 归零时显示死亡信息
- [ ] 状态下中毒/燃烧 DoT 每跳生效

---

### Week 9：合成系统

**本周学习目标**
- ScriptableObject 存储合成配方
- 合成 UI 面板
- 玩家靠近合成台时显示可用配方

---

#### 合成配方数据

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "TerraUnity/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("产出")]
    public ItemData outputItem;
    public int outputAmount = 1;

    [Header("所需材料")]
    public CraftingIngredient[] ingredients;

    [Header("需要的合成台")]
    public CraftingStationType requiredStation;

    // 检查玩家是否有足够材料
    public bool CanCraft(PlayerInventory inventory)
    {
        foreach (var ingredient in ingredients)
        {
            if (inventory.GetItemCount(ingredient.item) < ingredient.amount)
                return false;
        }
        return true;
    }

    // 执行合成
    public bool Craft(PlayerInventory inventory)
    {
        if (!CanCraft(inventory)) return false;

        // 消耗材料
        foreach (var ingredient in ingredients)
            inventory.ConsumeItem(ingredient.item, ingredient.amount);

        // 给予产出
        inventory.AddItem(outputItem, outputAmount);
        Debug.Log($"合成了 {outputItem.itemName} ×{outputAmount}");
        return true;
    }
}

[System.Serializable]
public class CraftingIngredient
{
    public ItemData item;
    public int amount;
}

public enum CraftingStationType { Hand, Workbench, Furnace, Anvil }
```

**本周自检清单**
- [ ] 有木材时能合成木镐
- [ ] 合成界面显示所需材料（红色=不足，绿色=足够）
- [ ] 合成成功消耗材料，背包增加产出物品
- [ ] 靠近对应合成台时才显示该台的可用配方（GDD §6.3）

---

#### Day 4-5 补充：合成台的建造与放置（GDD §6.3）

GDD 定义了 5 种合成台：工作台/熔炉/铁砧/法术台/暗铁台。每种合成台是一个可放置的 Tile / GameObject。

```csharp
public class CraftingStation : MonoBehaviour
{
    public CraftingStationType stationType;
    public float interactionRange = 2f;   // 交互距离
    public SpriteRenderer spriteRenderer;

    private bool playerInRange;

    private void Update()
    {
        // 检测玩家是否在交互范围内
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);
        playerInRange = dist <= interactionRange;

        // 高亮提示
        if (spriteRenderer != null)
            spriteRenderer.color = playerInRange ? Color.yellow : Color.white;
    }

    private void OnMouseDown()
    {
        if (playerInRange)
        {
            // 打开合成 UI，并只显示该合成台的配方
            CraftingUI.Instance.ShowRecipesFor(stationType);
        }
    }

    // 玩家在背包中选择合成台物品，"使用"时在世界中放置
    public static void PlaceStation(CraftingStationType type, Vector3 worldPos)
    {
        // Instantiate 对应预制件（需在 Inspector 中配置映射）
        // 或对 Tilemap 设置特殊 Tile
    }
}
```

> 放置逻辑：玩家在快捷栏选中合成台物品 → 点击地面 → 在鼠标位置生成合成台 GameObject。

---

### Week 10：NPC 基础 + 白天黑夜

**本周学习目标**
- 简单 NPC 商店 UI
- 昼夜循环（更改环境光）
- 夜晚触发不同敌人生成

---

#### 昼夜循环

```csharp
using UnityEngine;
using UnityEngine.Rendering.Universal; // 需要 Universal RP

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private float dayDurationSeconds = 300f;  // 一天 = 5 分钟
    [SerializeField] private Light2D globalLight;

    [Header("光照颜色")]
    [SerializeField] private Color dayColor = new Color(1f, 0.95f, 0.8f);     // 白天暖白
    [SerializeField] private Color nightColor = new Color(0.1f, 0.1f, 0.3f);  // 夜晚深蓝

    // 0 = 午夜，0.5 = 正午，1 = 下一个午夜
    public float TimeOfDay { get; private set; } = 0.25f;  // 从早上 6 点开始

    private void Update()
    {
        TimeOfDay = (TimeOfDay + Time.deltaTime / dayDurationSeconds) % 1f;

        // 用 Sin 曲线模拟光照强度（正午最亮）
        float intensity = Mathf.Clamp01(Mathf.Sin(TimeOfDay * Mathf.PI * 2f) + 0.5f);
        globalLight.intensity = Mathf.Lerp(0.05f, 1f, intensity);
        globalLight.color = Color.Lerp(nightColor, dayColor, intensity);
    }

    public bool IsNight => TimeOfDay < 0.2f || TimeOfDay > 0.8f;
}
```

**本周自检清单**
- [ ] 游戏中有昼夜变化（光照颜色和亮度改变）
- [ ] 可以与 NPC 交互开启简单商店
- [ ] 夜晚控制台提示"夜晚模式"

---

#### NPC 房间入住系统（GDD §8.1）

GDD 要求 NPC 入住条件为：房间至少 6×10 Tile，有墙壁、门、灯、桌、椅。

```csharp
using UnityEngine;
using System.Collections.Generic;

public class RoomDetector : MonoBehaviour
{
    // 房间最小要求（GDD §8.1）：6×10
    private const int MIN_ROOM_WIDTH = 6;
    private const int MIN_ROOM_HEIGHT = 10;

    [System.Serializable]
    public class Room
    {
        public RectInt bounds;           // 房间矩形区域（Tile 坐标）
        public bool hasWalls;            // 四面被 Tile 封闭
        public bool hasDoor;             // 有门 Tile
        public bool hasLightSource;      // 有火把/灯
        public bool hasTable;            // 有桌子
        public bool hasChair;            // 有椅子
        public NPCData assignedNPC;      // 已分配的 NPC
        public Vector3 spawnPoint;       // NPC 出生位置（世界坐标）

        public bool IsValid => bounds.width >= MIN_ROOM_WIDTH
            && bounds.height >= MIN_ROOM_HEIGHT
            && hasWalls && hasDoor && hasLightSource && hasTable && hasChair
            && assignedNPC == null;      // 房间未被占用
    }

    public List<Room> detectedRooms = new List<Room>();

    // 检查一个封闭区域是否构成有效房间
    public Room DetectRoom(Vector3Int seedCell, Tilemap wallTilemap)
    {
        // 用 Flood Fill 从 seedCell 向四面探测，直到碰到墙壁 Tile
        // 统计内部物件（火把=LightSource, 桌子=Table, 椅子=Chair, 门=Door）
        // 若满足所有条件且尺寸 ≥ 6×10，返回 Room 对象
        // 完整 Flood Fill 实现略（使用 Queue<Vector3Int> BFS）
        return null;
    }

    // 每放置/破坏一个 Tile 后重新检测
    public void OnTileChanged(Vector3Int cellPos)
    {
        detectedRooms.Clear();
        // 以该位置为中心重新检测附近区域...
    }
}
```

> 简化版：玩家在背包中使用"NPC 召唤物"时，只需检测玩家附近是否有封闭房间；满足条件则 NPC 出现在房间中。

**本周额外自检**
- [ ] 建造封闭房间后，使用 NPC 召唤物成功生成 NPC
- [ ] 房间不满足条件时提示缺少什么（如"缺少光源"）

---

## 第 3 阶段：进阶内容（第 11-14 周）

---

### Week 11：更多地形 + 多生物群系

**本周目标**：完善世界生成，加入洞穴、矿脉、多生物群系

#### Day 1：RuleTile 自动匹配地形（GDD §3.3）

> GDD 推荐使用 RuleTile 让相邻 Tile 自动选择正确贴图（如草地顶部圆角、泥土无缝衔接）。

**安装 2D Extras**
- Window → Package Manager → 搜索 `2D Extras` → Install

**创建 RuleTile**
1. Project → Create → 2D → Tiles → Rule Tile（或 Animated Tile）
2. 设置规则：如"当上方是空气且左右是草地时，显示草地顶部边缘贴图"
3. 在 TilePalette 中使用 RuleTile 代替普通 Tile 绘制

```csharp
// RuleTile 不需要额外代码，完全在 Inspector 中配置规则
// 参考：Unity 官方文档 "RuleTile"
```

#### Day 2-5：改进世界生成 + 洞穴 + 生物群系

```csharp
// 在 WorldGenerator 中加入洞穴生成
private bool IsCave(int x, int y)
{
    // 多层 Perlin Noise 叠加生成洞穴
    float noise1 = Mathf.PerlinNoise((x + seed) * 0.08f, (y + seed) * 0.08f);
    float noise2 = Mathf.PerlinNoise((x + seed + 1000) * 0.04f, (y + seed + 1000) * 0.04f);

    // 深层（y < 40）洞穴更多
    float threshold = 0.55f - (y < 40 ? 0.1f : 0f);

    return noise1 * noise2 < threshold;
}
```

#### 生物群系系统

```csharp
public enum BiomeType { Forest, Desert, Snow, Mushroom, Corruption }

private BiomeType GetBiomeAt(int x)
{
    // 根据 x 坐标分配生物群系（简单版本：分段划分）
    float t = (float)x / worldWidth;
    if (t < 0.2f) return BiomeType.Forest;
    if (t < 0.4f) return BiomeType.Desert;
    if (t < 0.6f) return BiomeType.Forest;   // 中间是森林
    if (t < 0.8f) return BiomeType.Snow;
    return BiomeType.Corruption;
}
```

**本周自检清单**
- [ ] 世界中有自然洞穴
- [ ] 地下有铜矿、铁矿分布
- [ ] 不同区域地表贴图不同（沙子/雪地/紫草）

---

### Week 12：Boss 1 — 眼球之王

**本周目标**：设计并实现第一个 Boss

**Boss 设计要点**

```csharp
public class EyeKingBoss : EnemyBase
{
    [Header("Boss 阶段")]
    private int phase = 1;  // 1 = 正常，2 = 狂暴

    [Header("移动")]
    [SerializeField] private float hoverSpeed = 3f;
    [SerializeField] private float dashSpeed = 12f;

    private bool isDashing = false;
    private float dashTimer;

    protected override void OnChase()
    {
        if (phase == 1)
        {
            // 第一阶段：悬浮追击，偶尔俯冲
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * hoverSpeed;
        }
        else
        {
            // 第二阶段：更快的悬浮 + 频繁俯冲
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * hoverSpeed * 1.5f;
        }
    }

    public override void TakeDamage(int dmg)
    {
        base.TakeDamage(dmg);

        // 进入第二阶段
        if (currentHP < maxHP * 0.5f && phase == 1)
        {
            phase = 2;
            // 触发变形动画
            Debug.Log("眼球之王进入狂暴阶段！");
        }
    }

    protected override void OnAttack() { /* 俯冲实现 */ }
    protected override void OnPatrol() { OnChase(); }
}
```

**本周自检清单**
- [ ] Boss 有两个阶段（50% HP 切换）
- [ ] 击败 Boss 后有掉落物
- [ ] Boss 的 HP 条显示在 UI 顶部

---

### Week 13：存档系统

**本周目标**：实现完整的存档/读档功能

```csharp
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // 存档文件保存路径（游戏自动管理的持久化目录）
    private string SavePath => Path.Combine(Application.persistentDataPath, "saves");

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);  // 切换场景时不销毁
    }

    public void SaveGame(int slot = 0)
    {
        // 收集需要存档的数据
        SaveData data = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            playerHP = FindObjectOfType<PlayerHealth>().currentHP,
            playTime = Time.time,
            // ... 其他数据
        };

        // 序列化为 JSON 字符串
        string json = JsonUtility.ToJson(data, prettyPrint: true);

        // 写入文件
        Directory.CreateDirectory(SavePath);  // 确保目录存在
        string filePath = Path.Combine(SavePath, $"save_{slot}.json");
        File.WriteAllText(filePath, json);

        Debug.Log($"游戏已保存到 {filePath}");
    }

    public SaveData LoadGame(int slot = 0)
    {
        string filePath = Path.Combine(SavePath, $"save_{slot}.json");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("存档文件不存在！");
            return null;
        }

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<SaveData>(json);
    }
}

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public int playerHP;
    public float playTime;
    public string[] defeatedBosses;
    // 背包数据等（后续完善）
}
```

**本周自检清单**
- [ ] 按 F5 保存游戏（手动存档）
- [ ] 每 5 分钟自动存档（GDD §10.3）
- [ ] 重启游戏后按 F9 读取，玩家出现在保存时的位置
- [ ] 支持 3 个存档槽（GDD §10.3）
- [ ] 存档文件在系统对应目录中能看到

---

#### 自动存档实现（GDD §10.3）

在 SaveManager 中添加自动存档逻辑：

```csharp
// 在 SaveManager 中添加以下字段和方法：

[SerializeField] private float autoSaveInterval = 300f;  // 5 分钟（GDD §10.3）
private float autoSaveTimer;

private void Start()
{
    autoSaveTimer = autoSaveInterval;
}

private void Update()
{
    autoSaveTimer -= Time.deltaTime;
    if (autoSaveTimer <= 0f)
    {
        SaveGame(0);  // 自动存档到槽位 0
        autoSaveTimer = autoSaveInterval;
        Debug.Log("自动存档完成");
    }
}

// 游戏退出时存档（在 OnApplicationQuit 中调用）
private void OnApplicationQuit()
{
    SaveGame(0);
}
```

**GDD §10.3 补充**：进入/离开游戏时自动存档。在 Scene 加载时调用 `LoadGame`，场景切换前调用 `SaveGame`。

---

### Week 14：UI 完善 + 小地图

**本周目标**：完成完整的 HUD、背包 UI、死亡界面、小地图（GDD §9.3）

#### 小地图实现（GDD §9.3）

> GDD 要求：右上角显示探索过的区域（Fog of War），白点=玩家，绿点=NPC，红点=Boss，按 M 切换全屏地图。

```csharp
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    [Header("小地图设置")]
    [SerializeField] private RawImage minimapDisplay;         // 显示用 RawImage
    [SerializeField] private GameObject fullMapPanel;         // 全屏地图面板（M 键）
    [SerializeField] private int minimapPixelSize = 128;      // 小地图像素尺寸
    [SerializeField] private int fullMapPixelSize = 512;      // 全屏地图尺寸
    [SerializeField] private float worldCoverage = 100f;      // 小地图覆盖的世界范围

    [Header("图标")]
    [SerializeField] private Sprite playerIcon;
    [SerializeField] private Sprite npcIcon;
    [SerializeField] private Sprite bossIcon;

    // Fog of War：已探索区域纹理
    private Texture2D fogTexture;
    private Color32[] fogPixels;     // alpha=0 = 未探索, alpha=255 = 已探索
    private bool[,] exploredChunks;  // 每 Chunk 是否探索过

    private Transform playerTransform;
    private Camera minimapCamera;    // 专用相机渲染小地图（或手动绘制）

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // 初始化 Fog of War 纹理
        fogTexture = new Texture2D(fullMapPixelSize, fullMapPixelSize, TextureFormat.RGBA32, false);
        fogPixels = new Color32[fullMapPixelSize * fullMapPixelSize];
        for (int i = 0; i < fogPixels.Length; i++)
            fogPixels[i] = new Color32(0, 0, 0, 0);  // 全透明 = 未探索
        fogTexture.SetPixels32(fogPixels);
        fogTexture.Apply();

        fullMapPanel.SetActive(false);
    }

    private void Update()
    {
        // 更新 Fog of War：以玩家为中心，揭露周围区域
        UpdateFogOfWar();

        // M 键切换全屏地图
        if (Input.GetKeyDown(KeyCode.M))
            fullMapPanel.SetActive(!fullMapPanel.activeSelf);

        // 绘制小地图图标（略，见下方辅助方法）
    }

    private void UpdateFogOfWar()
    {
        // 将玩家世界坐标映射到 Fog 纹理坐标
        Vector2Int texCoord = WorldToFogCoord(playerTransform.position);

        // 以玩家为中心，揭露半径 R 的圆形区域
        int revealRadius = 8;  // Fog 纹理像素半径
        for (int dx = -revealRadius; dx <= revealRadius; dx++)
            for (int dy = -revealRadius; dy <= revealRadius; dy++)
            {
                if (dx * dx + dy * dy > revealRadius * revealRadius) continue;  // 圆形
                int px = texCoord.x + dx;
                int py = texCoord.y + dy;
                if (px < 0 || px >= fullMapPixelSize || py < 0 || py >= fullMapPixelSize) continue;
                fogPixels[py * fullMapPixelSize + px] = new Color32(0, 0, 0, 255);  // 已探索
            }

        fogTexture.SetPixels32(fogPixels);
        fogTexture.Apply();
        minimapDisplay.texture = fogTexture;
    }

    private Vector2Int WorldToFogCoord(Vector3 worldPos)
    {
        // 将世界坐标映射到 [0, fullMapPixelSize] 范围
        float normalizedX = (worldPos.x + 2100) / 4200f;  // GDD 世界宽度 4200
        float normalizedY = worldPos.y / 1200f;           // GDD 世界高度 1200
        return new Vector2Int(
            Mathf.FloorToInt(normalizedX * fullMapPixelSize),
            Mathf.FloorToInt(normalizedY * fullMapPixelSize));
    }

    // 绘制 NPC/Boss/玩家图标到小地图上（每帧更新图标位置）
    private void DrawIcons() { /* 略：用 RawImage 上的子 RectTransform 标记位置 */ }
}
```

**死亡界面实现**

```csharp
public class DeathScreen : MonoBehaviour
{
    [SerializeField] private GameObject deathPanel;   // 死亡面板 GameObject
    [SerializeField] private TMPro.TextMeshProUGUI deathReasonText;
    [SerializeField] private TMPro.TextMeshProUGUI surviveTimeText;

    public void ShowDeathScreen(string reason, float surviveTime)
    {
        deathPanel.SetActive(true);
        Time.timeScale = 0;  // 暂停游戏

        deathReasonText.text = $"死亡原因：{reason}";
        surviveTimeText.text = $"存活时间：{Mathf.Floor(surviveTime / 60):00}:{surviveTime % 60:00}";
    }

    // "重新开始"按钮绑定这个方法
    public void OnRestartButton()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
```

**本周自检清单**
- [ ] HP 条显示正确的血量
- [ ] 快捷栏高亮当前选中格
- [ ] 死亡时显示死亡界面
- [ ] 重新开始能正常重置游戏

---

## 第 4 阶段：打磨（第 15-16 周）

---

### Week 15：音效与特效

**本周目标**：让游戏有声有色

#### AudioManager

```csharp
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SoundEntry
    {
        public string name;
        public AudioClip clip;
        [Range(0, 1)] public float volume = 1f;
        [Range(0.5f, 2f)] public float pitch = 1f;
    }

    [SerializeField] private SoundEntry[] sounds;
    private Dictionary<string, SoundEntry> soundDict;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        soundDict = new Dictionary<string, SoundEntry>();
        foreach (var s in sounds)
            soundDict[s.name] = s;
    }

    public void Play(string soundName)
    {
        if (!soundDict.TryGetValue(soundName, out var sound))
        {
            Debug.LogWarning($"找不到音效：{soundName}");
            return;
        }

        // 创建临时音频源播放后自动销毁
        var go = new GameObject($"Sound_{soundName}");
        var source = go.AddComponent<AudioSource>();
        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = sound.pitch + Random.Range(-0.05f, 0.05f);  // 轻微音调随机，避免重复感
        source.Play();
        Destroy(go, sound.clip.length);
    }
}

// 使用示例（在挖掘成功处调用）：
// AudioManager.Instance.Play("mine_stone");
```

**本周自检清单**
- [ ] 挖掘有音效
- [ ] 跳跃有音效
- [ ] 受伤有音效
- [ ] 背景音乐循环播放

---

### Week 16：性能优化 + Bug 修复 + 最终测试

**本周目标**：让游戏稳定运行

#### 对象池（解决频繁 Instantiate/Destroy 的性能问题）

```csharp
using UnityEngine;
using System.Collections.Generic;

// 通用对象池
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;  // 预创建数量
    }

    [SerializeField] private List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // 预创建对象
        foreach (var pool in pools)
        {
            var queue = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                var obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            poolDictionary[pool.tag] = queue;
        }
    }

    // 从池中取出对象（替代 Instantiate）
    public GameObject Get(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"对象池中没有 tag: {tag}");
            return null;
        }

        var obj = poolDictionary[tag].Dequeue();
        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        // 把对象放回队尾（循环复用）
        poolDictionary[tag].Enqueue(obj);
        return obj;
    }
}
```

#### 性能检测方法
1. 运行游戏，打开 **Profiler**（Window → Analysis → Profiler）
2. 观察 `CPU Usage` 图表，找出每帧最耗时的函数
3. 常见优化：
   - 减少 `FindObjectOfType` 的使用（改用缓存引用）
   - 减少每帧的物理查询（`OverlapCircle` 等）
   - 使用对象池代替 `Instantiate/Destroy`

**最终自检清单（发布前检查）**
- [ ] 帧率稳定在 60fps（Profiler 观察）
- [ ] 所有功能正常运行（每个系统单独测试一遍）
- [ ] 不存在明显 Bug（连续玩 30 分钟无崩溃）
- [ ] 存档读档正常
- [ ] 音效和背景音乐正常
- [ ] 可以构建成 `.exe` 文件运行（File → Build Settings → Build）

---

## GDD 对齐说明（v1.1 更新）

本文档已针对 GDD v1.0 进行了以下关键修复：

### 已修复项
| GDD 要求 | 修复前 | 修复后 |
|----------|--------|--------|
| §3.3 Chunk 系统 | 仅概念 | Week 3 有完整 ChunkManager 实现 |
| §4.3 挖掘进度条 | 瞬间挖掉 | Week 2 有进度条 + 硬度公式 |
| §4.4 装备槽位 | 无 | Week 6 有 PlayerEquipment (8槽) |
| §4.1 MP 魔法值 | 无 | Week 8 有 PlayerStats (HP+MP) |
| §5.3 无敌帧 | 无 | Week 8 有 0.5s iFrame + 闪烁 |
| §5.4 状态效果 | 无 | Week 8 有 StatusEffectManager |
| §6.1 物品稀有度 | 无 | Week 5 有 ItemRarity + 颜色 |
| §6.3 合成台建造 | 无 | Week 9 有 CraftingStation 放置 |
| §8.1 NPC 房间 | 只有商店 | Week 10 有 RoomDetector |
| §8.3 货币体系 | 无 | Week 5 有 PlayerCurrency (四级) |
| §9.3 小地图 | 标题空 | Week 14 有 Minimap + Fog of War |
| §10.3 自动存档 | 手动 | Week 13 有 5 分钟自动存档 |
| §3.3 RuleTile | 未提 | Week 11 有 RuleTile 使用说明 |

### 已知偏差（16 周时限内暂不实现）
| GDD 内容 | 说明 |
|----------|------|
| §7.3 Boss 2 (石之巨人) & Boss 3 (暗影领主) | 16 周只能完成 Boss 1。列为 Week 17+ 扩展内容 |
| §2.3 硬模式 (Hard Mode) | 需先完成 Boss 1，列为扩展内容 |
| §4.2 双段跳/钩爪/飞行 | 进阶移动，列为扩展内容 |
| §5.1 远程/投掷武器 | Week 8 仅实现近战和魔法，远程可复用模式 |

---

## 附录 A：常见新手错误汇总

| 错误 | 原因 | 解决方法 |
|------|------|----------|
| 角色在运行时飞出屏幕 | Rigidbody2D Gravity 太大，或没加碰撞 | 检查 Rigidbody2D 和 Collider2D |
| 地图整体往下掉 | Tilemap 的 Rigidbody2D 没设为 Static | Body Type → Static |
| GetComponent 每帧调用很慢 | 在 Update 中反复调用 | 在 Awake/Start 中缓存到变量 |
| 找不到 Player 对象 | Tag 拼写错误 | 确保 Tag 精确匹配，区分大小写 |
| 单例 NullReferenceException | 在 Start 之前访问了单例 | 把单例初始化放在 Awake，访问放在 Start |
| 存档路径不存在 | 直接写文件但目录不存在 | 写文件前调用 `Directory.CreateDirectory` |
| 攻击打不到敌人 | Layer Mask 没设置 | 检查 Inspector 中 LayerMask 是否勾选了 Enemy 层 |
| 相机看到世界边缘外 | 没配置 Cinemachine Confiner | 添加 CinemachineConfiner2D 并设置边界 |

---

## 附录 B：Unity 快捷键速查

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+P` | 运行/停止游戏 |
| `Ctrl+Z` | 撤销 |
| `F` | 聚焦到选中对象（Scene 视图中） |
| `Q/W/E/R` | 切换工具（选择/移动/旋转/缩放）|
| `Alt + 左键` | 旋转 Scene 视图（3D 模式）|
| `Ctrl+D` | 复制选中对象 |
| `Ctrl+S` | 保存场景 |
| `Ctrl+Shift+S` | 另存场景 |

---

## 附录 C：推荐学习视频（B 站搜索关键词）

| 阶段 | 搜索词 |
|------|--------|
| Week 1 | "Unity Tilemap 教程 2D" |
| Week 2 | "Unity Cinemachine 2D 跟随" |
| Week 3 | "Unity Perlin Noise 地图生成" |
| Week 4 | "Unity 2D 平台控制器 coyote time" |
| Week 5 | "Unity ScriptableObject 物品系统" |
| Week 6 | "Unity UGUI 背包系统" |
| Week 7 | "Unity 有限状态机 FSM 敌人 AI" |
| Week 9 | "Unity 合成系统" |
| Week 13 | "Unity 存档系统 JSON" |
| Week 15 | "Unity 对象池 Object Pool" |

---

*本文档为配套 GDD 的实操开发指南，根据学习进度可自由调整每周任务量。遇到问题不要放弃——每一个 Bug 都是一次学习机会！*
