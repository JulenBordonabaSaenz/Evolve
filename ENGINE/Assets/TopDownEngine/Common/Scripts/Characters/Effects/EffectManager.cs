﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{

    public class EffectManager : MonoBehaviour
    {
        public List<Health> healthList = new List<Health>();

        public GameObject iconPrefab;

        public AudioSource effectAudioSource;

        public GameObject iconContainer;

        public List<EffectData> activeEffects = new List<EffectData>();

        public List<EffectIcon> activeIcons = new List<EffectIcon>();

        private InputManager inputManager;

        public EffectData[] effects;

        private void Start()
        {
            healthList = new List<Health>(GetComponents<Health>());
            effects = Resources.LoadAll<EffectData>("Data/Effects");
        }

        public UnityEvent OnEffectStart = new UnityEvent();
        public UnityEvent OnEffectEnd = new UnityEvent();


        //[PunRPC]
        public void StartEffect(string _effect)
        {
            EffectData ed = GetEffectByID(_effect);

            if (CheckEffect(_effect))
            {
                EffectData _ed = GetActiveEffectByID(ed.id);

                EffectIcon icon = GetActiveIconByID(_ed.id);

                if (!icon)
                {
                    icon = CreateIcon(_ed);
                }
                else
                {
                    StopCoroutine(icon.durationCoroutine);
                    icon.durationCoroutine = null;
                }

                icon.durationCoroutine = StartCoroutine(EffectDuration(icon));
                if (_ed.effectClip)
                {
                    effectAudioSource.PlayOneShot(_ed.effectClip);
                }
                //print("resetea tiempo");
            }
            else
            {
                //print("new effect");
                activeEffects.Add(ed);
                ed.dot.dotEffect = StartCoroutine(DOTEffect(ed.dot));
                EffectIcon icon = CreateIcon(ed);
                icon.durationCoroutine = StartCoroutine(EffectDuration(icon));
                if (ed.effectClip)
                {
                    effectAudioSource.PlayOneShot(ed.effectClip);
                }
            }

            OnEffectStart.Invoke();

        }

        EffectIcon CreateIcon(EffectData ed)
        {
            EffectIcon newIcon = Instantiate(iconPrefab, iconContainer.transform).GetComponent<EffectIcon>();
            if (ed.icon)
            {
                newIcon.icon.sprite = ed.icon;
            }
            newIcon.effect = ed;
            activeIcons.Add(newIcon);
            return newIcon;
        }

        EffectIcon GetActiveIconByID(string _id)
        {
            foreach (EffectIcon ei in activeIcons)
            {
                if (ei.effect.id == _id) return ei;
            }
            return null;
        }

        EffectData GetEffectByID(string _id)
        {
            foreach (EffectData ed in effects)
            {
                if (ed.id == _id) return ed;
            }
            return null;
        }

        EffectData GetActiveEffectByID(string _id)
        {
            foreach (EffectData ed in activeEffects)
            {
                if (ed.id == _id) return ed;
            }
            return null;
        }

        bool CheckEffect(string _effectDataID)
        {
            foreach (EffectData ed in activeEffects)
            {
                if (ed.id == _effectDataID) return true;
            }
            return false;
        }

        //[PunRPC]
        public void StopEffect(string ed)
        {
            //print("Para el efecto");
            if (CheckEffect(ed))
            {
                EffectData _ed = GetActiveEffectByID(ed);

                //si es permanente no se puede borrar
                if (_ed.permanent) return;

                //se quita el efecto de los efectos activos
                activeEffects.Remove(_ed);

                EffectIcon icon = GetActiveIconByID(_ed.id);

                //borar icono
                if (icon)
                {
                    activeIcons.Remove(icon);
                    Destroy(icon.gameObject);
                }

                //se para el dot en caso de que lo haya
                if (_ed.dot.dotEffect != null)
                {
                    StopCoroutine(_ed.dot.dotEffect);
                }
                OnEffectEnd.Invoke();
            }
        }

        //[PunRPC]
        public void StopPermanentEffect(string ed)
        {
            //print("Para el efecto");
            if (CheckEffect(ed))
            {
                EffectData _ed = GetActiveEffectByID(ed);

                //se quita el efecto de los efectos activos
                activeEffects.Remove(_ed);

                EffectIcon icon = GetActiveIconByID(_ed.id);

                //borar icono
                if (icon)
                {
                    activeIcons.Remove(icon);
                    Destroy(icon.gameObject);
                }

                //se para el dot en caso de que lo haya
                if (_ed.dot.dotEffect != null)
                {
                    StopCoroutine(_ed.dot.dotEffect);
                }
                OnEffectEnd.Invoke();
            }
        }

        public void ClearEffects()
        {
            foreach (EffectData ed in activeEffects)
            {
                StopCoroutine(ed.dot.dotEffect);
            }
            activeEffects.Clear();
            OnEffectEnd.Invoke();
        }



        IEnumerator EffectDuration(EffectIcon icon)
        {
            if (!icon.effect.permanent)
            {
                icon.currentDuration = icon.effect.duration;
                while (icon.currentDuration > 0)
                {
                    icon.currentDuration -= Time.deltaTime;
                    yield return null;
                }
                StopEffect(icon.effect.id);
            }
            else
            {
                icon.currentDuration = Mathf.Infinity;
                while (icon.currentDuration > 0)
                {
                    icon.currentDuration -= Time.deltaTime;
                    yield return null;
                }
                StopEffect(icon.effect.id);
            }
        }

        IEnumerator DOTEffect(DOT dot)
        {

            while (true)
            {
                
                if (healthList.Count <= 0) yield break;
                foreach (Health h in healthList)
                {
                    //if (photonView.isMine)
                    {
                        if (dot.damagePerTick > 0)
                        {
                            //dm.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.AllBuffered, dot.damagePerTick, true);
                            //dm.GetDamage(dot.damagePerTick);

                            h.Damage((int)dot.damagePerTick, null, 0f, 0f);
                        }
                    }
                }
                yield return new WaitForSeconds(dot.loopTime);


            }
        }
        

        #region SilenceAbilities


        public bool SilenceAbilities
        {
            get
            {
                foreach (EffectData ed in activeEffects)
                {
                    if (ed.silenceAbilities) return true;
                }
                return false;
            }
        }
        #endregion

        #region InvertControls
        //public bool InvertControls
        //{
        //    get
        //    {
        //        foreach (EffectData ed in activeEffects)
        //        {
        //            if (ed.invertControls) return true;
        //        }
        //        return false;
        //    }
        //}
        #endregion


        #region color

        //public Color EffectColor
        //{
        //    get
        //    {
        //        Color myColor = Color.white;
        //        foreach (EffectData ed in activeEffects)
        //        {
        //            myColor *= ed.effectColor;
        //        }

        //        return myColor;
        //    }
        //}
        #endregion

        #region stats

        public float DamageReduction
        {
            get
            {
                float damageReduction = 0;
                foreach (EffectData ed in activeEffects)
                {
                    damageReduction += ed.damageReduction;
                }
                return Mathf.Clamp(damageReduction, 0f, 100f);
            }
        }


        public float Slow
        {
            get
            {
                float slow = 0;
                foreach (EffectData ed in activeEffects)
                {
                    slow += ed.slow;
                }
                return Mathf.Clamp(slow, 0f, 100f);
            }
        }

        public float MovementSpeed
        {
            get
            {
                float movementSpeed = 0;
                foreach (EffectData ed in activeEffects)
                {
                    movementSpeed += ed.movementSpeed;
                }
                return movementSpeed;
            }
        }

        public int MaxHealth
        {
            get
            {
                int maxHealth = 0;
                foreach (EffectData ed in activeEffects)
                {
                    maxHealth += ed.maxHealth;
                }
                return maxHealth;
            }
        }


        public float MaxHealthMultiplier
        {
            get
            {
                float maxHealthMultiplier = 1;
                foreach (EffectData ed in activeEffects)
                {
                    maxHealthMultiplier *= ed.maxHealthMultiplier;
                }
                return maxHealthMultiplier;
            }
        }

        public float MovementSpeedMultiplier
        {
            get
            {
                float movementSpeedMultiplier = 1;
                foreach (EffectData ed in activeEffects)
                {
                    movementSpeedMultiplier *= ed.movementSpeedMultiplier;
                }
                return movementSpeedMultiplier;
            }
        }

        public float AttackSpeed
        {
            get
            {
                float attackSpeed = 0;
                foreach (EffectData ed in activeEffects)
                {
                    attackSpeed += ed.attackSpeed;
                }
                return attackSpeed;
            }
        }

        public float AttackSpeedMultiplier
        {
            get
            {
                float attackSpeedMultiplier = 1;
                foreach (EffectData ed in activeEffects)
                {
                    attackSpeedMultiplier *= ed.attackSpeedMultiplier;
                }
                return attackSpeedMultiplier;
            }
        }

        //public float AttackDistance
        //{
        //    get
        //    {
        //        float attackDistance = 0;
        //        foreach (EffectData ed in activeEffects)
        //        {
        //            attackDistance += ed.attackDistance;
        //        }
        //        return attackDistance;
        //    }
        //}

        //public float AttackDamage
        //{
        //    get
        //    {
        //        float attackDamage = 0;
        //        foreach (EffectData ed in activeEffects)
        //        {
        //            attackDamage += ed.attackDamage;
        //        }
        //        return attackDamage;
        //    }
        //}


        //public float Maneuver
        //{
        //    get {
        //        float maneuverability = 0;
        //        foreach (EffectData ed in activeEffects)
        //        {
        //            maneuverability += ed.maneuverability;
        //        }
        //        return Mathf.Clamp(maneuverability, 0f, 100f);
        //    }
        //}


        //public float Acceleration
        //{
        //    get {
        //        float acceleration = 0;
        //        foreach (EffectData ed in activeEffects)
        //        {
        //            acceleration += ed.acceleration;
        //        }
        //        return Mathf.Clamp(acceleration, 0f, 100f);
        //    }
        //}


        //public float ShotDamage
        //{
        //    get {
        //        float shotDamage = 0;
        //        foreach (EffectData ed in activeEffects)
        //        {
        //            shotDamage += ed.shotDamage;
        //        }
        //        return Mathf.Clamp(shotDamage, 0f, 100f);
        //    }
        //}

        #endregion

    }
}
