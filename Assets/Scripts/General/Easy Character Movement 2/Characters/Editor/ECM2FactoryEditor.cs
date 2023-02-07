using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace EasyCharacterMovement.Editor
{
    public static class ECM2FactoryEditor
    {
        private const string PATH = "GameObject/ECM2/";
        private const int PRIORITY = 1;

        private static void InitRigidbodyAndCollider(GameObject go)
        {
            Rigidbody rb = go.GetComponent<Rigidbody>();

            rb.drag = 0.0f;
            rb.angularDrag = 0.0f;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            CapsuleCollider capsuleCollider = go.GetComponent<CapsuleCollider>();

            capsuleCollider.center = new Vector3(0f, 1f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 2.0f;
        }

        [MenuItem(PATH + "Character", false, PRIORITY)]
        public static void CreateCharacter()
        {
            // Create an initialize a new Character GameObject

            GameObject go = new GameObject("ECM2_Character", typeof(Rigidbody), typeof(CapsuleCollider),
                typeof(CharacterMovement), typeof(Character));

            InitRigidbodyAndCollider(go);

            // Assign default input actions

            Character character = go.GetComponent<Character>();
            if (character)
                character.inputActions =
                    AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                        "Assets/ECM2/Input Actions/ECM2_Character_InputActions.inputactions");
            
            // Focus the newly created character

            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

            Selection.activeGameObject = go;
            SceneView.FrameLastActiveSceneView();
        }

        [MenuItem(PATH + "AgentCharacter", false, PRIORITY)]
        public static void CreateAgentCharacter()
        {
            // Create an initialize a new Character GameObject

            GameObject go = new GameObject("ECM2_AgentCharacter", typeof(NavMeshAgent), typeof(Rigidbody),
                typeof(CapsuleCollider), typeof(CharacterMovement), typeof(AgentCharacter));

            InitRigidbodyAndCollider(go);

            // Assign default input actions

            Character character = go.GetComponent<Character>();
            if (character)
                character.inputActions =
                    AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                        "Assets/ECM2/Input Actions/ECM2_Agent_InputActions.inputactions");

            // Focus the newly created character

            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

            Selection.activeGameObject = go;
            SceneView.FrameLastActiveSceneView();
        }

        [MenuItem(PATH + "FirstPersonCharacter", false, PRIORITY)]
        public static void CreateFirstPersonCharacter()
        {
            GameObject go = new GameObject("ECM2_FirstPersonCharacter", typeof(Rigidbody), typeof(CapsuleCollider),
                typeof(CharacterLook), typeof(CharacterMovement), typeof(FirstPersonCharacter));

            InitRigidbodyAndCollider(go);

            // Assign default input actions

            Character character = go.GetComponent<Character>();
            if (character)
                character.inputActions =
                    AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                        "Assets/ECM2/Input Actions/ECM2_FirstPerson_InputActions.inputactions");

            // Init first person RIG FirstPersonCharacter -> root -> eye -> camera

            GameObject rootPivot = new GameObject("Root");
            rootPivot.transform.SetParent(go.transform);

            GameObject eyePivot = new GameObject("Eye");
            eyePivot.transform.SetParent(rootPivot.transform);
            eyePivot.transform.localPosition = new Vector3(0.0f, 1.6f, 0.0f);

            GameObject cameraGameObject = new GameObject("Camera", typeof(Camera), typeof(AudioListener));
            cameraGameObject.transform.SetParent(eyePivot.transform, false);

            FirstPersonCharacter fpc = go.GetComponent<FirstPersonCharacter>();

            fpc.camera = cameraGameObject.GetComponent<Camera>();
            fpc.rootPivot = rootPivot.transform;
            fpc.eyePivot = eyePivot.transform;

            // Focus the newly created character

            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

            Selection.activeGameObject = go;
            SceneView.FrameLastActiveSceneView();
        }
    }
}
