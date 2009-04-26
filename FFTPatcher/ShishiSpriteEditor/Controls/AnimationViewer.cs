﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Bimixual.Animation;

namespace FFTPatcher.SpriteEditor
{
    public partial class AnimationViewer : UserControl
    {
        FpsTimer fpsTimer;
        DrawManager drawManager;
        SpriteManager spriteManager;
        public AnimationViewer()
        {
            InitializeComponent();
            playButton.Enabled = false;
            trackBar1.Enabled = false;

        }

        protected override void OnHandleDestroyed( EventArgs e )
        {
            base.OnHandleDestroyed( e );
        }

        private void Go()
        {
            if (drawManager != null)
            {
                drawManager.DrawGame();
            }
        }

        JustSitThereSprite sprite;
        FlipBook flipBook;
        public void ShowAnimation(IList<Bitmap> bitmaps, IList<double> delays)
        {
            fpsTimer = new FpsTimer(60);

            LoopControl.FpsTimer = fpsTimer;
            LoopControl.SetAndStartAction(control1, Go);
            drawManager = null;
            var myDrawManager = new DrawManager(control1, fpsTimer);

            if (bitmaps.Count != delays.Count)
                throw new ArgumentException("must have same number of bitmaps as delays");

            spriteManager = new SpriteManager(fpsTimer);
            sprite = new JustSitThereSprite(new Point(0, 0));

            if (flipBook != null)
            {
                flipBook.FrameChanged -= flipBook_FrameChanged;
            }

            flipBook = new FlipBook(bitmaps, delays);
            flipBook.Loop = true;
            flipBook.Paused = true;
            flipBook.FrameChanged += new EventHandler(flipBook_FrameChanged);
            sprite.AddAlteration(flipBook);
            spriteManager.AddObject(sprite);
            myDrawManager.AddDrawable(spriteManager);
            playButton.Enabled = true;

            drawManager = myDrawManager;

            trackBar1.Minimum = 0;
            trackBar1.Maximum = bitmaps.Count - 1;
            trackBar1.Value = 0;
        }

        void flipBook_FrameChanged(object sender, EventArgs e)
        {
            trackBar1.Value = flipBook.CurrentFrame;
        }

        public void ShowAnimation(IList<Bitmap> bitmaps, double delay)
        {
            double[] delays = new double[bitmaps.Count];
            for (int i = 0; i < bitmaps.Count; i++)
                delays[i] = delay;
            ShowAnimation(bitmaps, delays);
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            playButton.Enabled = false;
            pauseButton.Enabled = true;
            forwardButton.Enabled = false;
            backButton.Enabled = false;
            trackBar1.Enabled = false;
            flipBook.Unpause();
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            pauseButton.Enabled = false;
            playButton.Enabled = true;
            forwardButton.Enabled = true;
            backButton.Enabled = true;
            trackBar1.Enabled = true;
            flipBook.Pause();
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            flipBook.BackOneFrame();
        }

        private void forwardButton_Click(object sender, EventArgs e)
        {
            flipBook.ForwardOneFrame();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            flipBook.SetFrame(trackBar1.Value);
        }
    }
}