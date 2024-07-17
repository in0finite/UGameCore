using UnityEngine;
using System.Collections.Generic;
using UGameCore.Utilities;
using UnityEngine.Events;

namespace UGameCore
{
    public class Spectator : MonoBehaviour
	{
		public List<Component> SpectatableObjects = new();
        public Component CurrentlySpectatedObject { get; private set; }
        public Spectatable CurrentlySpectatedObjectAsSpectatable { get; private set; }

        public bool IsSpectating => CurrentlySpectatedObject != null;

        public int SpectateMode { get; private set; } = 0;

        public struct SpectatedObjectChangedEvent
		{
			public Component NewObject;
            public Component OldObject;
			public Context Context;
        }

		public UnityEvent<SpectatedObjectChangedEvent> OnSpectatedObjectChanged;
        public UnityEvent<SpectatedObjectChangedEvent> OnSpectatingModeChanged;

        public enum DirectionChange
		{
			Random = 0,
			Next = 1,
			Previous = 2,
		}

		public struct Context
		{
			public Spectator Spectator;
			public readonly int SpectateMode => this.Spectator.SpectateMode;

			public Context(Spectator spectator)
			{
				this.Spectator = spectator;
			}
        }



		public void FindObjectForSpectating(DirectionChange direction)
		{
			SpectatableObjects.RemoveDeadObjects();

			if (0 == SpectatableObjects.Count)
				return;

			// find index of currently spectated object
			int index = -1 ;
			if (CurrentlySpectatedObject != null)
			{
				index = SpectatableObjects.IndexOf(CurrentlySpectatedObject);
			}

			Component newObject = null;

			if (index != -1)
			{
				// object found in list

				if (DirectionChange.Random == direction)
				{
					FindRandomObjectForSpectating();
				}
				else if (DirectionChange.Previous == direction)
				{
					int newIndex = index - 1;
					if (newIndex < 0)
						newIndex = SpectatableObjects.Count - 1;
                    newObject = SpectatableObjects[newIndex];
				}
				else if (DirectionChange.Next == direction)
				{
					int newIndex = index + 1;
					if (newIndex >= SpectatableObjects.Count)
						newIndex = 0;
                    newObject = SpectatableObjects[newIndex];
				}
			}
			else
			{
                // object not found in list
                // find random object

                newObject = FindRandomObjectForSpectating();
			}

			SetSpectatedObject(newObject);
		}

		Component FindRandomObjectForSpectating()
		{
			if (0 == SpectatableObjects.Count)
				return null;

			foreach(int index in GetRandomIndicesInCollection(SpectatableObjects.Count))
			{
                Component obj = SpectatableObjects[index];
				if (obj != null)
					return obj;
			}

			return null;
		}

		IEnumerable<int> GetRandomIndicesInCollection(int collectionLength)
		{
			if (0 == collectionLength)
				yield break;

			int startIndex = UnityEngine.Random.Range(0, collectionLength);

			for (int i = startIndex, count = 0, adder = 0; count < collectionLength;
				count++, adder++, i += (count % 2 == 0 ? adder : -adder))
			{
				int index = i;
				if (index < 0)
					index = collectionLength - (-index);
				if (index >= collectionLength)
					index -= collectionLength;

				yield return index;
			}
		}

		public void SetSpectatedObject(Component obj)
		{
			if (CurrentlySpectatedObject == obj)
				return;

			Component oldObject = CurrentlySpectatedObject;
			Spectatable oldObjectAsSpectatable = CurrentlySpectatedObjectAsSpectatable;

            CurrentlySpectatedObject = obj;
			CurrentlySpectatedObjectAsSpectatable = obj != null ? obj.GetComponent<Spectatable>() : null;

			NotifySpectatedObjectChanged(oldObject, oldObjectAsSpectatable);
        }

        Context CreateContext()
		{
			return new Context { Spectator = this };
		}

        void NotifySpectatedObjectChanged(Component oldObject, Spectatable oldObjectAsSpectatable)
		{
			if (oldObject == CurrentlySpectatedObject)
				return;

			// note that any of below invoked events can change currently spectated object

			var ev = new SpectatedObjectChangedEvent
			{
				NewObject = CurrentlySpectatedObject,
				OldObject = oldObject,
				Context = CreateContext(),
			};

            OnSpectatedObjectChanged.Invoke(ev);

			if (oldObjectAsSpectatable != null
				&& oldObjectAsSpectatable != CurrentlySpectatedObjectAsSpectatable) // make sure it has not became active in the meantime
                oldObjectAsSpectatable.OnStoppedSpectating?.Invoke(ev);

            if (CurrentlySpectatedObjectAsSpectatable != null)
                CurrentlySpectatedObjectAsSpectatable.OnStartedSpectating?.Invoke(ev);
        }

        public void SetSpectateMode(int newSpectateMode)
		{
			if (newSpectateMode == this.SpectateMode)
				return;

			this.SpectateMode = newSpectateMode;

			this.OnSpectatingModeChanged.Invoke(new SpectatedObjectChangedEvent
			{
				Context = this.CreateContext(),
				OldObject = this.CurrentlySpectatedObject,
				NewObject = this.CurrentlySpectatedObject,
			});

            if (this.CurrentlySpectatedObjectAsSpectatable != null)
                this.CurrentlySpectatedObjectAsSpectatable.OnSpectatingModeChanged?.Invoke(this.CreateContext());
        }
    }
}
