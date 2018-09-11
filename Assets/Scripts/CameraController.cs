﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace PC3D
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController camController;


        // Camera movement variables
        public GameObject player;
        public float yLowClampAngle = -30.0f;
        public float yClampRange = 60.0f;
        public float mouseSensitivity = 50.0f;
        public float smoothing = 3.0f;

        private Vector2 _smoothMouse;
        private bool isCursorVisible = false;
        private Vector2 _mouseAbsolute;

        // Camera switcher variables
        public bool isFirstPerson = false;

        public Transform thirdPerson;

        private Camera thirdPersonCamera;
        private Camera firstPersonCamera;

        private Vector3 thirdCamOffset;

        [HideInInspector]
        public Camera cam;

        public float camTransitionSpeed = 5.0f;

        // First person variables

        [Space(10)]
        [SerializeField] private MouseLook mouseLook;
        [SerializeField] private MouseLook mouseLookThirdPerson;

        void Start()
        {
            if (camController == null)
                camController = this;
            else
                Destroy(this);

            GameObject camObj = GameObject.Find("Main Camera");

            if (camObj == null)
            {
                Debug.LogWarning("Who is the main camera?");
                return;
            }

            thirdPersonCamera = camObj.GetComponent<Camera>();

            if (player == null)
            {
                Debug.LogWarning("Who is the player?");
                return;
            }

            firstPersonCamera = player.GetComponentInChildren<Camera>();

            if (firstPersonCamera == null)
            {
                Debug.LogWarning("There's no camera on player object");
            }

            // Define our offsets to work on top of
            thirdCamOffset = thirdPerson.position - transform.position;

            mouseLook.Init(player.transform, firstPersonCamera.transform);
            mouseLookThirdPerson.Init(transform);

            SetCameraMode();
        }

        void Update()
        {
            // Follow the the player
            transform.position = player.transform.position;

            // See if our player wants first or third person camera
            if (Input.GetKeyDown(KeyCode.F))
            {
                isFirstPerson = !isFirstPerson;
                SetCameraMode();
            }

            if (isFirstPerson)
            {
                FirstPersonCameraHandling();
                return;
            }

            ThirdPersonCameraHandling();
        }

        private void SetCameraMode()
        {
            if (cam != null)
                cam.enabled = false;

            if (isFirstPerson)
            {
                cam = firstPersonCamera;
                mouseLook.Init(player.transform, cam.transform);
            }
            else
            {
                cam = thirdPersonCamera;
                mouseLookThirdPerson.Init(transform);

                // Att thirdPerson position
                thirdPerson.position = transform.position + thirdCamOffset;

                // Use this to lerp from first person camera position to third person one
                cam.transform.position = firstPersonCamera.transform.position;
            }

            cam.enabled = true;
        }

        private void FirstPersonCameraHandling()
        {
            mouseLook.LookRotation(player.transform, cam.transform);
        }

        private void ThirdPersonCameraHandling()
        {
            mouseLookThirdPerson.LookRotationThirPerson(transform);

            // Lerp camera position to desire position
            cam.transform.position = Vector3.Lerp(cam.transform.position, thirdPerson.position, camTransitionSpeed * Time.deltaTime);

            // After all movement, make camera look at the player
            cam.transform.LookAt(player.transform.position);
        }

        private void FixedUpdate()
        {
            // It's in order to see mouse cursor pressing ESC
            if (!isCursorVisible)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isCursorVisible = !isCursorVisible;
            }

            mouseLook.UpdateCursorLock();
            mouseLookThirdPerson.UpdateCursorLock();
        }
    }


    #region From standart assets
    [System.Serializable]
    public class MouseLook
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;
        public bool lockCursor = true;


        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = true;

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }

        public void Init(Transform camera)
        {
            m_CameraTargetRot = camera.localRotation;
        }

        public void LookRotation(Transform character, Transform camera)
        {
            float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
            float xRot = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (smooth)
            {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }

            UpdateCursorLock();
        }

        public void LookRotationThirPerson(Transform cameraTracker)
        {
            float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
            float xRot = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;

            m_CameraTargetRot *= Quaternion.Euler(-xRot, yRot, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (smooth)
            {
                cameraTracker.localRotation = Quaternion.Slerp(cameraTracker.localRotation, m_CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                cameraTracker.localRotation = m_CameraTargetRot;
            }

            UpdateCursorLock();
        }

        public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if (!lockCursor)
            {//we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void UpdateCursorLock()
        {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (lockCursor)
                InternalLockUpdate();
        }

        private void InternalLockUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
    #endregion
}