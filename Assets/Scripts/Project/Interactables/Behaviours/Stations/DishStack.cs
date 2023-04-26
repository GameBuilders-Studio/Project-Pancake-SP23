using UnityEngine;

public class DishStack : StationController
{
    [SerializeField]
    [Tooltip("Dish stack can only hold clean or dirty plates, not both")]
    private bool _isClean = false;
    [SerializeField]
    private GameObject _cleanPlatePrefab;
    [SerializeField]
    private GameObject _dirtyPlatePrefab;
    [SerializeField]
    private int _maxPlates = 3;
    [SerializeField]
    private int _count = 0;
    [SerializeField]
    private GameObject _cleanVisualDishPrefab;
    [SerializeField]
    private GameObject _dirtyVisualDishPrefab;
    [SerializeField]
    private Transform _visualDishSpawnTransform;
    public int Count
    {
        get => _count;
        set
        {
            //clamp the count
            _count = Mathf.Clamp(value, 0, _maxPlates);
            //change the prefab to a different one
            for (int i = 0; i < _count; i++)
            {
                _visualDishSpawnTransform.GetChild(i).gameObject.SetActive(true);
            }
            for (int i = _count; i < _maxPlates; i++)
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
        GameObject visualDishPrefab = _isClean ? _cleanVisualDishPrefab : _dirtyVisualDishPrefab;
        for (int i = 0; i < _maxPlates; i++)
        {
            GameObject ingredientGO = Instantiate(visualDishPrefab, _visualDishSpawnTransform.position + new Vector3(0, 0.15f * i, 0), _visualDishSpawnTransform.rotation);
            ingredientGO.transform.parent = _visualDishSpawnTransform;
            ingredientGO.SetActive(false);
        }
        for (int i = 0; i < _count; i++)
        {
            _visualDishSpawnTransform.GetChild(i).gameObject.SetActive(true);
        }
    }
    public override void ItemRemoved(ref Carryable item)
    {
        if (item == null)
        {
            if (_count <= 0) return;
            if (_isClean)
            {
                var ingredientGo = Instantiate(_cleanPlatePrefab, transform.position, transform.rotation);
                item = ingredientGo.GetComponent<Carryable>();
                Count--;
            }
            else
            {
                var ingredientGO = Instantiate(_dirtyPlatePrefab, transform.position, transform.rotation);
                DirtyPlate dirtyPlate = ingredientGO.GetComponent<DirtyPlate>();
                dirtyPlate.Count = Count;
                Count = 0;
                item = ingredientGO.GetComponent<Carryable>();
            }
        }
    }

    //Will call this in RespawnManaer 
    //When we are full, we are gonna move plate to open DishStack
    public bool IsFull()
    {
        if (_count == _maxPlates) //This means we are full, we are gonna move plate to open DishStack
        {                                 //the _size != 0 is to avoid the dishStack thats conected to dishWasher
            return true;
        }
        return false; //Meaning that the dishstack can allow a plate, WE WANT THIS
    }
}
