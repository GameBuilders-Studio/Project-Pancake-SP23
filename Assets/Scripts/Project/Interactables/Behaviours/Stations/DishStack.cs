using UnityEngine;
using CustomAttributes;
public class DishStack : StationController
{
    [SerializeField]
    [Tooltip("Whether this dish stack holds clean or dirty plates")]
    private bool _isClean = true;

    [SerializeField]
    private int _maxPlates = 3;

    [SerializeField]
    private int _plateCount = 0;

    [Space(15f)]
    [SerializeField]
    [Required]
    private Transform _visualDishSpawnTransform;

    [SerializeField]
    [Required]
    private GameObject _cleanPlatePrefab;

    [SerializeField]
    [Required]
    private GameObject _dirtyPlatePrefab;

    [Space(15f)]
    [SerializeField]
    [Required]
    private GameObject _cleanVisualDishPrefab;

    [SerializeField]
    [Required]
    private GameObject _dirtyVisualDishPrefab;

    public int Count
    {
        get => _plateCount;
        set
        {
            //clamp the count
            _plateCount = Mathf.Clamp(value, 0, _maxPlates);
            //change the prefab to a different one
            for (int i = 0; i < _plateCount; i++)
            {
                _visualDishSpawnTransform.GetChild(i).gameObject.SetActive(true);
            }
            for (int i = _plateCount; i < _maxPlates; i++)
            {
                _visualDishSpawnTransform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    //You can't place anything except dish on a dish stackx
    public override bool ValidateItem(Carryable item)
    {
        //if count is full, you can't place anything including dish
        if (Count >= _maxPlates) return false;
        FoodContainer dish;
        if (item.TryGetBehaviour(out dish))
        {
            Destroy(dish.gameObject);
            Count++;
            return true;
        }
        return false;
    }

    private void Awake()
    {
        // Instatiate the visual dishes based on the count and isClean
        GameObject visualDishPrefab = _isClean ? _cleanVisualDishPrefab : _dirtyVisualDishPrefab;
        for (int i = 0; i < _maxPlates; i++)
        {
            GameObject ingredientGO = Instantiate(visualDishPrefab, _visualDishSpawnTransform.position + new Vector3(0, 0.15f * i, 0), _visualDishSpawnTransform.rotation);
            ingredientGO.transform.parent = _visualDishSpawnTransform;
            ingredientGO.SetActive(false);
        }

        // Show the correct number of visual dishes
        for (int i = 0; i < _plateCount; i++)
        {
            _visualDishSpawnTransform.GetChild(i).gameObject.SetActive(true);
        }
    }
    public override void ItemRemoved(ref Carryable item)
    {
        if (item == null)
        {
            if (_plateCount <= 0) return;
            if (_isClean)
            {
                // Clean plates are carried one at a time
                var ingredientGo = Instantiate(_cleanPlatePrefab, transform.position, transform.rotation);
                item = ingredientGo.GetComponent<Carryable>();
                Count--;
            }
            else
            {
                // Dirty plates are carried in stacks for player to move it to the sink
                var ingredientGO = Instantiate(_dirtyPlatePrefab, transform.position, transform.rotation);
                DirtyPlate dirtyPlate = ingredientGO.GetComponent<DirtyPlate>();
                dirtyPlate.Count = Count;
                Count = 0;
                item = ingredientGO.GetComponent<Carryable>();
            }
        }
    }

    public override bool HasItem()
    {
        return _plateCount > 0;
    }

    //Will call this in RespawnManaer
    //When we are full, we are gonna move plate to open DishStack
    public bool IsFull()
    {
        return _plateCount >= _maxPlates;
    }
}
