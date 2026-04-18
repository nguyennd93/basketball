using Basketball.Interface;
using UnityEngine;

namespace Basketball.Controller
{
    public class InputController : MonoBehaviour
    {
        private IInput _iInput;

        public void Initialize(IInput input)
        {
            _iInput = input;
        }

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        _iInput.OnTouchBegin(touch.position);
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        _iInput.OnTouchMove(touch.position);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        _iInput.OnTouchEnd(touch.position);
                        break;
                }

                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                _iInput.OnTouchBegin(Input.mousePosition);
            }

            if (Input.GetMouseButton(0))
            {
                _iInput.OnTouchMove(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                _iInput.OnTouchEnd(Input.mousePosition);
            }
        }
    }
}
