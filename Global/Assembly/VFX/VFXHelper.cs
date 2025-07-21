using BEH;
using Cysharp.Threading.Tasks;
using MadApper;
using System;
using UnityEngine;

namespace BEH
{
    [Serializable]
    public class VFXHelper : MonoBehaviour
    {
        [Space(10)][SerializeField] string m_ID;
        [Space(10)][SerializeField] float m_TimeToDespawn;
        [Space(10)][SerializeField][AutoGetInChildren] VFX m_VFX;
        [Space(10)][SerializeField] Courier.Params m_CourierParams;
        [Space(10)][SerializeField] Transform m_StartVFXPos;
        [Space(10)][SerializeField] VFX.Args m_Args;

        Camera cam;
        Camera m_Camera
        {
            get
            {
                if (cam == null)
                    cam = Camera.main;
                return cam;
            }
        }

        public Camera GetCamera() => m_Camera;

        public void RunVFX(string id, Vector3 endPosition, Action onEnded)
        {
            var vfx = PoolVFX.Get(id: id, prefab: m_VFX);

            Action<Courier> onstarted = (courier) =>
            {
                vfx.transform.SetParent(courier.transform);
                vfx.transform.localPosition = Vector3.zero;
                vfx.OnStarted(new VFX.Args()
                {
                    Delay = 0
                });
            };

            Action<Courier> onended = (courier) =>
            {
                vfx.transform.SetParent(null);
                vfx.OnEnded();
                onEnded?.Invoke();
            };

            var courierArgs = new Courier.Args
            {
                StartPosition = m_StartVFXPos.position,
                EndPosition = endPosition,
                Parameters = m_CourierParams,
                OnStarted = onstarted,
                OnEnded = onended,
                Camera = m_Camera
            };

            courierArgs.Move();
        }


        public async void z_GetFromPoolAndStartVFX()
        {
            var vfx = PoolVFX.Get(id: m_ID, prefab: m_VFX);
            vfx.transform.localPosition = transform.position;
            vfx.OnStarted(new VFX.Args());
            if (m_TimeToDespawn > 0)
                await UniTask.WaitForSeconds(m_TimeToDespawn);
            vfx.OnEnded();
        }
       
        public  void z_StartVFX(Color? color)
        {
            if (m_VFX == null)
                return;

            m_VFX.OnStarted(new VFX.Args() { Color = color, NotFadeColor2 = m_Args.NotFadeColor2 });       
        }
    }

}
