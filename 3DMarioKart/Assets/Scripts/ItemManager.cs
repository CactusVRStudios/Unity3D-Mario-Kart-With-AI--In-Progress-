﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ItemManager : MonoBehaviour
{
    private Player player_script;
    private PlayerSounds playersounds;
    bool start_select = false;
    public GameObject ItemUI;
    public AudioSource PlaySelectsound;
    public AudioSource Selected;

    public Sprite[] items_possible;
    public GameObject[] item_gameobjects;
    public Image your_item;

    [Header("ITEMS")]
    public GameObject shell;
    public GameObject redShell;
    public GameObject banana;
    public GameObject coin;
    public GameObject bobomb;
    public GameObject BlueShell;
    public Transform shellSpawnPos;
    public Transform backshellPos; //also for bananas
    public Transform BananaSpawnPos;
    public Transform coinSpawnPos;

    [HideInInspector]
    public int item_index = 0;
    [HideInInspector]
    public int tripleItemCount = 0;
    [HideInInspector]
    public string current_Item;
    bool item_decided = false; //player can only use item once the scroll thingy decides item unless triple
    float GoldenMushroomTimer = 0;

    //we need to keep track of every self-moving item's waypoints because since we want the item to follow its waypoints from where the player shoots the shell on the track, we have to identify the current waypoint for that shell, or bullet bill, etc
    [HideInInspector]
    public int currentWayPoint = 0;
    [Header("ITEM WAYPOINT SYSTEM")]
    public Transform path;

    [Header("Renderers and Particles For Star Powerup")]
    public Material[] normalMaterials;
    public Renderer[] playerRenderers;
    public Material starMat;
    public GameObject starPS;
    [HideInInspector]
    public bool StarPowerUp;

    [HideInInspector]
    public bool isBullet = false;
    [Header("BulletStuff")]
    public GameObject bulletPlayer;
    public GameObject kart;

    public GameObject CurrentTrailingItem;
    public GameObject[] trailingItems;



    


    // Start is called before the first frame update
    void Start()
    {
        player_script = GetComponent<Player>();
        playersounds = GetComponent<PlayerSounds>();
        
    }

    // Update is called once per frame
    void Update()
    {
        wayPointSystemCurrent();

        if (player_script.hasitem)  //player cas collided with an itembox, and this bool is set true in the onTriggerEnter in the playerscript, and player is in need of an item, so this will start the item select method
        if (player_script.hasitem)  //player cas collided with an itembox, and this bool is set true in the onTriggerEnter in the playerscript, and player is in need of an item, so this will start the item select method
        {
            if (!start_select) //this ensures item select process does not begin until player has used up curret item
            {
                start_select = true;
                StartCoroutine(Item_Select());
            }

            if(GoldenMushroomTimer > 0)
            {
                GoldenMushroomTimer -= Time.deltaTime;
            }


            if ((Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.DownArrow)) && item_decided) //if item array order changes, change the indexes of utility methods and these if statements
            {
                if (current_Item.Equals("GreenShell")) //.Equals, not ==
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                            if (CurrentTrailingItem != null)
                            {
                                CurrentTrailingItem.SetActive(false);
                                CurrentTrailingItem = null;
                            }
                            player_script.Driver.SetTrigger("ThrowForward");
                        item_gameobjects[2].SetActive(true);
                        StartCoroutine(spawnShell(shellSpawnPos, 1));
                            current_Item = ""; //1 use only
                            used_Item_Done();
                        }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        if(CurrentTrailingItem == null)
                        {
                                CurrentTrailingItem = trailingItems[0]; //shell on back
                                player_script.Driver.SetBool("hasItem", false);
                                CurrentTrailingItem.SetActive(true);
                        }
                        else
                            {
                                CurrentTrailingItem.SetActive(false);
                                CurrentTrailingItem = null;
                                
                                player_script.Driver.SetTrigger("ThrowBackward");
                                item_gameobjects[2].SetActive(true);
                                StartCoroutine(spawnShell(backshellPos, -1));
                                if (!StarPowerUp)
                                    player_script.current_face_material = player_script.faces[1]; //look left
                                current_Item = ""; //1 use only
                                used_Item_Done();
                            }
                        
                    }


                    
                }
                else if (current_Item.Equals("TripleGreenShells") && tripleItemCount > 0)
                {

                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                        player_script.Driver.SetTrigger("ThrowForward");
                        item_gameobjects[2].SetActive(true);
                        StartCoroutine(spawnShell(shellSpawnPos, 1));
                        tripleItemCount--;
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        player_script.Driver.SetTrigger("ThrowBackward");
                        item_gameobjects[2].SetActive(true);
                        StartCoroutine(spawnShell(backshellPos, -1));
                        tripleItemCount--;
                        if (!StarPowerUp)
                            player_script.current_face_material = player_script.faces[1]; //look left
                    }

                    item_gameobjects[item_index].transform.GetChild(tripleItemCount).gameObject.SetActive(false); //turn off one of the 3 shells. Index is valid as we subtracted 1 before

                    if (tripleItemCount < 1) //if you used up all of triple shells, reset everything
                    {
                        current_Item = "";
                        item_gameobjects[item_index].SetActive(false);
                        item_gameobjects[item_index].transform.GetChild(0).gameObject.SetActive(true);
                        item_gameobjects[item_index].transform.GetChild(1).gameObject.SetActive(true);
                        item_gameobjects[item_index].transform.GetChild(2).gameObject.SetActive(true);
                        used_Item_Done();
                    }
                } //THIS IS FOR TRIPLE GREEEN SHELLS
                else if (current_Item.Equals("RedShell"))
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                            if (CurrentTrailingItem != null)
                            {
                                CurrentTrailingItem.SetActive(false);
                                CurrentTrailingItem = null;
                            }
                            player_script.Driver.SetTrigger("ThrowForward");
                        item_gameobjects[4].SetActive(true);
                        StartCoroutine(spawnRedShell(shellSpawnPos, 1));
                        current_Item = ""; //1 use only
                        used_Item_Done();
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                            if (CurrentTrailingItem == null)
                            {
                                CurrentTrailingItem = trailingItems[1]; //shell on back
                                player_script.Driver.SetBool("hasItem", false);
                                CurrentTrailingItem.SetActive(true);
                            }
                            else
                            {
                                CurrentTrailingItem.SetActive(false);
                                CurrentTrailingItem = null;
                                player_script.Driver.SetTrigger("ThrowBackward");
                                item_gameobjects[4].SetActive(true);
                                StartCoroutine(spawnRedShell(backshellPos, -1));
                                player_script.current_face_material = player_script.faces[1]; //look left material
                                current_Item = ""; //1 use only
                                used_Item_Done();
                            }
                            
                    }

                    

                }
                else if (current_Item.Equals("TripleRedShells") && tripleItemCount > 0)
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                        player_script.Driver.SetTrigger("ThrowForward");
                        item_gameobjects[4].SetActive(true);
                        StartCoroutine(spawnRedShell(shellSpawnPos, 1));
                        tripleItemCount--;
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        player_script.Driver.SetTrigger("ThrowBackward");
                        item_gameobjects[4].SetActive(true);
                        StartCoroutine(spawnRedShell(backshellPos, -1));
                        tripleItemCount--;
                        if (!StarPowerUp)
                            player_script.current_face_material = player_script.faces[1]; //look left
                    }
                    item_gameobjects[item_index].transform.GetChild(tripleItemCount).gameObject.SetActive(false); //turn off one of the 3 shells. Index is valid as we subtracted 1 before

                    if (tripleItemCount < 1) //if you used up all of triple shells, reset everything
                    {
                        current_Item = "";
                        item_gameobjects[item_index].SetActive(false);
                        item_gameobjects[item_index].transform.GetChild(0).gameObject.SetActive(true);
                        item_gameobjects[item_index].transform.GetChild(1).gameObject.SetActive(true);
                        item_gameobjects[item_index].transform.GetChild(2).gameObject.SetActive(true);
                        used_Item_Done();
                    }
                }
                else if (current_Item.Equals("Mushroom"))
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                        player_script.Boost_time = 2f;
                        for (int i = 0; i < player_script.BoostBurstPS.transform.childCount; i++) //boost burst
                        {
                            player_script.BoostBurstPS.transform.GetChild(i).GetComponent<ParticleSystem>().Play(); //left and right included
                        }
                        if (playersounds.Check_if_playing())
                        {
                            playersounds.Mario_Boost_Sounds[playersounds.sound_count].Play();
                            playersounds.sound_count++;
                        }
                        item_gameobjects[item_index].SetActive(false);
                        current_Item = ""; //1 use only
                        used_Item_Done();
                    }
                }
                else if (current_Item.Equals("TripleMushroom") && tripleItemCount > 0)
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                        player_script.Boost_time = 2.5f;
                        tripleItemCount--;
                        for (int i = 0; i < player_script.BoostBurstPS.transform.childCount; i++)
                        {
                            player_script.BoostBurstPS.transform.GetChild(i).GetComponent<ParticleSystem>().Play(); //left and right included
                        }
                        item_gameobjects[item_index].transform.GetChild(tripleItemCount).gameObject.SetActive(false);
                        if (playersounds.Check_if_playing())
                        {
                            playersounds.Mario_Boost_Sounds[playersounds.sound_count].Play();
                            playersounds.sound_count++;
                        }
                        if (tripleItemCount < 1) //if you used up all of triple mushrooms, reset everything
                        {
                            current_Item = "";
                            item_gameobjects[item_index].SetActive(false);
                            item_gameobjects[item_index].transform.GetChild(0).gameObject.SetActive(true);
                            item_gameobjects[item_index].transform.GetChild(1).gameObject.SetActive(true);
                            item_gameobjects[item_index].transform.GetChild(2).gameObject.SetActive(true);
                            used_Item_Done();
                        }
                    }
                }
                else if (current_Item.Equals("Banana"))
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                        if(CurrentTrailingItem != null)
                            {
                                CurrentTrailingItem.SetActive(false);
                                CurrentTrailingItem = null;
                            }
                        player_script.Driver.SetTrigger("ThrowForward");
                        StartCoroutine(spawnBanana(1));
                            current_Item = ""; //1 use only
                            used_Item_Done();
                    }
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                            if (CurrentTrailingItem == null)
                            {
                                CurrentTrailingItem = trailingItems[2]; //banana on back
                                player_script.Driver.SetBool("hasItem", false);
                                CurrentTrailingItem.SetActive(true);

                            }
                            else
                            {
                                StartCoroutine(spawnBanana(-1));
                                CurrentTrailingItem.SetActive(false);
                                CurrentTrailingItem = null;
                                player_script.Driver.SetTrigger("ThrowBackward");
                                if (!StarPowerUp)
                                    player_script.current_face_material = player_script.faces[1]; //look left
                                current_Item = ""; //1 use only
                                used_Item_Done();
                            }
                            
                    }
                    
                }
                else if (current_Item.Equals("TripleBananas"))
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                        player_script.Driver.SetTrigger("ThrowForward");
                        StartCoroutine(spawnBanana(1));
                    }
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        StartCoroutine(spawnBanana(-1));
                        
                        player_script.Driver.SetTrigger("ThrowBackward");
                        if (!StarPowerUp)
                            player_script.current_face_material = player_script.faces[1]; //look left
                    }
                    tripleItemCount--;
                    item_gameobjects[item_index].transform.GetChild(tripleItemCount).gameObject.SetActive(false); //turn off one of the 3 shells. Index is valid as we subtracted 1 before


                    if (tripleItemCount < 1) //if you used up all of triple shells, reset everything
                    {
                        current_Item = "";
                        item_gameobjects[item_index].SetActive(false);
                        item_gameobjects[item_index].transform.GetChild(0).gameObject.SetActive(true);
                        item_gameobjects[item_index].transform.GetChild(1).gameObject.SetActive(true);
                        item_gameobjects[item_index].transform.GetChild(2).gameObject.SetActive(true);
                        used_Item_Done();
                    }
                }
                else if (current_Item.Equals("GoldenMushroom"))
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                        player_script.Boost_time = 2f;
                        for (int i = 0; i < player_script.BoostBurstPS.transform.childCount; i++) //boost burst
                        {
                            player_script.BoostBurstPS.transform.GetChild(i).GetComponent<ParticleSystem>().Play(); //left and right included
                        }
                        if (playersounds.Check_if_playing())
                        {
                            playersounds.Mario_Boost_Sounds[playersounds.sound_count].Play();
                            playersounds.sound_count++;
                        }
                        if (GoldenMushroomTimer < 0)
                        {
                            item_gameobjects[item_index].SetActive(false);
                            current_Item = ""; 
                            used_Item_Done();
                        }
                    }

                    
                }
                else if (current_Item.Equals("Coin"))
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                        StartCoroutine(UseCoin());
                    }

                    current_Item = ""; //1 use only
                    used_Item_Done();
                }
                else if (current_Item.Equals("ItemStar"))
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                        current_Item = ""; //1 use only
                        used_Item_Done();
                        StartCoroutine(UseStar());

                    }

                }
                else if (current_Item.Equals("Bullet"))
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                        current_Item = "";
                        StartCoroutine(UseBullet());
                    }
                }
                else if (current_Item.Equals("Bobomb-Hold"))
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                        player_script.Driver.SetTrigger("ThrowForward");
                        StartCoroutine(useBobomb(1));
                        used_Item_Done();
                        current_Item = "";
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                            player_script.Driver.SetTrigger("ThrowBackward");
                            StartCoroutine(useBobomb(-1));
                            used_Item_Done();
                            current_Item = "";
                    }
                }
                else if (current_Item.Equals("BlueShell"))
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                        player_script.Driver.SetTrigger("ThrowForward");
                        StartCoroutine(useBlueShell());
                        used_Item_Done();
                        current_Item = "";
                    }
                }
                else
                {
                    used_Item_Done();
                    for(int i = 0; i < item_gameobjects.Length; i++)
                    {
                        item_gameobjects[i].SetActive(false);
                    }
                } //for now, since we only have green shells working, everything else just turns off when you use the item

                
                
                

            }
        }

        
    }


    IEnumerator Item_Select()
    {
        //random index
        item_index = Random.Range(0, 14);



        your_item.sprite = items_possible[item_index];

        ItemUI.GetComponent<Animator>().SetBool("StartSelecting", true);
        ItemUI.transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetBool("Scroll", true);
        yield return new WaitForSeconds(4);
        item_gameobjects[item_index].SetActive(true); //show the gameobject
        if (item_gameobjects[item_index].tag != "Non-Hold-Item")
        {
            player_script.Driver.SetBool("hasItem", true);
            player_script.has_item_hold = true;
            tripleItemCount = 0;

            if(item_gameobjects[item_index].name == "GoldenMushroom")
            {
                GoldenMushroomTimer = 8f;
            }
        }
        else
        {
            tripleItemCount = 3; //triple item
        }

        current_Item = item_gameobjects[item_index].name;

        PlaySelectsound.Stop();
        Selected.Play();
        item_decided = true;
    }

    //SPAWN FUNCTIONS
    IEnumerator spawnShell(Transform position, int direction) //spawns a green shell when shot
    {

        yield return new WaitForSeconds(0.15f);
        GameObject clone = Instantiate(shell, position.position, Quaternion.identity);
        clone.GetComponent<GreenShell>().who_threw_shell = gameObject.name;
        if (direction == 1) //backwards or forwards -1 and 1 respectively
        {
            clone.GetComponent<GreenShell>().myVelocity = transform.forward.normalized * 5500 * Time.deltaTime;
            yield return new WaitForSeconds(0.25f);
            item_gameobjects[2].SetActive(false); //hand shell

        }
        
        if (direction == -1)
        {
            clone.GetComponent<GreenShell>().myVelocity = -transform.forward.normalized * 3500 * Time.deltaTime;
            yield return new WaitForSeconds(0.25f);
            item_gameobjects[2].SetActive(false); //hand shell
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
        }
        
        




    }
    IEnumerator spawnRedShell(Transform position, int direction)
    {

        if(direction == 1)
        {
            yield return new WaitForSeconds(0.15f);
            GameObject clone = Instantiate(redShell, position.position, position.rotation);
            clone.SetActive(true);
            clone.GetComponent<RedShell>().who_threw_shell = gameObject.name;
            clone.transform.forward = transform.forward;
            clone.GetComponent<RedShell>().current_node = currentWayPoint;
            yield return new WaitForSeconds(0.25f);
            item_gameobjects[4].SetActive(false); //hand shell
        }
        else if(direction == -1)
        {
            yield return new WaitForSeconds(0.15f);
            GameObject clone = Instantiate(redShell, position.position, position.rotation);
            clone.SetActive(true);
            clone.GetComponent<RedShell>().who_threw_shell = gameObject.name;

            clone.GetComponent<RedShell>().enabled = false;
            clone.AddComponent<GreenShell>();
            clone.GetComponent<GreenShell>().myVelocity = -transform.GetChild(0).forward.normalized * 3500 * Time.deltaTime;
            clone.GetComponent<GreenShell>().lifetime = 19.5f;
            yield return new WaitForSeconds(0.25f);
            item_gameobjects[4].SetActive(false); //hand shell
            for(int i = 0; i < 75; i++)
            {
                if (!StarPowerUp)
                    player_script.current_face_material = player_script.faces[1]; //make sure it is not changed, by repeating in for loop
                yield return new WaitForSeconds(0.01f);
            }
            if (!StarPowerUp)
                player_script.current_face_material = player_script.faces[2]; //blink
            yield return new WaitForSeconds(0.1f);
            if (!StarPowerUp)
                player_script.current_face_material = player_script.faces[0];//normal
        }
        

    }
    IEnumerator useBobomb(int direction)
    {
        if(direction == 1)
        {
            yield return new WaitForSeconds(0.1f);
            item_gameobjects[item_index].SetActive(false);

            GameObject clone = Instantiate(bobomb, BananaSpawnPos.position, BananaSpawnPos.rotation);
            clone.SetActive(true);
            //clone.GetComponent<Rigidbody>().drag = 10;
            clone.GetComponent<Bobomb>().bomb_thrown(transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity).z * 400);
            clone.GetComponent<AudioSource>().enabled = true;

            for (int i = 0; i < clone.GetComponent<Bobomb>().renderers.Length; i++)
            {
                clone.GetComponent<Bobomb>().renderers[i].enabled = true;
            }
            for (int i = 0; i < clone.GetComponent<Bobomb>().spark.Length; i++)
            {
                clone.GetComponent<Bobomb>().spark[i].SetActive(true);
            }
            clone.GetComponent<Bobomb>().whoThrewBomb = gameObject.name;

        }
        if(direction == -1)
        {
            yield return new WaitForSeconds(0.1f);
            item_gameobjects[item_index].SetActive(false);

            GameObject clone = Instantiate(bobomb, backshellPos.position, BananaSpawnPos.rotation);
            clone.SetActive(true);
            clone.GetComponent<AudioSource>().enabled = true;
            clone.GetComponent<Bobomb>().bounce_count = 4;

            for (int i = 0; i < clone.GetComponent<Bobomb>().renderers.Length; i++)
            {
                clone.GetComponent<Bobomb>().renderers[i].enabled = true;
            }
            for (int i = 0; i < clone.GetComponent<Bobomb>().spark.Length; i++)
            {
                clone.GetComponent<Bobomb>().spark[i].SetActive(true);
            }
            clone.GetComponent<Bobomb>().whoThrewBomb = gameObject.name;

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
        }
    }
    IEnumerator spawnBanana(int direction)
    {
        GameObject clone;
        if(direction == 1)//forward
        {
            yield return new WaitForSeconds(0.1f);
            item_gameobjects[1].SetActive(false);
            clone = Instantiate(banana, BananaSpawnPos.position, BananaSpawnPos.rotation);
            clone.GetComponent<Banana>().Banana_thrown(transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity).z * 200);
            clone.GetComponent<Banana>().whoThrewBanana = gameObject.name;
        }
        else
        {
            yield return new WaitForSeconds(0.25f);
            clone = Instantiate(banana, backshellPos.position, BananaSpawnPos.rotation);
            for (int i = 0; i < 75; i++)
            {
                if (!StarPowerUp)
                    player_script.current_face_material = player_script.faces[1]; //make sure it is not changed, by repeating in for loop
                yield return new WaitForSeconds(0.01f);
            }
            item_gameobjects[1].SetActive(false);
            if (!StarPowerUp)
                player_script.current_face_material = player_script.faces[2]; //blink
            yield return new WaitForSeconds(0.1f);
            if (!StarPowerUp)
                player_script.current_face_material = player_script.faces[0];//normal
            clone.GetComponent<Banana>().whoThrewBanana = gameObject.name;
        }


    }
    IEnumerator UseCoin()
    {
        GameObject clone = Instantiate(coin, coinSpawnPos.position, coinSpawnPos.rotation);
        clone.transform.SetParent(transform);
        item_gameobjects[item_index].SetActive(false);
        GetComponent<ScoreCount>().COINCOUNT++;

        yield return new WaitForSeconds(0.3f);
        playersounds.effectSounds[9].Play();
    }
    IEnumerator UseStar()
    {
        item_gameobjects[item_index].SetActive(false);
        StarPowerUp = true;
        for(int i = 0; i < playerRenderers.Length; i++)
        {
            playerRenderers[i].material = starMat;
            playerRenderers[i].sharedMaterial = starMat;
        }
        GameObject.FindGameObjectWithTag("CourseMusic").GetComponent<AudioSource>().volume = 0;
        GameObject.Find("StarMusic").GetComponent<AudioSource>().Play();
        for(int i = 0; i < starPS.transform.childCount; i++)
        {
            starPS.transform.GetChild(i).GetComponent<ParticleSystem>().Play();
        }

        if (playersounds.Check_if_playing())
        {
            playersounds.MarioStarSounds[playersounds.star_count_sound].Play();
            playersounds.star_count_sound++;
            if(playersounds.star_count_sound > 2)
            {
                playersounds.star_count_sound = 0;
            }
        }


        yield return new WaitForSeconds(7.5f);

        GameObject.FindGameObjectWithTag("CourseMusic").GetComponent<AudioSource>().volume = 1;
        GameObject.Find("StarMusic").GetComponent<AudioSource>().Stop();
        StarPowerUp = false;
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            playerRenderers[i].material = normalMaterials[i];
            playerRenderers[i].sharedMaterial = normalMaterials[i];
        }
        for (int i = 0; i < starPS.transform.childCount; i++)
        {
            starPS.transform.GetChild(i).GetComponent<ParticleSystem>().Stop();
        }


    }
    IEnumerator UseBullet()
    {
        item_gameobjects[item_index].SetActive(false);
        isBullet = true;
        bulletPlayer.SetActive(true);
        kart.SetActive(false);
        playersounds.effectSounds[0].Stop(); //drifting  noise
        playersounds.effectSounds[1].Stop(); //drifting spark noise
        player_script.Boost_time = 0;
        playersounds.BulletSounds[1].Play();
        playersounds.BulletSounds[0].Play();

        yield return new WaitForSeconds(11);
        used_Item_Done();

        isBullet = false;
        bulletPlayer.SetActive(false);
        kart.SetActive(true);
        player_script.currentspeed = 70;
        playersounds.BulletSounds[2].Play();
        playersounds.BulletSounds[0].Stop();


    }

    IEnumerator useBlueShell()
    {
        yield return new WaitForSeconds(0.15f);
        GameObject clone = Instantiate(BlueShell, shellSpawnPos.position, shellSpawnPos.transform.rotation);
        clone.SetActive(true);
        clone.GetComponent<BlueShell>().current_node = currentWayPoint + 1;
        item_gameobjects[item_index].SetActive(false); //hand shell
        clone.GetComponent<BlueShell>().who_threw_shell = gameObject.name;
    }



    public void used_Item_Done() //resets the ui and bools
    {
        player_script.hasitem = false;
        player_script.has_item_hold = false;
        item_decided = false;
        start_select = false;
        ItemUI.GetComponent<Animator>().SetBool("StartSelecting", false);
        player_script.Driver.SetBool("hasItem", false);
        ItemUI.transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetBool("Scroll", false);
    }
    public int wayPointSystemCurrent()
    {
        return currentWayPoint;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (path.GetChild(currentWayPoint) == other.transform )
        {
            if (currentWayPoint == path.childCount - 1) //if last node, set the next node to first
            {
                currentWayPoint = 0;
            }
            else
            {
                currentWayPoint++;
            }

        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        string name = collision.gameObject.name;
        for(int i = 0; i < 3; i++)
        {
            if(item_gameobjects[7].transform.GetChild(i).name == name)
            {
                Physics.IgnoreCollision(collision.collider, transform.GetComponent<SphereCollider>());
            }
        }
    }
}
