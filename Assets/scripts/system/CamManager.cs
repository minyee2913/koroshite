using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CamManager : MonoBehaviour
{
    public static CamManager main;
    public CinemachineVirtualCamera cam;
    public CinemachineBasicMultiChannelPerlin noise;
    public CinemachineCameraOffset camOffset;
    float orSize_d;
    float dutch_d;

    IEnumerator dutchRoutine = null;
    IEnumerator offRoutine = null;

    private void Awake() {
        cam = GetComponent<CinemachineVirtualCamera>();
        camOffset = GetComponent<CinemachineCameraOffset>();
        noise = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        main = this;

        orSize_d = cam.m_Lens.OrthographicSize;
        dutch_d = cam.m_Lens.Dutch;
    }

    private void Update() {
        if (Player.Local != null && Player.Local.state == "room") {
            transform.position = Vector3.Lerp(transform.position, Player.Local.transform.position, 4 * Time.deltaTime);
            float x = Mathf.Clamp(transform.position.x, -58f, -52f);
            transform.position = new Vector3(x, -17.2f, -2f);
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
            float dSize = cam.m_Lens.OrthographicSize, dDutch = cam.m_Lens.Dutch;

            for (int i = 1; i <= 10; i++) {
                cam.m_Lens.OrthographicSize = dSize - (dSize - orSize) / 10 * i;
                cam.m_Lens.Dutch = dDutch - (dDutch - dutch) / 10 * i;

                yield return new WaitForSeconds(dur / 10);
            }
        }

        cam.m_Lens.OrthographicSize = orSize;
        cam.m_Lens.Dutch = dutch;

        dutchRoutine = null;
    }

    IEnumerator _closeOut(float dur) {
        if (dur > 0) {
            float dSize = cam.m_Lens.OrthographicSize, dDutch = cam.m_Lens.Dutch;

            for (int i = 1; i <= 10; i++) {
                cam.m_Lens.OrthographicSize = dSize + (orSize_d - dSize) / 10 * i;
                cam.m_Lens.Dutch = dDutch + (dutch_d - dDutch) / 10 * i;

                yield return new WaitForSeconds(dur / 10);
            }
        }
        
        cam.m_Lens.OrthographicSize = orSize_d;
        cam.m_Lens.Dutch = dutch_d;

        dutchRoutine = null;
    }

    IEnumerator _offset(Vector3 off, float dur = 0) {
        if (dur > 0) {
            Vector2 beforeOff = camOffset.m_Offset;

            for (int i = 1; i <= 10; i++) {
                camOffset.m_Offset = new Vector3(
                    beforeOff.x - (beforeOff.x - off.x) / 10 * i,
                    beforeOff.y - (beforeOff.y - off.y) / 10 * i
                );

                yield return new WaitForSeconds(dur / 10);
            }
        }

        camOffset.m_Offset = off;

        offRoutine = null;
    }

    IEnumerator _shake(float strength, float dur)
    {
        noise.m_AmplitudeGain = strength;
        noise.m_FrequencyGain = strength;

        yield return new WaitForSeconds(dur);

        noise.m_AmplitudeGain = 0;
        noise.m_FrequencyGain = 0;
    }
}
