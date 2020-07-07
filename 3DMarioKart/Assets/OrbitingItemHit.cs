using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingItemHit : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Opponent")
        {
            if(transform.parent.parent.parent.parent.parent.parent.gameObject.name == "Player")
            {
                if(gameObject.tag == "Shell")
                {
                    ItemManager itemManager = transform.parent.parent.parent.parent.parent.parent.GetComponent<ItemManager>();
                    itemManager.tripleItemCount--;

                    other.gameObject.GetComponent<OpponentItemManager>().hitByShell();

                    GameObject clone = Instantiate(transform.GetChild(0), transform.GetChild(0).position, transform.GetChild(0).rotation).gameObject; //instantiate the particle sustem
                    itemManager.item_gameobjects[itemManager.item_index].transform.GetChild(itemManager.tripleItemCount).gameObject.SetActive(false);

                    for (int i = 0; i < clone.transform.childCount; i++)
                    {
                        clone.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>().Play();
                    } //play the particle systems

                    if (itemManager.tripleItemCount < 1)
                    {
                        itemManager.used_Item_Done();
                        itemManager.item_gameobjects[itemManager.item_index].SetActive(false);
                        itemManager.item_gameobjects[itemManager.item_index].transform.GetChild(0).gameObject.SetActive(true);
                        itemManager.item_gameobjects[itemManager.item_index].transform.GetChild(1).gameObject.SetActive(true);
                        itemManager.item_gameobjects[itemManager.item_index].transform.GetChild(2).gameObject.SetActive(true);

                    }
                }
                else if(gameObject.tag == "Banana")
                {
                    ItemManager itemManager = transform.parent.parent.parent.parent.parent.parent.GetComponent<ItemManager>();
                    itemManager.tripleItemCount--;
                    other.gameObject.GetComponent<OpponentItemManager>().hitByBanana();
                    itemManager.item_gameobjects[itemManager.item_index].transform.GetChild(itemManager.tripleItemCount).gameObject.SetActive(false);
                    if (itemManager.tripleItemCount < 1)
                    {
                        itemManager.used_Item_Done();
                        itemManager.item_gameobjects[itemManager.item_index].SetActive(false);
                        itemManager.item_gameobjects[itemManager.item_index].transform.GetChild(0).gameObject.SetActive(true);
                        itemManager.item_gameobjects[itemManager.item_index].transform.GetChild(1).gameObject.SetActive(true);
                        itemManager.item_gameobjects[itemManager.item_index].transform.GetChild(2).gameObject.SetActive(true);

                    }
                }
                
            }
        }
    }
}
