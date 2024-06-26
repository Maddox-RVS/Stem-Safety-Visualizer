﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace StemSolvers
{
    internal class Robot
    {
        private const float DEGREES_TO_RADIANS = (float) Math.PI / 180.0f;
        private const float RADIANS_TO_DEGREES = 180.0f / (float) Math.PI;

        private float pivotSpeedRadsPerSec, wristSpeedRadsPerSec, telescopePixelSpeed;
        private float currentPivotRads, currentWristRads, currentTelescopePixels;
        private float targetPivotRads, targetWristRads, targetTelescopePixels;
        private RenderTarget2D subsystemsRenderTarget, driveBaseRenderTarget;
        private bool hasCollidedWithDriveBase;
        private float wristLength;
        private float wristHeight;
        private float wristOffsetLength;
        private Texture2D texture;
        private Vector2 roboPos;
        private Rectangle allowedBounds; 
        private float backWallBoundX, frontWallBoundX, roofBoundY, floorBoundY;

        public Robot(Texture2D texture, Vector2 position, float pivotSpeedDegsPerSec, float wristSpeedDegsPerSec, float telescopePixelSpeed, GraphicsDevice graphicsDevice)
        {
            this.roboPos = position;
            this.texture = texture;

            wristLength = 100f;
            wristHeight = 50f;
            wristOffsetLength = 15f;

            backWallBoundX = 40.0f;
            frontWallBoundX = 650.0f;
            roofBoundY = 350.0f;
            floorBoundY = 0; 
            allowedBounds = new Rectangle((int) backWallBoundX, (int) floorBoundY, (int) Math.Abs(backWallBoundX - frontWallBoundX), (int) Math.Abs(roofBoundY - floorBoundY));

            targetPivotRads = 0.0f * DEGREES_TO_RADIANS;
            targetWristRads = 40.0f * DEGREES_TO_RADIANS;
            targetTelescopePixels = 380.0f;

            currentPivotRads = targetPivotRads;
            currentWristRads = targetWristRads;
            currentTelescopePixels = targetTelescopePixels;

            this.pivotSpeedRadsPerSec = pivotSpeedDegsPerSec * DEGREES_TO_RADIANS;
            this.wristSpeedRadsPerSec = wristSpeedDegsPerSec * DEGREES_TO_RADIANS;
            this.telescopePixelSpeed = telescopePixelSpeed;

            this.subsystemsRenderTarget = new RenderTarget2D(
                graphicsDevice,
                Game1.screenBounds.Width,
                Game1.screenBounds.Height,
                false,
                graphicsDevice.PresentationParameters.BackBufferFormat,
                graphicsDevice.PresentationParameters.DepthStencilFormat);

            this.driveBaseRenderTarget = new RenderTarget2D(
                graphicsDevice,
                Game1.screenBounds.Width,
                Game1.screenBounds.Height,
                false,
                graphicsDevice.PresentationParameters.BackBufferFormat,
                graphicsDevice.PresentationParameters.DepthStencilFormat);

            hasCollidedWithDriveBase = false;
        }

        public void moveToState(RoboState state)
        {
            setPivotDegrees(state.getPivotDegrees());
            setWristDegrees(state.getWristDegrees());
            setTelescopePixels(state.getTelescopePixels());
        }
        public void setPivotDegrees(float degrees)
        {
            targetPivotRads = degrees * DEGREES_TO_RADIANS;
        }
        public float getPivotDegrees()
        {
            return currentPivotRads * RADIANS_TO_DEGREES;
        }
        public void setWristDegrees(float degrees)
        {
            targetWristRads = degrees * DEGREES_TO_RADIANS;
        }
        public float getWristDegrees()
        {
            return (currentWristRads * RADIANS_TO_DEGREES);
        }
        public void setTelescopePixels(float pixels)
        {
            targetTelescopePixels = pixels;
        }
        public float getTelescopePixels()
        {
            return currentTelescopePixels;
        }
        public float getUmbrellaLength()
        {
            return wristLength;
        }
        public float getUmbrellaHeight()
        {
            return wristHeight;
        }
        public float getWristOffsetLength()
        {
            return wristOffsetLength;
        }

        public bool hasReachedTarget()
        {
            if (currentPivotRads == targetPivotRads && currentWristRads == targetWristRads && currentTelescopePixels == targetTelescopePixels) return true;
            return false;
        }

        public float badPID(float currentValue, float targetValue, float speed)
        {
            if (currentValue == targetValue || Math.Abs(currentValue - targetValue) <= speed) return targetValue;
            if (currentValue < targetValue) return currentValue += speed;
            else return currentValue -= speed;
        }

        public void update()
        {
            currentPivotRads = badPID(currentPivotRads, targetPivotRads, pivotSpeedRadsPerSec);
            currentWristRads = badPID(currentWristRads, targetWristRads, wristSpeedRadsPerSec);
            currentTelescopePixels = badPID(currentTelescopePixels, targetTelescopePixels, telescopePixelSpeed);
        }

        public Rectangle getDriveBaseRect()
        {
            return new Rectangle((int) roboPos.X, (int) roboPos.Y, 400, 55);
        }
        public Rectangle getTelescopeRect()
        {
            return new Rectangle((int) roboPos.X - 200 + 50, (int) roboPos.Y + 35, (int) currentTelescopePixels, 20);
        }

        public Rectangle getAllowedBounds()
        {
            return allowedBounds;
        }

        public static bool calculatePixelCollision(SpriteBatch spriteBatch, Texture2D texture1, Rectangle rectangle1, Texture2D texture2, Rectangle rectangle2)
        {
            Rectangle overlap = Rectangle.Intersect(rectangle1, rectangle2);
            Rectangle normalizedOverlap1 = new Rectangle(
                overlap.X - rectangle1.X, overlap.Y - rectangle1.Y, 
                overlap.Width, overlap.Height);
            Rectangle normalizedOverlap2 = new Rectangle(
                overlap.X - rectangle2.X, overlap.Y - rectangle2.Y,
                overlap.Width, overlap.Height);

            int pixelCount = overlap.Width * overlap.Height;

            Color[] colorData1 = new Color[pixelCount];
            Color[] colorData2 = new Color[pixelCount];

            texture1.GetData<Color>(0, normalizedOverlap1, colorData1, 0, colorData1.Length);
            texture2.GetData<Color>(0, normalizedOverlap2, colorData2, 0, colorData2.Length);

            for (int i = 0; i < pixelCount; i++)
            {
                if (colorData1[i].A != 0 && colorData2[i].A != 0) return true;
            }

            return false;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.GraphicsDevice.SetRenderTarget(subsystemsRenderTarget);
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();

            //Draws the telescope at the pivot degrees
            spriteBatch.Draw(
            texture,
            getTelescopeRect(),
            null,
            Color.CornflowerBlue,
            currentPivotRads,
            new Vector2(0.0f, 0.5f),
            SpriteEffects.None, 
            0);

            MechanismPoints mechPtsCurrent = new MechanismPoints(new RoboState(getPivotDegrees(), getWristDegrees(), getTelescopePixels()), this);

            //Draws the wrist at the wrist degrees
            spriteBatch.Draw(
            texture,
            new Rectangle((int) (mechPtsCurrent.umbrellaBottomLeftPoint.X), (int) (mechPtsCurrent.umbrellaBottomLeftPoint.Y), (int) wristHeight, (int) wristLength),
            null,
            Color.CornflowerBlue,
            (180 - (180 - (180 - (currentWristRads * RADIANS_TO_DEGREES)) - (currentPivotRads * RADIANS_TO_DEGREES)) - 90) * DEGREES_TO_RADIANS,
            new Vector2(0.0f, 1.0f),
            SpriteEffects.None, 
            0);

            spriteBatch.End();
            spriteBatch.GraphicsDevice.SetRenderTarget(driveBaseRenderTarget);
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();

            //Draws the robot's body
            spriteBatch.Draw(
            texture,
            new Rectangle((int) roboPos.X, (int) roboPos.Y, 400, 50),
            null,
            Color.White,
            0.0f * DEGREES_TO_RADIANS,
            new Vector2(0.5f, 0.5f),
            SpriteEffects.None, 
            0);

            //Draws the front wall
            spriteBatch.Draw(
            texture,
            new Rectangle((int)allowedBounds.Right, 0, 5, Game1.screenBounds.Height),
            null,
            Color.White,
            0.0f * DEGREES_TO_RADIANS,
            new Vector2(0.0f, 0.0f),
            SpriteEffects.None,
            0);

            //Draws the back wall
            spriteBatch.Draw(
            texture,
            new Rectangle((int)allowedBounds.Left - 5, 0, 5, Game1.screenBounds.Height),
            null,
            Color.White,
            0.0f * DEGREES_TO_RADIANS,
            new Vector2(0.0f, 0.0f),
            SpriteEffects.None,
            0);

            //Draws the roof
            spriteBatch.Draw(
            texture,
            new Rectangle((int)0, (int)allowedBounds.Bottom - 5, Game1.screenBounds.Width, 5),
            null,
            Color.White,
            0.0f * DEGREES_TO_RADIANS,
            new Vector2(0.0f, 0.0f),
            SpriteEffects.None,
            0);

            spriteBatch.End();
            spriteBatch.GraphicsDevice.SetRenderTarget(Game1.viewPort);
            spriteBatch.GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();

            MechanismPoints mechPtsTarget = new MechanismPoints(new RoboState(targetPivotRads * RADIANS_TO_DEGREES, targetWristRads * RADIANS_TO_DEGREES, targetTelescopePixels), this);
            
            //Draws the target telescope at the pivot degrees
            spriteBatch.Draw(
            texture,
            new Rectangle((int) roboPos.X - 200 + 50, (int) roboPos.Y + 35, (int) targetTelescopePixels, 20),
            null,
            Color.LightGray,
            targetPivotRads,
            new Vector2(0.0f, 0.5f),
            SpriteEffects.None, 
            0);

            //Draws the target wrist at the wrist degrees
            spriteBatch.Draw(
            texture,
            new Rectangle((int)(mechPtsTarget.umbrellaBottomLeftPoint.X), (int)(mechPtsTarget.umbrellaBottomLeftPoint.Y), (int)wristHeight, (int)wristLength),
            null,
            Color.LightGray,
            (180 - (180 - (180 - (targetWristRads * RADIANS_TO_DEGREES)) - (targetPivotRads * RADIANS_TO_DEGREES)) - 90) * DEGREES_TO_RADIANS,
            new Vector2(0.0f, 1.0f),
            SpriteEffects.None,
            0);

            hasCollidedWithDriveBase = calculatePixelCollision(
                spriteBatch,
                (Texture2D) subsystemsRenderTarget,
                Game1.screenBounds,
                (Texture2D) driveBaseRenderTarget,
                Game1.screenBounds);

            spriteBatch.Draw((Texture2D) subsystemsRenderTarget, Game1.screenBounds, Color.White);
            if (hasCollidedWithDriveBase)
            {
                spriteBatch.Draw((Texture2D) driveBaseRenderTarget, Game1.screenBounds, Color.Red);
            }
            else spriteBatch.Draw((Texture2D) driveBaseRenderTarget, Game1.screenBounds, Color.CornflowerBlue);

            //Draws the pivot axel trunion thingy
            spriteBatch.Draw(
            texture,
            new Rectangle(getTelescopeRect().X, getTelescopeRect().Y - 10, 40, 40),
            null,
            Color.CornflowerBlue,
            45.0f * DEGREES_TO_RADIANS,
            new Vector2(0.5f, 0.5f),
            SpriteEffects.None,
            0);
        }
    }
}
