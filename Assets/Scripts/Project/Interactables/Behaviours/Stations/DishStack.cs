using UnityEngine;

public class DishStack : StationController
{
    [SerializeField]
    private GameObject _dishPrefab;
    [SerializeField]
    private int _size = 3;
    [SerializeField]
    private int _count = 0;
    [SerializeField]
    private GameObject _visualDishPrefab;
    [SerializeField]
    private Transform _visualDishSpawnTransform;
    [SerializeField]
    private int changeNum = 0;
    public int Count
    {
        get => _count;
        set
        {
            changeNum = value - _count;
            _count = value;
            if (_count > _size) _count = _size;
            //change the prefab to a different one
            if (changeNum > 0)
            {
                for (int i = 0; i < changeNum; i++)
                {
                    
                    var ingredientGo = Instantiate(_visualDishPrefab, _visualDishSpawnTransform.position + new Vector3(0, 0.15f * (i+_visualDishSpawnTransform.childCount), 0), _visualDishSpawnTransform.rotation);
                    ingredientGo.transform.parent = _visualDishSpawnTransform;
                }
            }
            else if (changeNum < 0)
            {
                for (int i = 0; i < -changeNum; i++)
                {
                    if (_visualDishSpawnTransform.childCount > 0)
                    {
                        Destroy(_visualDishSpawnTransform.GetChild(_visualDishSpawnTransform.childCount - 1).gameObject);
                    }
                }
            }
        }
    }
    private void Awake()
    {
        for (int i = 0; i < _count; i++)
        {
            var ingredientGo = Instantiate(_visualDishPrefab, _visualDishSpawnTransform.position + new Vector3(0, 0.15f * i, 0), _visualDishSpawnTransform.rotation);
            ingredientGo.transform.parent = _visualDishSpawnTransform;
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
