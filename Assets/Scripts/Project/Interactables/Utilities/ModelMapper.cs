
using System.Collections.Generic;
using UnityEngine;

// on-demand model map that also pools all available models
// could be made better by limiting access but eh
// use as readonly
public class ModelMapper: MonoBehaviour
{
    public static ModelMapper Instance {
        get
        {
            if (s_instance == null)
            {
                var newMapper = new GameObject();
                newMapper.gameObject.name = "ModelMapper";
                s_instance = newMapper.AddComponent<ModelMapper>();
            }

            return s_instance;

        }
        private set { s_instance = value; }
    }

    private static ModelMapper s_instance;

    private void Awake()
    {
        // someone else has already made another capsule
        // self destruct
        if (s_instance != null)
        {
            Destroy(gameObject);
        }
        // otherwise this is the instance
        s_instance = this;
        DontDestroyOnLoad(this);
    }

    private DishModelSpriteRegistry _dishModelSpriteRegistry;

    // this might be a good place to use scriptable objects...
    // but this will work
    public (GameObject, Sprite) GetDishModelSprite(HashSet<IngredientType> ingredients)
    {
        InitStatics();
        if (!_dishModelSpriteRegistry.modelMap.ContainsKey(ingredients))
        {
            return (null, null);
        }

        var pair = _dishModelSpriteRegistry.modelMap[ingredients];
        return (pair.Item1, pair.Item2);
    }

    public bool DishExist(HashSet<IngredientType> ingredients)
    {
        InitStatics();
        return _dishModelSpriteRegistry.modelMap.ContainsKey(ingredients);
    }

    private void InitStatics()
    {
        // cheat it a bit, load itself at runtime and dump everything in
        // might be good to separate into two objects instead
        if (_dishModelSpriteRegistry == null)
        {
            _dishModelSpriteRegistry = Resources.Load<GameObject>("DishModelRegister").GetComponent<DishModelSpriteRegistry>();
        }

        if (_dishModelSpriteRegistry.modelMap == null)
        {
            _dishModelSpriteRegistry.Init();
        }
    }
}
