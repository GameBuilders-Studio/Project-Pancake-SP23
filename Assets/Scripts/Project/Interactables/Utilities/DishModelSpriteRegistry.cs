using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IngredientType;

public class DishModelSpriteRegistry : MonoBehaviour
{
    // this basically redefines the scriptable objects
    // but far easier imo
    public GameObject redPlankton;
    public Sprite redPlanktonSprite;
    public GameObject greenPlankton;
    public Sprite greenPlanktonSprite;
    public GameObject bluePlankton;
    public Sprite bluePlanktonSprite;
    public GameObject rice;
    public Sprite riceSprite;
    public GameObject seaweed;
    public Sprite seaweedSprite;
    public GameObject pasta;
    public Sprite pastaSprite;
    public GameObject redBluePlankton;
    public Sprite redBluePlanktonSprite;
    public GameObject redGreenPlankton;
    public Sprite redGreenPlanktonSprite;
    public GameObject greenBluePlankton;
    public Sprite greenBluePlanktonSprite;
    public GameObject redGreenBluePlankton;
    public Sprite redGreenBluePlanktonSprite;
    public GameObject riceSeaweed;
    public Sprite riceSeaweedSprite;
    public GameObject redSushi;
    public Sprite redSushiSprite;
    public GameObject greenSushi;
    public Sprite greenSushiSprite;

    public Dictionary<HashSet<IngredientType>, (GameObject, Sprite)> modelMap;

    public void Init()
    {
        modelMap = new Dictionary<HashSet<IngredientType>, (GameObject, Sprite)>(HashSet<IngredientType>.CreateSetComparer())
        {
            { new() { RedPlankton }, (redPlankton, redPlanktonSprite) },
            { new() { BluePlankton }, (bluePlankton, bluePlanktonSprite) },
            { new() { GreenPlankton }, (greenPlankton, greenPlanktonSprite) },
            { new() { Rice }, (rice, riceSprite) },
            { new() { Seaweed }, (seaweed, seaweedSprite) },
            { new() { Pasta }, (pasta, pastaSprite) },
            // ^ single ingredient dishes
            { new() { RedPlankton, BluePlankton }, (redBluePlankton, redBluePlanktonSprite) },
            { new() { RedPlankton, GreenPlankton, BluePlankton }, (redGreenBluePlankton, redGreenBluePlanktonSprite) },
            { new() { Rice, Seaweed }, (riceSeaweed, riceSeaweedSprite) },
            { new() { RedPlankton, Seaweed, Rice }, (redSushi, redSushiSprite) },
            { new() { GreenPlankton, Seaweed, Rice }, (greenSushi, greenSushiSprite) },
        };
    }
}
