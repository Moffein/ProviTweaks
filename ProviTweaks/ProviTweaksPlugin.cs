using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.Projectile;
using System;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.AddressableAssets;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace R2API.Utils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute
    {
    }
}

namespace ProviTweaks
{
    [BepInPlugin("com.Moffein.ProviTweaks","ProviTweaks","1.0.0")]
    public class ProviTweaksPlugin : BaseUnityPlugin
    {
        public static bool buffDamage, freezeResist, nerfWurmHP, nerfPhase4HP, removePhaseSkips, buffProviMalachiteHoming;

        void Awake()
        {
            freezeResist = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Freeze Resistance"), true).Value;
            if (freezeResist) FreezeResist();

            buffDamage = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Buff Damage"), true).Value;
            if (buffDamage) BuffDamage();

            buffProviMalachiteHoming = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Consistent Malachite Energy Ball Homing"), true).Value;
            if (buffProviMalachiteHoming) MalachiteHoming();

            nerfWurmHP = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Phase 2 - Nerf Wurm HP"), true).Value;
            if (nerfWurmHP) NerfWurmHP();

            nerfPhase4HP = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Phase 4 - Nerf HP"), true).Value;
            if (nerfPhase4HP) NerfPhase4HP();

            removePhaseSkips = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Remove Phase Skipping"), true).Value;
            if (removePhaseSkips) RemovePhaseSkips();
        }

        #region Prefab Accessors
        public GameObject GetProviPhase1()
        {
            return Addressables.LoadAssetAsync<GameObject>("49a36f2f5301b6d4c81788b564647653").WaitForCompletion();
        }

        public GameObject GetProviPhase3()
        {
            return Addressables.LoadAssetAsync<GameObject>("7760f3431e7b28e4cb9db0fc8d596c34").WaitForCompletion();
        }

        public GameObject GetProviPhase4()
        {
            return Addressables.LoadAssetAsync<GameObject>("321baa9bbc94f3a41a1e0d1c53faf14d").WaitForCompletion();
        }

        public GameObject GetProviShadowClone()
        {
            return Addressables.LoadAssetAsync<GameObject>("7dca732a74031394280060323650cec2").WaitForCompletion();
        }

        public GameObject GetProviShadowCloneLasor()
        {
            return Addressables.LoadAssetAsync<GameObject>("9ae8aed032df19a449a39553df1dd1d3").WaitForCompletion();
        }

        public GameObject GetGildedWurmLaser()
        {
            return Addressables.LoadAssetAsync<GameObject>("79b1ab93254bec7459befdb838c09aee").WaitForCompletion();
        }

        public GameObject GetGildedWurmOrb()
        {
            return Addressables.LoadAssetAsync<GameObject>("b03cedc4247b68245a41b3a219d77082").WaitForCompletion();
        }
        #endregion

        private void FreezeResist()
        {
            void AddFreezeResist(GameObject bodyObject)
            {
                //if (!bodyObject) return;  //Fail loudly, don't nullcheck

                AlterBuffDuration[] components = bodyObject.GetComponents<AlterBuffDuration>();
                bool hasFreezeResist = false;

                if (components != null)
                {
                    for (int i = 0; i < components.Length; i++)
                    {
                        var comp = components[i];
                        if (comp && comp.customName == "FreezeResistance")
                        {
                            hasFreezeResist = true;
                            return;
                        }
                    }
                }

                if (hasFreezeResist) return;

                AlterBuffDuration freezeResist = bodyObject.AddComponent<AlterBuffDuration>();
                freezeResist.customName = "FreezeResistance";
                freezeResist.durationScalar = 0.5f;
                freezeResist.affectsFreeze = true;
                freezeResist.affectsMidasTouch = true;
                freezeResist.affectsStunStates = true;
            }

            AddFreezeResist(GetProviPhase1());
            AddFreezeResist(GetProviPhase3());
            AddFreezeResist(GetProviPhase4());
            AddFreezeResist(GetProviShadowClone());
        }

        private void MalachiteHoming()
        {
            //30f vanilla, worms use 40f
            Addressables.LoadAssetAsync<GameObject>("f659b06778fd6174397fbaf44baf2484").WaitForCompletion().GetComponent<ProvidenceMalachiteEnergyProjectileController>().turnSpeedAfterActivation = 40f;
        }

