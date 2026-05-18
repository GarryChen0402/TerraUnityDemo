using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileInteraction : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private float mineRange = 3f;
    [SerializeField] private GameObject itemDropPrefab;
    [SerializeField] private Slider miningProgressBar;

    [Header("Player Tool Properties")]
    public int currentMiningPower = 1;
    public int currentMiningLevel = 0;

    private Camera mainCamera;
    private Vector3Int currentTargetCell;
    private float miningProgress;

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
            ResetMining();
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

        TileData tileData = TileDataManager.Instance.GetTileData(tile);
        if (tileData == null) { ResetMining(); return; }

        if (currentMiningLevel < tileData.requiredMiningLevel)
        {
            Debug.Log($"Need mining level {tileData.requiredMiningLevel} or higher!");
            ResetMining();
            return;
        }

        if (cellPos != currentTargetCell)
        {
            currentTargetCell = cellPos;
            miningProgress = 0f;
        }

        if (miningProgressBar != null)
        {
            miningProgressBar.gameObject.SetActive(true);
            miningProgressBar.transform.position = Input.mousePosition + new Vector3(0, 30, 0);
        }

        float mineSpeed = currentMiningPower / tileData.hardness;
        miningProgress += mineSpeed * Time.deltaTime;

        if (miningProgressBar != null)
            miningProgressBar.value = miningProgress;

        if (miningProgress >= 1f)
        {
            groundTilemap.SetTile(cellPos, null);
            ChunkManager.Instance.MarkTileRemoved(cellPos);

            Vector3 dropPos = groundTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
            GameObject drop = Instantiate(itemDropPrefab, dropPos, Quaternion.identity);
            ItemDrop itemDrop = drop.GetComponent<ItemDrop>();
            if (itemDrop != null)
            {
                itemDrop.itemData = tileData.dropItem;
                itemDrop.amount = tileData.dropAmount;
            }

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
