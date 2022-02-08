using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnhollowerRuntimeLib;
using Il2CppSystem.Collections.Generic;
using System;
using System.IO;
using HeroCameraName;
using Item;
using BulletChange;
using OCServerMoveNS;
using DataHelper;
using TMPro;

namespace zcMod
{
    public static class BuildInfo
    {
        public const string Name = "zcMod"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "Gunfire Reborn Aimbot"; // Description for the Mod.  (Set as null if none)
        public const string Author = "zhuchong"; // Author of the Mod.  (Set as null if none)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class zcMod : MelonMod
    {
        public static bool autoAimOn = false;
        public static string lastTargetName = "";
        public override void OnUpdate() // Runs once per frame.
        {
            try
            {
                if (Input.GetKeyUp(KeyCode.X))
                {
                    autoAimOn = !autoAimOn;
                    if(autoAimOn) MelonLogger.Msg(ConsoleColor.Yellow, "Auto aim is ON.");
                    else MelonLogger.Msg(ConsoleColor.Yellow, "Auto aim is OFF.");
                }

                if ((Input.GetMouseButton(0) || Input.GetMouseButtonDown(1)) && autoAimOn)//按F键自瞄(请按个人喜好修改)
                {
                    List<NewPlayerObject> monsters = NewPlayerManager.GetMonsters();

                    Camera curCamera = null;
                    foreach (var v in GameObject.FindObjectsOfType<Camera>())
                    {
                        if (v.isActiveAndEnabled)
                        {
                            curCamera = v;
                            break;
                        }
                    }

                    if (monsters != null && curCamera !=null)
                    {
                        Vector3 campos = curCamera.transform.position;
                        Transform nearmons = null;
                        float neardis = 99999f;
                        foreach (NewPlayerObject monster in monsters)
                        {
                            if (monster == null) continue;
                            if (monster.BodyPartCom == null) continue;

                            Transform montrans = monster.BodyPartCom.GetWeakTrans();

                            if (montrans == null) continue;

                            Vector3 limtitVec = curCamera.WorldToViewportPoint(montrans.position);
                            if (limtitVec.z <= 0) continue;
                            limtitVec.y = (0.5f - limtitVec.y) * Screen.height;
                            limtitVec.x = (0.5f - limtitVec.x) * Screen.width;
                            limtitVec.z = 0f;

                            float weight = limtitVec.magnitude;
                            if (weight > 250f) continue;
                            

                            Vector3 vec = montrans.position - campos;
                            Ray ray = new Ray(campos, vec);
                            var hits = Physics.RaycastAll(ray, vec.magnitude);
                            bool visible = true;
                            foreach (var hit in hits)
                            {

                                if (hit.collider.gameObject.layer == 0 || hit.collider.gameObject.layer == 30 || hit.collider.gameObject.layer == 31) //&& hit.collider.name.Contains("_")
                                {
                                    visible = false;
                                    break;
                                }
                            }

                            if (visible)
                            {
                                if (weight < neardis)
                                {
                                    neardis = weight;
                                    nearmons = montrans;
                                }
                            }
                        }

                        if (nearmons != null)
                        {
                            if(nearmons.name != lastTargetName)
                            {
                                lastTargetName = nearmons.name;
                                MelonLogger.Msg("Auto aim at monspos : " + nearmons.name);
                            }

                            float neckFix = 0f;
                            if (nearmons.name.Contains("Head"))
                            {
                                neckFix = 0.25f;
                            }

                            Vector3 fw1 = nearmons.position - HeroCameraManager.HeroObj.gameTrans.position;
                            fw1.y = 0;
                            HeroCameraManager.HeroObj.gameTrans.rotation = Quaternion.LookRotation(fw1);

                            var cameraParent = curCamera.transform.parent;
                            Vector3 fw2 = nearmons.position - cameraParent.position;
                            fw2.y += neckFix;
                            var tr = Quaternion.LookRotation(fw2);
                            cameraParent.rotation = tr;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                MelonLogger.Msg("Exception : " + e.Message);
            }
        }

        public override void OnFixedUpdate() // Can run multiple times per frame. Mostly used for Physics.
        {
        }

        public override void OnLateUpdate() // Runs once per frame after OnUpdate and OnFixedUpdate have finished.
        {
        }

        public override void OnGUI() // Can run multiple times per frame. Mostly used for Unity's IMGUI.
        {
        }
    }
}
