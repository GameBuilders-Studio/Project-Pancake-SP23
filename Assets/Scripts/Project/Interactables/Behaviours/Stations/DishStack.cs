using UnityEngine;

public class DishStack : StationController
{
    [SerializeField]
    private GameObject _dishPrefab;
    [SerializeField]
    private int _maxSize = 3;
    [SerializeField]
    private int _count = 0;
    [SerializeField]
    private GameObject _visualDishPrefab;
    [SerializeField]
    private Transform _visualDishSpawnTransform;
    public int Count
    {
        get => _count;
        set
        {
            //clamp the count
            _count=Mathf.Clamp(value, 0, _maxSize);
            //change the prefab to a different one
            for(int i = 0; i < _count; i++){
                _visualDishSpawnTransform.GetChild(i).gameObject.SetActive(true);
            }
            for(int i = _count; i < _maxSize; i++){
                _visualDishSpawnTransform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    //You can't place anything except dish on a dish stack
    public override bool ValidateItem(Carryable item)
    {
        //if count is full, you can't place anything including dish
        if(Count >= _maxSize) return false;
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
        for (int i = 0; i < _maxSize; i++)
        {
            var ingredientGo = Instantiate(_visualDishPrefab, _visualDishSpawnTransform.position + new Vector3(0, 0.15f * i, 0), _visualDishSpawnTransform.rotation);
            ingredientGo.transform.parent = _visualDishSpawnTransform;
            ingredientGo.SetActive(false);
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
            var ingredientGo = Instantiate(_dishPrefab, transform.position, transform.rotation);
            item = ingredientGo.GetComponent<Carryable>();
            Count--;
        }
    }
}
