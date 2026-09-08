using UnityEngine;

[CreateAssetMenu(menuName = "Alchemy/Ingredient", fileName = "Ingredient")]
public class IngredientSO : ScriptableObject
{
    // unikalny identyfikator (ustaw w Inspectorze)
    public int id = 1;

    // opcjonalnie: nazwa do debug
    public string displayName;
}
