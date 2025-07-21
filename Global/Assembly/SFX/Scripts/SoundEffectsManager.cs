using DG.Tweening;
using MadApper.Singleton;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MadApper
{
    [Serializable]
    public class SFXData
    {
        public SFXSO SO;
        public string ID;
        public float Vol = .5f;
        public float Pitch = 1f;
        public bool RandomVol = true;
        public bool RandomPitch = true;
        public bool IsMusic = false;
    }



    public class SoundEffectsManager : PersistentSingleton<SoundEffectsManager>
    {
        const string k_PrefabName = "SoundEffectsManager";
        public const float k_Pitch = .05946309436f;

        [Space(10)] public List<SFXSO> m_SFXs;

        Dictionary<string, SFXSO> mappedSFXs;
        Dictionary<string, SFXSO> m_MappedSFXs
        {
            get
            {
                if (mappedSFXs == null || mappedSFXs.Count == 0)
                {
                    mappedSFXs = new Dictionary<string, SFXSO>();

                    foreach (var item in m_SFXs)
                    {
                        if (mappedSFXs.ContainsKey(item.m_ID))
                            continue;

                        mappedSFXs.Add(item.m_ID, item);
                    }
                }
                return mappedSFXs;
            }
        }

        Dictionary<string, AudioSource> sources;
        public Dictionary<string, AudioSource> m_Sources
        {
            get
            {
                if (sources == null)
                    sources = new Dictionary<string, AudioSource>();

                return sources;
            }
        }


        public static bool s_IsSoundOn => GameSettingsData.IsSoundOn;
        public static bool s_IsMusicOn => GameSettingsData.IsMusicOn;



        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            MADUtility.TryLoadAndInstantiate<SoundEffectsManager>(k_PrefabName);
        }


        private void OnEnable()
        {
            SFXHelper.onSFX += OnSFX;
        }
        private void OnDisable()
        {
            SFXHelper.onSFX -= OnSFX;
        }
        protected override void Awake()
        {
            mappedSFXs = null;

            base.Awake();
        }


        public void OnSFX(string id, float vol, float pitch, bool randomVol = true, bool randomPitch = true)
        {
            if (!s_IsSoundOn)
                return;

            if (!m_MappedSFXs.ContainsKey(id))
                return;

            var sfx = m_MappedSFXs[id];
            var source = GetSource(id);
            var ac = sfx.GetAudioClip();

            PlaySound(source, ac, volume: vol, pitch: pitch, randomVol: randomVol, randomPitch: randomPitch);
        }
        public void OnSFX(SFXSO sfx, float vol, float pitch, bool randomVol = true, bool randomPitch = true)
        {
            if (!s_IsSoundOn)
                return;

            var id = sfx.m_ID;
            var source = GetSource(id);
            var ac = sfx.GetAudioClip();

            PlaySound(source, ac, volume: vol, pitch: pitch, randomVol: randomVol, randomPitch: randomPitch);
        }       
        public void OnSFXClips(string id, AudioClip[] clips, float vol, float pitch, bool randomVol = true, bool randomPitch = true)
        {
            if (!s_IsSoundOn)
                return;

            if (clips == null || clips.Length == 0)
                return;

            var source = GetSource(id);
            var ac = clips[Random.Range(0, clips.Length)];

            PlaySound(source, ac, volume: vol, pitch: pitch, randomVol: randomVol, randomPitch: randomPitch);
        }
        public void OnSFXClip(string id, AudioClip clip, float vol, float pitch, bool randomVol = true, bool randomPitch = true)
        {
            if (!s_IsSoundOn)
                return;

            if (clip == null)
                return;

            var source = GetSource(id);
           
            PlaySound(source, clip, volume: vol, pitch: pitch, randomVol: randomVol, randomPitch: randomPitch);
        }



        public AudioSource OnSFX(SFXData data)
        {
            if (data == null)
                return null;

            var isMusic = data.IsMusic;

            if (isMusic)
            {
                if (!s_IsMusicOn)
                    return null;
            }
            else
            {
                if (!s_IsSoundOn)
                    return null;
            }

            var sfx = data.SO;

            if (sfx == null)
            {
                var dID = data.ID;

                if (!m_MappedSFXs.ContainsKey(dID))
                    return null;

                sfx = m_MappedSFXs[dID];
            }

            if (sfx == null)
                return null;


            var id = sfx.m_ID;
            var source = GetSource(id);
            var ac = sfx.GetAudioClip();
            var vol = data.Vol;
            var pitch = data.Pitch;
            var randomVol = data.RandomVol;
            var randomPitch = data.RandomPitch;
            var stop = isMusic ? true : false;
            var loop = isMusic ? true : false;

            PlaySound(source, ac, volume: vol, pitch: pitch, randomVol: randomVol, randomPitch: randomPitch, stop: stop, loop: loop);

            return source;
        }


        public static void PlaySound(AudioSource audioSource, AudioClip audioClip, float volume, float pitch,
            bool randomVol = true, bool randomPitch = true, bool stop = false, bool loop = false)
        {
            if (audioSource == null)
                return;

            volume = randomVol ? Random.Range(volume - .15f, volume + .15f) : volume;
            pitch = randomPitch ? Random.Range(pitch - .2f, pitch + .2f) : pitch;

            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;

            if (stop)
                audioSource.Stop();

            if (audioClip != null)
            {
                if (!loop)
                    audioSource.PlayOneShot(audioClip);
                else
                {
                    audioSource.clip = audioClip;
                    audioSource.Play();
                }
            }
        }


        AudioSource GetSource(string ID)
        {
            var ac = GetNotPlayingSource(ID, 0);
            ac.DOKill();
            return ac;
        }

        AudioSource GetNotPlayingSource(string ID, int index)
        {
            var uid = ID + "_" + index;

            if (m_Sources.ContainsKey(uid))
            {
                var s = m_Sources[uid];

                if (s.isPlaying)
                {
                    index++;
                    return GetNotPlayingSource(ID, index);
                }

                return s;
            }
            else
            {
                var go = new GameObject("Source " + uid);
                go.transform.SetParent(this.transform);

                var s = go.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.loop = false;

                m_Sources[uid] = s;
                return s;
            }
        }
    }

}