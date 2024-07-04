using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using Unity.VisualScripting;
using static UnityEngine.UI.CanvasScaler;

public class HexUnit : MonoBehaviour
{
    const float travelSpeed = 4f;

    const float rotationSpeed = 180f;

    HexCell location;

    float orientation;

    public static HexUnit unitPrefab;

    public GameObject unitAssetPrefab;

    List<HexCell> pathToTravel;

    public AnimatorController[] animatorControllers;

    Animator unitAssetPrefabAnimator;

    GameObject gridObj;

    HexGrid grid;

    public Material[] materials;

    public GameObject dummyMesh;

    Renderer dummyMeshRenderer;

    int teamIndex;

    public int Health { get; set; }

    public HexUnit Opponent { get; set; }

    public HexCell Location
    {
        get
        {
            return location;
        }
        set
        {
            if (location)
            {
                location.Unit = null;
            }
            location = value;
            value.Unit = this;
            transform.localPosition = value.Position;
        }
    }

    public float Orientation
    {
        get
        {
            return orientation;
        }
        set
        {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
        }
    }

    public int TeamIndex
    {
        get
        {
            return teamIndex;
        }
        set
        {
            teamIndex = value;
            dummyMeshRenderer.material = materials[teamIndex];
        }
    }

    private void Awake()
    {
        unitAssetPrefabAnimator = unitAssetPrefab.GetComponent<Animator>();
        gridObj = GameObject.Find("Hex Grid");
        grid = gridObj.GetComponent<HexGrid>();
        dummyMeshRenderer = dummyMesh.GetComponent<Renderer>();
    }

    void OnEnable()
    {
        if (location)
        {
            transform.localPosition = location.Position;
        }
    }

    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }

    public bool IsValidDestination(HexCell cell)
    {
        return !cell.Unit;
    }

    // Метод для атаки
    public void AttackUnit(HexUnit enemy)
    {
        StopAllCoroutines();
        StartCoroutine(Attack(enemy));
        enemy.TakeDamage(this);
    }

    IEnumerator Attack(HexUnit enemy)
    {
        yield return LookAt(enemy.Location.Position);
        unitAssetPrefabAnimator.runtimeAnimatorController = animatorControllers[2];
        yield return new WaitForSeconds(1.4f);
        unitAssetPrefabAnimator.runtimeAnimatorController = animatorControllers[0];
    }

    // Метод для получения урона
    public void TakeDamage(HexUnit enemy)
    {
        --Health;
        StopAllCoroutines();
        StartCoroutine(Damage(enemy));
    }

    IEnumerator Damage(HexUnit enemy)
    {
        if (Health <= 0)
        {
            yield return new WaitForSeconds(.4f);
            unitAssetPrefabAnimator.runtimeAnimatorController = animatorControllers[4];
            yield return new WaitForSeconds(1.267f);
            unitAssetPrefabAnimator.runtimeAnimatorController = animatorControllers[0];
            grid.RemoveUnit(this);
        }
        else
        {
            yield return LookAt(enemy.Location.Position);
            unitAssetPrefabAnimator.runtimeAnimatorController = animatorControllers[3];
            yield return new WaitForSeconds(1.3f);
            unitAssetPrefabAnimator.runtimeAnimatorController = animatorControllers[0];
            AttackUnit(enemy);
        }
    }

    // Метод для уничтожения юнита
    public void Die()
    {
        if (Opponent != null)
            Opponent.Opponent = null;
        Opponent = null;
        location.Unit = null;
        Destroy(gameObject);
    }

    // Метод для перемещения юнита по пути
    public void Travel(List<HexCell> path, HexUnit enemy = null)
    {
        Location = path[path.Count - 1];
        pathToTravel = path;
        StopAllCoroutines();
        if (enemy)
        {
            StartCoroutine(TravelPath(enemy));
        }
        else
        {
            StartCoroutine(TravelPath());
        }
    }

    IEnumerator TravelPath(HexUnit enemy = null)
    {
        Vector3 a, b, c = pathToTravel[0].Position;
        transform.localPosition = c;
        yield return LookAt(pathToTravel[1].Position);

        unitAssetPrefabAnimator.runtimeAnimatorController = animatorControllers[1];
        float t = Time.deltaTime * travelSpeed;
        for (int i = 1; i < pathToTravel.Count; i++)
        {
            a = c;
            b = pathToTravel[i - 1].Position;
            c = (b + pathToTravel[i].Position) * 0.5f;
            for (; t < 1f; t += Time.deltaTime * travelSpeed)
            {
                transform.localPosition = Bezier.GetPoint(a, b, c, t);
                Vector3 d = Bezier.GetDerivative(a, b, c, t);
                d.y = 0f;
                transform.localRotation = Quaternion.LookRotation(d);
                yield return null;
            }
            t -= 1f;
        }

        a = c;
        b = pathToTravel[pathToTravel.Count - 1].Position;
        c = b;
        for (; t < 1f; t += Time.deltaTime * travelSpeed)
        {
            transform.localPosition = Bezier.GetPoint(a, b, c, t);
            Vector3 d = Bezier.GetDerivative(a, b, c, t);
            d.y = 0f;
            transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
        }
        transform.localPosition = location.Position;
        orientation = transform.localRotation.eulerAngles.y;
        unitAssetPrefabAnimator.runtimeAnimatorController = animatorControllers[0];
        ListPool<HexCell>.Add(pathToTravel);
        pathToTravel = null;
        if (enemy)
        {
            AttackUnit(enemy);
        }
    }

    IEnumerator LookAt(Vector3 point)
    {
        point.y = transform.localPosition.y;
        Quaternion fromRotation = transform.localRotation;
        Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);
        float angle = Quaternion.Angle(fromRotation, toRotation);

        if (angle > 0f)
        {
            float speed = rotationSpeed / angle;
            for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed)
            {
                transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
                yield return null;
            }
        }

        transform.LookAt(point);
        orientation = transform.localRotation.eulerAngles.y;
    }
}
