using System;
using System.Collections.Generic;
using System.Linq;
using Prefabulous.Universal.Common.Runtime;
using UnityEditor;
using UnityEngine;

namespace Prefabulous.Universal.Shared.Editor
{
    [CustomEditor(typeof(PrefabulousReplaceTextures))]
    public class PrefabulousReplaceTexturesEditor : UnityEditor.Editor
    {
        private Texture[] _foundTextures;
        private Texture2D _iconBackground;
        private Dictionary<Material, HashSet<Component>> _materialToComponent;
        private Dictionary<Texture, HashSet<Material>> _textureToMaterial;

        private void OnEnable()
        {
            var my = (PrefabulousReplaceTextures)target;
            
            var descriptor = PrefabulousUtil.GetAvatarRootOrNull(my.transform);
            if (descriptor == null) return;

            var skinnedMeshes = PrefabulousUtil.GetAllComponentsInChildrenExceptEditorOnly<SkinnedMeshRenderer>(descriptor);
            var meshes = PrefabulousUtil.GetAllComponentsInChildrenExceptEditorOnly<MeshRenderer>(descriptor);
            var trails = PrefabulousUtil.GetAllComponentsInChildrenExceptEditorOnly<TrailRenderer>(descriptor);
            var particleSystems = PrefabulousUtil.GetAllComponentsInChildrenExceptEditorOnly<ParticleSystemRenderer>(descriptor);

            var skinnedMeshesMaterials = skinnedMeshes.SelectMany(renderer => renderer.sharedMaterials).Where(material => material != null).ToArray();
            var meshesMaterials = meshes.SelectMany(renderer => renderer.sharedMaterials).Where(material => material != null).ToArray();
            var trailsMaterials = trails.SelectMany(renderer => renderer.sharedMaterials).Where(material => material != null).ToArray();
            var particleSystemMaterials = particleSystems
                .SelectMany(renderer => renderer.sharedMaterials)
                .Concat(particleSystems.Select(renderer => renderer.trailMaterial)).Where(material => material != null).ToArray();

            _materialToComponent = new Dictionary<Material, HashSet<Component>>();
            // TODO: How to turn this into a function?!
            foreach (var renderer in skinnedMeshes)
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    AddMTC(material, renderer);
                }
            }
            foreach (var renderer in meshes)
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    AddMTC(material, renderer);
                }
            }
            foreach (var renderer in trails)
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    AddMTC(material, renderer);
                }
            }
            foreach (var renderer in particleSystems)
            {
                var materials = renderer.sharedMaterials
                    .Concat(new []{ renderer.trailMaterial })
                    .Where(material => material != null)
                    .ToArray();
                foreach (var material in materials)
                {
                    AddMTC(material, renderer);
                }
            }

            var allDistinctMaterials = skinnedMeshesMaterials
                .Concat(meshesMaterials)
                .Concat(trailsMaterials)
                .Concat(particleSystemMaterials)
                .Distinct();

            _textureToMaterial = new Dictionary<Texture, HashSet<Material>>();
            foreach (var material in allDistinctMaterials)
            {
                var textures = material.GetTexturePropertyNameIDs()
                    .Select(material.GetTexture).Where(texture => texture != null)
                    .ToArray();
                foreach (var texture in textures)
                {
                    if (!_textureToMaterial.ContainsKey(texture))
                    {
                        _textureToMaterial[texture] = new HashSet<Material>();
                    }
                    _textureToMaterial[texture].Add(material);
                }
            }
            
            _foundTextures = _textureToMaterial.Keys.ToArray();
        }

        private void AddMTC(Material materialNullable, Component renderer)
        {
            if (materialNullable == null) return;
            
            if (!_materialToComponent.ContainsKey(materialNullable))
            {
                _materialToComponent[materialNullable] = new HashSet<Component>();
            }

            _materialToComponent[materialNullable].Add(renderer);
        }

        public override void OnInspectorGUI()
        {
            if (_iconBackground == null)
            {
                _iconBackground = new Texture2D(1, 1);
                _iconBackground.SetPixel(0, 0, new Color(0.01f, 0.2f, 0.2f));
                _iconBackground.Apply();
            }
            var my = (PrefabulousReplaceTextures)target;
            if (my.replacements == null) my.replacements = Array.Empty<PrefabulousTextureSubstitution>();
            
            var sources = new HashSet<Texture2D>(my.replacements
                .Select(substitution => substitution.source)
                .Where(texture2D => texture2D != null));

            var replacementsProperty = serializedObject.FindProperty(nameof(PrefabulousReplaceTextures.replacements));
            EditorGUILayout.PropertyField(replacementsProperty, new GUIContent("Replacements"));
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PrefabulousReplaceTextures.executeInPlayMode)), new GUIContent("Execute in Play Mode"));
            EditorGUILayout.HelpBox(@"If you choose to execute Replace Textures in Play Mode, it can be tremendously confusing for your workflow as you will no longer be able to edit the materials of your avatar while in Play Mode.

For this reason, it is NOT recommended to execute this component in Play Mode. Replace Textures will always be executed when building your avatar for upload, or when baking your avatar.", MessageType.Warning);

            
            foreach (var foundTexture in _foundTextures)
            {
                EditorGUILayout.BeginVertical("GroupBox");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Box(foundTexture, new GUIStyle("box")
                    {
                        normal = new GUIStyleState { background = _iconBackground }
                    },
                    GUILayout.Width(128), GUILayout.Height(128)
                );
                EditorGUILayout.BeginVertical();
                
                var hasTexture = sources.Contains(foundTexture);
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(foundTexture, typeof(Texture), false);
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.LabelField($"{foundTexture.width} \u00d7 {foundTexture.height}");
                
                EditorGUI.BeginDisabledGroup(hasTexture);
                if (GUILayout.Button("+ Add", GUILayout.Width(50)))
                {
                    replacementsProperty.arraySize += 1;
                    var that = replacementsProperty.GetArrayElementAtIndex(replacementsProperty.arraySize - 1);
                    that.FindPropertyRelative(nameof(PrefabulousTextureSubstitution.source)).objectReferenceValue = foundTexture;
                    that.FindPropertyRelative(nameof(PrefabulousTextureSubstitution.target)).objectReferenceValue = null;
                }
                EditorGUI.EndDisabledGroup();
                if (hasTexture)
                {
                    var index = SourceIndexOf(my.replacements, foundTexture);
                    if (replacementsProperty.arraySize <= index) continue; // Workaround
                    
                    var field = replacementsProperty
                        .GetArrayElementAtIndex(index)
                        .FindPropertyRelative(nameof(PrefabulousTextureSubstitution.target));
                    EditorGUILayout.LabelField("Replace with:");
                    EditorGUILayout.PropertyField(field, new GUIContent(""));
                    
                    var replaceWith = (Texture2D)field.objectReferenceValue;
                    if (replaceWith != null)
                    {
                        EditorGUILayout.LabelField($"{replaceWith.width} \u00d7 {replaceWith.height}");
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.LabelField("Used in:");
                EditorGUI.BeginDisabledGroup(true);
                foreach (var material in _textureToMaterial[foundTexture])
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(material, typeof(Material), false);
                    EditorGUILayout.BeginVertical();
                    foreach (var component in _materialToComponent[material])
                    {
                        EditorGUILayout.ObjectField(component, component.GetType(), false);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private int SourceIndexOf(PrefabulousTextureSubstitution[] myReplacements, Texture foundTexture)
        {
            // Bruh
            for (var index = 0; index < myReplacements.Length; index++)
            {
                var replacement = myReplacements[index];
                if (replacement.source == foundTexture)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}