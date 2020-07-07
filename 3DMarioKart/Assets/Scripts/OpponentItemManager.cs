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

    private string current_item = "";
    private int itemIndex;
    private float timeHavingItem = 0;

    [HideInInspector]
    public bool HitByShell_ = false;
    public bool HitByBanana_ = false;
    public bool invincible = false;
    public float invincibleTime = 0;

    [Header("Items To Spawn")]
    public GameObject greenShell;
    public Transform shellPos;
    public Transform shellposBack;


    // Start is called before the first frame update
    void Start()
    {
        ai_script = GetComponent<ComputerDriver>();
    }

    // Update is called once per frame
    void Update()
    {
        invincibleTime -= Time.deltaTime;
        if(invincibleTime > 0)
        {
            invincible = true;
        }
        else
        {
            invincible = false;
            HitByShell_ = false;
            invincible = false;
            HitByBanana_ = false;
        }

        //item stuff
        if (already_has_item)
        {
            usingItem();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Banana")//regular banana
        {
            if (collision.gameObject.GetComponent<Banana>().lifetime > 0.5f)
            {
                hitByBanana();
                if (collision.gameObject.GetComponent<Banana>().whoThrewBanana == "Player")
                {
                    GameObject.Find("Player").GetComponent<Player>().Driver.SetTrigger("HitItem");
                }
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

        

        if(other.gameObject.tag == "Explosion")
        {
            hitByBanana();//effect as if a normal Banana hit the opponent
            if (other.gameObject.GetComponent<Bobomb>().whoThrewBomb == "Player")
            {
                GameObject.Find("Player").GetComponent<Player>().Driver.SetTrigger("HitItem");
            }

        }
    }

    //selects item
    void ItemSelect()
    {
        int index = Random.Range(0, 1);
        ItemsPossible[index].SetActive(true);
        if (ItemsPossible[index].tag != "Non-Hold-Item")
        {
            ai_script.DriverAnim.SetBool("hasItem", true);
        }
        already_has_item = true;
        current_item = ItemsPossible[index].name;
        itemIndex = index;
    }

    //these methods control what happens when hit by an item
    public void hitByShell() //same method for red and green shells since they do the exact same effect when hitting the player
    {

        if (!invincible)
        {
            invincibleTime = 1.25f;
            transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetTrigger("ShellHit");
            invincible = true;
            HitByShell_ = true;
        }
           

            
        

    }
    public void hitByBanana()
    {
        if (!invincible)
        {
            invincibleTime = 1.25f;
            transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetTrigger("BananaHit");
            invincible = true;
            HitByBanana_ = true;
        }
    }

   
    void usingItem()
    {
        timeHavingItem += Time.deltaTime; //stores the time for how long the opponent has an item for
        //this raycast thing will be used for some items such as projectiles and bananas
        RaycastHit hit;

        if(current_item == "GreenShell")
        {
            if (Physics.Raycast(transform.position,-transform.forward, out hit, Mathf.Infinity) || Physics.Raycast(transform.position + new Vector3(0,0.3f, 0), -transform.forward, out hit, Mathf.Infinity) || Physics.Raycast(transform.position + new Vector3(0, -0.3f, 0), -transform.forward, out hit, Mathf.Infinity))
            {
                if(hit.collider.tag == "Opponent" || hit.collider.tag == "Player")
                {
                    StartCoroutine(useShell(-1, shellposBack));
                    already_has_item = false;
                    ai_script.DriverAnim.SetBool("hasItem", false);
                }
            }
            else if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity) || Physics.Raycast(transform.position + new Vector3(0, 0.3f, 0), transform.forward, out hit, Mathf.Infinity) || Physics.Raycast(transform.position + new Vector3(0, -0.3f, 0), transform.forward, out hit, Mathf.Infinity))
            {
                if (hit.collider.tag == "Opponent" || hit.collider.tag == "Player")
                {
                    StartCoroutine(useShell(1, shellPos));
                    already_has_item = false;
                    ai_script.DriverAnim.SetBool("hasItem", false);
                }
            }
        }
    }

    IEnumerator useShell(int direction, Transform position)
    {

            yield return new WaitForSeconds(0.15f);
            GameObject clone = Instantiate(greenShell, position.position, Quaternion.identity);
            if (direction == 1) //backwards or forwards -1 and 1 respectively
            {
                clone.GetComponent<GreenShell>().myVelocity = transform.forward.normalized * 5500 * Time.deltaTime;
                yield return new WaitForSeconds(0.25f);
                ItemsPossible[2].SetActive(false); //hand shell
            }

            if (direction == -1)
            {
                clone.GetComponent<GreenShell>().myVelocity = -transform.forward.normalized * 3000 * Time.deltaTime;
                yield return new WaitForSeconds(0.25f);
                ItemsPossible[itemIndex].SetActive(false); //hand shell

                /*
                for (int i = 0; i < 75; i++)
                {
                    if (!StarPowerUp)
                        player_script.current_face_material = player_script.faces[1]; //look left
                    yield return new WaitForSeconds(0.01f);
                }
                if (!StarPowerUp)
                    player_script.current_face_material = player_script.faces[2]; //blink
                yield return new WaitForSeconds(0.1f);
                if (!StarPowerUp)
                    player_script.current_face_material = player_script.faces[0];//normal
                */
            }
            clone.GetComponent<GreenShell>().who_threw_shell = gameObject.name;
    }






}