        private void BuffDamage()
        {
            GameObject provPhase4Object = GetProviPhase4();

            GameObject shadowCloneObject = GetProviShadowClone();
            var shadowCloneBody = shadowCloneObject.GetComponent<CharacterBody>();
            shadowCloneBody.baseDamage = 11f;   //9f vanilla, Provi is 15
            shadowCloneBody.levelDamage = 2.2f;

            GameObject lasorCloneObject = GetProviShadowCloneLasor();
            var lasorCloneBody = lasorCloneObject.GetComponent<CharacterBody>();
            lasorCloneBody.baseDamage = 11f;    //9f vanilla, Provi is 15
            lasorCloneBody.levelDamage = 2.2f;

            //Malachite Energy Balls
            void BuffMalachiteEnergyBall(GameObject bodyObject)
            {
                ProvidenceController pc = bodyObject.GetComponent<ProvidenceController>();
                if (pc)
                {
                    pc.malachiteEnergyDamageCoefficient = 2f;   //1.5f vanilla
                }

                ProvidenceShadowCloneController psc = bodyObject.GetComponent<ProvidenceShadowCloneController>();
                if (psc)
                {
                    psc.malachiteEnergyDamageCoefficient = 2f;  //1.5f vanilla
                }
            }
            BuffMalachiteEnergyBall(GetProviPhase1());
            BuffMalachiteEnergyBall(GetProviPhase3());
            BuffMalachiteEnergyBall(GetProviPhase4());
            BuffMalachiteEnergyBall(provPhase4Object);
            BuffMalachiteEnergyBall(shadowCloneObject);
            BuffMalachiteEnergyBall(lasorCloneObject);
            GetGildedWurmOrb().GetComponent<WormBodyPositions2>().meatballDamageCoefficient = 2.4f; //1.8f vanilla, matches Provi buff (Wurms have lower base damage than Provi)

            //Sword Swipe
            SneedUtils.SetAddressableEntityStateField("87cb60f0d335f8142abdc70bd7da6858", "damageCoefficient", "5");//4f vanilla

            //Shadow Clone Sword Swipe
            SneedUtils.SetAddressableEntityStateField("1a829e96fe9101d4ba6fd723b674f991", "damageCoefficient", "5");//4f vanilla

            //Downslam
            SneedUtils.SetAddressableEntityStateField("51756521a12695b48b42ceca58712e71", "damageCoefficient", "7");//6f vanilla
            SneedUtils.SetAddressableEntityStateField("51756521a12695b48b42ceca58712e71", "projectileDamageCoefficient", "3.5");//3f vanilla

            //Undead Army Downslam
            SneedUtils.SetAddressableEntityStateField("2af7311b34b9940428edc7c6630ef48c", "damageCoefficient", "7");//6f vanilla
            SneedUtils.SetAddressableEntityStateField("2af7311b34b9940428edc7c6630ef48c", "projectileDamageCoefficient", "3.5");//3f vanilla 

            //Shadow Clone Downslam
            SneedUtils.SetAddressableEntityStateField("3c9674995c358654f8289f8017058c00", "damageCoefficient", "7");//6f vanilla
            SneedUtils.SetAddressableEntityStateField("3c9674995c358654f8289f8017058c00", "projectileDamageCoefficient", "3.5");//3f vanilla 

            //Vertical Lasers
            SneedUtils.SetAddressableEntityStateField("51756521a12695b48b42ceca58712e71", "desperationProjectileDamageCoefficient", "5");//Downslam, 2.5f vanilla
            SneedUtils.SetAddressableEntityStateField("2af7311b34b9940428edc7c6630ef48c", "desperationProjectileDamageCoefficient", "5");//Undead Army Downslam, 2.5f vanilla
            GetGildedWurmLaser().GetComponent<WormBodyPositions2>().meatballDamageCoefficient = 6.25f; //3.125f vanilla, matches Provi buff (Wurms have lower base damage than Provi)

            //ChannelMines
            Addressables.LoadAssetAsync<GameObject>("301aad72a0758474cad5ca864f9f4079").WaitForCompletion().GetComponent<ProvidenceLocalMineController>().damageCoefficient = 8f;//5f vanilla

            //Final Phase stuff
            SneedUtils.SetAddressableEntityStateField("cdb3ef2d24b3f35419f825a646c0e8bb", "malachiteOrbDamageCoefficient", "2");//1.5f vanilla
            SneedUtils.SetAddressableEntityStateField("cdb3ef2d24b3f35419f825a646c0e8bb", "lasorPillarDamageCoefficient", "5");//2.5f vanilla
            provPhase4Object.GetComponent<ProvidenceFinalPhaseDamageAura>().hitFrequency = 10f; //20f x 0.02 per tick in vanilla (-40% HP/s), reduce this to -20% HP/s
        }

        private void NerfPhase4HP()
        {
            var bodyObject = GetProviPhase4();
            CharacterBody body = bodyObject.GetComponent<CharacterBody>();
            if (body.baseMaxHealth > 1000f) body.baseMaxHealth = 1000f;
            if (body.levelMaxHealth > 300f) body.levelMaxHealth = 300f;
        }

        private void NerfWurmHP()
        {
            void ModifyStats(GameObject bodyObject)
            {
                CharacterBody body = bodyObject.GetComponent<CharacterBody>();
                if (body.baseMaxHealth > 400f) body.baseMaxHealth = 400f;
                if (body.levelMaxHealth > 120f) body.levelMaxHealth = 120f;
            }
            ModifyStats(GetGildedWurmOrb());
            ModifyStats(GetGildedWurmLaser());
        }

        private void RemovePhaseSkips()
        {
            void ApplyPhaseSkipProtection(GameObject bodyObject)
            {
                bodyObject.AddComponent<RemoveGodModeAfterDelay>();
                bodyObject.GetComponent<HealthComponent>().isDefaultGodMode = true;
            }

            ApplyPhaseSkipProtection(GetProviPhase1());
            ApplyPhaseSkipProtection(GetGildedWurmOrb());
            ApplyPhaseSkipProtection(GetGildedWurmLaser());
            ApplyPhaseSkipProtection(GetProviPhase3());
            ApplyPhaseSkipProtection(GetProviPhase4());
        }
    }

    [RequireComponent(typeof(HealthComponent))]
    public class RemoveGodModeAfterDelay : MonoBehaviour
    {
        public float delay = 0.25f;

        private float stopwatch;
        
        private void OnEnable()
        {
            stopwatch = 0f;
        }

        private void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= delay)
            {
                HealthComponent hc = GetComponent<HealthComponent>();
                if (hc)
                {
                    hc.godMode = false;
                }
                this.enabled = false;
            }
        }
    }
}
