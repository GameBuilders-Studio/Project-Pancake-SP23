using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace EasyCharacterMovement
{
    /// <summary>
    /// ThirdPersonCharacter.
    ///
    /// This extends the Character class to add controls for a typical third person movement.
    /// </summary>

    public class ThirdPersonCharacter : Character
    {
        #region FIELDS

        private ThirdPersonCameraController _cameraController;

        #endregion

        #region PROPERTIES


        /// <summary>
        /// Cached camera controller.
        /// </summary>
        
        protected ThirdPersonCameraController cameraController
        {
            get
            {
                if (_cameraController == null)
                    _cameraController = camera.GetComponent<ThirdPersonCameraController>();

                return _cameraController;
            }
        }

        #endregion

        #region INPUT ACTIONS

        protected InputAction mouseLookInputAction { get; set; }

        protected InputAction mouseScrollInputAction { get; set; }

        protected InputAction controllerLookInputAction { get; set; }

        protected InputAction cursorLockInputAction { get; set; }

        protected InputAction cursorUnlockInputAction { get; set; }

        #endregion

        #region INPUT ACTION HANDLERS

        /// <summary>
        /// Gets the mouse look value.
        /// Return its current value or zero if no valid InputAction found.
        /// </summary>
        
        protected virtual Vector2 GetMouseLookInput()
        {
            if (mouseLookInputAction != null)
                return mouseLookInputAction.ReadValue<Vector2>();

            return Vector2.zero;
        }


        /// <summary>
        /// Gets the mouse scroll input value.
        /// Return its current value or zero if no valid InputAction found.
        /// </summary>
        
        protected virtual Vector2 GetMouseScrollInput()
        {
            if (mouseScrollInputAction != null)
                return mouseScrollInputAction.ReadValue<Vector2>();

            return Vector2.zero;
        }

        /// <summary>
        /// Gets the controller look input value.
        /// Return its current value or zero if no valid InputAction found.
        /// </summary>
        
        protected virtual Vector2 GetControllerLookInput()
        {
            if (controllerLookInputAction != null)
                return controllerLookInputAction.ReadValue<Vector2>();

            return Vector2.zero;
        }

        /// <summary>
        /// Handle cursor lock InputAction.
        /// </summary>
        
        protected virtual void OnCursorLock(InputAction.CallbackContext context)
        {
            // Do not allow to lock cursor if using UI

            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
                return;

            if (context.started)
                cameraController.LockCursor();
        }

        /// <summary>
        /// Handle cursor unlock InputAction.
        /// </summary>

        protected virtual void OnCursorUnlock(InputAction.CallbackContext context)
        {
            if (context.started)
                cameraController.UnlockCursor();
        }

        #endregion

        #region METHODS        

        /// <summary>
        /// Perform camera related input actions, eg: Look Up / Down, Turn, etc.
        /// </summary>

        protected virtual void HandleCameraInput()
        {
            if (!cameraController.IsCursorLocked())
                return;
            
            Vector2 mouseLookInput = GetMouseLookInput();
            if (mouseLookInput.sqrMagnitude > 0)
            {
                // Mouse look input

                if (mouseLookInput.x != 0.0f)
                    cameraController.Turn(mouseLookInput.x);

                if (mouseLookInput.y != 0.0f)
                    cameraController.LookUp(mouseLookInput.y);

            }
            else
            {
                // Controller look input

                Vector2 controllerLookInput = GetControllerLookInput();
                
                if (controllerLookInput.x != 0.0f)
                    cameraController.TurnAtRate(controllerLookInput.x);

                if (controllerLookInput.y != 0.0f)
                    cameraController.LookUpAtRate(controllerLookInput.y);
            }

            // Mouse scroll input

            Vector2 mouseScrollInput = GetMouseScrollInput();

            if (mouseScrollInput.y != 0.0f)
                cameraController.ZoomAtRate(mouseScrollInput.y);
        }

        /// <summary>
        /// Extends the HandleInput method to add Camera related inputs.
        /// </summary>

        protected override void HandleInput()
        {
            base.HandleInput();

            HandleCameraInput();
        }

        /// <summary>
        /// Initialize player InputActions (if any).
        /// E.g. Subscribe to input action events and enable input actions here.
        /// </summary>

        protected override void InitPlayerInput()
        {
            // Call base method implementation

            base.InitPlayerInput();
            
            // Attempts to cache and init this InputActions (if any)

            if (inputActions == null)
                return;

            mouseLookInputAction = inputActions.FindAction("Mouse Look");
            mouseLookInputAction?.Enable();

            mouseScrollInputAction = inputActions.FindAction("Mouse Scroll");
            mouseScrollInputAction?.Enable();

            controllerLookInputAction = inputActions.FindAction("Controller Look");
            controllerLookInputAction?.Enable();
            
            cursorLockInputAction = inputActions.FindAction("Cursor Lock");
            if (cursorLockInputAction != null)
            {
                cursorLockInputAction.started += OnCursorLock;
                cursorLockInputAction.Enable();
            }
            
            cursorUnlockInputAction = inputActions.FindAction("Cursor Unlock");
            if (cursorUnlockInputAction != null)
            {
                cursorUnlockInputAction.started += OnCursorUnlock;
                cursorUnlockInputAction.Enable();
            }
        }

        /// <summary>
        /// Unsubscribe from input action events and disable input actions.
        /// </summary>

        protected override void DeinitPlayerInput()
        {
            // Call base method implementation

            base.DeinitPlayerInput();
            
            if (mouseLookInputAction != null)
            {
                mouseLookInputAction.Disable();
                mouseLookInputAction = null;
            }

            if (mouseScrollInputAction != null)
            {
                mouseScrollInputAction.Disable();
                mouseScrollInputAction = null;
            }

            if (controllerLookInputAction != null)
            {
                controllerLookInputAction.Disable();
                controllerLookInputAction = null;
            }
            
            if (cursorLockInputAction != null)
            {
                cursorLockInputAction.started -= OnCursorLock;

                cursorLockInputAction.Disable();
                cursorLockInputAction = null;
            }
            
            if (cursorUnlockInputAction != null)
            {
                cursorUnlockInputAction.started -= OnCursorUnlock;

                cursorUnlockInputAction.Disable();
                cursorUnlockInputAction = null;
            }
        }

        #endregion
    }
}