using System;
using System.Linq;
using System.Reflection;
using HG.GeneralSerializer;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace EnemiesPlus
{
    public static class Utils
    {
        public static void ReorderSkillDrivers(this GameObject master, int targetIdx)
        {
            var c = master.GetComponents<AISkillDriver>();
            master.ReorderSkillDrivers(c, c.Length - 1, targetIdx);
        }
        public static void ReorderSkillDrivers(this GameObject master, AISkillDriver targetSkill, int targetIdx)
        {
            var c = master.GetComponents<AISkillDriver>();
            master.ReorderSkillDrivers(c, Array.IndexOf(c, targetSkill), targetIdx);
        }
        public static void ReorderSkillDrivers(this GameObject master, AISkillDriver[] skills, int currentIdx, int targetIdx)
        {
            if (currentIdx < 0 || currentIdx >= skills.Length)
            {
                Log.Error($"{currentIdx} index not found or out of range. Must be less than {skills.Length}");
                return;
            }
            var targetName = skills[currentIdx].customName;

            if (targetIdx < 0 || targetIdx >= skills.Length)
            {
                Log.Error($"Unable to reorder skilldriver {targetName} into position {targetIdx}. target must be less than {skills.Length}");
                return;
            }

            if (targetIdx == currentIdx)
            {
                Log.Warning($"Skilldriver {targetName} already has the target index of {targetIdx}");
                return;
            }

            // reference to original might get nulled so they need to be re-added later
            var overrides = skills.Where(s => s.nextHighPriorityOverride != null)
                .ToDictionary(
                s => s.customName,
                s => s.nextHighPriorityOverride.customName);

            // move down. this modifies the order.
            if (targetIdx > currentIdx)
            {
                master.AddComponentCopy(skills[currentIdx]);
                Component.DestroyImmediate(skills[currentIdx]);
            }

            // anything before the target idx can be ignored.
            // move all elements after the target target skilldriver without modifying order
            for (var i = targetIdx; i < skills.Length; i++)
            {
                if (i != currentIdx)
                {
                    // start with skill that currently occupies target idx
                    master.AddComponentCopy(skills[i]);
                    Component.DestroyImmediate(skills[i]);
                }
            }

            // sanity check
            skills = master.GetComponents<AISkillDriver>();
            var newTarget = skills.FirstOrDefault(s => s.customName == targetName);
            if (newTarget != null && Array.IndexOf(skills, newTarget) == targetIdx)
                Log.Debug($"Successfully set {targetName} to {targetIdx}");
            else
                Log.Error($"Done fucked it up on {targetName} with {targetIdx}");

            // restore overrides
            if (overrides.Any())
            {
                for (var i = 0; i < skills.Length; i++)
                {
                    var skill = skills[i];
                    if (skill && overrides.TryGetValue(skill.customName, out var target))
                    {
                        var skillComponent = skills.FirstOrDefault(s => s.customName == target);
                        if (skillComponent == null)
                        {
                            Log.Error($"Unable to reset skill override for {skill.customName} targeting {target}");
                        }
                        else
                        {
                            skill.nextHighPriorityOverride = skillComponent;
                            Log.Debug($"successfully reset override for {skill.customName} targeting {target}");
                        }
                    }
                }
            }
        }

        public static void RemoveComponent<T>(this GameObject go) where T : Component
        {
            if (go.TryGetComponent<T>(out var component))
            {
                Component.Destroy(component);
            }
        }

        public static void RemoveComponents<T>(this GameObject go) where T : Component
        {
            var coms = go.GetComponents<T>();
            for (var i = 0; i < coms.Length; i++)
            {
                Component.Destroy(coms[i]);
            }
        }

        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            var type = comp.GetType();
            if (type != other.GetType())
                return null; // type mis-match

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            var pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }

            var finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }

            return comp as T;
        }

        public static T GetCopyOf<T>(this ScriptableObject comp, T other) where T : ScriptableObject
        {
            var type = comp.GetType();
            if (type != other.GetType())
                return null; // type mis-match

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            var pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }

            var finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }

            return comp as T;
        }

        public static T AddComponentCopy<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd);
        }

        public static bool TryModifyFieldValue<T>(this EntityStateConfiguration entityStateConfiguration, string fieldName, T value)
        {
            ref var serializedField = ref entityStateConfiguration.serializedFieldsCollection.GetOrCreateField(fieldName);
            var type = typeof(T);
            if (serializedField.fieldValue.objectValue && typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                serializedField.fieldValue.objectValue = value as UnityEngine.Object;
                return true;
            }
            else if (serializedField.fieldValue.stringValue != null && StringSerializer.CanSerializeType(type))
            {
                serializedField.fieldValue.stringValue = StringSerializer.Serialize(type, value);
                return true;
            }
            return false;
        }
    }
}
