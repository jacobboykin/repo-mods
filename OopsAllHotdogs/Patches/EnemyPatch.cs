using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OopsAllHotdogs.Patches
{
    [HarmonyPatch(typeof(Enemy))]
    internal class EnemyAwakeUniversalHotdogPatch
    {
        private const string HotdogInstanceName = "UniversalHotdogReplacement_HotdogInstance";

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void ReplaceVisualsWithHotdog(Enemy __instance)
        {
            var logger = OopsAllHotdogs.Instance.ModLogger;

            if (__instance == null || __instance.gameObject == null)
            {
                logger.LogError("Enemy instance or its gameObject is null. Cannot replace model.");
                return;
            }

            var controllerTransform = __instance.transform;
            var enableTransform = __instance.transform.parent;

            var hotdogPrefab = OopsAllHotdogs.HotdogPrefab;
            if (hotdogPrefab == null)
            {
                logger.LogError("HotdogPrefab is null!");
                return;
            }

            if (enableTransform == null)
            {
                logger.LogError($"Parent of '{controllerTransform.name}' (expected 'Enable') is null. " +
                                $"Cannot safely disable original visuals. Aborting for this instance.");
                return;
            }
            
            // Instantiate the new hotdog model
            GameObject hotdogInstance = Object.Instantiate(hotdogPrefab, controllerTransform);
            hotdogInstance.name = HotdogInstanceName;
            hotdogInstance.SetActive(true);

            var hotdogRenderers = hotdogInstance.GetComponentsInChildren<Renderer>(true);
            foreach (var r in hotdogRenderers) { r.enabled = true; }

            // Disable existing visual components of the original enemy
            Renderer[] existingRenderers = enableTransform.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in existingRenderers)
            {
                if (renderer.transform.IsChildOf(hotdogInstance.transform) || 
                    renderer.gameObject == hotdogInstance) continue;
                renderer.enabled = false;
            }
            
            SkinnedMeshRenderer[] existingSkinnedRenderers = enableTransform.
                GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer smr in existingSkinnedRenderers)
            {
                if (smr.transform.IsChildOf(hotdogInstance.transform) || smr.gameObject == hotdogInstance) continue;
                smr.enabled = false;
            }
            
            // Try to position the hotdog
            if (__instance.CenterTransform != null)
            {
                // Convert CenterTransform's world position to controllerTransform's local space
                hotdogInstance.transform.localPosition = controllerTransform.
                    InverseTransformPoint(__instance.CenterTransform.position);
                // Set rotation relative to controllerTransform, trying to match CenterTransform's world orientation
                hotdogInstance.transform.localRotation = Quaternion.
                    Inverse(controllerTransform.rotation) * __instance.CenterTransform.rotation;
            }
            else
            {
                hotdogInstance.transform.localPosition = Vector3.zero;
                hotdogInstance.transform.localRotation = Quaternion.identity;
            }

            // Rotate the hotdog to be standing up a bit
            float standUpAngle = 70f;
            Vector3 standUpAxis = Vector3.right;
            Quaternion additionalRotation = Quaternion.AngleAxis(standUpAngle, standUpAxis);
            hotdogInstance.transform.localRotation *= additionalRotation;

            // Tweak this if model size is off
            hotdogInstance.transform.localScale = Vector3.one * 85f;

            int defaultLayer = LayerMask.NameToLayer("Default");
            hotdogInstance.layer = defaultLayer;
            foreach (Transform childTransform in hotdogInstance.GetComponentsInChildren<Transform>(true))
            {
                childTransform.gameObject.layer = defaultLayer;
            }

            logger.LogInfo($"Replacement complete for script on '{controllerTransform.name}'. Hotdog parented to it. " +
                           $"Visuals under '{enableTransform.name}' (hopefully) disabled.");
        }
    }
}