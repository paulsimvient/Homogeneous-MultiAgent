using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;



public class FighterPlaneAgent : Agent
{
    [SerializeField]
    private GameObject _agent;

    [SerializeField]
    private Transform _agentBody;

    [SerializeField]
    private RayPerception _frontSensor;

    [SerializeField]
    private RayPerception _rearSensor;

    [SerializeField]
    private float _rayDistance;

    [SerializeField]
    public int _team;

    [SerializeField]
    private GameObject _redTeam;

    [SerializeField]
    private GameObject _blueTeam;

    [SerializeField]
    private GameObject _redBase, _blueBase;

    [SerializeField]
    private GameObject _redGun, _blueGun;

    [HideInInspector]
    public BodyIntegrity bodyIntegrity;

    public const int TeamSize = 10;
    public const int TrackedNumber = 5;
    private const int ShootingCD = 40;

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private GameObject _fake;

    List<state> states = new List<state>();

    float speed = 50;
    float horTurnSpeed = 20;
    float verTurnSpeed = 20;
    private int _shootingCD;
    List<Vector3> closestEnemies = new List<Vector3>(TrackedNumber);
    List<GameObject> enemies = new List<GameObject>(TeamSize);
    public bool gunsDestroyed = false;
    private bool bSaved = false;
 

