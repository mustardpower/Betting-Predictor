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
            this.bet365Checkbox.Location = new System.Drawing.Point(52, 75);
            this.bet365Checkbox.Name = "bet365Checkbox";
            this.bet365Checkbox.Size = new System.Drawing.Size(63, 17);
            this.bet365Checkbox.TabIndex = 9;
            this.bet365Checkbox.Text = "Bet 365";
            this.bet365Checkbox.UseVisualStyleBackColor = true;
            // 
            // interwettenCheckbox
            // 
            this.interwettenCheckbox.AutoSize = true;
            this.interwettenCheckbox.Location = new System.Drawing.Point(52, 107);
            this.interwettenCheckbox.Name = "interwettenCheckbox";
            this.interwettenCheckbox.Size = new System.Drawing.Size(79, 17);
            this.interwettenCheckbox.TabIndex = 10;
            this.interwettenCheckbox.Text = "Interwetten";
            this.interwettenCheckbox.UseVisualStyleBackColor = true;
            // 
            // betWinCheckbox
            // 
            this.betWinCheckbox.AutoSize = true;
            this.betWinCheckbox.Location = new System.Drawing.Point(52, 147);
            this.betWinCheckbox.Name = "betWinCheckbox";
            this.betWinCheckbox.Size = new System.Drawing.Size(61, 17);
            this.betWinCheckbox.TabIndex = 11;
            this.betWinCheckbox.Text = "BetWin";
            this.betWinCheckbox.UseVisualStyleBackColor = true;
            // 
            // victorChandlerCheckbox
            // 
            this.victorChandlerCheckbox.AutoSize = true;
            this.victorChandlerCheckbox.Location = new System.Drawing.Point(52, 187);
            this.victorChandlerCheckbox.Name = "victorChandlerCheckbox";
            this.victorChandlerCheckbox.Size = new System.Drawing.Size(98, 17);
            this.victorChandlerCheckbox.TabIndex = 12;
            this.victorChandlerCheckbox.Text = "Victor Chandler";
            this.victorChandlerCheckbox.UseVisualStyleBackColor = true;
            // 
            // williamHillCheckbox
            // 
            this.williamHillCheckbox.AutoSize = true;
            this.williamHillCheckbox.Location = new System.Drawing.Point(220, 75);
            this.williamHillCheckbox.Name = "williamHillCheckbox";
            this.williamHillCheckbox.Size = new System.Drawing.Size(76, 17);
            this.williamHillCheckbox.TabIndex = 13;
            this.williamHillCheckbox.Text = "William Hill";
            this.williamHillCheckbox.UseVisualStyleBackColor = true;
            // 
            // pinnacleSportCheckbox
            // 
            this.pinnacleSportCheckbox.AutoSize = true;
            this.pinnacleSportCheckbox.Location = new System.Drawing.Point(220, 107);
            this.pinnacleSportCheckbox.Name = "pinnacleSportCheckbox";
            this.pinnacleSportCheckbox.Size = new System.Drawing.Size(95, 17);
            this.pinnacleSportCheckbox.TabIndex = 15;
            this.pinnacleSportCheckbox.Text = "Pinnacle Sport";
            this.pinnacleSportCheckbox.UseVisualStyleBackColor = true;
            // 
            // DatabaseSettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 319);
            this.Controls.Add(this.pinnacleSportCheckbox);
            this.Controls.Add(this.williamHillCheckbox);
            this.Controls.Add(this.victorChandlerCheckbox);
            this.Controls.Add(this.betWinCheckbox);
            this.Controls.Add(this.interwettenCheckbox);
            this.Controls.Add(this.bet365Checkbox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "DatabaseSettingsWindow";
            this.Text = "Select Bookmakers";
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
    }
}