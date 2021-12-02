﻿/*  Created by: Team 2 (Ted, Matt, Bilal, Dylan, and Colbey)
 *  Project: Brick Breaker
 *  Date Started: 11/3/2021 - 11/17/2021
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
using System.Xml;
using System.IO;

namespace BrickBreaker
{
    public partial class GameScreen : UserControl
    {
        #region global values

        //player1 button control keys - DO NOT CHANGE
        Boolean leftArrowDown, rightArrowDown;

        // Game values
        int lives;
        int level;
        int instaCounter;
        int speedCounter;
        int paddleCounter;
        int gunCounter;
        bool instaBreak;
        bool speedIncrease;
        bool paddleSize;
        bool gun;
        
        float startDirection = 180;
        bool directionLeftKey = false;
        bool directionRightKey = false;

        // Paddle and Ball objects
        Paddle paddle;
        Ball ball;
        List<PowerUp> powerUps = new List<PowerUp>();

        // list of all blocks for current level
        List<Block> blocks = new List<Block>();

        // Brushes
        SolidBrush paddleBrush = new SolidBrush(Color.White);
        SolidBrush ballBrush = new SolidBrush(Color.White);
        SolidBrush blockBrush = new SolidBrush(Color.Red);
        SolidBrush powerUpBrush = new SolidBrush(Color.Green);

        //Tracks powerup

        // Random
        Random randGen = new Random();

        // Should ball move
        bool ballMoving = false;

        // Images
        Image brickImage = Properties.Resources.Brick;
        Image ballImage = Properties.Resources.BALL;
        Image paddleImage = Properties.Resources.DABABY_PADDLe;
        Image lifeImage = Properties.Resources.LIFE;


        Image powerup1 = Properties.Resources.breakpowerup;
        Image powerup2 = Properties.Resources.speedpowerup;
        Image powerup3 = Properties.Resources.increasesizepowerup;
        Image powerup4 = Properties.Resources.gunpowerup;
        Image powerup5 = Properties.Resources.dababylaunchpng;

        #endregion

        int playerScore;



        public GameScreen()
        {
            InitializeComponent();
            OnStart();
        }

        private void GameScreen_Load(object sender, EventArgs e)
        {
            //SoundPlayer daPlayer = new SoundPlayer(Properties.Resources.dababy2);
            //daPlayer.Play();
        }

        void SetupLevel(int _level)
        {
            blocks.Clear();

            XmlReader reader = XmlReader.Create("Resources/XML.xml");

            int thelevel = -1;
            while (reader.Read())
            {
                if(reader.NodeType == XmlNodeType.Element && reader.Name == "Level")
                {
                    thelevel++;
                }
                if (thelevel == _level)
                {
                    if (reader.NodeType == XmlNodeType.Text)
                    {
                        int __x = Convert.ToInt16(reader.ReadString());
                        reader.ReadToFollowing("y");
                        int __y = Convert.ToInt16(reader.ReadString());
                        reader.ReadToFollowing("type");
                        int __type = Convert.ToInt16(reader.ReadString());

                        blocks.Add(new Block(__x, __y, __type, Color.White));
                    }
                }
            }

            reader.Close();
        }

        void NewLevel()
        {
            level++;
            SetupLevel(level);
            if(blocks.Count == 0)
            {
                gameTimer.Enabled = false;
                OnEnd();
                return;
            }
            // Moves the ball back to origin
            ball.x = ((Convert.ToInt32(paddle.x) - (ball.size / 2)) + (Convert.ToInt32(paddle.width) / 2));
            ball.y = (this.Height - Convert.ToInt32(paddle.height)) - 80;
            float dir = Convert.ToSingle(randGen.Next(0, 360));
            ball.xSpeed = Convert.ToSingle(Math.Sin(dir / (180 / 3.14)) * 4); // 6
            ball.ySpeed = Convert.ToSingle(Math.Cos(dir / (180 / 3.14)) * 4); // 6
            ballMoving = false;
        }

        public void OnStart()
        {
            // set starting score to 0
            playerScore = 0;
            scoreLabel.Text = $"Your Score: {playerScore}";  

            
            //set life counter
            lives = 3;

            // MAKE SURE THE BALL FREEZES IN PLACE AND DIES
            ballMoving = false;

            //set all button presses to false.
            leftArrowDown = rightArrowDown = false;

            // setup starting paddle values and create paddle object
            int paddleWidth = 100;
            int paddleHeight = 20;
            int paddleX = ((this.Width / 2) - (paddleWidth / 2));
            int paddleY = (this.Height - paddleHeight) - 60;
            int paddleSpeed = 3; // 5
            paddle = new Paddle(paddleX, paddleY, paddleWidth, paddleHeight, paddleSpeed, Color.White);

            // setup starting ball values
            float ballX = this.Width / 2 - 10;
            float ballY = this.Height - Convert.ToInt32(paddle.height) - 80;

            // Creates a new ball
            startDirection = 0; 
            int ballSize = 20;
            ball = new Ball(ballX, ballY, 0, 0, ballSize, Properties.Resources.whiteBrick2);

            // Setup level
            level = 3;
            SetupLevel(level);

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
                    if (ballMoving == false)
                    {
                        ball.xSpeed = Convert.ToSingle(Math.Sin(startDirection / (180 / 3.14)) * 6);
                        ball.ySpeed = Convert.ToSingle(Math.Cos(startDirection / (180 / 3.14)) * 6);
                    }
                    ballMoving = true;
                    break;
                case Keys.A:
                    directionLeftKey = true;
                    break;
                case Keys.D:
                    directionRightKey = true;
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
                case Keys.A:
                    directionLeftKey = false;
                    break;
                case Keys.D:
                    directionRightKey = false;
                    break;
                default:
                    break;
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            instaCounter++;
            speedCounter++;
            paddleCounter++;
            gunCounter++;

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

            // Stop move ball
            if(!ballMoving)
            {
                ball.x = Convert.ToInt16(paddle.x + paddle.width / 2 - ball.size / 2);

                // Change the direction at start as necessary
                if (directionLeftKey)
                {
                    startDirection += 2;
                }
                if (directionRightKey)
                {
                    startDirection -= 2;
                }
                if(startDirection < 100)
                {
                    startDirection = 100;
                }
                else if(startDirection > 260)
                {
                    startDirection = 260;
                }
                ball.currentBlockCol = "none";
            }

            // Move ball
            for (float i = 0; i < 10; i++)
            {
                if (ballMoving)
                {
                    ball.Move(10);
                }

                // Check for collision with top and side walls
                ball.WallCollision(this);

                // Check for ball hitting bottom of screen
                if (ball.BottomCollision(this))
                {
                    lives--;
                    scoreLabel.Text = $"Your Score:{playerScore}";


                    // Moves the ball back to origin
                    ball.x = ((Convert.ToInt32(paddle.x) - (ball.size / 2)) + (Convert.ToInt32(paddle.width) / 2));
                    ball.y = (this.Height - Convert.ToInt32(paddle.height)) - 80;
                    ball.xSpeed = Convert.ToSingle(Math.Sin(startDirection / (180 / 3.14)) * 6);
                    ball.ySpeed = Convert.ToSingle(Math.Cos(startDirection / (180 / 3.14)) * 6);
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
                        playerScore++;
                        scoreLabel.Text = $"Your Score:{playerScore}";
                        b.hp--;
                        if (instaBreak)
                        {
                            b.hp = 0;
                        }
                        if (b.hp <= 0)
                        {
                            if (randGen.Next(0, 5) >= 0)
                            {
                                powerUps.Add(new PowerUp(b.x + b.width / 2 - 20, b.y + b.height / 2 - 20));
                            }

                            blocks.Remove(b);


                            if (blocks.Count == 0)
                            {
                                NewLevel();
                            }
                        }

                        break;
                    }
                }
            }
            
            // Powerups
            for(int i = 0;i < powerUps.Count;i ++){
                  powerUps[i].Drop();
                  if(powerUps[i].PaddleCollision(paddle)){
                        PowerUpMethod(powerUps[i].type);
                        powerUps.RemoveAt(i);
                        break;
                  }
                  if(powerUps[i].BottomCollision(this)){
                    powerUps.RemoveAt(i);
                  }
            }

            if(instaCounter == 500 && instaBreak)
            {
                instaBreak = false;
            }
            if (speedCounter == 240 && speedIncrease)
            {
                speedIncrease = false;
                ball.xSpeed /= 1.7f;
                ball.ySpeed /= 1.7f;
            }
            if (paddleCounter == 300 && paddleSize)
            {
                paddleSize = false;
                paddle.width -= 25;
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
            //e.Graphics.FillRectangle(paddleBrush, paddle.x, paddle.y, paddle.width, paddle.height);
            e.Graphics.DrawImage(paddleImage, paddle.x, paddle.y - 30, paddle.width, 50);
            //e.Graphics.DrawRectangle(new Pen(Color.Green, 2), paddle.x, paddle.y, paddle.width, paddle.height);

            // Draws blocks
            foreach (Block b in blocks)
            {
                e.Graphics.DrawImage(brickImage, b.x, b.y, b.width, b.height);
                //e.Graphics.DrawRectangle(new Pen(Color.Red, 2), b.x, b.y, b.width, b.height);
                //e.Graphics.DrawString(b.hp.ToString(), DefaultFont, new SolidBrush(Color.Black), b.x + b.width / 2, b.y + b.height / 2);
            }

            foreach (PowerUp pwrUp in powerUps) {
                //e.Graphics.FillRectangle(powerUpBrush, pwrUp.x, pwrUp.y, pwrUp.size, pwrUp.size);

                switch (pwrUp.type)
                {
                    case 1:
                        e.Graphics.DrawImage(powerup1, pwrUp.x, pwrUp.y, 40, 40);
                        break;
                    case 2:
                        e.Graphics.DrawImage(powerup2, pwrUp.x, pwrUp.y, 40, 40);
                        break;
                    case 3:
                        e.Graphics.DrawImage(powerup3, pwrUp.x, pwrUp.y, 40, 40);
                        break;
                    case 4:
                        e.Graphics.DrawImage(powerup5, pwrUp.x, pwrUp.y, 40, 40);
                        break;
                }
            }

            // Draws ball
            //e.Graphics.DrawRectangle(new Pen(Color.Yellow, 2), ball.x, ball.y, ball.size, ball.size);
            e.Graphics.DrawImage(ballImage, ball.x, ball.y);

            /*
            double dir = Math.Atan2(ball.ySpeed, ball.xSpeed);
            for (int i = 0; i < 4; i++)
            {
                PointF basePoint = new PointF(ball.x, ball.y);
                if (i == 1)
                {
                    basePoint = new PointF(ball.x + ball.size, ball.y);
                }
                if (i == 2)
                {
                    basePoint = new PointF(ball.x + ball.size, ball.y + ball.size);
                }
                if (i == 3)
                {
                    basePoint = new PointF(ball.x, ball.y + ball.size);
                }

                PointF currentPoint = new PointF(basePoint.X + Convert.ToSingle(Math.Cos(dir) * ball.size * 3 / 4), basePoint.Y + Convert.ToSingle(Math.Sin(dir) * ball.size * 3 / 4));
                PointF point5Ago = new PointF(basePoint.X - (ball.xSpeed * 5), basePoint.Y - (ball.ySpeed * 5));
                e.Graphics.DrawLine(new Pen(Color.Blue, 2), currentPoint, point5Ago);
            }*/
            //e.Graphics.DrawLine(new Pen(Color.Blue, 2), ball.x + ball.size / 2, ball.y + ball.size / 2, ball.x + ball.size / 2 + Convert.ToSingle(Math.Cos(dir) * ball.size * 3/4), ball.y + ball.size / 2 + Convert.ToSingle(Math.Sin(dir) * ball.size * 3/4));

            //e.Graphics.DrawString(ball.collisionTotals.ToString(), new Font(FontFamily.GenericSansSerif, 20, FontStyle.Regular), new SolidBrush(Color.White), ball.x, ball.y);

            if (!ballMoving)
            {
                for (int i = 0; i < 5; i++) {
                    float xS = Convert.ToSingle(Math.Sin(startDirection / (180 / 3.14)) * 4);
                    float yS = Convert.ToSingle(Math.Cos(startDirection / (180 / 3.14)) * 4);
                    e.Graphics.FillEllipse(new SolidBrush(Color.Gray), ball.x + ((10 - i * 2) / 2) + xS * ((i + 1) * 5), ball.y + ((10 - i * 2) / 2) + yS * ((i + 1) * 5), 10 - i * 2, 10 - i * 2);
                }
            }

            //e.Graphics.DrawImage(lives1Image, 0, 0);

            // Lives
            for(int i = 0;i < lives;i++)
            {
                e.Graphics.DrawImage(lifeImage, this.Width - 50 - i * 50, 451);

            }
        }

        public void PowerUpMethod(int _type)
        {
            switch(_type){
              case 1:
                    instaBreak = true;
                    InstaBreak();
                break;
                case 2:
                    speedIncrease = true;
                    SpeedIncrease();
                    break;
                case 3:
                    paddleSize = true;
                    IncreasePaddleSize();
                    break;
                case 4:
                    ExtraLife();
                    break;
            }
        }

        public void InstaBreak()
        {
            instaCounter = 0;
            instaBreak = true;
        }
        public void SpeedIncrease()
        {
            if (speedIncrease == true)
            {
                ball.xSpeed *= 1.7f;
                ball.ySpeed *= 1.7f;
            }
        }
        public void IncreasePaddleSize()
        {
            if (paddleSize == true)
            {
                paddle.width +=25;

            }
        }
        public void Gun()
        {
            if (gun == true)
            {

            }

        }
        public void ExtraLife()
        {
            lives++;
        }
    }
}

