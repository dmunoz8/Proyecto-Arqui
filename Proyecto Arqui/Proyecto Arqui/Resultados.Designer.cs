namespace Proyecto_Arqui
{
    partial class Resultados
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.regs = new System.Windows.Forms.RichTextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.datos = new System.Windows.Forms.RichTextBox();
            this.mem = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ciclos = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.regs);
            this.groupBox1.Location = new System.Drawing.Point(31, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(895, 141);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Registros";
            // 
            // regs
            // 
            this.regs.Location = new System.Drawing.Point(6, 19);
            this.regs.Name = "regs";
            this.regs.Size = new System.Drawing.Size(883, 112);
            this.regs.TabIndex = 3;
            this.regs.Text = "";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.datos);
            this.groupBox2.Location = new System.Drawing.Point(31, 159);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(895, 141);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Cache de Datos";
            // 
            // datos
            // 
            this.datos.Location = new System.Drawing.Point(6, 19);
            this.datos.Name = "datos";
            this.datos.Size = new System.Drawing.Size(883, 113);
            this.datos.TabIndex = 4;
            this.datos.Text = "";
            // 
            // mem
            // 
            this.mem.Location = new System.Drawing.Point(31, 425);
            this.mem.Name = "mem";
            this.mem.Size = new System.Drawing.Size(895, 145);
            this.mem.TabIndex = 2;
            this.mem.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 316);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Ciclos";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // ciclos
            // 
            this.ciclos.Location = new System.Drawing.Point(37, 332);
            this.ciclos.Name = "ciclos";
            this.ciclos.Size = new System.Drawing.Size(895, 55);
            this.ciclos.TabIndex = 8;
            this.ciclos.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 409);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Memoria";
            // 
            // Resultados
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(938, 583);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ciclos);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mem);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Resultados";
            this.Text = "Resultados";
            this.Load += new System.EventHandler(this.Resultados_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.RichTextBox regs;
        public System.Windows.Forms.RichTextBox datos;
        public System.Windows.Forms.RichTextBox mem;
        public System.Windows.Forms.RichTextBox ciclos;
    }
}