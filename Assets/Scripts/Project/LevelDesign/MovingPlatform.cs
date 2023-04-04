using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private List<GameObject> wayPoints;
    [SerializeField] private float moveSpeed = 5f;
    private int _currentWayPoint = 0;
    private GameObject _movingPlat;
    private Rigidbody _rb;
    private List<Transform> _wayPoints = new List<Transform>();

    private void Start()
    {
        _currentWayPoint = 0;
        _movingPlat = Instantiate(wayPoints[0], transform);
        _movingPlat.transform.position = wayPoints[0].transform.position;
        _rb = _movingPlat.GetComponent<Rigidbody>();
        foreach (var wp in wayPoints)
        {
            //_wayPoints.Add(wp.transform);
            wp.SetActive(false);
        }
    }

    void FixedUpdate()
    {

        var position = _movingPlat.transform.position;
        var target =
            Vector3.MoveTowards(position, wayPoints[_currentWayPoint].transform.position,
                moveSpeed * Time.fixedDeltaTime);
        _rb.MovePosition(target);
        //_movingPlat.transform.localPosition = localTarget;
        if (Vector3.Distance
                (wayPoints[_currentWayPoint].transform.position,
                    _movingPlat.transform.position) <= 0.1)
        {
            _currentWayPoint++;
        }
        if (_currentWayPoint == wayPoints.Count)
        {
            wayPoints.Reverse();
            _currentWayPoint = 0;
        }

    }


}
