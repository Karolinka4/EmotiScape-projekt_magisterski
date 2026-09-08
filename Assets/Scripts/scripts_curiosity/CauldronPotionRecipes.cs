using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CauldronPotionRecipes : MonoBehaviour
{
    [Serializable]
    public struct Recipe3
    {
        public IngredientSO a;
        public IngredientSO b;
        public IngredientSO c;

        public Color color;

        // Co ta mikstura robi po wypiciu
        public PotionEffectType effect;
    }

    [Header("Recipes (3 składniki → kolor + efekt)")]
    public List<Recipe3> recipes = new List<Recipe3>();

    [Header("Default")]
    public Color defaultColor = Color.green;

    [Header("Cauldron Water Visual")]
    public Renderer cauldronWaterRenderer;

    public string colorPropertyA = "_BaseColor";
    public string colorPropertyB = "_Color";

    [Header("Wrong Potion")]
    public Color wrongColor = Color.black;
    public float wrongResetSeconds = 3f;

    Coroutine wrongRoutine;

    IngredientSO[] current = new IngredientSO[3];
    int currentCount = 0;

    Dictionary<int, Recipe3> lookup;

    public Color CurrentCauldronColor { get; private set; }

    // NOWE
    public PotionEffectType CurrentPotionEffect { get; private set; }

    MaterialPropertyBlock mpb;
    int propAId;
    int propBId;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();

        propAId = Shader.PropertyToID(colorPropertyA);
        propBId = Shader.PropertyToID(colorPropertyB);

        BuildLookup();

        SetCauldronState(
            defaultColor,
            PotionEffectType.None
        );
    }

    void BuildLookup()
    {
        lookup = new Dictionary<int, Recipe3>(recipes.Count);

        foreach (var r in recipes)
        {
            if (!r.a || !r.b || !r.c)
                continue;

            int key = MakeKey(
                r.a.id,
                r.b.id,
                r.c.id
            );

            lookup[key] = r;
        }
    }

    public void AddIngredient(IngredientSO ing)
    {
        if (!ing)
            return;

        current[currentCount] = ing;
        currentCount++;

        // Czekamy aż będą 3 składniki
        if (currentCount < 3)
            return;

        int key = MakeKey(
            current[0].id,
            current[1].id,
            current[2].id
        );

        if (lookup.TryGetValue(key, out Recipe3 recipe))
        {
            // Jeżeli wcześniej była zła mikstura,
            // zatrzymujemy jej reset
            if (wrongRoutine != null)
            {
                StopCoroutine(wrongRoutine);
                wrongRoutine = null;
            }

            // POPRAWNA MIKSTURA
            SetCauldronState(
                recipe.color,
                recipe.effect
            );
        }
        else
        {
            // ZŁA MIKSTURA
            TriggerWrong();
        }

        currentCount = 0;

        Array.Clear(
            current,
            0,
            current.Length
        );
    }

    void SetCauldronState(
        Color color,
        PotionEffectType effect
    )
    {
        CurrentCauldronColor = color;
        CurrentPotionEffect = effect;

        ApplyCauldronVisual(color);
    }

    void ApplyCauldronVisual(Color c)
    {
        if (!cauldronWaterRenderer)
            return;

        cauldronWaterRenderer.GetPropertyBlock(mpb);

        mpb.SetColor(propAId, c);
        mpb.SetColor(propBId, c);

        cauldronWaterRenderer.SetPropertyBlock(mpb);
    }

    static int MakeKey(int x, int y, int z)
    {
        if (x > y)
            (x, y) = (y, x);

        if (y > z)
            (y, z) = (z, y);

        if (x > y)
            (x, y) = (y, x);

        return x * 100_000_000
             + y * 10_000
             + z;
    }

    void TriggerWrong()
    {
        Debug.Log("!!! ZŁA MIKSTURA - USTAWIAM CZARNY KOLOR !!!");
        SetCauldronState(
            wrongColor,
            PotionEffectType.None
        );

        if (wrongRoutine != null)
            StopCoroutine(wrongRoutine);

        wrongRoutine =
            StartCoroutine(WrongResetCoroutine());
    }

    IEnumerator WrongResetCoroutine()
    {
        yield return new WaitForSeconds(
            wrongResetSeconds
        );

        SetCauldronState(
            defaultColor,
            PotionEffectType.None
        );

        wrongRoutine = null;
    }
}