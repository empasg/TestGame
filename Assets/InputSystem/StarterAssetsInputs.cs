using UnityEngine;
using Photon.Pun;
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
		public float scroll;
		public bool jump;
		public bool sprint;
		public bool rightClick;
		public bool leftClick;
		public bool e;
		public bool r;
		public bool y;
		public bool u;
		public bool i;
		public bool q;
		public bool f;

		public bool _1;
		public bool _2;
		public bool _3;
		public bool _4;
		public bool _5;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		private PhotonView _photonView;

		private void Start()
		{
			_photonView = GetComponent<PhotonView>();
		}

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		public void OnMove(InputValue value)
		{
			if (!_photonView.IsMine) return;
			MoveInput(value.Get<Vector2>());
		}
		public void OnScroll(InputValue value)
		{
			if (!_photonView.IsMine) return;
			float val = value.Get<float>();

			float _multiplier = scroll != 0 ? 0 : 1;

			if (val > 0 && scroll <= 0)
				ScrollInput(1 * _multiplier);
			else if (val < 0 && scroll >= 0)
				ScrollInput(-1 * _multiplier);
		}
		public void OnLook(InputValue value)
		{
			if (!_photonView.IsMine) return;
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			if (!_photonView.IsMine) return;
			JumpInput(value.isPressed);
		}

		public void OnE(InputValue value)
		{
			if (!_photonView.IsMine) return;
			EInput(value.isPressed);
		}

		public void OnR(InputValue value)
		{
			if (!_photonView.IsMine) return;
			RInput(value.isPressed);
		}

		public void OnY(InputValue value)
		{
			if (!_photonView.IsMine) return;
			YInput(value.isPressed);
		}
		public void OnU(InputValue value)
		{
			if (!_photonView.IsMine) return;
			UInput(value.isPressed);
		}
		public void OnI(InputValue value)
		{
			if (!_photonView.IsMine) return;
			IInput(value.isPressed);
		}
		public void OnQ(InputValue value)
		{
			if (!_photonView.IsMine) return;
			QInput(value.isPressed);
		}
		public void OnF(InputValue value)
		{
			if (!_photonView.IsMine) return;
			FInput(true);
		}	
		public void On_1(InputValue value)
		{
			if (!_photonView.IsMine) return;
			_1Input(value.isPressed);
		}		
		public void On_2(InputValue value)
		{
			if (!_photonView.IsMine) return;
			_2Input(value.isPressed);
		}	
		public void On_3(InputValue value)
		{
			if (!_photonView.IsMine) return;
			_3Input(value.isPressed);
		}	
		public void On_4(InputValue value)
		{
			if (!_photonView.IsMine) return;
			_4Input(value.isPressed);
		}	
		public void On_5(InputValue value)
		{
			if (!_photonView.IsMine) return;
			_5Input(value.isPressed);
		}										
		public void OnSprint(InputValue value)
		{
			if (!_photonView.IsMine) return;
			SprintInput(value.isPressed);
		}
		public void OnRightClick(InputValue value)
		{
			if (!_photonView.IsMine) return;
			RightClickInput(value.isPressed);
		}
		public void OnLeftClick(InputValue value)
		{
			if (!_photonView.IsMine) return;
			LeftClickInput(value.isPressed);
		}				
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void ScrollInput(float newScrollDirection)
		{
			scroll = newScrollDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void EInput(bool newEState)
		{
			e = newEState;
		}
		public void RInput(bool newRState)
		{
			r = newRState;
		}
		public void YInput(bool newYState)
		{
			y = newYState;
		}
		public void UInput(bool newUState)
		{
			u = newUState;
		}
		public void IInput(bool newIState)
		{
			i = newIState;
		}	
		public void QInput(bool newQState)
		{
			q = newQState;
		}
		public void FInput(bool newFState)
		{
			f = newFState;
		}		
		public void _1Input(bool new1State)
		{
			_1 = new1State;
		}										
		public void _2Input(bool new2State)
		{
			_2 = new2State;
		}										
		public void _3Input(bool new3State)
		{
			_3 = new3State;
		}										
		public void _4Input(bool new4State)
		{
			_4 = new4State;
		}										
		public void _5Input(bool new5State)
		{
			_5 = new5State;
		}										
		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void RightClickInput(bool newRightClickState)
		{
			rightClick = newRightClickState;
		}
		public void LeftClickInput(bool newLeftClickState)
		{
			leftClick = newLeftClickState;
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