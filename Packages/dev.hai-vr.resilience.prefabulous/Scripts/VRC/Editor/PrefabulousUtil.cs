using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using Prefabulous.VRC.Runtime;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Prefabulous.VRC.Editor
{
    public static class PrefabulousUtil
    {
        public static string[] GetAllBlendshapeNames(SkinnedMeshRenderer smr)
        {
            if (smr.sharedMesh == null) return Array.Empty<string>();
            
            var sharedMesh = smr.sharedMesh;

            return Enumerable.Range(0, sharedMesh.blendShapeCount)
                .Select(i => sharedMesh.GetBlendShapeName(i))
                .ToArray();
        }

        public static AnimationClip[] FindAllRelevantAnimationClips(VRCAvatarDescriptor descriptor)
        {
            var isFxBlank = descriptor.transform.GetComponentsInChildren<PrefabulousBlankFXAnimator>(true).Length > 0;
            var isGestureBlank = descriptor.transform.GetComponentsInChildren<PrefabulousBlankGestureAnimator>(true).Length > 0;
            var replaceActionNullable = descriptor.transform.GetComponentInChildren<PrefabulousReplaceActionAnimator>();
            var replaceLocomotionNullable = descriptor.transform.GetComponentInChildren<PrefabulousReplaceLocomotionAnimator>();

            var runtimeAnimatorControllers = descriptor.baseAnimationLayers
                .Where(layer => !layer.isDefault)
                .Where(layer =>
                {
                    if (layer.type == VRCAvatarDescriptor.AnimLayerType.FX && isFxBlank) return false;
                    if (layer.type == VRCAvatarDescriptor.AnimLayerType.Gesture && isGestureBlank) return false;
                    if (layer.type == VRCAvatarDescriptor.AnimLayerType.Action && replaceActionNullable != null) return false;
                    if (layer.type == VRCAvatarDescriptor.AnimLayerType.Base && replaceLocomotionNullable != null) return false;

                    return true;
                })
                .Select(layer => layer.animatorController);

            var additionalControllers = new List<RuntimeAnimatorController>();
            if (replaceActionNullable != null) additionalControllers.Add(replaceActionNullable.controller);
            if (replaceLocomotionNullable != null) additionalControllers.Add(replaceLocomotionNullable.controller);
            
            return runtimeAnimatorControllers
                .Concat(descriptor.GetComponentsInChildren<ModularAvatarMergeAnimator>(true)
                    .Select(animator => animator.animator))
                .Concat(additionalControllers)
                .Where(controller => controller != null)
                .SelectMany(controller => controller.animationClips)
                .Distinct()
                .ToArray();
        }
    }
}