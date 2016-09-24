namespace Proyecto_Arqui
{
    partial class Datos
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.hilos = new System.Windows.Forms.TextBox();
            this.quantum = new System.Windows.Forms.TextBox();
            this.Direccion = new System.Windows.Forms.Button();
            this.buscar = new System.Windows.Forms.OpenFileDialog();
            this.NoDatos = new System.Windows.Forms.Button();
            this.SiDatos = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(47, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Número de Hilos";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(47, 141);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 19);
            this.label2.TabIndex = 1;
            this.label2.Text = "Ubicación";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Century Gothic", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(47, 215);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 19);
            this.label3.TabIndex = 2;
            this.label3.Text = "Quantum";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // hilos
            // 
            this.hilos.Location = new System.Drawing.Point(250, 68);
            this.hilos.Name = "hilos";
            this.hilos.Size = new System.Drawing.Size(100, 20);
            this.hilos.TabIndex = 3;
            // 
            // quantum
            // 
            this.quantum.Location = new System.Drawing.Point(250, 214);
            this.quantum.Name = "quantum";
            this.quantum.Size = new System.Drawing.Size(100, 20);
            this.quantum.TabIndex = 4;
            // 
            // Direccion
            // 
            this.Direccion.Location = new System.Drawing.Point(250, 137);
            this.Direccion.Name = "Direccion";
            this.Direccion.Size = new System.Drawing.Size(100, 30);
            this.Direccion.TabIndex = 5;
            this.Direccion.Text = "Buscar...";
            this.Direccion.UseVisualStyleBackColor = true;
            this.Direccion.Click += new System.EventHandler(this.Direccion_Click);
            // 
            // buscar
            // 
            this.buscar.FileName = "buscar";
            // 
            // NoDatos
            // 
            this.NoDatos.Location = new System.Drawing.Point(235, 269);
            this.NoDatos.Name = "NoDatos";
            this.NoDatos.Size = new System.Drawing.Size(115, 40);
            this.NoDatos.TabIndex = 7;
            this.NoDatos.Text = "Cancelar";
            this.NoDatos.UseVisualStyleBackColor = true;
            this.NoDatos.Click += new System.EventHandler(this.NoDatos_Click);
            // 
            // SiDatos
            // 
            this.SiDatos.Location = new System.Drawing.Point(51, 269);
            this.SiDatos.Name = "SiDatos";
            this.SiDatos.Size = new System.Drawing.Size(115, 40);
            this.SiDatos.TabIndex = 8;
            this.SiDatos.Text = "Aceptar";
            this.SiDatos.UseVisualStyleBackColor = true;
            this.SiDatos.Click += new System.EventHandler(this.SiDatos_Click);
            // 
            // Datos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 335);
            this.Controls.Add(this.SiDatos);
            this.Controls.Add(this.NoDatos);
            this.Controls.Add(this.Direccion);
            this.Controls.Add(this.quantum);
            this.Controls.Add(this.hilos);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Datos";
            this.Text = "Ingreso de Datos";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox hilos;
        private System.Windows.Forms.TextBox quantum;
        private System.Windows.Forms.Button Direccion;
        private System.Windows.Forms.OpenFileDialog buscar;
        private System.Windows.Forms.Button NoDatos;
        private System.Windows.Forms.Button SiDatos;
    }
}

