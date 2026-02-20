using UnityEditor;
using UnityEngine;

public class GenerateCardPrefabs
{
    [MenuItem("Tools/Generate Card Prefabs")]
    public static void Generate()
    {
        GameObject baseCard = GameObject.Find("InteractableCard");
        if (baseCard == null)
        {
            Debug.LogError("InteractableCard not found in scene!");
            return;
        }

        string[] spriteNames = new string[]
        {
            "icon_checkmark.png",
            "icon_circle.png",
            "icon_cross.png",
            "icon_square.png",
            "star.png",
            "check_round_color.png",
            "check_square_color.png",
            "arrow_basic_e.png",
            "arrow_basic_n.png",
            "arrow_decorative_e.png"
        };

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        Transform cardFront = baseCard.transform.Find("Card_Front");
        if (cardFront == null)
        {
            Debug.LogError("Card_Front child not found!");
            return;
        }

        SpriteRenderer frontRenderer = cardFront.GetComponent<SpriteRenderer>();
        if (frontRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on Card_Front!");
            return;
        }

        CardInteractable cardScript = baseCard.GetComponent<CardInteractable>();
        if (cardScript == null)
        {
            Debug.LogError("CardInteractable script not found on InteractableCard!");
            return;
        }

        for (int i = 0; i < 10; i++)
        {
            string spritePath = "Assets/Sprites/PNG/Blue/Default/" + spriteNames[i];

            TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }
                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }

            Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (newSprite == null)
            {
                Debug.LogWarning("Sprite not found: " + spritePath);
                continue;
            }

            // Update Sprite
            frontRenderer.sprite = newSprite;

            // Update Type
            cardScript.cardType = (CardType)i;

            // Create Prefab
            string prefabPath = "Assets/Prefabs/Card_" + ((CardType)i).ToString() + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(baseCard, prefabPath);
            Debug.Log("Created prefab: " + prefabPath);
        }

        Debug.Log("Finished generating 10 card prefabs.");
    }
}
