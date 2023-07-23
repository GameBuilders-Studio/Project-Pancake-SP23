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
    public GameObject riceRedPlankton;
    public Sprite riceRedPlanktonSprite;
    public GameObject riceGreenPlankton;
    public Sprite riceGreenPlanktonSprite;
    public GameObject greenPlanktonSeaweed;
    public Sprite greenPlanktonSeaweedSprite;
    public GameObject redPlanktonSeaweed;
    public Sprite redPlanktonSeaweedSprite;
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
            { new() { RedPlankton, GreenPlankton }, (redGreenPlankton, redGreenPlanktonSprite) },
            { new() { GreenPlankton, BluePlankton }, (greenBluePlankton, greenBluePlanktonSprite) },

            { new() { RedPlankton, GreenPlankton, BluePlankton }, (redGreenBluePlankton, redGreenBluePlanktonSprite) },
            { new() { Rice, Seaweed }, (riceSeaweed, riceSeaweedSprite) },
            { new() { Rice, GreenPlankton }, (riceGreenPlankton, riceGreenPlanktonSprite) },
            { new() { Rice, RedPlankton }, (riceRedPlankton, riceRedPlanktonSprite) },
            { new() { Seaweed, RedPlankton }, (redPlanktonSeaweed, redPlanktonSeaweedSprite) },
            { new() { Seaweed, GreenPlankton }, (greenPlanktonSeaweed, greenPlanktonSeaweedSprite) },

            { new() { RedPlankton, Seaweed, Rice }, (redSushi, redSushiSprite) },
            { new() { GreenPlankton, Seaweed, Rice }, (greenSushi, greenSushiSprite) },
        };
    }
}
