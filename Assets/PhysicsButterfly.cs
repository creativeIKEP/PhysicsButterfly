using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsButterfly : MonoBehaviour
{
    public Transform targetPosition;
    public float timeParam;

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

    [SerializeField] Transform rightWing;
    [SerializeField] Transform leftWing;
    [SerializeField] float maxliftPower = 50;
    [SerializeField] float maxdragPower = 50;

    Vector3 velocity;
    Vector3 position;
    float rotVelocity_degps;//[deg/s]

    float preFlappingAngle;
    float preFeatheringAngle;
    

    float liftPower(float rad)
    {
        //TODO: 関数定義
        //return 1.1f;
        return Mathf.Sin(2 * rad);
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
        float square = width * height;
        float power = 0.5f * airDensity * v * v * square * liftPower(attackAngle);
        //power = Mathf.Min(maxliftPower, power);
        return power * Vector3.up;
    }

    Vector3 drag(float attackAngle, Vector3 velocityAir)
    {
        float v = velocityAir.magnitude;
        float square = width * height;
        float power = 0.5f * airDensity * v * v * square * dragPower(attackAngle);
        //power = Mathf.Min(maxdragPower, power);
        return power * transform.forward;
    }

    float flappingAngle(float freq, float downFlapping_deg, float upFlapping_deg)
    {
        //TODO: 関数定義
        float t = Mathf.Sin(2 * Mathf.PI * Time.time * timeParam * freq);
        return Mathf.Lerp(downFlapping_deg, upFlapping_deg, t);
    }

    float featheringAngle(float freq)
    {
        //TODO: 関数定義
        float t = Mathf.Sin(2 * Mathf.PI * Time.time * timeParam * freq);
        return Mathf.Lerp(downFeathering_deg, upFeathering_deg, t);
    }

    float pitchingAngle(float freq)
    {
        //TODO: 関数定義
        float t = Mathf.Sin(2 * Mathf.PI * Time.time * timeParam * freq);
        return Mathf.Lerp(downPitching_deg, upPitching_deg, t);
    }


    // Start is called before the first frame update
    void Start()
    {
        velocity = Vector3.zero;
        position = transform.position;
        rotVelocity_degps = 360 * rotVelocity;
        preFlappingAngle = 0;
        preFeatheringAngle = 0;
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.deltaTime * timeParam;

        //旋回運動
        //Vector3 forward = transform.forward;
        Vector3 toTarget = targetPosition.position - position;
        float phi = Vector3.SignedAngle(transform.forward, toTarget, Vector3.up);
        if (Mathf.Abs(phi) < phiMax_deg)
        {
            transform.Rotate(0, rotVelocity_degps * deltaTime, 0);
        }
        else if(Mathf.Abs(phi) > phiMax_deg)
        {
            //急旋回モード
            transform.Rotate(0, phi * deltaTime, 0);
        }

        //高度調整
        float upFlapping_deg;
        float downFlapping_deg;
        float freq;
        float psiOffset_deg;

        float psi = Vector3.SignedAngle(toTarget, transform.forward, transform.right);
        //Debug.Log(psi);
        if (psiUp_deg <= psi)
        {
            //Debug.Log("上昇");
            //上昇モード
            upFlapping_deg = maxUpFlapping_deg;
            downFlapping_deg = maxDownFlapping_deg;
            freq = maxFreq;
            psiOffset_deg = offsetAngleRate * psiUp_deg;
        }
        else if (psiDown_deg <= psi && psi < psiUp_deg)
        {
            //Debug.Log("巡行");
            float t = (psi - psiDown_deg) / (psiUp_deg - psiDown_deg);
            upFlapping_deg = minUpFlapping_deg + (maxUpFlapping_deg - minUpFlapping_deg) * t;
            downFlapping_deg = minDownFlapping_deg;
            freq = minFreq + (maxFreq - minFreq) * t;
            psiOffset_deg = offsetAngleRate * psi;
        }
        else if(psiDown_deg <= psi && psi < psiDown_deg)
        {
            //Debug.Log("降下");
            //降下モード
            upFlapping_deg = minUpFlapping_deg;
            downFlapping_deg = minDownFlapping_deg;
            freq = minFreq;
            psiOffset_deg = offsetAngleRate * psiDown_deg;
        }
        else
        {
            //Debug.Log("急降下");
            //急降下モード
            upFlapping_deg = maxUpFlapping_deg;
            downFlapping_deg = maxUpFlapping_deg;
            freq = 0;
            psiOffset_deg = offsetAngleRate * psiDown_deg;
        }



        //TODO: omegaを求める
        float flapAngle_deg = flappingAngle(freq, downFlapping_deg, upFlapping_deg);
        float featheringAngle_deg = featheringAngle(freq);
        float pitchAngle_deg = pitchingAngle(freq) + psiOffset_deg;

        rightWing.localRotation = Quaternion.Euler(featheringAngle_deg, 0, -flapAngle_deg);
        leftWing.localRotation = Quaternion.Euler(featheringAngle_deg, 0, flapAngle_deg);
        var currentRot = transform.rotation.eulerAngles;
        transform.Rotate(-pitchAngle_deg * deltaTime, 0, 0);
        //transform.rotation = Quaternion.Euler(-pitchAngle_deg, currentRot.y, currentRot.z);

        var subFlap = flapAngle_deg - preFlappingAngle;
        var subFeather = featheringAngle_deg - preFeatheringAngle;

        var subFlap_rad = subFlap * Mathf.Deg2Rad;
        var subFeather_rad = subFeather * Mathf.Deg2Rad;
        var omega1 = subFlap_rad / deltaTime;
        var omega2 = subFeather_rad / deltaTime;

        Vector3 om1 = Quaternion.Euler(0, 0, flapAngle_deg) * Vector3.down;
        om1 *= omega1;
        Vector3 om2 = Quaternion.Euler(featheringAngle_deg, 0, 0) * Vector3.down;
        om2 *= omega2;
        Vector3 omega = om1 + om2;
        Vector3 velocityWing = width * averagePointRate * omega;
        velocityWing = transform.rotation * velocityWing;

        Vector3 velocityAir = -(velocity + velocityWing);


        float attackAngle_r = Vector3.Angle(rightWing.forward, -velocityAir);
        float attackAngle_l = Vector3.Angle(leftWing.forward, -velocityAir);
        Vector3 forceWing_r = lift(attackAngle_r, velocityAir) + drag(attackAngle_r, velocityAir);
        forceWing_r /= 100000.0f;
        Vector3 forceWing_l = lift(attackAngle_l, velocityAir) + drag(attackAngle_l, velocityAir);
        forceWing_l /= 100000.0f;

        float mass_kg = mass / 1000.0f;
        Vector3 force = forceWing_r + forceWing_l + (mass_kg * gravity * Vector3.down); //両翅の力の合力と重力

        position += velocity * deltaTime / 100.0f;
        velocity += force / mass_kg * deltaTime*100.0f;
        transform.position = position;
        Debug.Log(position);

        preFlappingAngle = flapAngle_deg;
        preFeatheringAngle = featheringAngle_deg;
    }
}
