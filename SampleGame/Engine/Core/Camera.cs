﻿using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using RenderEngine = SampleGame.Engine.Core.Engine;

namespace SampleGame.Engine.Core
{
    public class Camera
    {
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;
        private float _pitch;
        private float _yaw = -MathHelper.PiOver2;
        private float _fov = MathHelper.PiOver2;

        public Camera(Vector3 position)
        {
            Position = position;
            _firstMove = true;
        }

        public Vector3 Position { get; set; }
        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;

        private bool _firstMove;
        private Vector2 _lastPos;

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                var angle = MathHelper.Clamp(value, -89f, 89f); // Lock pitch angle so that no wierd camera movements occur
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }


        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 180f); // Make sure the fov is within a reasonable range
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, RenderEngine.WindowVariables.Aspect, 0.01f, 100f);
        }

        public Matrix4 GetProjectionMatrix(float fov)
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), RenderEngine.WindowVariables.Aspect, 0.01f, 100f);
        }

        public void HandleCamera(float sensitivity)
        {
            var mouse = RenderEngine.WindowVariables.Mouse;

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                Yaw += deltaX * sensitivity;
                Pitch -= deltaY * sensitivity;
            }
        }


        public void HandleMovement(FrameEventArgs args, float cameraSpeed)
        {
            var input = RenderEngine.WindowVariables.Keyboard;

            if (input.IsKeyDown(Keys.W))
            {
                Position += Front * cameraSpeed * (float)args.Time;
            }

            if (input.IsKeyDown(Keys.S))
            {
                Position -= Front * cameraSpeed * (float)args.Time;
            }
            if (input.IsKeyDown(Keys.A))
            {
                Position -= Right * cameraSpeed * (float)args.Time;
            }
            if (input.IsKeyDown(Keys.D))
            {
                Position += Right * cameraSpeed * (float)args.Time;
            }
            if (input.IsKeyDown(Keys.Space))
            {
                Position += Up * cameraSpeed * (float)args.Time;
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                Position -= Up * cameraSpeed * (float)args.Time;
            }
        }

        private void UpdateVectors()
        {
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            // Make sure the vectors are normalized
            _front = Vector3.Normalize(_front);

            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}
