﻿namespace Proyecto_Arqui
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
            this.modoLentoLbl = new System.Windows.Forms.Label();
            this.ModoLentoCheck = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(56, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(141, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Número de Hilos";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(56, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Ubicación";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(56, 175);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Quantum";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // hilos
            // 
            this.hilos.Location = new System.Drawing.Point(259, 28);
            this.hilos.Name = "hilos";
            this.hilos.Size = new System.Drawing.Size(100, 20);
            this.hilos.TabIndex = 3;
            this.hilos.TextChanged += new System.EventHandler(this.hilos_TextChanged);
            // 
            // quantum
            // 
            this.quantum.Location = new System.Drawing.Point(259, 174);
            this.quantum.Name = "quantum";
            this.quantum.Size = new System.Drawing.Size(100, 20);
            this.quantum.TabIndex = 4;
            // 
            // Direccion
            // 
            this.Direccion.Location = new System.Drawing.Point(259, 97);
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
            // modoLentoLbl
            // 
            this.modoLentoLbl.AutoSize = true;
            this.modoLentoLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modoLentoLbl.Location = new System.Drawing.Point(56, 226);
            this.modoLentoLbl.Name = "modoLentoLbl";
            this.modoLentoLbl.Size = new System.Drawing.Size(104, 20);
            this.modoLentoLbl.TabIndex = 9;
            this.modoLentoLbl.Text = "Modo Lento";
            // 
            // ModoLentoCheck
            // 
            this.ModoLentoCheck.AutoSize = true;
            this.ModoLentoCheck.Location = new System.Drawing.Point(167, 228);
            this.ModoLentoCheck.Name = "ModoLentoCheck";
            this.ModoLentoCheck.Size = new System.Drawing.Size(15, 14);
            this.ModoLentoCheck.TabIndex = 10;
            this.ModoLentoCheck.UseVisualStyleBackColor = true;
            // 
            // Datos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 335);
            this.Controls.Add(this.ModoLentoCheck);
            this.Controls.Add(this.modoLentoLbl);
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
            this.Load += new System.EventHandler(this.Datos_Load);
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
        private System.Windows.Forms.Label modoLentoLbl;
        private System.Windows.Forms.CheckBox ModoLentoCheck;
    }
}

