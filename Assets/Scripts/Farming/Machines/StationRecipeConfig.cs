using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Farming/Station Recipe")]
public class StationRecipeConfig : ScriptableObject
{
    [Serializable]
    public struct Ingredient
    {
        public Item Item;
        public int Amount;
    }

    [Header("Рецепт")]
    public string RecipeName;              // "Мука", "Хлеб"
    public Sprite RecipeIcon;              // Иконка для UI (опционально)
    public Ingredient[] Ingredients;       // [{Пшеница, 2}]

    [Header("Результат")]
    public Item ResultItem;                // SO Муки
    public int ResultAmount = 1;           // 1

    [Header("Время")]
    public float ProductionTimeSeconds;    // 30 сек для муки
}
