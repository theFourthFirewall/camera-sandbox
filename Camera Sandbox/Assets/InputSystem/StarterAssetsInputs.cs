using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool rotateCameraRight;
		public bool rotateCameraLeft;
		public bool vcam1;
		public bool vcam2;
		public bool vcam3;
		public bool vcam4;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
		public void OnRotateCameraRight(InputValue value)
		{
			RotateCameraRightInput(value.isPressed);
		}
		public void OnRotateCameraLeft(InputValue value)
		{
			RotateCameraLeftInput(value.isPressed);
		}
		public void OnVcam1(InputValue value)
		{
			Vcam1Input(value.isPressed);
		}
		public void OnVcam2(InputValue value)
		{
			Vcam2Input(value.isPressed);
		}
		public void OnVcam3(InputValue value)
		{
			Vcam3Input(value.isPressed);
		}
		public void OnVcam4(InputValue value)
		{
			Vcam4Input(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		public void RotateCameraRightInput(bool newRotateCameraRightState)
		{
			rotateCameraRight = newRotateCameraRightState;
		}
		public void RotateCameraLeftInput(bool newRotateCameraLeftState)
		{
			rotateCameraLeft = newRotateCameraLeftState;
		}
		public void Vcam1Input(bool newVcam1State)
		{
			vcam1 = newVcam1State;
		}
		public void Vcam2Input(bool newVcam2State)
		{
			vcam2 = newVcam2State;
		}
		public void Vcam3Input(bool newVcam3State)
		{
			vcam3 = newVcam3State;
		}
		public void Vcam4Input(bool newVcam4State)
		{
			vcam4 = newVcam4State;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}

}
