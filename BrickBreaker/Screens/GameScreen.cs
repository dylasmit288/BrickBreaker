﻿/*  Created by: Team 2 (Ted, Matt, Bilal, Dylan, and Colbey)
 *  Project: Brick Breaker
 *  Date Started: 11/3/2021 - __/__/2021
 */ 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;

namespace BrickBreaker
{
    public partial class GameScreen : UserControl
    {
        #region global values

        //player1 button control keys - DO NOT CHANGE
        Boolean leftArrowDown, rightArrowDown;

        // Game values
        int lives;
        int powerupCounter;
        public static int powerChoice;

        // Paddle and Ball objects
        Paddle paddle;
        Ball ball;
        PowerUp powerUp;

        // list of all blocks for current level
        List<Block> blocks = new List<Block>();

        // Brushes
        SolidBrush paddleBrush = new SolidBrush(Color.White);
        SolidBrush ballBrush = new SolidBrush(Color.White);
        SolidBrush blockBrush = new SolidBrush(Color.Red);
        SolidBrush powerUpBrush = new SolidBrush(Color.Green);

        //Tracks powerup
        bool powerUpSpawned = false;

        // Random
        Random randGen = new Random();

        // Should ball move
        bool ballMoving = false;

        Image brickImage = Properties.Resources.whiteBrick2;

        #endregion



        public GameScreen()
        {
            InitializeComponent();
            OnStart();
        }

        private void GameScreen_Load(object sender, EventArgs e)
        {
            SoundPlayer daPlayer = new SoundPlayer(Properties.Resources.dababy2);
            daPlayer.Play();
        }
        public void OnStart()
        {
            //set life counter
            lives = 3;
            powerupCounter = 0;

            // MAKE SURE THE BALL FREEZES IN PLACE AND DIES
            ballMoving = false;

            //set all button presses to false.
            leftArrowDown = rightArrowDown = false;

            // setup starting paddle values and create paddle object
            int paddleWidth = 80;
            int paddleHeight = 20;
            int paddleX = ((this.Width / 2) - (paddleWidth / 2));
            int paddleY = (this.Height - paddleHeight) - 60;
            int paddleSpeed = 3; // 5
            paddle = new Paddle(paddleX, paddleY, paddleWidth, paddleHeight, paddleSpeed, Color.White);

            // setup starting ball values
            float ballX = this.Width / 2 - 10;
            float ballY = this.Height - Convert.ToInt32(paddle.height) - 80;

            // set powerup values
            int powerUpX = this.Width / 2;
            int powerUpY = 40;
            int powerUpSpeed = 5;
            int powerUpSize = 20;
            powerUp = new PowerUp(powerUpX, powerUpY, powerUpSpeed, powerUpSize);

            // Creates a new ball
            float dir = Convert.ToSingle(randGen.Next(0, 360));
            float xSpeed = Convert.ToSingle(Math.Sin(dir / (180 / 3.14)) * 4); // 6
            float ySpeed = Convert.ToSingle(Math.Cos(dir / (180 / 3.14)) * 4); // 6
            int ballSize = 20;
            ball = new Ball(ballX, ballY, xSpeed, ySpeed, ballSize, Properties.Resources.whiteBrick2);

            #region Creates blocks for generic level. Need to replace with code that loads levels.
            
            //TODO - replace all the code in this region eventually with code that loads levels from xml files
            
            blocks.Clear();
            int x = 10;

            while (blocks.Count < 12)
            {
                x += 57;
                Block b1 = new Block(x, 10, 1, Color.White);
                blocks.Add(b1);
            }

            #endregion

            // start the game engine loop
            gameTimer.Enabled = true;
        }

