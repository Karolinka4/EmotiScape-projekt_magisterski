using UnityEngine;

public class CauldronIngredientTrigger : MonoBehaviour
{
    public CauldronPotionRecipes recipes;

    void OnTriggerEnter(Collider other)
    {
        var ing = other.GetComponentInParent<IngredientPickup>();
        if (!ing || !ing.ingredient || ing.consumed) return;

        ing.consumed = true;
        recipes.AddIngredient(ing.ingredient);

        Destroy(ing.gameObject); // kopia znika, Ÿród³o zostaje
    }
}
