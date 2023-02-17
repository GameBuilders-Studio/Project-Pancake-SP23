/****************************
* Station.cs
* written by Richard
* 2023.2.16
******************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station : Ab_Container
{   
    
    protected GameObject _item;  

    private MeshRenderer meshRenderer;

    // [SerializeField]
    // public GameObject ObjectOnTheStation; for test only
    
    [Tooltip("should be container or food only")]
    [SerializeField]
    public GameObject Item
    {
        get => _item;
        set => _item = value;
    }

    private void Awake() {
        _isCarryable = false;
        _item = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = gameObject.transform.GetComponent<MeshRenderer> ();

        // string testobjname = "CubeProp";
        // ObjectOnTheStation = GameObject.Find(testobjname);
        // PlaceItemIntoContaier(ObjectOnTheStation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override bool PlaceItemIntoContaier(GameObject gameObject) {
        Container container = this.getContainer();
        if (!this.IsEmpty() && container == null) {             // non-container occupied
            //Debug.Log("1");
            return false;
        }
        else if (this.IsEmpty()) {                              // put directly if empty
            _item = gameObject;
            CenterItem(gameObject);
            //Debug.Log("2");
            return true;
        }
        //Debug.Log("3");
        return container.PlaceItemIntoContaier(gameObject);    // put into container 
    }

    /* either food or container, pick food from container processeded by container on it */
    public override void OnPickup(GameObject gameObject) { 
        gameObject.transform.SetParent (null); // nullify parent relation
        _item = null;
        return;
    }

    public override bool IsEmpty() {
        return _item == null;
    }

    public Container getContainer() {
        if (_item == null) {
            return null;
        }
        return _item.GetComponent<Container>();

    }

    private void CenterItem(GameObject go) {
        GameObject gameObject = this.gameObject;
        //getting externs bond
        Vector3 goBonds = go.GetComponent<MeshRenderer>().bounds.extents;
 
         //Setting the positon of the object
        go.transform.position = new Vector3 (gameObject.transform.position.x, meshRenderer.bounds.extents.y + goBonds.y + gameObject.transform.position.y, 0);
        go.transform.SetParent (gameObject.transform);

        //should not be collided 
        go.GetComponent<Rigidbody> ().isKinematic = true;
        return;
    }

}
