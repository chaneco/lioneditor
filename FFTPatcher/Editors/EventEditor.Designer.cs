﻿namespace FFTPatcher.Editors
{
    partial class EventEditor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && (components != null) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.unitSelectorListBox = new System.Windows.Forms.ListBox();
            this.eventUnitEditor = new FFTPatcher.Editors.EventUnitEditor();
            this.SuspendLayout();
            // 
            // unitSelectorListBox
            // 
            this.unitSelectorListBox.DisplayMember = "Description";
            this.unitSelectorListBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.unitSelectorListBox.FormattingEnabled = true;
            this.unitSelectorListBox.Location = new System.Drawing.Point( 0, 0 );
            this.unitSelectorListBox.Name = "unitSelectorListBox";
            this.unitSelectorListBox.Size = new System.Drawing.Size( 539, 212 );
            this.unitSelectorListBox.TabIndex = 1;
            // 
            // eventUnitEditor
            // 
            this.eventUnitEditor.AutoSize = true;
            this.eventUnitEditor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.eventUnitEditor.EventUnit = null;
            this.eventUnitEditor.Location = new System.Drawing.Point( 0, 218 );
            this.eventUnitEditor.Name = "eventUnitEditor";
            this.eventUnitEditor.Size = new System.Drawing.Size( 536, 388 );
            this.eventUnitEditor.TabIndex = 0;
            // 
            // EventEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add( this.unitSelectorListBox );
            this.Controls.Add( this.eventUnitEditor );
            this.Name = "EventEditor";
            this.Size = new System.Drawing.Size( 539, 609 );
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private EventUnitEditor eventUnitEditor;
        private System.Windows.Forms.ListBox unitSelectorListBox;
    }
}