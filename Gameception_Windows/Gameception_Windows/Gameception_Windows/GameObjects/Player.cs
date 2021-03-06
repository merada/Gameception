﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Gameception
{
    class Player : GameObject
    {
        #region Attributes

        // Used to determine which player this object represents
        private PlayerIndex playerIndex;

        // Handles the sound for this player
        SoundManager soundManager;

        // player control keys
        public Keys Up, Right, Down, Left, Fire;

        // The direction the player is facing, used for firing weapons
        Vector3 playerFacing;

        // The rotation of the model
        float rotationAngle;

        // The weapon used by this player
        Weapon playerWeapon;

        // Object held, only used by player 2
        bool objectHeld;

        // Indicates whether the players are allowed to move, this is based on the distance between them
        private bool canMove;

        // The ammount of ammunition the player has
        private int ammo;

        // The score of this player
        private int score;

        #endregion

        #region Properties

        public bool ObjectHeld
        {
            get { return objectHeld; }
            set { objectHeld = value; }
        }

        public bool CanMove
        {
            get { return canMove; }
            set { canMove = value; }
        }

        public Vector3 PlayerFacing
        {
            get { return playerFacing; }
            set { playerFacing = value; }
        }

        public Weapon PlayerWeapon
        {
            get { return playerWeapon; }
            set { playerWeapon = value; }
        }

        public int Ammo
        {
            get { return ammo; }
            set { ammo = value; }
        }

        public int Score
        {
            get { return score; }
            set { score = value; }
        }
        #endregion

        #region Initialization

        public Player(Model model, float moveSpeed, int initialHealth, Vector3 startPosition, float scale, Camera camera, PlayerIndex player)
            : base(model, moveSpeed, initialHealth, startPosition, scale, camera)
        {
            playerIndex = player;
            ammo = 10;
        }

        /// <summary>
        /// Set the player's keyboard control keys
        /// </summary>
        /// <param name="u"></param>Up
        /// <param name="r"></param>Right
        /// <param name="d"></param>Down
        /// <param name="l"></param>Left
        /// <param name="f"></param>Fire
        /// <param name="i"></param>Player Index
        public void setKeys(Keys u, Keys r, Keys d, Keys l, Keys f, PlayerIndex i)
        {
            Up = u;
            Right = r;
            Down = d;
            Left = l;
            Fire = f;
            playerIndex = i;

            rotationAngle = 0f;

            PlayerWeapon = null;
            CanMove = true;
            ObjectHeld = false;
        }

        #endregion

        #region Update

        public override void Update()
        {
            if (PlayerWeapon != null)
            {
                PlayerWeapon.Update();
            }

            base.Update();
        }

        public void setSoundManager(SoundManager s)
        {
            this.soundManager = s;
        }

        public void HandleInput()
        {
            KeyboardState keyboard = Keyboard.GetState();
            GamePadState gamepad = GamePad.GetState(playerIndex, GamePadDeadZone.Circular);

            if (CanMove == true)
            {
                if (Position != PreviousPosition)
                {
                    PlayerFacing = Position - PreviousPosition;
                }

                PreviousPosition = Position;

                float gamePadX = gamepad.ThumbSticks.Left.X;
                float gamePadY = gamepad.ThumbSticks.Left.Y;

                if ((gamePadX != 0) || (gamePadY != 0))
                {
                    rotationAngle = (float)(Math.Atan2(-gamePadX, gamePadY));
                }

                /*if (keyboard.IsKeyDown(Up) || (gamepad.ThumbSticks.Left.Y > 0))
                {
                    //Position = Position + Vector3.UnitZ * MovementSpeed;
                    //rotationAngle = 0f;

                    Matrix moveForward = Matrix.CreateRotationY(MathHelper.ToRadians(rotationAngle));
                    Vector3 velocityVector = new Vector3(0, 0, MovementSpeed);
                    velocityVector = Vector3.Transform(velocityVector, moveForward);
                    Vector3 tempPosition = new Vector3(Position.X + velocityVector.X, Position.Y, Position.Z + velocityVector.Z);
                    Position = tempPosition;
                }
                /*else if (keyboard.IsKeyDown(Down) || (gamepad.ThumbSticks.Left.Y < 0))
                {
                    Position = Position + Vector3.UnitZ * (-MovementSpeed);
                    rotationAngle = 180f;
                }//

                if (keyboard.IsKeyDown(Right) || (gamepad.ThumbSticks.Left.X > 0))
                {
                    //Position = Position + Vector3.UnitX * (-MovementSpeed);
                    //rotationAngle = 270f;
                    rotationAngle -= 2f;
                }
                else if (keyboard.IsKeyDown(Left) || (gamepad.ThumbSticks.Left.X < 0))
                {
                    //Position = Position + Vector3.UnitX * MovementSpeed;
                    //rotationAngle = 90f;
                    rotationAngle += 2f;
                }*/

                Position = new Vector3(Position.X - (gamepad.ThumbSticks.Left.X * MovementSpeed), Position.Y, Position.Z + (gamepad.ThumbSticks.Left.Y * MovementSpeed));
            }

            // This needs to be outside the if so that the release of the button can be detected
            if (keyboard.IsKeyDown(Fire) || (gamepad.Triggers.Right > 0))
            {
                if (playerIndex == PlayerIndex.One)
                {
                    if (ammo > 0)
                    {
                        Matrix forward = Matrix.CreateRotationY(rotationAngle);
                        Vector3 shootingDirection = new Vector3(0, 0, MovementSpeed);
                        shootingDirection = Vector3.Transform(shootingDirection, forward);

                        PlayerWeapon.fire(GameCamera, Position, shootingDirection);
                    }
                    else
                    {
                       // Play sound here that indicates ammo is finished
                    }
                }
                else if (playerIndex == PlayerIndex.Two && ObjectHeld == false) // Player 2 can't move while pulling an object
                {
                    Matrix forward = Matrix.CreateRotationY(rotationAngle);
                    Vector3 shootingDirection = new Vector3(0, 0, MovementSpeed);
                    shootingDirection = Vector3.Transform(shootingDirection, forward);

                    PlayerWeapon.fire(GameCamera, Position, shootingDirection);
                    CanMove = false;
                }
            }
            else
            {
                ObjectHeld = false;
                CanMove = true;
            }
        }

        #endregion

        #region Draw

        public override void Draw()
        {
            if (PlayerWeapon != null)
            {
                PlayerWeapon.Draw();
            }

            Matrix[] transforms = new Matrix[ObjectModel.Bones.Count];
            ObjectModel.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix rotation = Matrix.CreateRotationY(rotationAngle);

            // Only draw a gameObject if it's active
            if (Active)
            {
                if (InFrustrum)
                {
                    foreach (ModelMesh mesh in ObjectModel.Meshes)
                    {
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            effect.EnableDefaultLighting();

                            effect.View = GameCamera.View;
                            effect.Projection = GameCamera.Projection;
                            effect.World = rotation * transforms[mesh.ParentBone.Index] * Matrix.CreateScale(ScaleFactor) * Matrix.CreateTranslation(Position);
                        }

                        mesh.Draw();
                    }
                }
            }

            //base.Draw();
        }

        #endregion
    }
}
