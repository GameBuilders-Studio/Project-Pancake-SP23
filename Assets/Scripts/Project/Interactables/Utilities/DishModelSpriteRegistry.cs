using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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

    public GameObject pastaBluePlankton;
    public Sprite pastaBluePlanktonSprite;
    public GameObject pastaRedPlankton;
    public Sprite pastaRedPlanktonSprite;
    public GameObject pastaRedBluePlankton;
    public Sprite pastaRedBluePlanktonSprite;

    public Dictionary<HashSet<(IngredientType, IngredientStateData)>, (GameObject, Sprite)> modelMap;

    // please for the love of god do not use Scriptable Objects like enums ever again - Revan
    [FormerlySerializedAs("normal")] public IngredientStateData raw;
    public IngredientStateData cooked, chopped, carbs, fried, rawEdible;

    public void Init()
    {
        // this is not certainly not the right way to do this
        // too much repeated information just to accomodate two possible ways the fishes can be presented in dishes
        // but at this point I just want this to end
        modelMap = new Dictionary<HashSet<(IngredientType, IngredientStateData)>, (GameObject, Sprite)>(HashSet<(IngredientType, IngredientStateData)>.CreateSetComparer())
        {
            { new() { (RedPlankton,  chopped)}, (redPlankton, redPlanktonSprite) },
            { new() { (BluePlankton, chopped) }, (bluePlankton, bluePlanktonSprite) },
            { new() { (GreenPlankton, chopped) }, (greenPlankton, greenPlanktonSprite) },
            { new() { (RedPlankton,  fried)}, (redPlankton, redPlanktonSprite) },
            { new() { (BluePlankton, fried) }, (bluePlankton, bluePlanktonSprite) },
            { new() { (GreenPlankton, fried) }, (greenPlankton, greenPlanktonSprite) },
            { new() { (Rice, cooked) }, (rice, riceSprite) },
            { new() { (Rice, carbs) }, (rice, riceSprite) },
            { new() { (Seaweed, rawEdible) }, (seaweed, seaweedSprite) },
            { new() { (Pasta, cooked) }, (pasta, pastaSprite) },
            { new() { (Pasta, carbs) }, (pasta, pastaSprite) },
            // ^ single ingredient dishes
            { new() { (RedPlankton, chopped), (BluePlankton, chopped) }, (redBluePlankton, redBluePlanktonSprite) },
            { new() { (RedPlankton, chopped) , (GreenPlankton, chopped) }, (redGreenPlankton, redGreenPlanktonSprite) },
            { new() { (GreenPlankton, chopped), (BluePlankton, chopped) }, (greenBluePlankton, greenBluePlanktonSprite) },

            { new() { (RedPlankton, chopped), (GreenPlankton, chopped), (BluePlankton, chopped) }, (redGreenBluePlankton, redGreenBluePlanktonSprite) },
            { new() { (Rice, cooked), (Seaweed, rawEdible) }, (riceSeaweed, riceSeaweedSprite) },
            { new() { (Rice, cooked) , (GreenPlankton, chopped) }, (riceGreenPlankton, riceGreenPlanktonSprite) },
            { new() { (Rice, cooked), (RedPlankton, chopped) }, (riceRedPlankton, riceRedPlanktonSprite) },
            { new() { (Seaweed, rawEdible), (RedPlankton, chopped) }, (redPlanktonSeaweed, redPlanktonSeaweedSprite) },
            { new() { (Seaweed, rawEdible), (GreenPlankton, chopped) }, (greenPlanktonSeaweed, greenPlanktonSeaweedSprite) },

            { new() { (RedPlankton, chopped), (Seaweed, rawEdible), (Rice, cooked) }, (redSushi, redSushiSprite) },
            { new() { (GreenPlankton, chopped), (Seaweed, rawEdible), (Rice, cooked) }, (greenSushi, greenSushiSprite) },
            { new() { (RedPlankton, fried), (Pasta, cooked) }, (pastaRedPlankton, pastaRedPlanktonSprite) },
            { new() { (BluePlankton, fried), (Pasta, cooked) }, (pastaBluePlankton, pastaBluePlanktonSprite) },
            { new() { (RedPlankton, fried), (BluePlankton, fried), (Pasta, cooked) }, (pastaRedBluePlankton, pastaRedBluePlanktonSprite) },
        };
    }
}
