using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace BehaviourCollections
{
    public static class ComponentTupleExtensions
    {
        public static bool TryGetBehaviourTuple(this GameObject gameObject, string name)
        {
            if (!ComponentTuple.s_objectToTuple.TryGetValue(gameObject, out ComponentTuple tuple))
            {
                return false;
            }

            if (tuple.Name == name) { return tuple; }

            return false;
        }
    }

    public sealed class ComponentTuple : MonoBehaviour
    {
        [SerializeField]
        public string Name = "Default";

        [SerializeField]
        public bool DeleteManagedBehaviours = true;

        [SerializeField]
        public List<MonoBehaviour> Behaviours;

        [SerializeField, HideInInspector]
        private bool _usingTemplate = false;

        public static Dictionary<GameObject, ComponentTuple> s_objectToTuple = new();
        private static Dictionary<string, List<Type>> s_nameToType = new();

        private List<Component> _managedBehaviours = new();

        private void OnValidate()
        {
            if (s_nameToType.ContainsKey(Name))
            {
                _usingTemplate = true;
                DestroyManagedComponents();
                AddComponentsFromTemplate(Name);
                GetManagedComponents(Name);
            }
            else
            {
                _usingTemplate = false;
                RegisterTemplate(Name);
            }
        }

        private void RegisterTemplate(string name)
        {
            if (Behaviours.Count == 0)
            {
                Debug.LogWarning("Cannot register ComponentTuple with 0 components");
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogWarning("Cannot register ComponentTuple without a valid name");
                return;
            }

            List<Type> typeList = new();

            foreach (var behaviour in Behaviours)
            {
                typeList.Add(behaviour.GetType());
            }

            s_nameToType.Add(name, typeList);
        }

        private void AddComponentsFromTemplate(string templateName)
        {
            var template = s_nameToType[templateName];
            foreach (var type in template)
            {
                AddComponentIfMissing(type);
            }
        }

        private void AddComponentIfMissing(Type type)
        {
            if (!TryGetComponent(type, out _))
            {
                gameObject.AddComponent(type);
            }
        }

        private void DestroyManagedComponents()
        {
            foreach (var component in _managedBehaviours)
            {
                DestroyImmediate(component);
            }
        }

        private void GetManagedComponents(string templateName)
        {
            var template = s_nameToType[templateName];
            foreach (var type in template)
            {
                if (TryGetComponent(type, out Component component))
                {
                    _managedBehaviours.Add(component);
                }
            }
        }
    }
}