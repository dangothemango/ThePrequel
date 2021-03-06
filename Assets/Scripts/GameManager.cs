﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public static GameManager INSTANCE;

    public GameObject cam;
    public GameObject chaosTarget;
    public Button chaosButton;
    public GameObject thunderCloud;
    public Transform[] missleSpawns;

    public Coroutine chaosRoutine;

    Person virus;
    float oZ;

    public GameObject virusUI;

    public Person Virus {
        get {
            return virus;
        }
    }

    List<Person> people;
    List<Cloud> clouds;
    List<Car> cars;
    float chaos = 0;
    int superChaos = 0;

    public int SuperChaos {
        get {
            return superChaos;
        }
        set {
            superChaos = value;
            if (superChaos == 1) {
                StartCoroutine(ZoomInAndBackToZero());
            }
            else if (superChaos == 3) {
                StartCoroutine(ZoomInAndPastZero());
            }
            else if (superChaos == 5) {
                StartCoroutine(RotateCamAroundZ());
            } else if (superChaos == 15) {
                StartCoroutine(ZoomAway());
                StartCoroutine(ButtonToCenter());
            }
        }
    }

    public float Chaos {
        get {
            return chaos;
        }
        set {
            chaos = value;
        }
    }

    // Use this for initialization
    void Awake() {
        if (INSTANCE != null) {
            this.enabled = false;
            return;
        }
        INSTANCE = this;
        people = new List<Person>();
        clouds = new List<Cloud>();
        cars = new List<Car>();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            Chaos = 0.99f;
        } else if (Input.GetKeyDown(KeyCode.X)) {
            chaosRoutine = null;
            Chaos = 0;
            SuperChaos = 0;
            StopAllCoroutines();
        }
        if (Random.Range(0, 1.0f) > .994f) {
            SpawnCloud();
        }
        if (Random.Range(0, 1.0f) > .99f) {
            GetRandomPerson().SpawnBubbles();
        }
    }

    public void AddPerson(Person p) {
        people.Add(p);
    }

    public void RemovePerson(Person p) {
        people.Remove(p);
    }

    public void AddCar(Car c) {
        cars.Add(c);
    }

    public void RemoveCar(Car c) {
        cars.Remove(c);
    }

    public void AddCloud(Cloud c) {
        clouds.Add(c);
    }

    public void RemoveCloud(Cloud c) {
        clouds.Remove(c);
    }

    public Person GetRandomPerson() {
        if (people.Count == 0) {
            return null;
        }
        return people[Random.Range(0, people.Count)];
    }

    void SpawnCloud() {
        float x = Random.Range(-10.0f, 10.0f);
        float y = Mathf.Sqrt(100.0f - (x * x)) * (Random.Range(0, 1.0f) > .5f ? -1 : 1);
        Instantiate(thunderCloud, new Vector3(x, y, 0), Quaternion.Euler(0, 0, 0), transform);
    }

    public Transform GetRandomMissleSpawn() {
        return missleSpawns[Random.Range(0, missleSpawns.Length)];
    }

    public void IncChaos() {
        //TODO change this to 1f
        if (Chaos < 1f) {
            Chaos += .01f;
        }
        else if (virus == null && virusUI.transform.localScale == Vector3.zero) {
            Debug.Log("Sentience");
            virus = GetRandomPerson().BecomeSentient();
			virus.state = "sentient";
            people.Remove(virus);
        } else if (virus!=null && chaosRoutine ==null){
            chaosRoutine = StartCoroutine(GeneratePlayerChaos());
        }
        Debug.Log(Chaos);
    }

    public void ScaleVirusUI() {
        StartCoroutine(ScaleVirusUi());
    }

    IEnumerator ScaleVirusUi() {
        float t = 0;
        while (t <= 3f) {
            t += Time.deltaTime;
            virusUI.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t / 3f);
            yield return null;
        }
        StartCoroutine(GeneratePlayerChaos());
    }

    IEnumerator GeneratePlayerChaos() {

        float t = 0;
        float jumpTime = 1f;
        while (t < jumpTime) {
            t += Time.deltaTime;
            virusUI.transform.localPosition = new Vector3(0, Mathf.Sin(t * Mathf.PI) * 30, 0);
            yield return null;
        }
        chaosButton.onClick.Invoke();
        SuperChaos++;
        yield return new WaitForSeconds(2 * Mathf.PI - jumpTime);
        StartCoroutine(GeneratePlayerChaos());
    }

    IEnumerator ZoomInAndBackToZero() {
        oZ = cam.transform.position.z;
        float t = 0;
        while (SuperChaos != 3) {
            float prev = t;
            t += Time.deltaTime;
            float cur = t;
            cam.transform.position += transform.forward * (Mathf.Abs(Mathf.Sin(cur)) - Mathf.Abs(Mathf.Sin(prev))) * 5;
            yield return null;
        }
    }

    IEnumerator ZoomInAndPastZero() {
        Vector3 pos = cam.transform.position;
        pos.z = oZ;
        cam.transform.position = pos;
        while (cam.transform.position != pos) {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, pos, Time.deltaTime * 5);
            yield return null;
        }
        float t = 0;
        while (true) {
            if (SuperChaos == 15) {
                yield break;
            }
            float prev = t;
            t += Time.deltaTime;
            float cur = t;
            cam.transform.position += transform.forward * (Mathf.Sin(cur) - Mathf.Sin(prev)) * 5;
            yield return null;
        }
    }

    IEnumerator RotateCamAroundZ() {
        int dir = Random.Range(0, 1f) > .5f ? 1 : -1;
        while (true) {
            cam.transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * Random.Range(0, 20f) * dir);
            cam.transform.LookAt(Vector3.zero);
            if (Random.Range(0, 1.0f) > .994) {
                dir *= -1;
            }
            yield return null;
        }
    }

    IEnumerator ZoomAway() {
        while (true) {
            cam.transform.position -= transform.forward * Time.deltaTime * 14.45f;
            yield return null;
        }
    }

    IEnumerator ButtonToCenter() {
        float t = 0;
        Vector3 position = chaosButton.transform.localPosition;
        while (t < 10f) {
            t += Time.deltaTime;
            chaosButton.transform.localPosition = Vector3.Lerp(position, Vector3.zero, t / 10f);
            yield return null;
        }
        while (true) {
            chaosButton.transform.localPosition = Vector3.zero;
            yield return null;
        }
    }
    
}
