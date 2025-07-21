using System;
using UnityEngine;

namespace MadApper.Input
{
    public interface IDraggableInputSystem
    {
        public void ForceReleaseDraggable();
    }
    public interface IDraggable
    {      
        public void Pressed(Vector3 pos);
        public void Dragging(Vector3 pos);
        public void Released(Vector3 pos);
        public void ReleaseForced(Vector3 pos);
    }

    public struct IDraggableCallbacks<TDraggable> where TDraggable : class, IDraggable
    {
        public Action<TDraggable> OnDraggablePressed;
        public Action<TDraggable> OnDraggableReleasedValid;
        public Action<TDraggable> OnDraggableReleasedInvalid;
    }

    public class DraggableInputSystem<TDraggable> : InputObjectSystemBase<TDraggable>, IDraggableInputSystem where TDraggable : class, IDraggable
    {
       // protected TDraggable draggable;


        //private void Update()
        //{
        //    OnDragging();
        //}

        //protected override void OnPressed(TDraggable target)
        //{
        //    TrySetDraggable(target);
        //}

        //protected override void OnDragged(TDraggable target)
        //{
        //    TrySetDraggable(target);
        //}

        //protected override void OnReleased()
        //{
        //    TryReleaseDraggable();
        //}

        //void TrySetDraggable(TDraggable target)
        //{
        //    if (draggable != null) return;

        //    draggable = target;
        //    draggable.i_DraggableSystem = this;
        //    draggable.Pressed(GetProjectedWorldPosition());
        //}
        //void TryReleaseDraggable()
        //{
        //    if (draggable == null) return;

        //    draggable.Released(GetProjectedWorldPosition());
        //    draggable = null;
        //}


        //void OnDragging()
        //{
        //    if (isActive == false) return;
        //    if (draggable == null) return;

        //    draggable.Dragging(GetProjectedWorldPosition());
        //}

        ///// <summary>
        ///// e.g. when player is dragging a piece and loses/wins or other cancel scenarios
        ///// </summary>
        //public void ForceReleaseDraggable()
        //{
        //    if (draggable == null) return;

        //    draggable.ReleaseForced(GetProjectedWorldPosition());
        //    draggable = null;
        //}


        //public void z_ForceReleaseDraggable()
        //{
        //    ForceReleaseDraggable();
        //}


    }
}
