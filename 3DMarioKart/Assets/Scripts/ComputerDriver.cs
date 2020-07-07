using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerDriver : MonoBehaviour
{
    public Player playerscript; //some parts of this script are the same constants defined in player script, so just access them directly

    private OpponentItemManager item_manage;

    private Rigidbody rb;
    public LayerMask mask;

    

    [Header("Movement")]
    public Transform path;
    private List<Transform> nodes = new List<Transform>();
    private int current_node = 0;

    public float max_steer_angle;
    public float max_speed;
    float current_speed;
    public float Desired_Max_Speed;
    bool grounded = false;

    float dir;
    bool driftright;
    bool driftleft;
    bool hop_anim = true;
    float drift_time = 0;
    public GameObject DriftPS;
    GameObject Right_Wheel_Drift_PS;
    GameObject Left_Wheel_Drift_PS;

    [Header("BOOST")]
    public float Boost_Speed;
    public GameObject BoostPS;
    bool boost;
    float boost_time = 0;
    

    public Animator DriverAnim;

    


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        item_manage = GetComponent<OpponentItemManager>();

        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        //the paths
        for(int i = 0; i < pathTransforms.Length; i++)
        {
            if(pathTransforms[i] != path.transform) //make sure the node is not the same as the path parent object
            {
                nodes.Add(pathTransforms[i]);
            }
        }

        Right_Wheel_Drift_PS = DriftPS.transform.GetChild(0).gameObject;
        Left_Wheel_Drift_PS = DriftPS.transform.GetChild(1).gameObject;

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (RACE_MANAGER.RACE_STARTED)
        {
            steer();
            Move();
            drift_func();
            animations();



            //
            if (boost_time > 0)
            {
                boost_time -= Time.deltaTime; //always decrease no matter what
                max_speed = Boost_Speed;
                boost = true;

                for (int i = 0; i < 2; i++)
                {
                    ParticleSystem currentboost = BoostPS.transform.GetChild(i).GetComponent<ParticleSystem>();
                    if (!currentboost.isPlaying)
                        currentboost.Play();
                }
            }
            if (boost_time <= 0)
            {
                boost = false;
                max_speed = Desired_Max_Speed;
                boost_time = 0;

                for (int i = 0; i < 2; i++)
                {
                    ParticleSystem currentboost = BoostPS.transform.GetChild(i).GetComponent<ParticleSystem>();
                    if (currentboost.isPlaying)
                        currentboost.Stop();
                }
            }
        }
        


    }




    void steer()
    {

        Vector3 lookat = nodes[current_node].position;

        Ray ground = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ground, out hit, 10, mask))
        {
             Quaternion rot = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up * 2, hit.normal) * transform.rotation, 6 * Time.deltaTime);


            //angle calc
            Debug.DrawRay(hit.point, hit.normal, Color.white, 30f);
            Vector3 myangle = nodes[current_node].position - transform.position;
            Vector3 angle = Vector3.Cross(transform.forward, myangle);
            dir = Vector3.Dot(angle, transform.up);

            float none = 0;

            float y = Mathf.SmoothDamp(transform.eulerAngles.y, transform.eulerAngles.y + dir,ref none, 6.5f * Time.deltaTime);


           

            transform.eulerAngles =new Vector3(rot.eulerAngles.x, y, rot.eulerAngles.z);
        }

    }
    void Move()
    {
        item_manage.item_select_time += Time.deltaTime;
        if(!item_manage.HitByShell_)
            current_speed = Mathf.Lerp(current_speed, max_speed, 0.5f * Time.deltaTime);


        rb.velocity = transform.forward * current_speed;

        if (!grounded && !boost)
        {
            max_speed = 40;
            //current_speed = Mathf.Lerp(current_speed, max_speed, 4);
        }
        else
            max_speed = Desired_Max_Speed;

        if(item_manage.HitByShell_)
        {
            current_speed = Mathf.Lerp(current_speed, 0, 2 * Time.deltaTime);
        }
        if (item_manage.HitByBanana_)
        {
            
            current_speed = Mathf.Lerp(current_speed, 0, 3 * Time.deltaTime);

        }


    }
    void drift_func()
    {
        
        

        if(nodes[current_node].tag == "DriftLeft" && transform.InverseTransformDirection(rb.velocity).z > 40) //left
        {
            if(hop_anim)
            {
                transform.GetChild(0).gameObject.GetComponent<Animator>().SetTrigger("Drift");
                hop_anim = false;
            }
            driftleft = true;
            driftright = false;
            
            transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation, Quaternion.Euler(0, -20, 0), 8f * Time.deltaTime);
            
        }
        if (nodes[current_node].tag == "DriftRight" && transform.InverseTransformDirection(rb.velocity).z > 40) //right
        {
            if (hop_anim)
            {
                transform.GetChild(0).gameObject.GetComponent<Animator>().SetTrigger("Drift");
                hop_anim = false;
            }
            driftleft = false;
            driftright = true;
            transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation, Quaternion.Euler(0, 20, 0), 8 * Time.deltaTime);

        }

        if (driftright || driftleft)
        {
            drift_time += Time.deltaTime;
            max_speed = Desired_Max_Speed - 15; //a little slower

            //particle effects
            if (drift_time >= 1 && drift_time < 3)
            {
                
                for (int i = 0; i < 5; i++)
                {
                    //the two particle systems and their mai modules for drift effects
                    ParticleSystem DriftPS = Right_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>(); //right wheel particles
                    ParticleSystem.MainModule PSMAIN = DriftPS.main;

                    ParticleSystem DriftPS2 = Left_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>(); //left wheel particles
                    ParticleSystem.MainModule PSMAIN2 = DriftPS2.main;

                    PSMAIN.startColor = playerscript.drift1; //accessing them from playerscript because they are constants common to everyone
                    PSMAIN2.startColor = playerscript.drift1;

                    if (!DriftPS.isPlaying && !DriftPS2.isPlaying)
                    {
                        DriftPS.Play();
                        DriftPS2.Play();
                    }

                }
                
            }
            if (drift_time >= 3 && drift_time < 6)
            {
                for (int i = 0; i < 5; i++)
                {
                    ParticleSystem DriftPS = Right_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule PSMAIN = DriftPS.main;
                    ParticleSystem DriftPS2 = Left_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule PSMAIN2 = DriftPS2.main;
                    PSMAIN.startColor = playerscript.drift2;
                    PSMAIN2.startColor = playerscript.drift2;
            }

            }
            if (drift_time >= 6)
            {
                for (int i = 0; i < 5; i++)
                {

                    ParticleSystem DriftPS = Right_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule PSMAIN = DriftPS.main;
                    ParticleSystem DriftPS2 = Left_Wheel_Drift_PS.transform.GetChild(i).gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule PSMAIN2 = DriftPS2.main;
                    PSMAIN.startColor = playerscript.drift3;
                    PSMAIN2.startColor = playerscript.drift3;

                }
;
            }
        }

        if ((nodes[current_node].tag != "DriftLeft" && nodes[current_node].tag != "DriftRight") || transform.InverseTransformDirection(rb.velocity).z <= 40 || !grounded) //stop drifting
        {
            if(drift_time >= 1 && drift_time < 3)
            {
                boost = true;
                boost_time = 0.5f;
            }
            if(drift_time >= 3 && drift_time < 6)
            {
                boost = true;
                boost_time = 1.5f;
            }
            if (drift_time >= 6)
            {
                boost = true;
                boost_time = 2.5f;
            }

            //reset
            driftleft = false;
            driftright = false;
            hop_anim = true;
            drift_time = 0;
            transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation, Quaternion.Euler(0, 0, 0), 8f * Time.deltaTime);
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
        } 
    }
    void animations()
    {
        if(dir <= -5)
        {
            DriverAnim.SetBool("TurnLeft", true);
            DriverAnim.SetBool("TurnRight", false);
        }
        else if (dir >= 5)
        {
            DriverAnim.SetBool("TurnLeft", false);
            DriverAnim.SetBool("TurnRight", true);
        }
        else
        {
            DriverAnim.SetBool("TurnLeft", false);
            DriverAnim.SetBool("TurnRight", false);
        }
    }


    

    

    

    private IEnumerator OnTriggerEnter(Collider other)
    {
        //next waypoint
        if(other.transform == nodes[current_node])
        {
            if (current_node == nodes.Count - 1) //if last node, set the next node to first
            {
                current_node = 0;
            }
            else
                current_node++;

            other.GetComponent<BoxCollider>().enabled = false;

            yield return new WaitForSeconds(2);
            other.GetComponent<BoxCollider>().enabled = true;

        }

        
        
    }


    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == "Ground")
        {
            grounded = true;
        }
        if (other.gameObject.tag == "Dirt")
        {
            grounded = false;
            driftleft = false;
            driftright = false;
            drift_time = 0;
        }
    }


}
