using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public SpriteRenderer render;
    public Rigidbody2D rb;
    public Collider2D col;
    public ContactFilter2D filter;
    PhotonView pv;

    public Action<Transform, Projectile> OnHit = null;
    public List<Transform> targets = new();
    Collider2D[] results;
    bool hitted;
    public float LifeTime;
    float time;
    bool mine;
    
    void Awake()
    {
        render = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        pv = GetComponent<PhotonView>();

        if (UtilManager.CheckPhoton()) {
            mine = pv.IsMine;
        } else {
            mine = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mine) {
            results = new Collider2D[10];
            targets.Clear();

            int count = Physics2D.OverlapCollider(col, filter, results);

            if (count > 0) {
                for (int i = 0; i < count; i++) {
                    Collider2D col_ = results[i];
                    if ((LayerMask.GetMask("ground") & (1 << col_.gameObject.layer)) != 0) {
                        continue;
                    }

                    targets.Add(col_.transform);
                }
            }

            if (hitted) {
                if (targets.Count <= 0) {
                    hitted = false;
                }
            } else {
                if (targets.Count > 0) {
                    if (OnHit != null) {
                        OnHit(targets[0], this);
                    }

                    hitted = true;
                }
            }

            if (LifeTime != 0) {
                time += Time.deltaTime;

                if (time > LifeTime) {
                    Dispose();
                }
            }
        }
    }

    public void Dispose() {
        if (UtilManager.CheckPhoton()) {
            PhotonNetwork.Destroy(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}
