using UnityEngine;

namespace EasyCharacterMovement.Templates.SideScrollerTemplate
{
    public class MyCharacter : Character
    {
        // TODO Add your game custom code here...

        protected override void HandleInput()
        {
            // Should handle input ?

            if (inputActions == null)
                return;

            // Add horizontal input movement (in world space)

            float movementDirection1D = 0.0f;

            Vector2 movementInput = GetMovementInput();
            if (movementInput.x > 0.0f)
                movementDirection1D = 1.0f;
            else if (movementInput.x < 0.0f)
                movementDirection1D = -1.0f;

            SetMovementDirection(Vector3.right * movementDirection1D);

            // Snap side to side rotation

            if (movementDirection1D != 0.0f)
                SetYaw(movementDirection1D * 90.0f);
        }

        protected override void OnOnEnable()
        {
            // Call base method implementation

            base.OnOnEnable();

            // Disable character rotation

            SetRotationMode(RotationMode.None);
        }
    }
}
