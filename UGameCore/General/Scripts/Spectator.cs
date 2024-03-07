using UnityEngine;
using System.Collections.Generic;
using UGameCore.Utilities;
using UnityEngine.Events;

namespace UGameCore
{
    public class Spectator : MonoBehaviour
	{
		public List<Component> SpectatableObjects = new();
        public Component CurrentlySpectatedObject { get; private set; } = null;

		public struct SpectatedObjectChangedEvent
		{
			public Component newObject;
		}

		public UnityEvent<SpectatedObjectChangedEvent> OnSpectatedObjectChanged;

		public enum DirectionChange
		{
			Random = 0,
			Next = 1,
			Previous = 2,
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

			var oldObject = CurrentlySpectatedObject;
			CurrentlySpectatedObject = null;

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
					CurrentlySpectatedObject = SpectatableObjects[newIndex];
				}
				else if (DirectionChange.Next == direction)
				{
					int newIndex = index + 1;
					if (newIndex >= SpectatableObjects.Count)
						newIndex = 0;
					CurrentlySpectatedObject = SpectatableObjects[newIndex];
				}
			}
			else
			{
				// object not found in list
				// find random object

				FindRandomObjectForSpectating();
			}

			// notify
			if (oldObject != CurrentlySpectatedObject)
				OnSpectatedObjectChanged.Invoke(new SpectatedObjectChangedEvent { newObject = CurrentlySpectatedObject });
		}

		void FindRandomObjectForSpectating()
		{
			if (0 == SpectatableObjects.Count)
				return;

			foreach(int index in GetRandomIndicesInCollection(SpectatableObjects.Count))
			{
                Component obj = SpectatableObjects[index];
				if (obj != null)
				{
					CurrentlySpectatedObject = obj;
					break;
				}
			}
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

			CurrentlySpectatedObject = obj;

            OnSpectatedObjectChanged.Invoke(new SpectatedObjectChangedEvent { newObject = CurrentlySpectatedObject });
        }

        public bool IsSpectating => CurrentlySpectatedObject != null;
    }
}
