namespace Proyecto_Arqui
{
    partial class Procesador
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
            this.P1 = new System.Windows.Forms.GroupBox();
            this.p1HililloTxt = new System.Windows.Forms.TextBox();
            this.CD1 = new System.Windows.Forms.ListBox();
            this.ID1 = new System.Windows.Forms.Label();
            this.p1RelojTxt = new System.Windows.Forms.TextBox();
            this.R1 = new System.Windows.Forms.Label();
            this.P2 = new System.Windows.Forms.GroupBox();
            this.p2HililloTxt = new System.Windows.Forms.TextBox();
            this.CD2 = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.P3 = new System.Windows.Forms.GroupBox();
            this.p3HililloTxt = new System.Windows.Forms.TextBox();
            this.CD3 = new System.Windows.Forms.ListBox();
            this.ID3 = new System.Windows.Forms.Label();
            this.siguienteBtn = new System.Windows.Forms.Button();
            this.P1.SuspendLayout();
            this.P2.SuspendLayout();
            this.P3.SuspendLayout();
            this.SuspendLayout();
            // 
            // P1
            // 
            this.P1.Controls.Add(this.p1HililloTxt);
            this.P1.Controls.Add(this.CD1);
            this.P1.Controls.Add(this.ID1);
            this.P1.Location = new System.Drawing.Point(8, 44);
            this.P1.Name = "P1";
            this.P1.Size = new System.Drawing.Size(225, 506);
            this.P1.TabIndex = 0;
            this.P1.TabStop = false;
            this.P1.Text = "Procesador 1";
            this.P1.Enter += new System.EventHandler(this.P1_Enter);
            // 
            // p1HililloTxt
            // 
            this.p1HililloTxt.Location = new System.Drawing.Point(67, 31);
            this.p1HililloTxt.Name = "p1HililloTxt";
            this.p1HililloTxt.Size = new System.Drawing.Size(100, 20);
            this.p1HililloTxt.TabIndex = 3;
            // 
            // CD1
            // 
            this.CD1.FormattingEnabled = true;
            this.CD1.Location = new System.Drawing.Point(10, 65);
            this.CD1.Name = "CD1";
            this.CD1.Size = new System.Drawing.Size(199, 355);
            this.CD1.TabIndex = 2;
            // 
            // ID1
            // 
            this.ID1.AutoSize = true;
            this.ID1.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ID1.Location = new System.Drawing.Point(6, 33);
            this.ID1.Name = "ID1";
            this.ID1.Size = new System.Drawing.Size(54, 19);
            this.ID1.TabIndex = 0;
            this.ID1.Text = "Hilillo:";
            // 
            // p1RelojTxt
            // 
            this.p1RelojTxt.Location = new System.Drawing.Point(75, 11);
            this.p1RelojTxt.Name = "p1RelojTxt";
            this.p1RelojTxt.Size = new System.Drawing.Size(100, 20);
            this.p1RelojTxt.TabIndex = 6;
            // 
            // R1
            // 
            this.R1.AutoSize = true;
            this.R1.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.R1.Location = new System.Drawing.Point(12, 12);
            this.R1.Name = "R1";
            this.R1.Size = new System.Drawing.Size(50, 19);
            this.R1.TabIndex = 1;
            this.R1.Text = "Reloj:";
            // 
            // P2
            // 
            this.P2.Controls.Add(this.p2HililloTxt);
            this.P2.Controls.Add(this.CD2);
            this.P2.Controls.Add(this.label2);
            this.P2.Location = new System.Drawing.Point(239, 44);
            this.P2.Name = "P2";
            this.P2.Size = new System.Drawing.Size(225, 506);
            this.P2.TabIndex = 1;
            this.P2.TabStop = false;
            this.P2.Text = "Procesador 2";
            // 
            // p2HililloTxt
            // 
            this.p2HililloTxt.Location = new System.Drawing.Point(83, 31);
            this.p2HililloTxt.Name = "p2HililloTxt";
            this.p2HililloTxt.Size = new System.Drawing.Size(100, 20);
            this.p2HililloTxt.TabIndex = 4;
            // 
            // CD2
            // 
            this.CD2.FormattingEnabled = true;
            this.CD2.Location = new System.Drawing.Point(10, 65);
            this.CD2.Name = "CD2";
            this.CD2.Size = new System.Drawing.Size(199, 355);
            this.CD2.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 19);
            this.label2.TabIndex = 1;
            this.label2.Text = "Hilillo:";
            // 
            // P3
            // 
            this.P3.Controls.Add(this.p3HililloTxt);
            this.P3.Controls.Add(this.CD3);
            this.P3.Controls.Add(this.ID3);
            this.P3.Location = new System.Drawing.Point(470, 44);
            this.P3.Name = "P3";
            this.P3.Size = new System.Drawing.Size(225, 450);
            this.P3.TabIndex = 1;
            this.P3.TabStop = false;
            this.P3.Text = "Procesador 3";
            // 
            // p3HililloTxt
            // 
            this.p3HililloTxt.Location = new System.Drawing.Point(89, 31);
            this.p3HililloTxt.Name = "p3HililloTxt";
            this.p3HililloTxt.Size = new System.Drawing.Size(100, 20);
            this.p3HililloTxt.TabIndex = 5;
            // 
            // CD3
            // 
            this.CD3.FormattingEnabled = true;
            this.CD3.Location = new System.Drawing.Point(10, 65);
            this.CD3.Name = "CD3";
            this.CD3.Size = new System.Drawing.Size(199, 355);
            this.CD3.TabIndex = 4;
            // 
            // ID3
            // 
            this.ID3.AutoSize = true;
            this.ID3.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ID3.Location = new System.Drawing.Point(6, 33);
            this.ID3.Name = "ID3";
            this.ID3.Size = new System.Drawing.Size(54, 19);
            this.ID3.TabIndex = 2;
            this.ID3.Text = "Hilillo:";
            // 
            // siguienteBtn
            // 
            this.siguienteBtn.Location = new System.Drawing.Point(707, 12);
            this.siguienteBtn.Name = "siguienteBtn";
            this.siguienteBtn.Size = new System.Drawing.Size(75, 52);
            this.siguienteBtn.TabIndex = 2;
            this.siguienteBtn.Text = "Siguiente";
            this.siguienteBtn.UseVisualStyleBackColor = true;
            this.siguienteBtn.Click += new System.EventHandler(this.siguienteBtn_Click);
            // 
            // Procesador
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 476);
            this.Controls.Add(this.p1RelojTxt);
            this.Controls.Add(this.siguienteBtn);
            this.Controls.Add(this.P3);
            this.Controls.Add(this.R1);
            this.Controls.Add(this.P2);
            this.Controls.Add(this.P1);
            this.Name = "Procesador";
            this.Text = "Procesador";
            this.Load += new System.EventHandler(this.Procesador_Load);
            this.P1.ResumeLayout(false);
            this.P1.PerformLayout();
            this.P2.ResumeLayout(false);
            this.P2.PerformLayout();
            this.P3.ResumeLayout(false);
            this.P3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.GroupBox P1;
        public System.Windows.Forms.Label R1;
        public System.Windows.Forms.Label ID1;
        public System.Windows.Forms.GroupBox P2;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.GroupBox P3;
        public System.Windows.Forms.Label ID3;
        public System.Windows.Forms.ListBox CD1;
        public System.Windows.Forms.ListBox CD2;
        public System.Windows.Forms.ListBox CD3;
        public System.Windows.Forms.Button siguienteBtn;
        public System.Windows.Forms.TextBox p1RelojTxt;
        public System.Windows.Forms.TextBox p1HililloTxt;
        public System.Windows.Forms.TextBox p2HililloTxt;
        public System.Windows.Forms.TextBox p3HililloTxt;
    }
}