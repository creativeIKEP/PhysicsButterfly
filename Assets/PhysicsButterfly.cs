using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsButterfly : MonoBehaviour
{
    public Vector3 targetPosition;

    [SerializeField] float airDensity = 0.00128f; //[g/cm^3]
    [SerializeField] float width = 5.5f; //[cm]
    [SerializeField] float height = 2.5f; //TODO: 適当なので後で調整
    [SerializeField] float gravity = 9.8f;
   
    [SerializeField] float mass = 0.319f; //[g]
    //[SerializeField] float square = 13.75f * 2;
    [SerializeField] float averagePointRate = 0.58f;
    [SerializeField] float offsetAngleRate = 1.0f;
    [SerializeField] float maxUpFlapping_deg = 90;
    [SerializeField] float maxDownFlapping_deg = -75;
    [SerializeField] float minUpFlapping_deg = 5;
    [SerializeField] float minDownFlapping_deg = -5;
    [SerializeField] float upFeathering_deg = 20;
    [SerializeField] float downFeathering_deg = -5;
    [SerializeField] float upPitching_deg = 30;
    [SerializeField] float downPitching_deg = -20;
    [SerializeField] float phiMax_deg = 30;
    [SerializeField] float psiUp_deg = 60;
    [SerializeField] float psiDown_deg = -80;
    [SerializeField] float psiDrop_deg = -90;//アゲハはこれなし
    [SerializeField] float rotVelocity = 166;//[rps]
    [SerializeField] float maxFreq = 15;
    [SerializeField] float minFreq = 7;
 
    Vector3 velocity;
    Vector3 position;
    float rotVelocity_degps;//[deg/s]

    float omega;
    float preFlappingAngle;
    float preFeatheringAngle;
    

    float liftPower(float rad)
    {
        //TODO: 関数定義
        return 1.1f;
    }

    float dragPower(float rad)
    {
        //TODO: 関数定義
        //return 1.1f;
        return 0.5f * Mathf.Cos(2 * rad + Mathf.PI) + 0.75f;
    }

    Vector3 lift(float attackAngle, Vector3 velocityAir)
    {
        float v = velocityAir.magnitude;
        float square = width * 2 * height;
        float power = 0.5f * airDensity * v * v * square * liftPower(attackAngle);
        return power * Vector3.up;
    }

    Vector3 drag(float attackAngle, Vector3 velocityAir)
    {
        float v = velocityAir.magnitude;
        float square = width * 2 * height;
        float power = 0.5f * airDensity * v * v * square * dragPower(attackAngle);
        return power * Vector3.up;
    }

    float flappingAngle(float freq, float downFlapping_deg, float upFlapping_deg)
    {
        //TODO: 関数定義
        float t = Mathf.Sin(2 * Mathf.PI * Time.time * freq);
        return Mathf.Lerp(downFlapping_deg, upFlapping_deg, t);
    }

    float featheringAngle(float freq)
    {
        //TODO: 関数定義
        float t = Mathf.Sin(2 * Mathf.PI * Time.time * freq);
        return Mathf.Lerp(downFeathering_deg, upFeathering_deg, t);
    }

    float pitchingAngle(float freq)
    {
        //TODO: 関数定義
        float t = Mathf.Sin(2 * Mathf.PI * Time.time * freq);
        return Mathf.Lerp(downPitching_deg, upPitching_deg, t);
    }


    // Start is called before the first frame update
    void Start()
    {
        velocity = Vector3.zero;
        position = transform.position;
        rotVelocity_degps = 360 * rotVelocity;
        omega = 0;
        preFlappingAngle = 0;
        preFeatheringAngle = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //旋回運動
        Vector3 forward = -transform.forward;
        Vector3 toTarget = targetPosition - position;
        float phi = Vector3.SignedAngle(forward, toTarget, Vector3.up);
        float yRot;
        if (Mathf.Abs(phi) < phiMax_deg)
        {
            yRot = rotVelocity_degps * Time.deltaTime;
            //transform.Rotate(0, rotVelocity_degps * Time.deltaTime, 0);
        }
        else
        {
            //TODO: 急旋回モード
            yRot = rotVelocity_degps * Time.deltaTime;
            //transform.Rotate(0, rotVelocity_degps * Time.deltaTime, 0);
        }

        //高度調整
        float upFlapping_deg;
        float downFlapping_deg;
        float freq;
        float psiOffset_deg;

        float psi = Vector3.SignedAngle(forward, toTarget, Vector3.right);
        if (psiUp_deg >= psi)
        {
            //上昇モード
            upFlapping_deg = maxUpFlapping_deg;
            downFlapping_deg = maxDownFlapping_deg;
            freq = maxFreq;
            psiOffset_deg = offsetAngleRate * psiUp_deg;
        }
        else if (0 <= psi && psi < psiUp_deg)
        {
            float t = (psi - psiDown_deg) / (psiUp_deg - psiDown_deg);
            upFlapping_deg = minUpFlapping_deg + (maxUpFlapping_deg - minUpFlapping_deg) * t;
            downFlapping_deg = minDownFlapping_deg;
            freq = minFreq + (maxFreq - minFreq) * t;
            psiOffset_deg = offsetAngleRate * psi;
        }
        else if(psiDown_deg <= psi && psi < 0)
        {
            //降下モード
            upFlapping_deg = minUpFlapping_deg;
            downFlapping_deg = minDownFlapping_deg;
            freq = minFreq;
            psiOffset_deg = offsetAngleRate * psiDown_deg;
        }
        else
        {
            //急降下モード
            //TODO: アゲハはいらないのであとで実装
            upFlapping_deg = minUpFlapping_deg;
            downFlapping_deg = minDownFlapping_deg;
            freq = minFreq;
            psiOffset_deg = offsetAngleRate * psiDown_deg;
        }



        //TODO: omegaを求める
        float flapAngle_deg = flappingAngle(freq, downFlapping_deg, upFlapping_deg);
        float featheringAngle_deg = featheringAngle(freq);
        float pitchAngle_deg = pitchingAngle(freq);

        transform.Rotate(pitchAngle_deg, yRot, 0);

        var subFlap = flapAngle_deg - preFlappingAngle;
        var subFeather = featheringAngle_deg - preFeatheringAngle;

        var subFlap_rad = subFlap * Mathf.Deg2Rad;
        omega = subFlap_rad / Time.deltaTime;

        Vector3 om = Quaternion.Euler(0, 0, flapAngle_deg) * Vector3.down;
        Vector3 velocityWing = width * averagePointRate * omega * om.normalized;

        Vector3 velocityAir = -(velocity + velocityWing);

        float attackAngle = Vector3.SignedAngle(-Vector3.forward, -transform.forward, Vector3.right);
        Vector3 forceWing = lift(attackAngle, velocityAir) + drag(attackAngle, velocityAir);
        Vector3 force = 2 * forceWing + mass * gravity * Vector3.down; //両翅の力の合力と重力

        
        position += velocity * Time.deltaTime;
        velocity += force / mass * Time.deltaTime;
        transform.position = position;


        preFlappingAngle = flapAngle_deg;
        preFeatheringAngle = featheringAngle_deg;
    }
}
