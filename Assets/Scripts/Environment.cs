using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains all the information about the environment which can be passed to an AI
/// </summary>
public class Environment : MonoBehaviour
{



    //Mono behaviour stuff

    /// <summary>
    /// Internal item to show in inspector
    /// </summary>
    [System.Serializable]
    public struct Item 
    {
        public Inventory.Item item;
        public GameObject gameObject;
    }

    [SerializeField]
    public List<Item> items = new List<Item>();

    private void Start()
    {
        foreach(Item item in items) 
        {
            AddItem(item.item, item.gameObject);
        }
    }



    //Non Mono Behaviour stuff 



    /// <summary>
    /// List of items in the Environment
    /// </summary>
    private static Dictionary<Inventory.Item, GameObject> _objects = new Dictionary<Inventory.Item, GameObject>();

    public static Dictionary<Inventory.Item, GameObject> objects { get { return _objects; } }


    /// <summary>
    /// Add item to list of items in environment
    /// </summary>
    /// <param name="item"> The item with name </param>
    /// <param name="gameObject"> The actual gameobject </param>
    public static void AddItem(Inventory.Item item, GameObject gameObject) 
    {
        _objects.Add(item, gameObject);
    }

    /// <summary>
    /// Remove item with that name
    /// </summary>
    /// <param name="itemName"> The item name to remove </param>
    public static void RemoveItem(string itemName)
    {
        Inventory.Item toRemove = default;
        bool found = false;
        foreach(KeyValuePair<Inventory.Item,GameObject> item in _objects) 
        {
            if(item.Key.name == itemName) 
            {
                toRemove = item.Key;
                found = true;
            }
        }

        if (found) 
        {
            RemoveItem(toRemove);
        }
    }

    /// <summary>
    /// Remove item from list of items
    /// </summary>
    /// <param name="item"> The item to remove </param>
    public static void RemoveItem(Inventory.Item item) 
    {
        _objects.Remove(item);
    }





    //AI STUFF















}