        private void GameScreen_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //player 1 button presses
            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftArrowDown = true;
                    break;
                case Keys.Right:
                    rightArrowDown = true;
                    break;
                case Keys.Space:
                    ballMoving = true;
                    break;
                default:
                    break;
            }
        }

        private void GameScreen_KeyUp(object sender, KeyEventArgs e)
        {
            //player 1 button releases
            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftArrowDown = false;
                    break;
                case Keys.Right:
                    rightArrowDown = false;
                    break;
                default:
                    break;
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            // Move the paddle
            if (leftArrowDown && paddle.x > 0)
            {
                paddle.Move("left");
            }
            if (rightArrowDown && paddle.x < (this.Width - paddle.width))
            {
                paddle.Move("right");
            }

            paddle.updatePosition(this.Width);

            // Move ball
            if (ballMoving)
            {   
                ball.Move();
            }
            else
            {
                ball.x = Convert.ToInt16(paddle.x + paddle.width / 2 - ball.size / 2);
            }

            // Powerup Moving
            if(powerUpSpawned == true)
            {
                powerUp.Drop();
            }

            // Check for collision with top and side walls
            ball.WallCollision(this);

            // Check for ball hitting bottom of screen
            if (ball.BottomCollision(this))
            {
                lives--;

                // Moves the ball back to origin
                ball.x = ((Convert.ToInt32(paddle.x) - (ball.size / 2)) + (Convert.ToInt32(paddle.width) / 2));
                ball.y = (this.Height - Convert.ToInt32(paddle.height)) - 80;
                float dir = Convert.ToSingle(randGen.Next(0, 360));
                ball.xSpeed = Convert.ToSingle(Math.Sin(dir / (180 / 3.14)) * 4); // 6
                ball.ySpeed = Convert.ToSingle(Math.Cos(dir / (180 / 3.14)) * 4); // 6
                ballMoving = false;

                if (lives == 0)
                {
                    gameTimer.Enabled = false;
                    OnEnd();
                }
            }

            // Check for collision of ball with paddle, (incl. paddle movement)
            ball.PaddleCollision(paddle);

            // Check if ball has collided with any blocks
            foreach (Block b in blocks)
            {
                if (ball.BlockCollision(b))
                {
                    blocks.Remove(b);

                    PowerUpMethod();

                    if (blocks.Count == 0)
                    {
                        gameTimer.Enabled = false;
                        OnEnd();
                    }

                    break;
                }
            }
            //redraw the screen
            Refresh();
        }

        public void OnEnd()
        {
            // Goes to the game over screen
            Form form = this.FindForm();
            MenuScreen ps = new MenuScreen();
            
            ps.Location = new Point((form.Width - ps.Width) / 2, (form.Height - ps.Height) / 2);

            form.Controls.Add(ps);
            form.Controls.Remove(this);
        }

        public void GameScreen_Paint(object sender, PaintEventArgs e)
        {
            // Draws paddle
            paddleBrush.Color = paddle.colour;
            e.Graphics.FillRectangle(paddleBrush, paddle.x, paddle.y, paddle.width, paddle.height);

            // Draws blocks
            foreach (Block b in blocks)
            {
                e.Graphics.DrawImage(brickImage, b.x, b.y, b.width, b.height);
            }

            // Draws powerup
            if (powerupCounter >= 5)
            {
                powerUpSpawned = true;
                if (powerUpSpawned == true)
                {
                    e.Graphics.FillRectangle(powerUpBrush, powerUp.x, powerUp.y, powerUp.size, powerUp.size);
                }
               
            }

            // Draws ball
            e.Graphics.FillRectangle(new SolidBrush(Color.White), ball.x, ball.y, ball.size, ball.size);
        }

        public void PowerUpMethod()
        {
            powerupCounter++;

            if (powerUpSpawned == true)
            {
                Random rand = new Random();
                powerChoice = rand.Next(1, 6);

                if(powerUp.BottomCollision(this))
                {
                    powerUpSpawned = false;
                    powerupCounter = 0;
                }

                if (powerUp.PaddleCollision(paddle) && powerChoice == 1 )
                {
                    InstaBreak();
                    powerUpSpawned = false;
                    powerupCounter = 0;
                }
                if (powerUp.PaddleCollision(paddle) && powerChoice == 2)
                {
                    SpeedIncrease();
                    powerUpSpawned = false;
                    powerupCounter = 0;
                }
                if (powerUp.PaddleCollision(paddle) && powerChoice == 3)
                {
                    IncreasePaddleSize();
                    powerUpSpawned = false;
                    powerupCounter = 0;
                }
                if (powerUp.PaddleCollision(paddle) && powerChoice == 4)
                {
                    Gun();
                    powerUpSpawned = false;
                    powerupCounter = 0;
                }
                else if (powerUp.PaddleCollision(paddle) && powerChoice == 5)
                {
                    DaBabyLaunch();
                    powerUpSpawned = false;
                    powerupCounter = 0;
                }
            }
        }

        public void InstaBreak()
        {

        }
        public void SpeedIncrease()
        {

        }
        public void IncreasePaddleSize()
        {

        }
        public void Gun()
        {

        }
        public void DaBabyLaunch()
        {

        }
    }
}

