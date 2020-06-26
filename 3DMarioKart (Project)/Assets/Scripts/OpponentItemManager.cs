using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentItemManager : MonoBehaviour
{
    private ComputerDriver ai_script;


    [Header("ITEMS")]
    public GameObject[] ItemsPossible;
    [HideInInspector]
    public float item_select_time = 0;
    bool already_has_item = false;

    [HideInInspector]
    public bool HitByShell_ = false;
    public bool HitByBanana_ = false;
    public bool playSpinAnim = true;
    public bool invincible = false;

    // Start is called before the first frame update
    void Start()
    {
        ai_script = GetComponent<ComputerDriver>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Banana")//regular banana
        {
            if (collision.gameObject.GetComponent<Banana>().lifetime > 0.5f)
            {
                StartCoroutine(hitByBanana());
                Destroy(collision.gameObject);
            }
        }
    }

    private IEnumerator OnTriggerEnter(Collider other)
    {
        //item box
        if (other.gameObject.tag == "ItemBox" && item_select_time > 3)
        {
            item_select_time = 0;
            for (int i = 0; i < 5; i++)
            {
                other.transform.GetChild(0).GetChild(i).GetComponent<ParticleSystem>().Play();

            }

            //start hiding stuff
            other.gameObject.GetComponent<BoxCollider>().enabled = false;
            for (int i = 1; i < 3; i++)
            {
                other.transform.GetChild(2).GetChild(i).GetComponent<SkinnedMeshRenderer>().enabled = false; //box
                other.transform.GetChild(1).GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = false; //question mark
            }
            other.gameObject.GetComponent<Animator>().SetBool("Enlarge", false); //reset to start process

            //re-enable
            yield return new WaitForSeconds(1);
            for (int i = 1; i < 3; i++)
            {
                other.transform.GetChild(2).GetChild(i).GetComponent<SkinnedMeshRenderer>().enabled = true; //box
                other.transform.GetChild(1).GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = true; //question mark
            }
            other.gameObject.GetComponent<Animator>().SetBool("Enlarge", true);  //show the item box spawning with animation, even though it was already there
            other.gameObject.GetComponent<BoxCollider>().enabled = true;

            yield return new WaitForSeconds(2);
            if (!already_has_item)
                ItemSelect();


        }

        //not actual shells going around track - this code is for shells orbiting any opponent that hit this player - ORBITING SHELLS   ------ Right now, the player's orbiting items work properly
        if (other.gameObject.tag == "Shell" && other.gameObject.layer == 14)
        {
            other.gameObject.SetActive(false); //disable shell that hit
            StartCoroutine(hitByShell());//effect as if a normal shell hit the opponent
            GameObject clone = Instantiate(other.transform.GetChild(0), other.transform.GetChild(0).position, other.transform.GetChild(0).rotation).gameObject; //instantiate the particle sustem
            for (int i = 0; i < clone.transform.childCount; i++)
            {
                clone.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>().Play();
            } //play the particle systems

            //if this player got hit by the MAIN PLAYER's ORBITING SHELLS
            if (other.transform.parent.parent.parent.parent.parent.parent.tag == "Player")
            {
                ItemManager itemManager = other.transform.parent.parent.parent.parent.parent.parent.GetComponent<ItemManager>();

                other.transform.parent.parent.parent.parent.parent.parent.GetComponent<ItemManager>().tripleItemCount--;
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
        else if (other.gameObject.tag == "Banana" && other.gameObject.layer == 14)
        {
            other.gameObject.SetActive(false); //disable banana that hit
            StartCoroutine(hitByBanana());//effect as if a normal Banana hit the opponent

            //if this player got hit by the MAIN PLAYER's ORBITING Bananas
            if (other.transform.parent.parent.parent.parent.parent.parent.tag == "Player")
            {
                ItemManager itemManager = other.transform.parent.parent.parent.parent.parent.parent.GetComponent<ItemManager>(); //script attached to player

                itemManager.tripleItemCount--;
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

        if(other.gameObject.tag == "Explosion")
        {
            StartCoroutine(hitByBanana());//effect as if a normal Banana hit the opponent
        }
    }

    //selects item
    void ItemSelect()
    {
        int index = Random.Range(0, ItemsPossible.Length);
        ItemsPossible[index].SetActive(true);
        if (ItemsPossible[index].tag != "Non-Hold-Item")
        {
            ai_script.DriverAnim.SetBool("hasItem", true);
        }
        already_has_item = true;
    }

    //these methods control what happens when hit by an item
    public IEnumerator hitByShell() //same method for red and green shells since they do the exact same effect when hitting the player
    {
        HitByShell_ = true;
        yield return new WaitForSeconds(1.25f);
        HitByShell_ = false;
        playSpinAnim = true;
        invincible = false;
    }
    public IEnumerator hitByBanana()
    {
        HitByBanana_ = true;
        yield return new WaitForSeconds(1.25f);
        HitByBanana_ = false;
        playSpinAnim = true;
        invincible = false;
    }




}
