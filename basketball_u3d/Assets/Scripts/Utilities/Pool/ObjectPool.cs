using System.Collections.Generic;
using UnityEngine;

namespace Basketball.Utilities.Pool
{
    public class ObjectPool<T> where T : Component
    {
        Transform _parent;
        T _sample;
        List<T> _poolElements = new List<T>();

        public ObjectPool(T element, Transform parent)
        {
            _sample = element;
            _sample.gameObject.SetActive(false);
            _parent = parent;
        }

        public T Get()
        {
            if (_poolElements.Count == 0)
            {
                var newElement = Object.Instantiate(_sample, _parent);
                newElement.gameObject.SetActive(true);
                if (newElement is ISpawn newSpawn)
                {
                    newSpawn.OnSpawn();
                }
                return newElement;
            }

            var element = _poolElements[0];
            element.gameObject.SetActive(true);
            _poolElements.RemoveAt(0);
            if (element is ISpawn spawn)
            {
                spawn.OnSpawn();
            }
            return element;
        }

        public void Store(T element)
        {
            element.transform.SetParent(_parent);
            element.gameObject.SetActive(false);
            _poolElements.Add(element);
            if (element is ISpawn spawn)
            {
                spawn.OnStore();
            }
        }
    }
}