    void Start()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        bodyIntegrity = GetComponent<BodyIntegrity>();
        _fake = new GameObject();
        _fake.transform.position = new Vector3(99999, 99999, 99999);
    }


    private string GetEnemyTeamTag()
    {
        if (_team == 1) //Blue
        {
            return "redfighter";
        }

        return "bluefighter";
    }

    void ScanArea()
    {
        var position = transform.position;
        var jets = GameObject.FindGameObjectsWithTag(GetEnemyTeamTag());

        foreach (var jet in jets)
        {
            float distance = Vector3.Distance(position, jet.transform.position);
             
            enemies.Add(jet);
        }

        if (closestEnemies.Count == 0)
        {
            enemies = enemies.OrderBy(x => Vector2.Distance(position, x.transform.position)).ToList();

            if (enemies.Count > TrackedNumber)
                enemies.RemoveRange(TrackedNumber, enemies.Count - TrackedNumber);

            foreach (var enemy in enemies)
            {
                closestEnemies.Add(enemy.transform.position);
            }

            FillVector(closestEnemies);

            enemies.Clear();
        }
    }

    public void Examine()
    {

    }

    public override void CollectObservations()
    {

        var position = transform.position;
        var rotationEuler = transform.eulerAngles;
        var rotation = transform.rotation;
        var detectableObjects = new string[] { };
        AddVectorObs(bodyIntegrity.health);
        AddVectorObs(position);
        AddVectorObs(rotation);
        AddVectorObs(rotationEuler);
        AddVectorObs(gunsDestroyed);
        AddVectorObs(_team == 1 ? _redGun.GetComponent<BodyIntegrity>().health : _blueGun.GetComponent<BodyIntegrity>().health);

      
        if (_team == 1 && _redGun.activeInHierarchy) //Blue
            ObjectTrack(_redGun.transform.position, true);
        else if (_team == 2 && _blueGun.activeInHierarchy) //Red
            ObjectTrack(_blueGun.transform.position, true);
        else ObjectTrack(_fake.transform.position, false);


        ScanArea();

        foreach (var vc in closestEnemies)
        {
            ObjectTrack(vc, false);
        }

        closestEnemies.Clear();
    }

     
    void Move()
    {

    }


    //interepreter
    private void ObjectReview()
    {



    }

    private void ObjectTrack(Vector3 pos, bool IsGun)
    {

        Vector3 dirFromThistoTarget = (pos - transform.position).normalized;
        float dotProd = Vector3.Dot(dirFromThistoTarget, transform.forward);
        var heading = pos - transform.position;
        var distance = (pos - transform.position).magnitude;
        distance = (distance > 1000) ? 1000 : distance;
        AddVectorObs(pos);
        AddVectorObs(distance);
        AddVectorObs(dotProd);


        if (pos == _fake.transform.position)
            DotprodReward(0, distance, 0, 0);
        else if (IsGun)
            DotprodReward(dotProd, distance, 0.2f, -0.05f);
        else if (gunsDestroyed) DotprodReward(dotProd, distance, 0.12f, -0.01f);



    }

    public override float[] Heuristic()
    {
        var action = new float[2];

        action[0] = Input.GetAxis("Vertical");
        action[1] = Input.GetAxis("Horizontal");
        return action;
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        AddReward(-1f / agentParameters.maxStep);
        //var forSpeed = Mathf.Clamp(vectorAction[0], 0.5f, 1f);
        var verRotAction = Mathf.FloorToInt(vectorAction[0]);
        var horRotAction = Mathf.FloorToInt(vectorAction[1]);

        var verRot = 0f;
        var horRot = 0f;

        switch (verRotAction)
        {
            case 1:
                verRot = 1f;
                break;
            case 2:
                verRot = -1f;
                break;
        }

        switch (horRotAction)
        {
            case 1:
                horRot = 1f;
                break;
            case 2:
                horRot = -1f;
                break;
        }
      

        _shootingCD++;

        transform.position += transform.forward * speed * Time.deltaTime;
        _agentBody.Rotate(transform.right, verRot * verTurnSpeed * Time.deltaTime, Space.World);
        _agentBody.Rotate(transform.up, horRot * horTurnSpeed * Time.deltaTime, Space.World);
        if (bodyIntegrity.health <= 1 || transform.position.y <= 10 || transform.position.x < -1000 || transform.position.x > 3000 || transform.position.y > 700 || transform.position.z < -2000 || transform.position.z > 1500)
        {
            AddReward(-5f);
            Done();

        }
 
    }

    private void Update()
    {
        //print(_team + " update!");
        // add shoot state
      

    }

 
   

    private void DotprodReward(float dotProd, float distance, float reward, float penalty)
    {
        int track = 0;
        int shoot = 0;
        if (dotProd >= 0.8)
        {
            track = 1;
            if (distance <= 600)
            {
                track = 2;
                // Debug.Log(dotProd); 
                shoot = 1;
                AddReward(reward);
                Shoot(gunsDestroyed == false, distance);
 
            }
        }
        else if (dotProd < 0)
        {
            AddReward(penalty);
            track = -1;
        }


        state st = new state();
        st.time = Time.time;
        st.target = track;
        st.shoot = shoot;
        st.gun_active = (gunsDestroyed) ? 0 : 1;
        st.vector = dotProd > 1 ? 1 : dotProd;
        states.Add(st); 

    }

    public void PrintHit()
    {
        print(_team + " Hit!");
    }

    public void PrintDead()
    {
        print(name + " Dead!");
    }

    private void FillVector(List<Vector3> vec)
    {
        var infvec = new Vector3(9999, 9999, 9999);
        for (int i = vec.Count; i < TrackedNumber; i++)
        {
            vec.Add(infvec);
        }
    }

    private void Shoot(bool isGun, float distance)
    {
      
        if (_shootingCD > ShootingCD)
        {
          
            ProjectileHandler.instance.CreateBulletProjectile(transform.position, transform.position + transform.forward, bodyIntegrity, _agent, _team);
            _shootingCD = 0;
 

        }
    }

    public void ResetTransform()
    {
        transform.position = new Vector3(Random.Range(-1000, 3000), 200, Random.Range(-2000, 1000));
        Quaternion Rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
        transform.rotation = Rotation;
  
    }

    public override void AgentReset()
    {
        ResetTransform();
        //PrintDead();
        _shootingCD = 0;
        bodyIntegrity.health = 15;

        if (states.Count() != 0)
        {
            Debug.Log("write"); 
            CsvReadWrite writer = new CsvReadWrite();
            writer.Save(states);
            states.Clear();
        }

    }
}
