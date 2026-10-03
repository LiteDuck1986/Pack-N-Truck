using UnityEngine;
using UnityEngine.Tilemaps;

public class VehicleTerrainSpeed : MonoBehaviour
{
    [System.Serializable]
    public class TerrainRule
    {
        public Tilemap tilemap;

        [Tooltip("Leave empty to match any tile on this tilemap.")]
        public TileBase[] matchingTiles;

        [Range(0.1f, 2f)]
        public float speedMultiplier = 1f;
    }

    [SerializeField] private Transform groundCheck;

    [SerializeField, Range(0.1f, 2f)]
    private float defaultMultiplier = 0.6f;

    [Tooltip("The first matching rule is used.")]
    [SerializeField] private TerrainRule[] terrainRules;

    public float GetSpeedMultiplier()
    {
        Vector3 position = groundCheck != null
            ? groundCheck.position
            : transform.position;

        if (terrainRules == null)
            return defaultMultiplier;

        foreach (TerrainRule rule in terrainRules)
        {
            if (rule == null || rule.tilemap == null)
                continue;

            Vector3Int cell = rule.tilemap.WorldToCell(position);
            TileBase tile = rule.tilemap.GetTile(cell);

            if (tile == null)
                continue;

            // No tile filter means that every tile on this map matches
            if (rule.matchingTiles == null ||
                rule.matchingTiles.Length == 0)
            {
                return rule.speedMultiplier;
            }

            foreach (TileBase matchingTile in rule.matchingTiles)
            {
                if (tile == matchingTile)
                    return rule.speedMultiplier;
            }
        }

        return defaultMultiplier;
    }
}