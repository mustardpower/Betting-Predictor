namespace BettingPredictorV3
{
    partial class DatabaseSettingsWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.bet365Checkbox = new System.Windows.Forms.CheckBox();
            this.interwettenCheckbox = new System.Windows.Forms.CheckBox();
            this.betWinCheckbox = new System.Windows.Forms.CheckBox();
            this.victorChandlerCheckbox = new System.Windows.Forms.CheckBox();
            this.williamHillCheckbox = new System.Windows.Forms.CheckBox();
            this.pinnacleSportCheckbox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ignoredPlayedFixturesCheckbox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(229, 284);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(310, 284);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.cancel);
            // 
            // bet365Checkbox
            // 
            this.bet365Checkbox.AutoSize = true;
            this.bet365Checkbox.Location = new System.Drawing.Point(15, 31);
            this.bet365Checkbox.Name = "bet365Checkbox";
            this.bet365Checkbox.Size = new System.Drawing.Size(63, 17);
            this.bet365Checkbox.TabIndex = 9;
            this.bet365Checkbox.Text = "Bet 365";
            this.bet365Checkbox.UseVisualStyleBackColor = true;
            // 
            // interwettenCheckbox
            // 
            this.interwettenCheckbox.AutoSize = true;
            this.interwettenCheckbox.Location = new System.Drawing.Point(15, 63);
            this.interwettenCheckbox.Name = "interwettenCheckbox";
            this.interwettenCheckbox.Size = new System.Drawing.Size(79, 17);
            this.interwettenCheckbox.TabIndex = 10;
            this.interwettenCheckbox.Text = "Interwetten";
            this.interwettenCheckbox.UseVisualStyleBackColor = true;
            // 
            // betWinCheckbox
            // 
            this.betWinCheckbox.AutoSize = true;
            this.betWinCheckbox.Location = new System.Drawing.Point(15, 103);
            this.betWinCheckbox.Name = "betWinCheckbox";
            this.betWinCheckbox.Size = new System.Drawing.Size(61, 17);
            this.betWinCheckbox.TabIndex = 11;
            this.betWinCheckbox.Text = "BetWin";
            this.betWinCheckbox.UseVisualStyleBackColor = true;
            // 
            // victorChandlerCheckbox
            // 
            this.victorChandlerCheckbox.AutoSize = true;
            this.victorChandlerCheckbox.Location = new System.Drawing.Point(15, 143);
            this.victorChandlerCheckbox.Name = "victorChandlerCheckbox";
            this.victorChandlerCheckbox.Size = new System.Drawing.Size(98, 17);
            this.victorChandlerCheckbox.TabIndex = 12;
            this.victorChandlerCheckbox.Text = "Victor Chandler";
            this.victorChandlerCheckbox.UseVisualStyleBackColor = true;
            // 
            // williamHillCheckbox
            // 
            this.williamHillCheckbox.AutoSize = true;
            this.williamHillCheckbox.Location = new System.Drawing.Point(183, 31);
            this.williamHillCheckbox.Name = "williamHillCheckbox";
            this.williamHillCheckbox.Size = new System.Drawing.Size(76, 17);
            this.williamHillCheckbox.TabIndex = 13;
            this.williamHillCheckbox.Text = "William Hill";
            this.williamHillCheckbox.UseVisualStyleBackColor = true;
            // 
            // pinnacleSportCheckbox
            // 
            this.pinnacleSportCheckbox.AutoSize = true;
            this.pinnacleSportCheckbox.Location = new System.Drawing.Point(183, 63);
            this.pinnacleSportCheckbox.Name = "pinnacleSportCheckbox";
            this.pinnacleSportCheckbox.Size = new System.Drawing.Size(95, 17);
            this.pinnacleSportCheckbox.TabIndex = 15;
            this.pinnacleSportCheckbox.Text = "Pinnacle Sport";
            this.pinnacleSportCheckbox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.pinnacleSportCheckbox);
            this.groupBox1.Controls.Add(this.williamHillCheckbox);
            this.groupBox1.Controls.Add(this.bet365Checkbox);
            this.groupBox1.Controls.Add(this.victorChandlerCheckbox);
            this.groupBox1.Controls.Add(this.interwettenCheckbox);
            this.groupBox1.Controls.Add(this.betWinCheckbox);
            this.groupBox1.Location = new System.Drawing.Point(36, 75);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(312, 189);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bookmakers used";
            // 
            // ignoredPlayedFixturesCheckbox
            // 
            this.ignoredPlayedFixturesCheckbox.AutoSize = true;
            this.ignoredPlayedFixturesCheckbox.Location = new System.Drawing.Point(36, 29);
            this.ignoredPlayedFixturesCheckbox.Name = "ignoredPlayedFixturesCheckbox";
            this.ignoredPlayedFixturesCheckbox.Size = new System.Drawing.Size(126, 17);
            this.ignoredPlayedFixturesCheckbox.TabIndex = 17;
            this.ignoredPlayedFixturesCheckbox.Text = "Ignore played fixtures";
            this.ignoredPlayedFixturesCheckbox.UseVisualStyleBackColor = true;
            // 
            // DatabaseSettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 319);
            this.Controls.Add(this.ignoredPlayedFixturesCheckbox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Name = "DatabaseSettingsWindow";
            this.Text = "Select Bookmakers";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox bet365Checkbox;
        private System.Windows.Forms.CheckBox interwettenCheckbox;
        private System.Windows.Forms.CheckBox betWinCheckbox;
        private System.Windows.Forms.CheckBox victorChandlerCheckbox;
        private System.Windows.Forms.CheckBox williamHillCheckbox;
        private System.Windows.Forms.CheckBox pinnacleSportCheckbox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox ignoredPlayedFixturesCheckbox;
    }
}