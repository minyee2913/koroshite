using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CamManager : MonoBehaviour
{
    public static CamManager main;
    public CinemachineCamera cam;
    public CinemachineBasicMultiChannelPerlin noise;
    public CinemachineCameraOffset camOffset;
    float orSize_d;
    float dutch_d;
    public Player spectatorTarget = null;
    Vector3 spectatorCam = new Vector3(-55.28f, -17.2f, -2f);

    IEnumerator dutchRoutine = null;
    IEnumerator offRoutine = null;

    public bool autoSpector;

    private void Awake() {
        cam = GetComponent<CinemachineCamera>();
        camOffset = GetComponent<CinemachineCameraOffset>();
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();

        main = this;

        orSize_d = cam.Lens.OrthographicSize;
        dutch_d = cam.Lens.Dutch;
    }

    private void Update() {
        if (Player.Local != null) {
            if (Player.Local.state == "room") {
                if (Player.Local.isSpectator) {
                    transform.position = spectatorCam;
                } else {
                    transform.position = Vector3.Lerp(transform.position, Player.Local.transform.position, 4 * Time.deltaTime);
                    float x = Mathf.Clamp(transform.position.x, -58f, -52f);
                    transform.position = new Vector3(x, -17.2f, -2f);
                }
            }

            if (Player.Local.state == "ingame" || Player.Local.state == "ready") {
                if (Player.Local.isSpectator) {
                    if (spectatorTarget != null) {
                        Player.Local.transform.position = spectatorTarget.transform.position;
                        
                        var lerp = Vector2.Lerp(transform.position, new Vector3(spectatorTarget.transform.position.x, spectatorTarget.transform.position.y, -10), Time.smoothDeltaTime * 10);
                        transform.position = new Vector3(lerp.x, lerp.y, -10);
                    }
                }
            }

            if (Player.Local.state == "singlePlay") {
                var lerp = Vector2.Lerp(transform.position, new Vector3(Player.Local.transform.position.x, Player.Local.transform.position.y, -10), Time.smoothDeltaTime * 10);
                transform.position = new Vector3(lerp.x, lerp.y, -10);
            }
        }
    }

    void ClearRoutine(ref IEnumerator routine) {
        if (routine != null) {
            StopCoroutine(routine);

            routine = null;
        }
    }

    public void CloseUp(float orSize, float dutch, float dur = 0) {
        ClearRoutine(ref dutchRoutine);
        dutchRoutine = _closeUp(orSize, dutch, dur);

        StartCoroutine(dutchRoutine);
    }
    public void CloseOut(float dur = 0) {
        ClearRoutine(ref dutchRoutine);
        dutchRoutine = _closeOut(dur);

        StartCoroutine(dutchRoutine);
    }
    public void Offset(Vector2 off, float dur = 0) {
        ClearRoutine(ref offRoutine);

        offRoutine = _offset(off, dur);

        StartCoroutine(offRoutine);
    }

    public void Shake(float strength = 1, float dur = 0.05f)
    {
        StartCoroutine(_shake(strength, dur));
    }

    IEnumerator _closeUp(float orSize, float dutch, float dur) {
        if (dur > 0) {
            float dSize = cam.Lens.OrthographicSize, dDutch = cam.Lens.Dutch;

            for (int i = 1; i <= 30; i++) {
                cam.Lens.OrthographicSize = dSize - (dSize - orSize) / 30 * i;
                cam.Lens.Dutch = dDutch - (dDutch - dutch) / 30 * i;

                yield return new WaitForSeconds(dur / 30);
            }
        }

        cam.Lens.OrthographicSize = orSize;
        cam.Lens.Dutch = dutch;

        dutchRoutine = null;
    }

    IEnumerator _closeOut(float dur) {
        if (dur > 0) {
            float dSize = cam.Lens.OrthographicSize, dDutch = cam.Lens.Dutch;

            for (int i = 1; i <= 30; i++) {
                cam.Lens.OrthographicSize = dSize + (orSize_d - dSize) / 30 * i;
                cam.Lens.Dutch = dDutch + (dutch_d - dDutch) / 30 * i;

                yield return new WaitForSeconds(dur / 30);
            }
        }
        
        cam.Lens.OrthographicSize = orSize_d;
        cam.Lens.Dutch = dutch_d;

        dutchRoutine = null;
    }

    IEnumerator _offset(Vector3 off, float dur = 0) {
        if (dur > 0) {
            Vector2 beforeOff = camOffset.Offset;

            for (int i = 1; i <= 30; i++) {
                camOffset.Offset = new Vector3(
                    beforeOff.x - (beforeOff.x - off.x) / 30 * i,
                    beforeOff.y - (beforeOff.y - off.y) / 30 * i
                );

                yield return new WaitForSeconds(dur / 30);
            }
        }

        camOffset.Offset = off;

        offRoutine = null;
    }

    IEnumerator _shake(float strength, float dur)
    {
        noise.AmplitudeGain = strength;
        noise.FrequencyGain = strength;

        yield return new WaitForSeconds(dur);

        noise.AmplitudeGain = 0;
        noise.FrequencyGain = 0;
    }
}
