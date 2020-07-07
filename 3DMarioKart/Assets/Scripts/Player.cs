﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [HideInInspector]
    public bool Boost = false;

    //movements
    public float boost_speed = 90;
    public float drift_speed;
    public float desiredMaxSpeed = 75;
    public float max_speed;
    [HideInInspector]
    public float currentspeed = 0;

    private Rigidbody rb;

    public float desired_rotate_strength;
    private float rotateStrengthWithStar;
    float rotate_strength;
    public Transform raycastPos;

    [HideInInspector]
    public bool grounded = false;

    //steer and direction of drift
    float direction;
    int drift_direction; //-1 is left and 1 is right

    bool drift_right = false;
    bool drift_left = false;

    // what boost will I get
    float Drift_time = 0;
    [HideInInspector]
    public float Boost_time = 0;

    //before start boost
    private float beforeStartAccelTime;

    public GameObject Boost_PS;
    public GameObject BoostBurstPS;
    public GameObject DriftPS;
    GameObject Right_Wheel_Drift_PS;
    GameObject Left_Wheel_Drift_PS;

    public LayerMask mask;

    [Header("Dust")]
    public Transform dustParticles;
    public Transform DriftDustLeft;
    public Transform DriftDustRight;
    public Transform exhaustDust;
    public Transform AccelBeforeStartDust;


    [Header("Particles and Tires")]

    //particle colors
    public Color drift1;
    public Color drift2;
    public Color drift3;
    public GameObject trails;
    public GameObject ambient_particles;

    //drift force stuff
    [HideInInspector]
    public bool drifting = false;

    //kart gameobjects
    public GameObject Tires;
    GameObject FrontLeftTire;
    GameObject FrontRightTire;
    float max_tire_rotation = 20;
    public GameObject[] tires;
    public GameObject steeringwheel;

    //glider stuff
    public GameObject glider;
    [HideInInspector]
    public bool GLIDER_FLY = false;
    bool glider_close_confirm = false;
    float gliderSlerpRate = 0;
    float glidingTime = 0;

    //jump panel
    [HideInInspector]
    public bool JUMP_PANEL = false;
    private float downForce = -300000;


    public Animator Driver;

    private PlayerSounds playersounds;

    [HideInInspector]
    public bool hasitem = false; //true when player hits itembox
    [HideInInspector]
    public bool has_item_hold = false;
    public GameObject ItemBox;
    private ItemManager item_manager;

    //faces
    [Header("Faces")]
    public Material[] faces;
    public Material current_face_material;
    public Renderer MarioFace;

    //shell and banana
    public bool HitByBanana_ = false;
    private bool playSpinAnim = true;
    bool invincible = false;

    public Transform bone;

    //collision cooldown
    float collideCooldown = 0;





    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Right_Wheel_Drift_PS = DriftPS.transform.GetChild(0).gameObject;
        Left_Wheel_Drift_PS = DriftPS.transform.GetChild(1).gameObject;

        FrontLeftTire = Tires.transform.GetChild(0).gameObject;
        FrontRightTire = Tires.transform.GetChild(1).gameObject;

        playersounds = GetComponent<PlayerSounds>();
        item_manager = GetComponent<ItemManager>();

        rotateStrengthWithStar = desired_rotate_strength + 15;


    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (RACE_MANAGER.RACE_STARTED && !item_manager.isBullet)
        {
            AccelBeforeStartDust.GetChild(0).GetComponent<ParticleSystem>().Stop();
            AccelBeforeStartDust.GetChild(1).GetComponent<ParticleSystem>().Stop();
            transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetBool("StartTurbo", false);
            playersounds.effectSounds[11].Stop();

            //before start boost
            if(beforeStartAccelTime > 1 && beforeStartAccelTime < 2)
            {
                Boost_time = 1;
                playersounds.effectSounds[13].Play();
                beforeStartAccelTime = 0;
                transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetTrigger("StartBoostTilt");
                playersounds.Mario_Boost_Sounds[3].Play();
                for (int i = 0; i < BoostBurstPS.transform.childCount; i++)
                {
                    BoostBurstPS.transform.GetChild(i).GetComponent<ParticleSystem>().Play(); //left and right included
                }
                for(int i = 0; i < 60; i++)
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        currentspeed = Mathf.SmoothStep(currentspeed, boost_speed, 2f * Time.deltaTime);
                    }
                }
            }

            Move();
            Steer();
            Drift();
            player_animations();
            boostEffects();
            movingCarParts();
            GroundNormalRotation();
            mario_face();


            //dust particles
            if (currentspeed > 30 && !drift_left && !drift_right && !GLIDER_FLY && !JUMP_PANEL && !Input.GetKey(KeyCode.S) && grounded && !item_manager.isBullet)
            {
                dustParticles.GetChild(0).GetComponent<ParticleSystem>().Play();
            }
            else
            {
                dustParticles.GetChild(0).GetComponent<ParticleSystem>().Stop();
            }
            if (currentspeed > 30 && !drift_left && !drift_right && !GLIDER_FLY && !JUMP_PANEL && !Input.GetKey(KeyCode.S) && grounded && !item_manager.isBullet)
            {
                dustParticles.GetChild(1).GetComponent<ParticleSystem>().Play();
            }
            else
            {
                dustParticles.GetChild(1).GetComponent<ParticleSystem>().Stop();
            }

            exhaustDust.GetChild(0).GetComponent<ParticleSystem>().Stop();
            exhaustDust.GetChild(1).GetComponent<ParticleSystem>().Stop();


            //constant subtract of boost time
            if (Boost_time > 0)
            {
                Boost_time -= Time.deltaTime;
                max_speed = boost_speed;
                Boost = true;
                if (Boost_time <= 0)
                {
                    currentspeed = Mathf.Lerp(currentspeed, max_speed, 5 * Time.deltaTime);
                }
            }
            if (Boost_time <= 0)
            {
                Boost_time = 0;
                max_speed = desiredMaxSpeed;
                Boost = false;

            }
        }
        else if (item_manager.isBullet)
        {
            Vector3 lookat = item_manager.path.GetChild(item_manager.currentWayPoint).position;

            float dir = 0;

            Ray ground = new Ray(transform.position, -transform.up);
            RaycastHit hit;
            if (Physics.Raycast(ground, out hit, 10, mask))
            {
            //DIRECTION TO FACE
                Quaternion rot = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up * 2, hit.normal) * transform.rotation, 6 * Time.deltaTime);
                //angle calc
                Debug.DrawRay(hit.point, hit.normal, Color.white, 30f);
                Vector3 myangle = item_manager.path.GetChild(item_manager.currentWayPoint).position - transform.position;
                Vector3 angle = Vector3.Cross(transform.forward, myangle);
                dir = Vector3.Dot(angle, transform.up);

                float none = 0;

                float y = Mathf.SmoothDamp(transform.eulerAngles.y, transform.eulerAngles.y + dir, ref none, 3f * Time.deltaTime);
                transform.eulerAngles = new Vector3(rot.eulerAngles.x, y, rot.eulerAngles.z);

                //MOVE FORWARD
                currentspeed = 120;
                rb.velocity = transform.forward * currentspeed;
            }
        }
        else if(!RACE_MANAGER.RACE_STARTED)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetBool("StartTurbo", true);
                tires[2].transform.Rotate(-90 * Time.deltaTime * 75 / 5.5f, 0, 0);
                tires[3].transform.Rotate(-90 * Time.deltaTime * 75 / 5.5f, 0, 0);

                AccelBeforeStartDust.GetChild(0).GetComponent<ParticleSystem>().Play();
                AccelBeforeStartDust.GetChild(1).GetComponent<ParticleSystem>().Play();

                if (!playersounds.effectSounds[11].isPlaying)
                {
                    playersounds.effectSounds[11].Play();
                }
                beforeStartAccelTime += Time.deltaTime;

            }
            else
            {
                transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetBool("StartTurbo", false);
                playersounds.effectSounds[11].Stop();
                AccelBeforeStartDust.GetChild(0).GetComponent<ParticleSystem>().Stop();
                AccelBeforeStartDust.GetChild(1).GetComponent<ParticleSystem>().Stop();
                beforeStartAccelTime = 0;
            }
        }
 
    }

    private void LateUpdate()
    {
        lookatOpponent();
    }

    private void OnCollisionStay(Collision other)
    {
        if(other.gameObject.tag == "Ground")
        {
            grounded = true;
            if (other.gameObject.tag == "Ground" || other.gameObject.tag == "Dirt")
            {
                if (JUMP_PANEL)
                {
                    JUMP_PANEL = false;
                    downForce = -200000;

                }
            }
        }
        if(other.gameObject.tag == "Dirt")
        {
            max_speed = 30;
            grounded = false;
            drift_left = false;
            drift_right = false;
            Drift_time = 0;
        }
        if(other.gameObject.tag == "Boost")
        {
            Boost_time = 2;
            grounded = true;

            Boost = true;
            max_speed = boost_speed;
            if (playersounds.Check_if_playing())
            {
                playersounds.Mario_Boost_Sounds[playersounds.sound_count].Play();
                playersounds.sound_count++;
            }

        }
    }
    private IEnumerator OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Dirt")
        {
            if (JUMP_PANEL)
            {
                JUMP_PANEL = false;
                downForce = -200000;
                playersounds.effectSounds[7].Play();
                playersounds.effectSounds[4].Play();
                playersounds.effectSounds[5].Play();
            }
        }

        if((collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Dirt") && glider_close_confirm)
        {
            if (GLIDER_FLY && !item_manager.isBullet)
            {
                playersounds.effectSounds[4].Play();
                playersounds.effectSounds[5].Play();


            }

            GLIDER_FLY = false;
            glidingTime = 0;
          
            playersounds.effectSounds[3].Stop();
            glider.GetComponent<Animator>().SetBool("GliderOpen", false);
            glider.GetComponent<Animator>().SetBool("GliderClose", true);


        }
        if (collision.gameObject.tag == "Banana")//regular banana
        {
            if(collision.gameObject.GetComponent<Banana>().lifetime > 0.5f && !item_manager.StarPowerUp)
            {
                StartCoroutine(hitByBanana());
                Destroy(collision.gameObject);
            }
            if (item_manager.StarPowerUp)
            {
                Destroy(collision.gameObject);
            }
        }
        if(collision.gameObject.tag == "Opponent")
        {
            if(collideCooldown <= 0)
            {
                collideCooldown = 0.75f;
                //angle calc
                Vector3 myangle = collision.transform.position - transform.position;
                Vector3 angle = Vector3.Cross(transform.forward, myangle);
                float dir = Vector3.Dot(angle, transform.up);

                float force;
                if(currentspeed > 50 && !item_manager.StarPowerUp){
                    force = 50000;
                }
                else
                {
                    force = 35000;
                }

                playersounds.effectSounds[10].Play();
                if (dir < 0)
                {
                    if (currentspeed > 50)
                    {
                        transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetTrigger("HitRight");
                    }
                    if (item_manager.StarPowerUp)
                    {
                        collision.gameObject.GetComponent<Rigidbody>().AddForce(-collision.transform.right * 100000 * Time.deltaTime, ForceMode.Impulse);
                        gameObject.GetComponent<OpponentItemManager>().hitByShell();
                    }
                    if (item_manager.isBullet)
                    {
                        collision.gameObject.GetComponent<Rigidbody>().AddForce(-collision.transform.right * 75000 * Time.deltaTime, ForceMode.Impulse);
                        gameObject.GetComponent<OpponentItemManager>().hitByShell();
                    }
                    for (int i = 0; i < 30; i++)
                    {
                        force = Mathf.Lerp(force, 0, 3 * Time.deltaTime);
                        rb.AddForce(transform.right * force * Time.deltaTime, ForceMode.Impulse);
                        yield return new WaitForSeconds(0.01f);
                    }

                    
                }
                else
                {
                    if (currentspeed > 50)
                    {
                        transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetTrigger("HitLeft");
                    }
                    if (item_manager.StarPowerUp)
                    {
                        collision.gameObject.GetComponent<Rigidbody>().AddForce(collision.transform.right * 75000 * Time.deltaTime, ForceMode.Impulse);
                        collision.gameObject.GetComponent<OpponentItemManager>().hitByShell();
                    }
                    if (item_manager.isBullet)
                    {
                        collision.gameObject.GetComponent<Rigidbody>().AddForce(collision.transform.right * 75000 * Time.deltaTime, ForceMode.Impulse);
                        collision.gameObject.GetComponent<OpponentItemManager>().hitByShell();
                    }
                    for (int i = 0; i < 30; i++)
                    {
                        force = Mathf.Lerp(force, 0, 3 * Time.deltaTime);
                        rb.AddForce(-transform.right * force * Time.deltaTime, ForceMode.Impulse);
                        yield return new WaitForSeconds(0.01f);
                    }
                    
                }
            }
            
        }
        if (collision.gameObject.tag == "GliderPanel")
        {
            if (GLIDER_FLY)
            {
                playersounds.effectSounds[7].Play();
            }
        }
        if(collision.gameObject.tag == "fence")
        {
            
            if (!item_manager.isBullet)
            {
                rb.velocity = Vector3.zero;
                if (currentspeed > 50)
                    currentspeed -= 10;
                Vector3 oldvel = collision.contacts[0].normal;
                oldvel = oldvel.normalized;

                Vector3 newvel = oldvel * 25000;

                newvel.y = -100000 * Time.deltaTime;

                for (int i = 0; i < 60; i++)
                {
                    rb.AddForce(newvel * Time.deltaTime, ForceMode.Impulse);
                    rb.AddForce(Vector3.down * 5000 * Time.deltaTime, ForceMode.Acceleration);
                    newvel = Vector3.Lerp(newvel, Vector3.zero, 3 * Time.deltaTime);
                    yield return new WaitForSeconds(0.01f);
                }

            }
            
            


        }

    }

    IEnumerator OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "ItemBox")
        {
            item_manager.PlaySelectsound.Play();
            hasitem = true; //will trigger a method in the item manager script
            for(int i = 0; i < 5; i++)
            {

                other.transform.GetChild(0).GetChild(i).GetComponent<ParticleSystem>().Play();

            }

            //start hiding stuff
            other.gameObject.GetComponent<BoxCollider>().enabled = false;
            for(int i = 1; i < 3; i++)
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


        }
        if(other.gameObject.tag == "Explosion" && !item_manager.isBullet && !item_manager.StarPowerUp)
        {
            StartCoroutine(hitByBanana());
        }
        if (other.gameObject.tag == "JumpPanel")
        {
            JUMP_PANEL = true;
            Boost_time = 2;
            Boost = true;
            playersounds.effectSounds[8].Play();

            max_speed = boost_speed;
            for (int i = 0; i < BoostBurstPS.transform.childCount; i++)
            {
                BoostBurstPS.transform.GetChild(i).GetComponent<ParticleSystem>().Play(); //left and right included
            } //burst boost



            if (playersounds.Check_if_playing())
            {
                playersounds.Mario_Boost_Sounds[playersounds.sound_count].Play();
                playersounds.sound_count++;
            }



        }

    }
    private IEnumerator OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "GliderPanel")
        {
            Boost_time = 2;
            Boost = true;
            playersounds.effectSounds[8].Play();

            max_speed = boost_speed;
            for (int i = 0; i < BoostBurstPS.transform.childCount; i++)
            {
                BoostBurstPS.transform.GetChild(i).GetComponent<ParticleSystem>().Play(); //left and right included
            } //burst boost



            if (playersounds.Check_if_playing())
            {
                playersounds.Mario_Boost_Sounds[playersounds.sound_count].Play();
                playersounds.sound_count++;
            }

            for (int i = 0; i < 40; i++)
            {
               transform.eulerAngles += new Vector3(0.1f, 0, 0);
               yield return new WaitForSeconds(0.02f);
                rb.AddForce(Vector3.down * 10000 * Time.deltaTime, ForceMode.Acceleration);
            } //rotation
            

            


        }
        if(other.gameObject.tag == "GliderPanelFly")
        {
            if(currentspeed <= 60)
            {
                glider.GetComponent<Animator>().SetBool("GliderOpen", true);
                glider.GetComponent<Animator>().SetBool("GliderClose", false);

                GLIDER_FLY = true;
                glider_close_confirm = false;
                playersounds.effectSounds[2].Play();
            }



            drift_direction = 0;
            drift_left = false;
            drift_right = false;


            //reset everything
            Drift_time = 0;
            //stop particles
            for (int i = 0; i < 5; i++)
            {
                ParticleSystem DriftPS = Right_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>(); //right wheel particles
                ParticleSystem.MainModule PSMAIN = DriftPS.main;

                ParticleSystem DriftPS2 = Left_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>(); //left wheel particles
                ParticleSystem.MainModule PSMAIN2 = DriftPS2.main;

                DriftPS.Stop();
                DriftPS2.Stop();
            }
            Drift_time = 0;
            drift_right = false;
            drift_left = false;

            if (currentspeed > 60)
            {
                transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetTrigger("Glide1");
                playersounds.effectSounds[2].Play();

                yield return new WaitForSeconds(0.45f);

                glider.GetComponent<Animator>().SetBool("GliderOpen", true);
                glider.GetComponent<Animator>().SetBool("GliderClose", false);
                GLIDER_FLY = true;
                glider_close_confirm = false;

                yield return new WaitForSeconds(0.35f);
                Boost_time = 0.5f;
                Boost = true;
                max_speed = boost_speed;
                Driver.SetTrigger("GliderTrick");
                for (int i = 0; i < BoostBurstPS.transform.childCount; i++)
                {
                    BoostBurstPS.transform.GetChild(i).GetComponent<ParticleSystem>().Play(); //left and right included
                } //burst boost


                //sounds
                if (playersounds.Check_if_playing() && !playersounds.Mario_Glider.isPlaying)
                {
                    playersounds.Mario_Glider.Play();
                }
                //happy face
                for(int i = 0; i < 45; i++)
                {
                    current_face_material = faces[3];
                    yield return new WaitForSeconds(0.016f);
                }
                current_face_material = faces[0];
            }

            yield return new WaitForSeconds(1f);
            glider_close_confirm = true; //ensures glider does not instantly close in any scenario a glitch might happen
            if(GLIDER_FLY)
                playersounds.effectSounds[3].Play();
        }
        

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "ColliderInAir" && (glidingTime < 1 || glidingTime > 4))
        {
            rb.AddForce(Vector3.down * 15000 * Time.deltaTime, ForceMode.Acceleration);
        }
    }

    void Move()
    {
        collideCooldown -= Time.deltaTime;

        //input speed into velocity
        rb.velocity = transform.forward * currentspeed;

        if (GLIDER_FLY)
        {
            if (!Input.GetKey(KeyCode.Space))
            {
                currentspeed = Mathf.Lerp(currentspeed, 65, 2 * Time.deltaTime);
            }
            else
            {
                currentspeed = Mathf.Lerp(currentspeed, max_speed, 2 * Time.deltaTime);
            }
        }

        //accelerate
        if (Input.GetKey(KeyCode.Space))
        {
            currentspeed = Mathf.Lerp(currentspeed, max_speed, 0.5f * Time.deltaTime);
            if(!drift_right && !drift_left && (!item_manager.StarPowerUp || drift_left || drift_right))
                rotate_strength = desired_rotate_strength;
            if (item_manager.StarPowerUp && !drift_right && !drift_left)
            {
                rotate_strength = rotateStrengthWithStar;
            }
        }
        //reverse
        if (Input.GetKey(KeyCode.S) && !GLIDER_FLY)
        {
            currentspeed = Mathf.Lerp(currentspeed, -max_speed / 1.75f, 0.03f);
            rotate_strength = 120;
        }
        //slowdown by itself
        if (!Input.GetKey(KeyCode.Space))
        {
            currentspeed = Mathf.Lerp(currentspeed, 0, 0.01f);
            drift_right = false;
            drift_left = false;
            drift_direction = 0;
            transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation, Quaternion.Euler(0, 0, 0), 0.4f);
        }

        //speed on track vs offtrack
        if (!grounded && !Boost && !item_manager.StarPowerUp)
        {
            max_speed = 30;
            if (Input.GetKey(KeyCode.Space))
            {
                currentspeed = Mathf.Lerp(currentspeed, max_speed, 3 * Time.deltaTime);
            }

        }
        if (!grounded && Boost)
        {
            max_speed = boost_speed;
            currentspeed = boost_speed;
        }
        if (grounded && !Boost && !item_manager.StarPowerUp)
        {
            max_speed = desiredMaxSpeed;
        }
        if (grounded && Boost)
        {
            max_speed = boost_speed;
        }
        if (item_manager.StarPowerUp)
        {
            max_speed = boost_speed - 5;
            if (Input.GetKey(KeyCode.Space))
            {
                currentspeed = Mathf.Lerp(currentspeed, max_speed, 3 * Time.deltaTime);
            }
        }

        //glider movements
        if (GLIDER_FLY)
        {
            glidingTime += Time.deltaTime;
            glider.GetComponent<Animator>().SetBool("GliderOpen", true);
            glider.GetComponent<Animator>().SetBool("GliderClose", false);

            Transform kart = transform;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                transform.Rotate(new Vector3(10, transform.rotation.y, transform.rotation.z), 25f * Time.deltaTime);
            }


            //ground normal rotation
            Ray ground = new Ray(transform.position, -transform.up);
            RaycastHit hit;
            if (!Physics.Raycast(ground, out hit, 0, mask))
            {
                if(Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    gliderSlerpRate = 0;
                }
                rotate_strength = 15;
                //steering
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    float angle = transform.localEulerAngles.z;
                    angle = (angle > 180) ? angle - 360 : angle;


                    if (angle < 40)
                    {
                        float none = 0;
                        gliderSlerpRate = Mathf.SmoothDamp(gliderSlerpRate, 2, ref none, 0.05f);
                        kart.rotation = Quaternion.SlerpUnclamped(kart.rotation, Quaternion.Euler(kart.eulerAngles.x,kart.eulerAngles.y, 40), gliderSlerpRate * Time.deltaTime);
                    }

                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    float angle = transform.localEulerAngles.z;
                    angle = (angle > 180) ? angle - 360 : angle;

                    Debug.Log(angle);
                    if (angle > -40)
                    {
                        float none = 0;
                        gliderSlerpRate = Mathf.SmoothDamp(gliderSlerpRate, 2, ref none, 0.05f);
                        kart.rotation = Quaternion.SlerpUnclamped(kart.rotation, Quaternion.Euler(kart.eulerAngles.x, kart.eulerAngles.y, -40), gliderSlerpRate* Time.deltaTime);
                    }
                }
                else
                {

                    kart.rotation = Quaternion.SlerpUnclamped(kart.rotation, Quaternion.Euler(kart.eulerAngles.x, kart.eulerAngles.y, 0), gliderSlerpRate * Time.deltaTime);
                }

            }
            else
                rotate_strength = 25;


        }
        if (JUMP_PANEL)
        {
            downForce = Mathf.Lerp(downForce, 150000, 2.5f * Time.deltaTime);
            rb.AddForce(Vector3.down * downForce * Time.deltaTime, ForceMode.Acceleration);

            transform.localEulerAngles += new Vector3(0.05f, 0, 0);

            //Camera.main.transform.parent.parent.localEulerAngles += new Vector3(0.5f, 0, 0);
        }

        //trails
        if(drifting && grounded && !GLIDER_FLY && !JUMP_PANEL)
        {
            for(int i = 0; i < trails.transform.childCount; i++)
            {
                trails.transform.GetChild(i).GetChild(0).GetComponent<TrailRenderer>().emitting = true;
            }
        }
        else
        {
            for (int i = 0; i < trails.transform.childCount; i++)
            {
                trails.transform.GetChild(i).GetChild(0).GetComponent<TrailRenderer>().emitting = false;
            }
        }

        //ambient particles
        if(!GLIDER_FLY && !JUMP_PANEL)
        {
            ambient_particles.SetActive(true);
        }
        else
        {
            for (int i = 0; i < ambient_particles.transform.childCount; i++)
            {
                ambient_particles.SetActive(false);
            }
        }

        if (HitByBanana_)
        {
            if (playSpinAnim && !invincible)
            {
                invincible = true;
                transform.GetChild(0).GetChild(0).GetComponent<Animator>().SetTrigger("BananaHit");
                playSpinAnim = false;
            }
            currentspeed = Mathf.Lerp(currentspeed, 0, 3 * Time.deltaTime);

        }
    }
    void Steer()
    {
        float force = 0;

        //steer
        if (Input.GetAxis("Horizontal") != 0)
        {
            if (!drift_right && !drift_left) //no drift
            {
                direction = Input.GetAxis("Horizontal") > 0 ? 1 : -1; //-1 = left, 1 = right
                transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation, Quaternion.Euler(0, 0, 0), 8f * Time.deltaTime);
                max_speed = desiredMaxSpeed;
            }

            //turning mechanism if drifting
            if (drift_right && !drift_left)
            {
                direction = Input.GetAxis("Horizontal") > 0 ? 2.1f : 0.5f;
                transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation, Quaternion.Euler(0, 20f, 0), 8f * Time.deltaTime);
                max_speed = desiredMaxSpeed - 10;

                //force
                if (drifting)
                {
                    force = Mathf.SmoothStep(force, -8500, 10f * Time.deltaTime);
                    rb.velocity += transform.right * force * Time.deltaTime;

                }
                

            }
            if (drift_left && !drift_right)
            {
                direction = Input.GetAxis("Horizontal") < 0 ? -2.1f : -0.5f;
                transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation, Quaternion.Euler(0, -20f, 0), 8f * Time.deltaTime);
                max_speed = desiredMaxSpeed - 10;

                if (drifting)
                {
                    force = Mathf.SmoothStep(force, 8500, 10f * Time.deltaTime);
                    rb.velocity += transform.right * force * Time.deltaTime;
                }
                
            }


            //how strong the player can turn
            float speed_rotate_rate = 0;
            if (drift_left || drift_right)
                speed_rotate_rate = 1.2f;
            if (currentspeed > 10 && currentspeed < 40 && !drift_right && !drift_left)
                speed_rotate_rate = 1.3f;
            if (currentspeed >= 40 && !drift_right && !drift_left)
                speed_rotate_rate = 1.75f;
            if (currentspeed < -5 && !Input.GetKey(KeyCode.Space)) //rverse
                speed_rotate_rate = -0.5f;

            //final rotations
            transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y + rotate_strength * direction * speed_rotate_rate, transform.localEulerAngles.z), 1f * Time.deltaTime); //which direction to rotate kart 

        }
    }
    void Drift()
    {
        //drift hop
        if (Input.GetKeyDown(KeyCode.V) && ! GLIDER_FLY && ! JUMP_PANEL)
        {
            transform.GetChild(0).gameObject.GetComponent<Animator>().SetTrigger("Drift");
            if (direction > 0)
            {
                drift_direction = 1;
            }
            if (direction < 0)
            {
                drift_direction = -1;
            }
            rotate_strength = 5;

        }

        //while v is pressed, you are drifting
        if (Input.GetKey(KeyCode.V) && grounded && currentspeed > 40 && Input.GetAxis("Horizontal") != 0 && ! GLIDER_FLY && !JUMP_PANEL)
        {
            rotate_strength = Mathf.Lerp(rotate_strength, desired_rotate_strength, 3 * Time.deltaTime);
            if (drift_direction == -1)
            {
                drift_right = false;
                drift_left = true;
                Drift_time += Time.deltaTime;
                if (!playersounds.effectSounds[0].isPlaying)
                {
                    playersounds.effectSounds[0].PlayDelayed(0.25f); //drift sound steering
                }
            }
            if (drift_direction == 1)
            {
                drift_right = true;
                drift_left = false;
                Drift_time += Time.deltaTime;
                if (!playersounds.effectSounds[0].isPlaying)
                {
                    playersounds.effectSounds[0].PlayDelayed(0.25f); //drift sound steering
                }
            }

            //drift dust particles
            if(Drift_time < 1f && drifting) //drifting bool starts from animator when player hop touches ground
            {
                if (drift_left)
                {
                    DriftDustLeft.GetChild(0).GetComponent<ParticleSystem>().Play();
                    DriftDustLeft.GetChild(1).GetComponent<ParticleSystem>().Play();
                }
                else if (drift_right)
                {
                    DriftDustRight.GetChild(0).GetComponent<ParticleSystem>().Play();
                    DriftDustRight.GetChild(1).GetComponent<ParticleSystem>().Play();
                }
            }
            else
            {
                DriftDustLeft.GetChild(0).GetComponent<ParticleSystem>().Stop();
                DriftDustLeft.GetChild(1).GetComponent<ParticleSystem>().Stop();
                DriftDustRight.GetChild(0).GetComponent<ParticleSystem>().Stop();
                DriftDustRight.GetChild(1).GetComponent<ParticleSystem>().Stop();
            }

            //particles and tire colour
            if (Drift_time >= 1.5 && Drift_time < 4)
            {

                for (int i = 0; i < 5; i++)
                {
                    ParticleSystem DriftPS = Right_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>(); //right wheel particles
                    ParticleSystem.MainModule PSMAIN = DriftPS.main;

                    ParticleSystem DriftPS2 = Left_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>(); //left wheel particles
                    ParticleSystem.MainModule PSMAIN2 = DriftPS2.main;
                    PSMAIN.startColor = drift1;
                    PSMAIN2.startColor = drift1;

                    if (!DriftPS.isPlaying && !DriftPS2.isPlaying)
                    {
                        DriftPS.Play();
                        DriftPS2.Play();
                    }

                }
                if (!playersounds.effectSounds[1].isPlaying)
                    playersounds.effectSounds[1].Play();
            }
            if (Drift_time >= 4 && Drift_time < 7)
            {
                if (Drift_time >= 4 && Drift_time <= 4.05f)
                    playersounds.effectSounds[1].Stop();

                //drift color particles
                for (int i = 0; i < 5; i++)
                {
                    ParticleSystem DriftPS = Right_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule PSMAIN = DriftPS.main;
                    ParticleSystem DriftPS2 = Left_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule PSMAIN2 = DriftPS2.main;
                    PSMAIN.startColor = drift2;
                    PSMAIN2.startColor = drift2;


                }
                if (!playersounds.effectSounds[1].isPlaying)
                    playersounds.effectSounds[1].Play();

            }
            if (Drift_time >= 7)
            {
                if (Drift_time >= 7 && Drift_time <= 7.05f)
                    playersounds.effectSounds[1].Stop();

                for (int i = 0; i < 5; i++)
                {

                    ParticleSystem DriftPS = Right_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule PSMAIN = DriftPS.main;
                    ParticleSystem DriftPS2 = Left_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule PSMAIN2 = DriftPS2.main;
                    PSMAIN.startColor = drift3;
                    PSMAIN2.startColor = drift3;

                }
                if (!playersounds.effectSounds[1].isPlaying)
                    playersounds.effectSounds[1].Play();
            }



        }


        //no more drift
        if (!Input.GetKey(KeyCode.V)) //if not drifting, or drifting without direction
        {
            drifting = false;
            playersounds.effectSounds[0].Stop();
            playersounds.effectSounds[1].Stop();

            drift_direction = 0;
            drift_left = false;
            drift_right = false;

            //different boost times to get different boost durations
            if (Drift_time > 1.5 && Drift_time < 4)
            {
                Boost = true;
                Boost_time = 0.75f;
                if (playersounds.Check_if_playing())
                {
                    playersounds.Mario_Boost_Sounds[playersounds.sound_count].Play();
                    playersounds.sound_count++;
                }
                for(int i = 0; i < BoostBurstPS.transform.childCount; i++)
                {
                    BoostBurstPS.transform.GetChild(i).GetComponent<ParticleSystem>().Play(); //left and right included
                }

            }
            if (Drift_time >= 4 && Drift_time < 7)
            {
                Boost = true;
                Boost_time = 1.5f;
                if (playersounds.Check_if_playing())
                {
                    playersounds.Mario_Boost_Sounds[playersounds.sound_count].Play();
                    playersounds.sound_count++;
                }
                for (int i = 0; i < BoostBurstPS.transform.childCount; i++)
                {
                    BoostBurstPS.transform.GetChild(i).GetComponent<ParticleSystem>().Play(); //left and right included
                }
            }
            if (Drift_time >= 7)
            {
                Boost = true;
                Boost_time = 2.5f;
                if (playersounds.Check_if_playing())
                {
                    playersounds.Mario_Boost_Sounds[playersounds.sound_count].Play();
                    playersounds.sound_count++;
                }
                for (int i = 0; i < BoostBurstPS.transform.childCount; i++)
                {
                    BoostBurstPS.transform.GetChild(i).GetComponent<ParticleSystem>().Play(); //left and right included
                }
            }



            //reset everything
            Drift_time = 0;
            //stop particles
            for (int i = 0; i < 5; i++)
            {
                ParticleSystem DriftPS = Right_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>(); //right wheel particles
                ParticleSystem.MainModule PSMAIN = DriftPS.main;

                ParticleSystem DriftPS2 = Left_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>(); //left wheel particles
                ParticleSystem.MainModule PSMAIN2 = DriftPS2.main;

                DriftPS.Stop();
                DriftPS2.Stop();

            }
            DriftDustLeft.GetChild(0).GetComponent<ParticleSystem>().Stop();
            DriftDustLeft.GetChild(1).GetComponent<ParticleSystem>().Stop();
            DriftDustRight.GetChild(0).GetComponent<ParticleSystem>().Stop();
            DriftDustRight.GetChild(1).GetComponent<ParticleSystem>().Stop();
        }
    }
    void player_animations()
    {
        if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            Driver.SetBool("TurnLeft", false);
            Driver.SetBool("TurnRight", false);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Driver.SetBool("TurnLeft", false);
            Driver.SetBool("TurnRight", true);
        }
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            Driver.SetBool("TurnLeft", true);
            Driver.SetBool("TurnRight", false);
        }
        
    }
    void boostEffects()
    {
        //boost particle systems
        if (Boost)
        {
            for (int i = 0; i < 2; i++)
            {
                ParticleSystem currentboost = Boost_PS.transform.GetChild(i).GetComponent<ParticleSystem>();
                if (!currentboost.isPlaying)
                    currentboost.Play();
            }
        }
        if (!Boost)
        {
            for (int i = 0; i < 2; i++)
            {
                ParticleSystem currentboost = Boost_PS.transform.GetChild(i).GetComponent<ParticleSystem>();
                if (currentboost.isPlaying)
                    currentboost.Stop();
            }
        }
    }
    void movingCarParts()
    {
        //tire gameObject steer and rotate and steeringwheel rotate
        float x = Input.GetAxis("Horizontal"); //direction 
        float rotate_speed = 8f;
        if (x >= 0.1)
        {
            FrontRightTire.transform.localEulerAngles = Vector3.Lerp(FrontRightTire.transform.localEulerAngles, new Vector3(0, 205, 0), rotate_speed * Time.deltaTime);
            FrontLeftTire.transform.localEulerAngles = Vector3.Lerp(FrontLeftTire.transform.localEulerAngles, new Vector3(0, 205, 0), rotate_speed * Time.deltaTime);

            steeringwheel.transform.localEulerAngles = Vector3.Lerp(steeringwheel.transform.localEulerAngles, new Vector3(0, 205, 0), rotate_speed * Time.deltaTime);


        } //right
        if (x <= -0.1)
        {
            FrontRightTire.transform.localEulerAngles = Vector3.Lerp(FrontRightTire.transform.localEulerAngles, new Vector3(0, 155, 0), rotate_speed * Time.deltaTime);
            FrontLeftTire.transform.localEulerAngles = Vector3.Lerp(FrontLeftTire.transform.localEulerAngles, new Vector3(0, 155, 0), rotate_speed * Time.deltaTime);

            steeringwheel.transform.localEulerAngles = Vector3.Lerp(steeringwheel.transform.localEulerAngles, new Vector3(0, 155, 0), rotate_speed * Time.deltaTime);



        } //left
        if (x == 0)
        {
            FrontRightTire.transform.localEulerAngles = Vector3.Lerp(FrontRightTire.transform.localEulerAngles, new Vector3(0, 180, 0), rotate_speed * Time.deltaTime);
            FrontLeftTire.transform.localEulerAngles = Vector3.Lerp(FrontLeftTire.transform.localEulerAngles, new Vector3(0, 180, 0), rotate_speed * Time.deltaTime);

            steeringwheel.transform.localEulerAngles = Vector3.Lerp(steeringwheel.transform.localEulerAngles, new Vector3(0, 180, 0), rotate_speed * Time.deltaTime);

        } //0
          //tire spinning
        for (int i = 0; i < 4; i++)
        {
            
            if(currentspeed < 6.5 && currentspeed > -6.5)
            {
                tires[i].transform.Rotate(-90 * Time.deltaTime * currentspeed * 0.015f, 0, 0);
            }
            else
            {
                tires[i].transform.Rotate(-90 * Time.deltaTime * currentspeed / 5f, 0, 0);
            }
        }

      
    }
    void GroundNormalRotation()
    {
        if (!JUMP_PANEL && glidingTime < 3f)
        {
            //ground normal rotation
            Ray ground = new Ray(raycastPos.position, -transform.up);
            RaycastHit hit;
            if (Physics.Raycast(ground, out hit, 4, mask) && hit.normal.y > 0.5f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up * 2, hit.normal) * transform.rotation, 7.5f * Time.deltaTime);
                Debug.DrawRay(hit.point, hit.normal, Color.white, 20f);
            }
        }
      

    }
    void lookatOpponent()
    {
        
    }

    void mario_face()
    {
        if(!item_manager.StarPowerUp)
            MarioFace.sharedMaterial = current_face_material;
        else
        {
            MarioFace.sharedMaterial = item_manager.starMat;
        }
    }

    public IEnumerator hitByBanana()
    {
        HitByBanana_ = true;
        Boost_time = 0;
        yield return new WaitForSeconds(1.25f);
        HitByBanana_ = false;
        playSpinAnim = true;
        invincible = false;
    }





}